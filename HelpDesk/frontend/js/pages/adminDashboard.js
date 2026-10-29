document.addEventListener('DOMContentLoaded', () => {

    // Role guard
    const user = Session.requireRole('Admin');
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
    let technicians = [];

    let allUsers = [];
    let userFilter = 'all';
    let lookupsLoaded = false;
    let profileLoaded = false;
    let activityLoaded = false;

    let pendingTicketId = null;
    let editingUserId = null;

    // Panel switching
    const panels = ['overviewPanel', 'ticketsPanel', 'usersPanel', 'profilePanel', 'activityPanel'];

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
        if (panelId === 'usersPanel' && allUsers.length === 0) loadUsers();
        if (panelId === 'profilePanel') renderProfilePlaceholder();
        if (panelId === 'activityPanel') loadActivity();
    }

    const refreshBtn = document.getElementById('refreshAllBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            const active = document.querySelector('.admin-panel.is-active')?.id;
            if (active === 'ticketsPanel') loadTickets();
            if (active === 'usersPanel') loadUsers();
            if (active === 'overviewPanel') loadTickets();
            if (active === 'activityPanel') loadActivity(true);
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
        if (!ticketsLoading || !ticketsTable || !ticketsBody) return;

        ticketsLoading.style.display = 'block';
        ticketsLoading.textContent = 'Loading tickets…';
        ticketsEmpty.style.display = 'none';
        ticketsTable.style.display = 'none';

        try {
            const res = await ApiClient.get('/api/admin/tickets');
            if (!res.ok || !res.data?.success) {
                ticketsLoading.textContent = 'Could not load tickets.';
                return;
            }
            allTickets = res.data.data || [];
            ticketsLoading.style.display = 'none';
            updateStats();
            renderTickets();
            await loadTechniciansForSelect();
        } catch (err) {
            console.error(err);
            ticketsLoading.textContent = 'Network error.';
        }
    }

    async function loadTechniciansForSelect() {
        try {
            const res = await ApiClient.get('/api/admin/technicians');
            if (res.ok && res.data?.success) {
                technicians = res.data.data || [];
                const sel = document.getElementById('assignTech');
                if (sel) {
                    sel.innerHTML = technicians.length
                        ? technicians.map(t => `<option value="${t.ussId}">${escapeHtml(t.fullName)} — ${escapeHtml(t.officeLocation || '')}</option>`).join('')
                        : '<option value="">No technicians available</option>';
                }
            }
        } catch (err) { console.error(err); }
    }

    // STATS
    function updateStats() {
        const counts = { Open: 0, Assigned: 0, InProgress: 0, Resolved: 0, Confirmed: 0, Closed: 0 };
        allTickets.forEach(t => {
            if (counts[t.currentStatus] !== undefined) counts[t.currentStatus]++;
        });
        const set = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };
        set('statOpen', counts.Open);
        set('statAssigned', counts.Assigned);
        set('statInProgress', counts.InProgress);
        set('statResolved', counts.Resolved);
        set('statConfirmed', counts.Confirmed);
        set('statClosed', counts.Closed);
    }

    // TICKET TABLE
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

        const status = t.currentStatus || '';
        const actions = [];

        let hint = '';
        if (status === 'Assigned') hint = '<span style="color:#4527a0;font-size:12px;">Waiting for technician</span>';
        if (status === 'InProgress') hint = '<span style="color:#a35a00;font-size:12px;">Technician is working on it</span>';
        if (status === 'Resolved') hint = '<span style="color:#1b5e20;font-size:12px;">Awaiting staff confirmation</span>';

        if (status === 'Open' && !t.assignedTo) {
            actions.push(`<button class="btn btn-primary btn-xs" data-action="assign" data-id="${t.ticketId}">Assign</button>`);
        }

        if (status === 'Resolved') {
            actions.push(
                `<button class="btn btn-danger-outline btn-xs" data-action="close"
                         data-id="${t.ticketId}"
                         title="Staff has not confirmed yet">Close anyway</button>`
            );
            actions.push(
                `<button class="btn btn-outline-dark btn-xs" data-action="reopen"
                         data-id="${t.ticketId}">Reopen</button>`
            );
        }

        if (status === 'Confirmed') {
            actions.push(`<button class="btn btn-primary btn-xs" data-action="close"  data-id="${t.ticketId}">Close</button>`);
            actions.push(`<button class="btn btn-outline-dark btn-xs" data-action="reopen" data-id="${t.ticketId}">Reopen</button>`);
        }

        if (status === 'Closed') {
            actions.push(`<button class="btn btn-outline-dark btn-xs" data-action="reopen" data-id="${t.ticketId}">Reopen</button>`);
        }

        if (status === 'Cancelled') {
            actions.push(`<button class="btn btn-outline-dark btn-xs" data-action="reopen" data-id="${t.ticketId}">Reopen</button>`);
        }

        const actionsHtml = actions.length
            ? actions.join('')
            : (hint || '<span style="color:#999">—</span>');

        tr.innerHTML = `
            <td>#${t.ticketId}</td>
            <td>${formatDate(t.createdDate)}</td>
            <td>${escapeHtml(t.issueDescription)}</td>
            <td><span class="badge badge-${(t.urgencyLevel || '').toLowerCase()}">${t.urgencyLevel}</span></td>
            <td><span class="badge badge-${(status || '').toLowerCase()}">${status}</span></td>
            <td>${escapeHtml(t.submittedBy)}</td>
            <td>${t.assignedTo ? escapeHtml(t.assignedTo) : '<span style="color:#999">—</span>'}</td>
            <td>${attachmentCell(t.imageId)}</td>
            <td><div class="row-actions">${actionsHtml}</div></td>
        `;
        return tr;
    }

    if (ticketsBody) {
        ticketsBody.addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-action]');
            if (!btn) return;
            const id = parseInt(btn.dataset.id, 10);
            if (btn.dataset.action === 'assign') openAssignModal(id);
            if (btn.dataset.action === 'close') await closeTicket(id);
            if (btn.dataset.action === 'reopen') await reopenTicket(id);
        });
    }

    // Assign modal
    const assignModal = document.getElementById('assignModal');
    const assignForm = document.getElementById('assignForm');
    const assignTech = document.getElementById('assignTech');
    const assignInstr = document.getElementById('assignInstruction');
    const assignDead = document.getElementById('assignDeadline');
    const assignMsg = document.getElementById('assignMessage');
    const assignTicketId = document.getElementById('assignTicketId');

    function openAssignModal(ticketId) {
        if (!assignModal || !assignForm) return;
        pendingTicketId = ticketId;
        if (assignTicketId) assignTicketId.textContent = `#${ticketId}`;
        assignForm.reset();
        hideAssignMessage();

        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        tomorrow.setHours(17, 0, 0, 0);
        if (assignDead) assignDead.value = toLocalInputValue(tomorrow);

        assignModal.style.display = 'flex';
    }

    const cancelAssignBtn = document.getElementById('cancelAssign');
    if (cancelAssignBtn) {
        cancelAssignBtn.addEventListener('click', () => {
            if (assignModal) assignModal.style.display = 'none';
        });
    }

    if (assignForm) {
        assignForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideAssignMessage();

            const ussId = parseInt(assignTech?.value, 10);
            const instruction = assignInstr?.value.trim();
            const deadline = assignDead?.value;

            if (!ussId) return showAssignError('Choose a technician.');
            if (!instruction) return showAssignError('Instruction is required.');
            if (!deadline) return showAssignError('Deadline is required.');

            const confirmBtn = document.getElementById('confirmAssignBtn');
            if (confirmBtn) { confirmBtn.disabled = true; confirmBtn.textContent = 'Assigning…'; }

            try {
                const res = await ApiClient.post(`/api/admin/assign/${user.personId}`, {
                    ticketId: pendingTicketId, ussId, instruction, deadline
                });

                if (res.ok && res.data?.success) {
                    activityLoaded = false;
                    if (assignModal) assignModal.style.display = 'none';
                    await loadTickets();
                } else {
                    showAssignError(res.data?.message || 'Could not assign ticket.');
                }
            } catch (err) {
                console.error(err);
                showAssignError('Network error.');
            } finally {
                if (confirmBtn) { confirmBtn.disabled = false; confirmBtn.textContent = 'Assign'; }
            }
        });
    }

    async function closeTicket(ticketId) {
        if (!confirm(`Close ticket #${ticketId}? This confirms the resolution is verified.`)) return;
        const res = await ApiClient.post(`/api/admin/tickets/${ticketId}/close/${user.personId}`, {});
        if (res.ok && res.data?.success) {
            activityLoaded = false;
            await loadTickets();
        } else {
            alert(res.data?.message || 'Could not close ticket.');
        }
    }

    async function reopenTicket(ticketId) {
        if (!confirm(`Reopen ticket #${ticketId}? The technician will need to revisit it.`)) return;
        const res = await ApiClient.post(`/api/admin/tickets/${ticketId}/reopen/${user.personId}`, {});
        if (res.ok && res.data?.success) {
            activityLoaded = false;
            await loadTickets();
        } else {
            alert(res.data?.message || 'Could not reopen ticket.');
        }
    }

    // USERS
    const usersLoading = document.getElementById('usersLoading');
    const usersEmpty = document.getElementById('usersEmpty');
    const usersTable = document.getElementById('usersTable');
    const usersBody = document.getElementById('usersBody');

    const userFilterTabsEl = document.getElementById('userFilterTabs');
    if (userFilterTabsEl) {
        userFilterTabsEl.addEventListener('click', e => {
            const btn = e.target.closest('.tab');
            if (!btn) return;
            document.querySelectorAll('#userFilterTabs .tab').forEach(b =>
                b.classList.toggle('is-active', b === btn));
            userFilter = btn.dataset.filter;
            renderUsers();
        });
    }

    async function loadUsers() {
        if (!usersLoading || !usersTable || !usersBody) return;

        usersLoading.style.display = 'block';
        usersLoading.textContent = 'Loading users…';
        usersEmpty.style.display = 'none';
        usersTable.style.display = 'none';

        try {
            const res = await ApiClient.get('/api/admin/users/all');
            if (!res.ok || !res.data?.success) {
                usersLoading.textContent = 'Could not load users.';
                return;
            }
            allUsers = res.data.data || [];
            usersLoading.style.display = 'none';
            renderUsers();
        } catch (err) {
            console.error(err);
            usersLoading.textContent = 'Network error.';
        }
    }

    function renderUsers() {
        if (!usersBody || !usersTable || !usersEmpty) return;

        usersBody.innerHTML = '';
        const list = userFilter === 'all'
            ? allUsers
            : allUsers.filter(u => u.role === userFilter);

        if (list.length === 0) {
            usersEmpty.style.display = 'block';
            usersTable.style.display = 'none';
            return;
        }
        usersEmpty.style.display = 'none';
        usersTable.style.display = 'table';
        list.forEach(u => usersBody.appendChild(renderUserRow(u)));
    }

    function renderUserRow(u) {
        const tr = document.createElement('tr');
        const roleClass = 'role-' + (u.role || 'none').toLowerCase();
        const office = u.officeNo
            ? `${escapeHtml(u.officeNo)} — ${escapeHtml(u.officeLocation)}`
            : '—';

        tr.innerHTML = `
            <td>#${u.personId}</td>
            <td>${escapeHtml(u.fullName)}</td>
            <td>${escapeHtml(u.username)}</td>
            <td>${escapeHtml(u.workEmail)}</td>
            <td>${escapeHtml(u.staffNo || '—')}</td>
            <td><span class="role-badge ${roleClass}">${u.role}</span></td>
            <td>${office}</td>
            <td>${u.registeredAt ? formatDate(u.registeredAt) : '—'}</td>
            <td>
                <button class="btn btn-outline-dark btn-xs"
                        data-action="edit-user"
                        data-id="${u.personId}">Edit</button>
            </td>
        `;
        return tr;
    }

    if (usersBody) {
        usersBody.addEventListener('click', (e) => {
            const btn = e.target.closest('[data-action="edit-user"]');
            if (!btn) return;
            const id = parseInt(btn.dataset.id, 10);
            const userRow = (allUsers || []).find(u => u.personId === id);
            if (userRow) openEditUserModal(userRow);
        });
    }

    // Add Technician modal
    const techModal = document.getElementById('techModal');
    const techForm = document.getElementById('techForm');
    const techMessage = document.getElementById('techMessage');
    const selJobId = document.getElementById('tJobId');
    const selEStatusId = document.getElementById('tEStatusId');
    const selMid = document.getElementById('tMid');
    const selDepId = document.getElementById('tDepId');

    const addTechBtn = document.getElementById('addTechnicianBtn');
    if (addTechBtn) {
        addTechBtn.addEventListener('click', async () => {
            techForm?.reset();
            hideTechMessage();

            if (!lookupsLoaded) {
                try {
                    const res = await ApiClient.get(`/api/admin/users/lookups/${user.personId}`);
                    if (!res.ok || !res.data?.success) {
                        alert(res.data?.message || 'Could not load lookups.');
                        return;
                    }
                    const L = res.data.data;
                    if (selJobId) selJobId.innerHTML = L.jobTitles.map(j => `<option value="${j.id}">${escapeHtml(j.name)}</option>`).join('');
                    if (selEStatusId) selEStatusId.innerHTML = L.employmentStatuses.map(e => `<option value="${e.id}">${escapeHtml(e.name)}</option>`).join('');
                    if (selMid) selMid.innerHTML = L.municipalities.map(m => `<option value="${m.id}" selected>${escapeHtml(m.name)}</option>`).join('');
                    if (selDepId) selDepId.innerHTML = '<option value="">-- Select department --</option>' +
                        L.departments.map(d => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join('');
                    lookupsLoaded = true;
                } catch (err) {
                    console.error(err);
                    alert('Network error loading lookups.');
                    return;
                }
            }

            if (techModal) techModal.style.display = 'flex';
        });
    }

    const cancelTechBtn = document.getElementById('cancelTechBtn');
    if (cancelTechBtn) {
        cancelTechBtn.addEventListener('click', () => {
            if (techModal) techModal.style.display = 'none';
        });
    }

    if (techForm) {
        techForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideTechMessage();

            const payload = {
                firstName: document.getElementById('tFirstName')?.value.trim(),
                lastName: document.getElementById('tLastName')?.value.trim(),
                workEmail: document.getElementById('tEmail')?.value.trim(),
                cellNo: document.getElementById('tCell')?.value.trim(),
                username: document.getElementById('tUsername')?.value.trim(),
                password: document.getElementById('tPassword')?.value,
                staffNo: document.getElementById('tStaffNo')?.value.trim(),
                officeNo: document.getElementById('tOfficeNo')?.value.trim(),
                officeLocation: document.getElementById('tOfficeLocation')?.value.trim(),
                jobId: parseInt(selJobId?.value, 10),
                eStatusId: parseInt(selEStatusId?.value, 10),
                mid: parseInt(selMid?.value, 10),
                depId: parseInt(selDepId?.value, 10)
            };

            if (!payload.firstName || !payload.lastName || !payload.username || !payload.password
                || !payload.staffNo || !payload.workEmail) {
                return showTechError('Please fill in all required fields.');
            }
            if (payload.password.length < 6) {
                return showTechError('Password must be at least 6 characters.');
            }
            if (!payload.depId) {
                return showTechError('Please select a department.');
            }

            const submitBtn = document.getElementById('submitTechBtn');
            if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Creating…'; }

            try {
                const res = await ApiClient.post('/api/admin/users/technician', payload);

                if (res.ok && res.data?.success) {
                    showTechSuccess(res.data.message || 'Technician created.');
                    setTimeout(() => {
                        if (techModal) techModal.style.display = 'none';
                        allUsers = [];
                        loadUsers();
                    }, 3500);
                } else {
                    showTechError(res.data?.message || 'Could not create technician.');
                }
            } catch (err) {
                console.error(err);
                showTechError('Network error.');
            } finally {
                if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Create technician'; }
            }
        });
    }

    // EDIT USER MODAL
    const editUserModal = document.getElementById('editUserModal');
    const editUserForm = document.getElementById('editUserForm');
    const editUserName = document.getElementById('editUserName');
    const euFirstName = document.getElementById('euFirstName');
    const euLastName = document.getElementById('euLastName');
    const euUsername = document.getElementById('euUsername');
    const euWorkEmail = document.getElementById('euWorkEmail');
    const euCellNo = document.getElementById('euCellNo');
    const euStaffNo = document.getElementById('euStaffNo');
    const euRole = document.getElementById('euRole');
    const editUserMessage = document.getElementById('editUserMessage');

    function openEditUserModal(u) {
        if (!editUserModal) return;

        editingUserId = u.personId;
        if (editUserName) editUserName.textContent = `#${u.personId}`;
        if (euFirstName) euFirstName.value = (u.fullName || '').split(' ').slice(0, -1).join(' ') || u.fullName || '';
        if (euLastName) euLastName.value = (u.fullName || '').split(' ').slice(-1)[0] || '';
        if (euUsername) euUsername.value = u.username || '';
        if (euWorkEmail) euWorkEmail.value = u.workEmail || '';
        if (euCellNo) euCellNo.value = u.cellNo || '';
        if (euStaffNo) euStaffNo.value = u.staffNo || '';
        if (euRole) euRole.value = u.role || '';
        hideEditUserMessage();

        editUserModal.style.display = 'flex';
        euFirstName?.focus();
    }

    const cancelEditUserBtn = document.getElementById('cancelEditUser');
    if (cancelEditUserBtn) {
        cancelEditUserBtn.addEventListener('click', () => {
            if (editUserModal) editUserModal.style.display = 'none';
        });
    }

    if (editUserForm) {
        editUserForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideEditUserMessage();

            const payload = {
                firstName: euFirstName?.value.trim(),
                lastName: euLastName?.value.trim(),
                workEmail: euWorkEmail?.value.trim(),
                cellNo: euCellNo?.value.trim() || ''
            };

            if (!payload.firstName || !payload.lastName || !payload.workEmail) {
                return showEditUserError('First name, last name, and email are required.');
            }

            const btn = document.getElementById('saveEditUserBtn');
            if (btn) { btn.disabled = true; btn.textContent = 'Saving…'; }

            try {
                const res = await ApiClient.post(
                    `/api/admin/users/${editingUserId}/edit`, payload);

                if (res.ok && res.data?.success) {
                    if (editUserModal) editUserModal.style.display = 'none';
                    allUsers = [];
                    await loadUsers();
                } else {
                    showEditUserError(res.data?.message || 'Could not update user.');
                }
            } catch (err) {
                console.error(err);
                showEditUserError('Network error.');
            } finally {
                if (btn) { btn.disabled = false; btn.textContent = 'Save changes'; }
            }
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

    function renderProfilePlaceholder() {
        loadProfile(false);
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
            if (saveBtn) { saveBtn.disabled = true; saveBtn.textContent = 'Saving…'; }

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
                if (saveBtn) { saveBtn.disabled = false; saveBtn.textContent = 'Save changes'; }
            }
        });
    }

    // ACTIVITY LOG
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
            const res = await ApiClient.get('/api/activity');
            if (!res.ok || !res.data?.success) {
                activityLoading.textContent = 'Could not load activity.';
                return;
            }

            const items = res.data.data || [];
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
            confirmed: 'Confirmed by reporter',
            resolutionrejected: 'Reporter says "not fixed"',
            closed: 'Ticket closed',
            reopened: 'Ticket reopened',
            cancelled: 'Ticket cancelled',
            edited: 'Ticket edited'
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
                    <a href="#" data-ticket="${a.ticketId}" class="activity-ticket-link">
                        #${a.ticketId}
                    </a>
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

    if (activityList) {
        activityList.addEventListener('click', (e) => {
            const link = e.target.closest('.activity-ticket-link');
            if (!link) return;
            e.preventDefault();
            switchPanel('ticketsPanel');
        });
    }

    // Helpers
    function scrollMessageIntoView(el) {
        if (!el) return;
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function showAssignError(m) {
        if (!assignMsg) return;
        assignMsg.className = 'form-message is-error';
        assignMsg.textContent = m;
        scrollMessageIntoView(assignMsg);
    }
    function hideAssignMessage() {
        if (!assignMsg) return;
        assignMsg.className = 'form-message';
        assignMsg.textContent = '';
    }

    function showTechError(m) {
        if (!techMessage) return;
        techMessage.className = 'form-message is-error';
        techMessage.textContent = m;
        scrollMessageIntoView(techMessage);
    }
    function showTechSuccess(m) {
        if (!techMessage) return;
        techMessage.className = 'form-message is-success';
        techMessage.textContent = m;
        scrollMessageIntoView(techMessage);
    }
    function hideTechMessage() {
        if (!techMessage) return;
        techMessage.className = 'form-message';
        techMessage.textContent = '';
    }

    function showEditUserError(m) {
        if (!editUserMessage) return;
        editUserMessage.className = 'form-message is-error';
        editUserMessage.textContent = m;
        scrollMessageIntoView(editUserMessage);
    }
    function hideEditUserMessage() {
        if (!editUserMessage) return;
        editUserMessage.className = 'form-message';
        editUserMessage.textContent = '';
    }

    function showProfileError(m) {
        if (!profileMessage) return;
        profileMessage.className = 'form-message is-error';
        profileMessage.textContent = m;
        scrollMessageIntoView(profileMessage);
    }
    function showProfileSuccess(m) {
        if (!profileMessage) return;
        profileMessage.className = 'form-message is-success';
        profileMessage.textContent = m;
        scrollMessageIntoView(profileMessage);
    }
    function hideProfileMessage() {
        if (!profileMessage) return;
        profileMessage.className = 'form-message';
        profileMessage.textContent = '';
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
    }    function toLocalInputValue(d) {
        const p = n => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
    }
    function escapeHtml(s) {
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