document.addEventListener('DOMContentLoaded', () => {
    // Role guard
    const user = Session.requireRole('Admin');
    if (!user) return;

    document.getElementById('welcomeUser').textContent =
        `Welcome, ${user.firstName} ${user.lastName}`;

    document.getElementById('logoutBtn').addEventListener('click', () => {
        Session.clear();
        window.location.href = '../login.html';
    });

    // State
    let allUsers       = [];
    let currentFilter  = 'all';
    let lookupsLoaded  = false;

    // Elements
    const loadingEl = document.getElementById('usersLoading');
    const emptyEl   = document.getElementById('usersEmpty');
    const tableEl   = document.getElementById('usersTable');
    const tbodyEl   = document.getElementById('usersBody');

    const techModal     = document.getElementById('techModal');
    const techForm      = document.getElementById('techForm');
    const techMessage   = document.getElementById('techMessage');

    const selJobId      = document.getElementById('tJobId');
    const selEStatusId  = document.getElementById('tEStatusId');
    const selMid        = document.getElementById('tMid');
    const selDepId      = document.getElementById('tDepId');

    // Filters
    document.getElementById('filterTabs').addEventListener('click', e => {
        const btn = e.target.closest('.tab');
        if (!btn) return;
        document.querySelectorAll('.filter-tabs .tab').forEach(b =>
            b.classList.toggle('is-active', b === btn));
        currentFilter = btn.dataset.filter;
        renderTable();
    });

    document.getElementById('refreshBtn').addEventListener('click', loadUsers);

    // Load users
    async function loadUsers() {
        loadingEl.style.display = 'block';
        loadingEl.textContent = 'Loading users…';
        emptyEl.style.display = 'none';
        tableEl.style.display = 'none';

        try {
            const res = await ApiClient.get('/api/admin/users/all');
            if (!res.ok || !res.data?.success) {
                loadingEl.textContent = 'Could not load users.';
                return;
            }
            allUsers = res.data.data || [];
            loadingEl.style.display = 'none';
            renderTable();
        } catch (err) {
            console.error(err);
            loadingEl.textContent = 'Network error.';
        }
    }

    function renderTable() {
        tbodyEl.innerHTML = '';
        const list = currentFilter === 'all'
            ? allUsers
            : allUsers.filter(u => u.role === currentFilter);

        if (list.length === 0) {
            emptyEl.style.display = 'block';
            tableEl.style.display = 'none';
            return;
        }
        emptyEl.style.display = 'none';
        tableEl.style.display = 'table';
        list.forEach(u => tbodyEl.appendChild(renderRow(u)));
    }

    function renderRow(u) {
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
        `;
        return tr;
    }

    // Open modal + load lookups
    document.getElementById('addTechnicianBtn').addEventListener('click', async () => {
        techForm.reset();
        hideTechMessage();

        if (!lookupsLoaded) {
            try {
                const res = await ApiClient.get(`/api/admin/users/lookups/${user.personId}`);
                if (!res.ok || !res.data?.success) {
                    alert(res.data?.message || 'Could not load lookups.');
                    return;
                }
                const L = res.data.data;

                selJobId.innerHTML     = L.jobTitles.map(j => `<option value="${j.id}">${escapeHtml(j.name)}</option>`).join('');
                selEStatusId.innerHTML = L.employmentStatuses.map(e => `<option value="${e.id}">${escapeHtml(e.name)}</option>`).join('');
                selMid.innerHTML       = L.municipalities.map(m => `<option value="${m.id}" selected>${escapeHtml(m.name)}</option>`).join('');
                selDepId.innerHTML     = '<option value="">-- Select department --</option>' +
                                          L.departments.map(d => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join('');

                lookupsLoaded = true;
            } catch (err) {
                console.error(err);
                alert('Network error loading lookups.');
                return;
            }
        }

        techModal.style.display = 'flex';
    });

    document.getElementById('cancelTechBtn').addEventListener('click', () => {
        techModal.style.display = 'none';
    });

    // Submit new technician
    techForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideTechMessage();

        const payload = {
            firstName:      document.getElementById('tFirstName').value.trim(),
            lastName:       document.getElementById('tLastName').value.trim(),
            workEmail:      document.getElementById('tEmail').value.trim(),
            cellNo:         document.getElementById('tCell').value.trim(),
            username:       document.getElementById('tUsername').value.trim(),
            password:       document.getElementById('tPassword').value,
            staffNo:        document.getElementById('tStaffNo').value.trim(),
            officeNo:       document.getElementById('tOfficeNo').value.trim(),
            officeLocation: document.getElementById('tOfficeLocation').value.trim(),
            jobId:          parseInt(selJobId.value, 10),
            eStatusId:      parseInt(selEStatusId.value, 10),
            mid:            parseInt(selMid.value, 10),
            depId:          parseInt(selDepId.value, 10)
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
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating…';

        try {
            const res = await ApiClient.post('/api/admin/users/technician', payload);

            if (res.ok && res.data?.success) {
                showTechSuccess('Technician created.');
                setTimeout(() => {
                    techModal.style.display = 'none';
                    loadUsers();
                }, 800);
            } else {
                showTechError(res.data?.message || 'Could not create technician.');
            }
        } catch (err) {
            console.error(err);
            showTechError('Network error.');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Create technician';
        }
    });

    // Helpers
    function showTechError(m) {
        techMessage.className = 'form-message is-error';
        techMessage.textContent = m;
    }
    function showTechSuccess(m) {
        techMessage.className = 'form-message is-success';
        techMessage.textContent = m;
    }
    function hideTechMessage() {
        techMessage.className = 'form-message';
        techMessage.textContent = '';
    }
    function formatDate(iso) {
        const d = new Date(iso);
        return d.toLocaleString('en-ZA', { year:'numeric', month:'short', day:'2-digit' });
    }
    function escapeHtml(s) {
        return String(s ?? '')
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    loadUsers();
});