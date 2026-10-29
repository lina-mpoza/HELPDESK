document.addEventListener('DOMContentLoaded', () => {

    // Role guard
    const user = Session.requireRole('GeneralStaff');
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
    let uploadedImageId = null;
    let ticketsLoaded = false;
    let profileLoaded = false;

    let editingTicketId = null;  
    let notFixedTicket = null;   

    // Panel switching
    const panels = ['ticketsPanel', 'profilePanel'];

    document.querySelectorAll('.sidebar-item[data-panel]').forEach(item => {
        item.addEventListener('click', () => switchPanel(item.dataset.panel));
    });

    function switchPanel(panelId) {
        panels.forEach(pid => document.getElementById(pid)?.classList.remove('is-active'));
        document.getElementById(panelId)?.classList.add('is-active');

        document.querySelectorAll('.sidebar-item[data-panel]').forEach(item => {
            item.classList.toggle('is-active', item.dataset.panel === panelId);
        });

        if (panelId === 'ticketsPanel') loadTickets();
        if (panelId === 'profilePanel') loadProfile();
    }

    const refreshBtn = document.getElementById('refreshAllBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            const active = document.querySelector('.admin-panel.is-active')?.id;
            if (active === 'ticketsPanel') loadTickets();
            if (active === 'profilePanel') loadProfile(true);
        });
    }

    // TICKETS
    const newTicketBtn = document.getElementById('newTicketBtn');
    const newTicketPanel = document.getElementById('newTicketPanel');
    const newTicketForm = document.getElementById('newTicketForm');
    const cancelBtn = document.getElementById('cancelNewTicket');
    const submitBtn = document.getElementById('submitTicketBtn');
    const msgEl = document.getElementById('newTicketMessage');
    const descEl = document.getElementById('issueDescription');
    const urgencyEl = document.getElementById('urgencyLevel');
    const charCount = document.getElementById('charCount');

    const fileInput = document.getElementById('attachmentInput');
    const previewBox = document.getElementById('attachmentPreview');
    const previewThumb = document.getElementById('attachmentThumb');
    const previewName = document.getElementById('attachmentName');
    const previewSize = document.getElementById('attachmentSize');
    const removeBtn = document.getElementById('removeAttachmentBtn');

    const ticketsLoading = document.getElementById('ticketsLoading');
    const ticketsEmpty = document.getElementById('ticketsEmpty');
    const ticketsTable = document.getElementById('ticketsTable');
    const ticketsBody = document.getElementById('ticketsBody');

    // Toggle new ticket panel
    if (newTicketBtn) {
        newTicketBtn.addEventListener('click', () => {
            if (newTicketPanel) newTicketPanel.style.display = 'block';
            newTicketBtn.style.display = 'none';
            descEl?.focus();
        });
    }

    if (cancelBtn) cancelBtn.addEventListener('click', () => resetForm());

    if (descEl) {
        descEl.addEventListener('input', () => {
            if (charCount) charCount.textContent = descEl.value.length;
        });
    }

    // File selection and upload
    if (fileInput) {
        fileInput.addEventListener('change', async () => {
            hideMessage();
            const file = fileInput.files && fileInput.files[0];
            if (!file) return;

            if (file.size > 5 * 1024 * 1024) {
                showError('File too large (max 5 MB).');
                fileInput.value = '';
                return;
            }

            if (previewThumb) previewThumb.src = URL.createObjectURL(file);
            if (previewName) previewName.textContent = file.name;
            if (previewSize) previewSize.textContent = formatBytes(file.size);
            if (previewBox) previewBox.style.display = 'flex';

            try {
                if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Uploading…'; }

                const formData = new FormData();
                formData.append('file', file);

                const res = await fetch('/api/attachments/upload', {
                    method: 'POST',
                    body: formData
                });

                const data = await res.json();
                if (res.ok && data.success) {
                    uploadedImageId = data.data.imageId;
                } else {
                    showError((data && data.message) || 'Upload failed.');
                    clearAttachment();
                }
            } catch (err) {
                console.error(err);
                showError('Upload failed — network error.');
                clearAttachment();
            } finally {
                if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Submit ticket'; }
            }
        });
    }

    if (removeBtn) removeBtn.addEventListener('click', () => clearAttachment());

    function clearAttachment() {
        uploadedImageId = null;
        if (fileInput) fileInput.value = '';
        if (previewBox) previewBox.style.display = 'none';
        if (previewThumb) previewThumb.src = '';
        if (previewName) previewName.textContent = '';
        if (previewSize) previewSize.textContent = '';
    }

    // Submit ticket
    if (newTicketForm) {
        newTicketForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideMessage();

            const description = descEl?.value.trim();
            const urgency = urgencyEl?.value || 'Medium';

            if (!description) return showError('Please describe the issue.');

            if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Submitting…'; }

            try {
                const res = await ApiClient.post(`/api/tickets/${user.personId}`, {
                    issueDescription: description,
                    urgencyLevel: urgency,
                    imageId: uploadedImageId
                });

                if (res.ok && res.data && res.data.success) {
                    showSuccess('Ticket submitted.');
                    resetForm();
                    await loadTickets();
                } else {
                    showError((res.data && res.data.message) || 'Could not submit ticket.');
                }
            } catch (err) {
                console.error(err);
                showError('Network error. Please try again.');
            } finally {
                if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Submit ticket'; }
            }
        });
    }

    async function loadTickets() {
        if (!ticketsLoading || !ticketsBody) return;

        ticketsLoading.style.display = 'block';
        ticketsLoading.textContent = 'Loading your tickets…';
        if (ticketsEmpty) ticketsEmpty.style.display = 'none';
        if (ticketsTable) ticketsTable.style.display = 'none';
        ticketsBody.innerHTML = '';

        try {
            const res = await ApiClient.get(`/api/tickets/mine/${user.personId}`);
            if (!res.ok || !res.data || !res.data.success) {
                ticketsLoading.textContent = 'Could not load tickets.';
                return;
            }

            const tickets = res.data.data || [];
            window.__staffTicketsCache = tickets;
            ticketsLoading.style.display = 'none';
            ticketsLoaded = true;

            if (tickets.length === 0) {
                if (ticketsEmpty) ticketsEmpty.style.display = 'block';
                return;
            }

            if (ticketsTable) ticketsTable.style.display = 'table';
            tickets.forEach(t => ticketsBody.appendChild(renderTicketRow(t)));
        } catch (err) {
            console.error(err);
            ticketsLoading.textContent = 'Network error loading tickets.';
        }
    }

    function renderTicketRow(t) {
        const tr = document.createElement('tr');

        const status = t.currentStatus || '';
        let actionsHtml = '<span style="color:#999">—</span>';

        if (status === 'Open') {
            actionsHtml = `
                <button class="btn btn-outline-dark btn-xs" data-action="edit"   data-id="${t.ticketId}">Edit</button>
                <button class="btn btn-outline-dark btn-xs" data-action="cancel" data-id="${t.ticketId}">Cancel</button>
            `;
        } else if (status === 'Resolved') {
            actionsHtml = `
                <button class="btn btn-primary btn-xs" data-action="confirm"   data-id="${t.ticketId}">Confirm Fixed</button>
                <button class="btn btn-outline-dark btn-xs" data-action="not-fixed" data-id="${t.ticketId}">Not Fixed</button>
            `;
        } else if (status === 'Confirmed') {
            actionsHtml = `<span style="color:#006064;font-size:12px;">Waiting for admin to close</span>`;
        }

        tr.innerHTML = `
            <td>#${t.ticketId}</td>
            <td>${formatDate(t.createdDate)}</td>
            <td>${escapeHtml(t.issueDescription)}</td>
            <td><span class="badge badge-${(t.urgencyLevel || '').toLowerCase()}">${t.urgencyLevel}</span></td>
            <td><span class="badge badge-${(status || '').toLowerCase()}">${status}</span></td>
            <td>${attachmentCell(t.imageId)}</td>
            <td><div class="row-actions">${actionsHtml}</div></td>
        `;
        return tr;
    }

    if (ticketsBody) {
        ticketsBody.addEventListener('click', (e) => {
            const btn = e.target.closest('[data-action]');
            if (!btn) return;

            const id = parseInt(btn.dataset.id, 10);
            const action = btn.dataset.action;

            const ticket = (window.__staffTicketsCache || []).find(t => t.ticketId === id);
            if (!ticket) return;

            if (action === 'edit') openEditModal(ticket);
            else if (action === 'cancel') cancelTicket(ticket);
            else if (action === 'confirm') confirmFixed(ticket);
            else if (action === 'not-fixed') openNotFixedModal(ticket);
        });
    }

    // CANCEL TICKET
    async function cancelTicket(ticket) {
        const confirmed = confirm(
            `Cancel ticket #${ticket.ticketId}?\n\n` +
            `"${ticket.issueDescription}"\n\n` +
            `This cannot be undone by you. The admin can still see the ticket in the audit log.`
        );
        if (!confirmed) return;

        try {
            const res = await ApiClient.post(
                `/api/tickets/${ticket.ticketId}/cancel/${user.personId}`, {});

            if (res.ok && res.data?.success) {
                await loadTickets();
            } else {
                alert(res.data?.message || 'Could not cancel ticket.');
            }
        } catch (err) {
            console.error(err);
            alert('Network error.');
        }
    }

    // ==================== CONFIRM FIXED ====================
    async function confirmFixed(ticket) {
        const confirmed = confirm(
            `Confirm that ticket #${ticket.ticketId} is fixed?\n\n` +
            `"${ticket.issueDescription}"\n\n` +
            `The admin will then close it.`
        );
        if (!confirmed) return;

        try {
            const res = await ApiClient.post(
                `/api/tickets/${ticket.ticketId}/confirm/${user.personId}`, {});

            if (res.ok && res.data?.success) {
                await loadTickets();
            } else {
                alert(res.data?.message || 'Could not confirm the ticket.');
            }
        } catch (err) {
            console.error(err);
            alert('Network error.');
        }
    }

    function resetForm() {
        newTicketForm?.reset();
        if (charCount) charCount.textContent = '0';
        clearAttachment();
        hideMessage();
        if (newTicketPanel) newTicketPanel.style.display = 'none';
        if (newTicketBtn) newTicketBtn.style.display = 'inline-block';
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

    // EDIT TICKET MODAL
    const editTicketModal = document.getElementById('editTicketModal');
    const editTicketForm = document.getElementById('editTicketForm');
    const editTicketId = document.getElementById('editTicketId');
    const editIssueDescription = document.getElementById('editIssueDescription');
    const editUrgencyLevel = document.getElementById('editUrgencyLevel');
    const editCharCount = document.getElementById('editCharCount');
    const editTicketMessage = document.getElementById('editTicketMessage');

    function openEditModal(ticket) {
        if (!editTicketModal) return;

        editingTicketId = ticket.ticketId;
        if (editTicketId) editTicketId.textContent = `#${ticket.ticketId}`;
        if (editIssueDescription) editIssueDescription.value = ticket.issueDescription || '';
        if (editUrgencyLevel) editUrgencyLevel.value = ticket.urgencyLevel || 'Medium';
        if (editCharCount) editCharCount.textContent = (ticket.issueDescription || '').length;
        hideEditMessage();

        editTicketModal.style.display = 'flex';
        editIssueDescription?.focus();
    }

    if (editIssueDescription) {
        editIssueDescription.addEventListener('input', () => {
            if (editCharCount) editCharCount.textContent = editIssueDescription.value.length;
        });
    }

    const cancelEditTicketBtn = document.getElementById('cancelEditTicket');
    if (cancelEditTicketBtn) {
        cancelEditTicketBtn.addEventListener('click', () => {
            if (editTicketModal) editTicketModal.style.display = 'none';
        });
    }

    if (editTicketForm) {
        editTicketForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideEditMessage();

            const description = editIssueDescription?.value.trim();
            const urgency = editUrgencyLevel?.value || 'Medium';

            if (!description) return showEditError('Please describe the issue.');

            const saveBtn = document.getElementById('saveEditTicketBtn');
            if (saveBtn) { saveBtn.disabled = true; saveBtn.textContent = 'Saving…'; }

            try {
                const res = await ApiClient.post(
                    `/api/tickets/${editingTicketId}/edit/${user.personId}`,
                    { issueDescription: description, urgencyLevel: urgency });

                if (res.ok && res.data?.success) {
                    if (editTicketModal) editTicketModal.style.display = 'none';
                    await loadTickets();
                } else {
                    showEditError(res.data?.message || 'Could not update ticket.');
                }
            } catch (err) {
                console.error(err);
                showEditError('Network error.');
            } finally {
                if (saveBtn) { saveBtn.disabled = false; saveBtn.textContent = 'Save changes'; }
            }
        });
    }

    // NOT FIXED MODAL
    const notFixedModal = document.getElementById('notFixedModal');
    const notFixedForm = document.getElementById('notFixedForm');
    const notFixedTicketId = document.getElementById('notFixedTicketId');
    const notFixedNote = document.getElementById('notFixedNote');
    const notFixedCharCount = document.getElementById('notFixedCharCount');
    const notFixedMessage = document.getElementById('notFixedMessage');

    function openNotFixedModal(ticket) {
        if (!notFixedModal) return;

        notFixedTicket = ticket;
        if (notFixedTicketId) notFixedTicketId.textContent = `#${ticket.ticketId}`;
        if (notFixedNote) notFixedNote.value = '';
        if (notFixedCharCount) notFixedCharCount.textContent = '0';
        hideNotFixedMessage();

        notFixedModal.style.display = 'flex';
        notFixedNote?.focus();
    }

    if (notFixedNote) {
        notFixedNote.addEventListener('input', () => {
            if (notFixedCharCount) notFixedCharCount.textContent = notFixedNote.value.length;
        });
    }

    const cancelNotFixedBtn = document.getElementById('cancelNotFixed');
    if (cancelNotFixedBtn) {
        cancelNotFixedBtn.addEventListener('click', () => {
            if (notFixedModal) notFixedModal.style.display = 'none';
        });
    }

    if (notFixedForm) {
        notFixedForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            hideNotFixedMessage();

            const note = notFixedNote?.value.trim();
            if (!note) return showNotFixedError('Please tell us what\'s still wrong.');

            const btn = document.getElementById('submitNotFixedBtn');
            if (btn) { btn.disabled = true; btn.textContent = 'Sending…'; }

            try {
                const res = await ApiClient.post(
                    `/api/tickets/${notFixedTicket.ticketId}/reject-resolution/${user.personId}`,
                    { note });

                if (res.ok && res.data?.success) {
                    if (notFixedModal) notFixedModal.style.display = 'none';
                    await loadTickets();
                } else {
                    showNotFixedError(res.data?.message || 'Could not send the ticket back.');
                }
            } catch (err) {
                console.error(err);
                showNotFixedError('Network error.');
            } finally {
                if (btn) { btn.disabled = false; btn.textContent = 'Send back to technician'; }
            }
        });
    }

    // Helpers

    function scrollMessageIntoView(el) {
        if (!el) return;
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    // ticket form message
    function showError(m) {
        if (!msgEl) return;
        msgEl.className = 'form-message is-error';
        msgEl.textContent = m;
        scrollMessageIntoView(msgEl);
    }
    function showSuccess(m) {
        if (!msgEl) return;
        msgEl.className = 'form-message is-success';
        msgEl.textContent = m;
        scrollMessageIntoView(msgEl);
    }
    function hideMessage() {
        if (!msgEl) return;
        msgEl.className = 'form-message';
        msgEl.textContent = '';
    }

    // -- Profile message --
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

    // -- Edit ticket message --
    function showEditError(m) {
        if (!editTicketMessage) return;
        editTicketMessage.className = 'form-message is-error';
        editTicketMessage.textContent = m;
        scrollMessageIntoView(editTicketMessage);
    }
    function hideEditMessage() {
        if (!editTicketMessage) return;
        editTicketMessage.className = 'form-message';
        editTicketMessage.textContent = '';
    }

    // -- Not fixed message --
    function showNotFixedError(m) {
        if (!notFixedMessage) return;
        notFixedMessage.className = 'form-message is-error';
        notFixedMessage.textContent = m;
        scrollMessageIntoView(notFixedMessage);
    }
    function hideNotFixedMessage() {
        if (!notFixedMessage) return;
        notFixedMessage.className = 'form-message';
        notFixedMessage.textContent = '';
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
    }    function formatBytes(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / 1024 / 1024).toFixed(2) + ' MB';
    }
    function escapeHtml(str) {
        return String(str ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
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