document.addEventListener('DOMContentLoaded', () => {

    // Role guard
    const user = Session.requireRole('Technician');
    if (!user) return;

    const welcomeEl = document.getElementById('welcomeUser');
    if (welcomeEl) welcomeEl.textContent = `${user.firstName} ${user.lastName}`;

    const logoutEl = document.getElementById('logoutBtn');
    if (logoutEl) {
        logoutEl.addEventListener('click', () => {
            Session.clear();
            window.location.href = '../login.html';
        });
    }

    // State
    let allTickets = [];
    let currentFilter = 'all';
    let pendingTicketId = null;
    let activityLoaded = false;
    let profileLoaded = false;

    // Panel switching
    const panels = ['ticketsPanel', 'activityPanel', 'profilePanel'];

    document.querySelectorAll('.sidebar-item[data-panel]').forEach(item => {
        item.addEventListener('click', () => switchPanel(item.dataset.panel));
    });

    function switchPanel(panelId) {
        panels.forEach(pid => document.getElementById(pid)?.classList.remove('is-active'));
        document.getElementById(panelId)?.classList.add('is-active');

        document.querySelectorAll('.sidebar-item[data-panel]').forEach(item => {
            item.classList.toggle('is-active', item.dataset.panel === panelId);
        });

        if (panelId === 'ticketsPanel' && allTickets.length === 0) loadTickets();
        if (panelId === 'activityPanel') loadActivity();
        if (panelId === 'profilePanel') loadProfile();
    }

    const refreshBtn = document.getElementById('refreshAllBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            const active = document.querySelector('.admin-panel.is-active')?.id;
            if (active === 'ticketsPanel') loadTickets();
            if (active === 'activityPanel') loadActivity(true);
            if (active === 'profilePanel') loadProfile(true);
        });
    }

    // TICKETS
    const ticketsLoading = document.getElementById('ticketsLoading');
    const ticketsEmpty = document.getElementById('ticketsEmpty');
    const ticketsTable = document.getElementById('ticketsTable');
    const ticketsBody = document.getElementById('ticketsBody');

    const filterTabsEl = document.getElementById('filterTabs');
    if (filterTabsEl) {
        filterTabsEl.addEventListener('click', e => {
            const btn = e.target.closest('.tab');
            if (!btn) return;
            document.querySelectorAll('#filterTabs .tab').forEach(b =>
                b.classList.toggle('is-active', b === btn));
            currentFilter = btn.dataset.filter;
            renderTickets();
        });
    }

    async function loadTickets() {
        if (!ticketsLoading || !ticketsBody) return;

        ticketsLoading.style.display = 'block';
        ticketsLoading.textContent = 'Loading tickets…';
        ticketsEmpty.style.display = 'none';
        ticketsTable.style.display = 'none';

        try {
            const res = await ApiClient.get(`/api/technician/tickets/${user.personId}`);
            if (!res.ok || !res.data?.success) {
                ticketsLoading.textContent = 'Could not load tickets.';
                return;
            }
            allTickets = res.data.data || [];
            ticketsLoading.style.display = 'none';
            renderTickets();
        } catch (err) {
            console.error(err);
            ticketsLoading.textContent = 'Network error.';
        }
    }

    function renderTickets() {
        if (!ticketsBody || !ticketsTable || !ticketsEmpty) return;

        ticketsBody.innerHTML = '';
        const list = currentFilter === 'all'
            ? allTickets
            : allTickets.filter(t => t.currentStatus === currentFilter);

        if (list.length === 0) {
            ticketsEmpty.style.display = 'block';
            ticketsTable.style.display = 'none';
            return;
        }
        ticketsEmpty.style.display = 'none';
        ticketsTable.style.display = 'table';
        list.forEach(t => ticketsBody.appendChild(renderTicketRow(t)));
    }

    function renderTicketRow(t) {
        const tr = document.createElement('tr');

        const actions = [];
        if (t.currentStatus === 'Assigned') {
            actions.push(`<button class="btn btn-primary btn-xs" data-action="start" data-id="${t.ticketId}">Start</button>`);
        }
        if (t.currentStatus === 'InProgress') {
            actions.push(`<button class="btn btn-primary btn-xs" data-action="resolve" data-id="${t.ticketId}">Resolve</button>`);
        }
        if (t.currentStatus === 'Resolved') {
            actions.push(`<span style="color:#1b5e20;font-size:12px;">Awaiting admin review</span>`);
        }

        tr.innerHTML = `
            <td>
                <a href="#" class="ticket-link" data-ticket="${t.ticketId}">#${t.ticketId}</a>
            </td>
            <td>${formatDate(t.createdDate)}</td>
            <td>${escapeHtml(t.issueDescription)}</td>
            <td>${escapeHtml(t.submittedBy)}</td>
            <td><span class="badge badge-${(t.urgencyLevel || '').toLowerCase()}">${t.urgencyLevel}</span></td>
            <td><span class="badge badge-${(t.currentStatus || '').toLowerCase()}">${t.currentStatus}</span></td>
            <td>${t.deadline ? formatDate(t.deadline) : '—'}</td>
            <td>${attachmentCell(t.imageId)}</td>
            <td><div class="row-actions">${actions.join('') || '<span style="color:#999">—</span>'}</div></td>
        `;
        return tr;
    }

    if (ticketsBody) {
        ticketsBody.addEventListener('click', async (e) => {
            const link = e.target.closest('.ticket-link');
            if (link) {
                e.preventDefault();
                openTimelineModal(parseInt(link.dataset.ticket, 10));
                return;
            }

            const btn = e.target.closest('[data-action]');
            if (!btn) return;
            const id = parseInt(btn.dataset.id, 10);

            if (btn.dataset.action === 'start') {
                if (!confirm(`Start working on ticket #${id}?`)) return;
                await startTicket(id);
            }
            if (btn.dataset.action === 'resolve') {
                openResolveModal(id);
            }
        });
    }

    async function startTicket(ticketId) {
        try {
            const res = await ApiClient.post(
                `/api/technician/tickets/${ticketId}/start/${user.personId}`, {});
            if (res.ok && res.data?.success) {
                activityLoaded = false;
                await loadTickets();
            } else {
                alert(res.data?.message || 'Could not start ticket.');
            }
        } catch (err) {
            console.error(err);
            alert('Network error.');
        }
    }

    // RESOLVE MODAL
    const resolveModal = document.getElementById('resolveModal');
    const resolveForm = document.getElementById('resolveForm');
    const resolveNote = document.getElementById('resolveNote');
    const resolveMsg = document.getElementById('resolveMessage');
    const resolveTicketId = document.getElementById('resolveTicketId');
    const resolveCharCount = document.getElementById('resolveCharCount');

    function openResolveModal(ticketId) {
        if (!resolveModal) return;
        pendingTicketId = ticketId;
        if (resolveTicketId) resolveTicketId.textContent = `#${ticketId}`;
        resolveForm?.reset();
        if (resolveCharCount) resolveCharCount.textContent = '0';
        hideResolveMessage();
        resolveModal.style.display = 'flex';
    }

    const cancelResolveBtn = document.getElementById('cancelResolve');
    if (cancelResolveBtn) {
        cancelResolveBtn.addEventListener('click', () => {
            if (resolveModal) resolveModal.style.display = 'none';
        });
    }

    if (resolveNote) {
        resolveNote.addEventListener('input', () => {
            if (resolveCharCount) resolveCharCount.textContent = resolveNote.value.length;
        });
    }

    if (resolveForm) {
        resolveForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideResolveMessage();

            const note = resolveNote?.value.trim();
            if (!note) return showResolveError('Please describe what you did.');

            const btn = document.getElementById('confirmResolveBtn');
            if (btn) { btn.disabled = true; btn.textContent = 'Resolving…'; }

            try {
                const res = await ApiClient.post(
                    `/api/technician/tickets/${pendingTicketId}/resolve/${user.personId}`,
                    { note });

                if (res.ok && res.data?.success) {
                    if (resolveModal) resolveModal.style.display = 'none';
                    activityLoaded = false;
                    await loadTickets();
                } else {
                    showResolveError(res.data?.message || 'Could not resolve ticket.');
                }
            } catch (err) {
                console.error(err);
                showResolveError('Network error.');
            } finally {
                if (btn) { btn.disabled = false; btn.textContent = 'Resolve'; }
            }
        });
    }

    // ACTIVITY PANEL
    const activityLoading = document.getElementById('activityLoading');
    const activityEmpty = document.getElementById('activityEmpty');
    const activityList = document.getElementById('activityList');

    const refreshActivityBtn = document.getElementById('refreshActivityBtn');
    if (refreshActivityBtn) {
        refreshActivityBtn.addEventListener('click', () => loadActivity(true));
    }

    async function loadActivity(force = false) {
        if (!activityList) return;
        if (activityLoaded && !force) return;

        activityLoading.style.display = 'block';
        activityLoading.textContent = 'Loading activity…';
        activityEmpty.style.display = 'none';
        activityList.style.display = 'none';

        try {
            if (allTickets.length === 0) await loadTickets();

            const res = await ApiClient.get('/api/activity');
            if (!res.ok || !res.data?.success) {
                activityLoading.textContent = 'Could not load activity.';
                return;
            }

            const mineTicketIds = new Set(allTickets.map(t => t.ticketId));
            const items = (res.data.data || []).filter(a => mineTicketIds.has(a.ticketId));
            activityLoading.style.display = 'none';

            if (items.length === 0) {
                activityEmpty.style.display = 'block';
                return;
            }

            activityList.style.display = 'flex';
            activityList.innerHTML = '';
            items.forEach(a => activityList.appendChild(renderActivityItem(a)));
            activityLoaded = true;
        } catch (err) {
            console.error(err);
            activityLoading.textContent = 'Network error.';
        }
    }

    function renderActivityItem(a) {
        const div = document.createElement('div');
        div.className = 'activity-item';

        const action = (a.action || '').toLowerCase();
        const initial = (a.action || '?').charAt(0).toUpperCase();

        const actionLabel = {
            created: 'Ticket created',
            assigned: 'Ticket assigned',
            started: 'Work started',
            resolved: 'Ticket resolved',
            closed: 'Ticket closed',
            reopened: 'Ticket reopened'
        }[action] || a.action;

        const meta = a.performedBy
            ? `by <strong>${escapeHtml(a.performedBy)}</strong>`
            : '';

        const note = a.note
            ? `<div class="activity-note">"${escapeHtml(a.note)}"</div>`
            : '';

        div.innerHTML = `
            <div class="activity-time">${formatDate(a.timestamp)}</div>
            <div class="activity-icon activity-${action}">${initial}</div>
            <div class="activity-body">
                <div class="activity-title">
                    <a href="#" data-ticket="${a.ticketId}" class="ticket-link">#${a.ticketId}</a>
                    — ${escapeHtml(actionLabel)}
                </div>
                <div class="activity-meta">
                    ${meta}
                    ${a.ticketSummary ? ' · ' + escapeHtml(a.ticketSummary) + '…' : ''}
                </div>
                ${note}
            </div>
        `;
        return div;
    }

    // Global click handler for ticket links inside activity list
    document.addEventListener('click', (e) => {
        const link = e.target.closest('.ticket-link');
        if (!link) return;
        if (link.closest('#ticketsBody')) return;
        e.preventDefault();
        openTimelineModal(parseInt(link.dataset.ticket, 10));
    });

    // TIMELINE MODAL
    const timelineModal = document.getElementById('timelineModal');
    const timelineTicketId = document.getElementById('timelineTicketId');
    const timelineLoading = document.getElementById('timelineLoading');
    const timelineEmpty = document.getElementById('timelineEmpty');
    const timelineList = document.getElementById('timelineList');

    async function openTimelineModal(ticketId) {
        if (!timelineModal) return;

        if (timelineTicketId) timelineTicketId.textContent = `#${ticketId}`;
        timelineLoading.style.display = 'block';
        timelineLoading.textContent = 'Loading timeline…';
        timelineEmpty.style.display = 'none';
        timelineList.style.display = 'none';
        timelineList.innerHTML = '';
        timelineModal.style.display = 'flex';

        try {
            const res = await ApiClient.get(`/api/activity/ticket/${ticketId}`);
            if (!res.ok || !res.data?.success) {
                timelineLoading.textContent = 'Could not load timeline.';
                return;
            }

            const items = res.data.data || [];
            timelineLoading.style.display = 'none';

            if (items.length === 0) {
                timelineEmpty.style.display = 'block';
                return;
            }

            timelineList.style.display = 'flex';
            items.forEach(a => timelineList.appendChild(renderActivityItem(a)));
        } catch (err) {
            console.error(err);
            timelineLoading.textContent = 'Network error.';
        }
    }

    const closeTimelineBtn = document.getElementById('closeTimelineBtn');
    if (closeTimelineBtn) {
        closeTimelineBtn.addEventListener('click', () => {
            if (timelineModal) timelineModal.style.display = 'none';
        });
    }

    // PROFILE
    const profileLoading = document.getElementById('profileLoading');
    const profileForm = document.getElementById('profileForm');
    const profileMessage = document.getElementById('profileMessage');
    const pFirstName = document.getElementById('pFirstName');
    const pLastName = document.getElementById('pLastName');
    const pUsername = document.getElementById('pUsername');
    const pWorkEmail = document.getElementById('pWorkEmail');
    const pCellNo = document.getElementById('pCellNo');
    const pStaffNo = document.getElementById('pStaffNo');
    const pNewPassword = document.getElementById('pNewPassword');

    async function loadProfile(force = false) {
        if (!profileForm) return;
        if (profileLoaded && !force) return;

        if (profileLoading) profileLoading.style.display = 'block';
        profileForm.style.display = 'none';
        hideProfileMessage();

        try {
            const res = await ApiClient.get(`/api/admin/users/profile/${user.personId}`);
            if (!res.ok || !res.data?.success) {
                if (profileLoading) profileLoading.textContent = res.data?.message || 'Could not load profile.';
                return;
            }
            const p = res.data.data;

            if (pFirstName) pFirstName.value = p.firstName || '';
            if (pLastName) pLastName.value = p.lastName || '';
            if (pUsername) pUsername.value = p.username || '';
            if (pWorkEmail) pWorkEmail.value = p.workEmail || '';
            if (pCellNo) pCellNo.value = p.cellNo || '';
            if (pStaffNo) pStaffNo.value = p.staffNo || '';
            if (pNewPassword) pNewPassword.value = '';

            if (profileLoading) profileLoading.style.display = 'none';
            profileForm.style.display = 'block';
            profileLoaded = true;
        } catch (err) {
            console.error(err);
            if (profileLoading) profileLoading.textContent = 'Network error.';
        }
    }

    if (profileForm) {
        profileForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideProfileMessage();

            const payload = {
                firstName: pFirstName?.value.trim(),
                lastName: pLastName?.value.trim(),
                workEmail: pWorkEmail?.value.trim(),
                cellNo: pCellNo?.value.trim() || '',
                newPassword: pNewPassword?.value || ''
            };

            if (!payload.firstName || !payload.lastName || !payload.workEmail) {
                return showProfileError('First name, last name, and email are required.');
            }
            if (payload.newPassword && payload.newPassword.length < 6) {
                return showProfileError('New password must be at least 6 characters.');
            }

            const saveBtn = document.getElementById('saveProfileBtn');
            if (saveBtn) {
                saveBtn.disabled = true;
                saveBtn.textContent = 'Saving…';
            }

            try {
                const res = await ApiClient.post(
                    `/api/admin/users/profile/${user.personId}`, payload);

                if (res.ok && res.data?.success) {
                    showProfileSuccess(res.data.message || 'Profile updated.');

                    const updated = res.data.data;
                    if (updated) {
                        user.firstName = updated.firstName;
                        user.lastName = updated.lastName;
                        Session.save(user);
                        const welcome = document.getElementById('welcomeUser');
                        if (welcome) welcome.textContent = `${updated.firstName} ${updated.lastName}`;
                    }

                    if (pNewPassword) pNewPassword.value = '';
                } else {
                    showProfileError(res.data?.message || 'Could not update profile.');
                }
            } catch (err) {
                console.error(err);
                showProfileError('Network error.');
            } finally {
                if (saveBtn) {
                    saveBtn.disabled = false;
                    saveBtn.textContent = 'Save changes';
                }
            }
        });
    }

    function showProfileError(m) {
        if (!profileMessage) return;
        profileMessage.className = 'form-message is-error';
        profileMessage.textContent = m;
    }
    function showProfileSuccess(m) {
        if (!profileMessage) return;
        profileMessage.className = 'form-message is-success';
        profileMessage.textContent = m;
    }
    function hideProfileMessage() {
        if (!profileMessage) return;
        profileMessage.className = 'form-message';
        profileMessage.textContent = '';
    }

    // Helpers
    function showResolveError(m) {
        if (!resolveMsg) return;
        resolveMsg.className = 'form-message is-error';
        resolveMsg.textContent = m;
    }
    function hideResolveMessage() {
        if (!resolveMsg) return;
        resolveMsg.className = 'form-message';
        resolveMsg.textContent = '';
    }
    function formatDate(iso) {
        if (!iso) return '—';

        const utcString = iso.endsWith('Z') || iso.includes('+') ? iso : iso + 'Z';
        const d = new Date(utcString);
        return d.toLocaleString('en-ZA', {
            year: 'numeric', month: 'short', day: '2-digit',
            hour: '2-digit', minute: '2-digit',
            timeZone: 'Africa/Johannesburg'
        });
    }    function escapeHtml(s) {
        return String(s ?? '')
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }
    function attachmentCell(imageId) {
        if (!imageId) return '<span style="color:#999">—</span>';
        return `
            <a href="/api/attachments/${imageId}" target="_blank" title="View attachment">
                <img src="/api/attachments/${imageId}"
                     alt="Attachment"
                     class="ticket-thumb" />
            </a>
        `;
    }

    // Initial
    loadTickets();
});