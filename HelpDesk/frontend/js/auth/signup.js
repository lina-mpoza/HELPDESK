document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('signupForm');
    const messageEl = document.getElementById('signupMessage');
    const submitBtn = document.getElementById('submitBtn');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage();

        const payload = {
            firstName: document.getElementById('firstName').value.trim(),
            lastName: document.getElementById('lastName').value.trim(),
            workEmail: document.getElementById('workEmail').value.trim(),
            cellNo: document.getElementById('cellNo').value.trim(),
            staffNo: document.getElementById('staffNo').value.trim(),
            username: document.getElementById('username').value.trim(),
            password: document.getElementById('password').value
        };
        const confirm = document.getElementById('confirmPassword').value;

        // Client validation
        if (!payload.firstName || !payload.lastName || !payload.username || !payload.password)
            return showError('Please fill in all required fields.');

        if (payload.password.length < 6)
            return showError('Password must be at least 6 characters.');

        if (payload.password !== confirm)
            return showError('Passwords do not match.');

        // Submit to API
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating account…';

        try {
            const res = await ApiClient.post('/api/auth/register', payload);

            if (res.ok && res.data && res.data.success) {
                showSuccess('Account created successfully. Redirecting to sign in…');
                setTimeout(() => { window.location.href = 'login.html'; }, 1400);
            } else {
                showError((res.data && res.data.message) || 'Could not create account.');
            }
        } catch (err) {
            console.error(err);
            showError('Network error. Please try again.');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Create account';
        }
    });

    // helpers
    function scrollMessageIntoView(el) {
        if (!el) return;
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    function showError(msg) {
        messageEl.className = 'form-message is-error';
        messageEl.textContent = msg;
        scrollMessageIntoView(messageEl);
    }
    function showSuccess(msg) {
        messageEl.className = 'form-message is-success';
        messageEl.textContent = msg;
        scrollMessageIntoView(messageEl);
    }
    function hideMessage() {
        messageEl.className = 'form-message';
        messageEl.textContent = '';
    }
});