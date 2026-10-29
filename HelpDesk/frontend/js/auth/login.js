document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('loginForm');
    const messageEl = document.getElementById('loginMessage');
    const submitBtn = document.getElementById('submitBtn');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideMessage();

        const username = document.getElementById('username').value.trim();
        const password = document.getElementById('password').value;

        if (!username || !password) {
            return showError('Please enter your username and password.');
        }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Signing in…';

        try {
            const res = await ApiClient.post('/api/auth/login', { username, password });

            if (res.ok && res.data && res.data.success) {
                // Persist user in session storage
                Session.save(res.data.data);

                showSuccess('Signing you in…');

                // Redirect by role
                setTimeout(() => {
                    Session.redirectToDashboard(res.data.data.role);
                }, 500);
            } else {
                const msg = (res.data && res.data.message) || 'Invalid username or password.';
                showError(msg);
            }
        } catch (err) {
            console.error(err);
            showError('Network error. Please try again.');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Sign in';
        }
    });

    // helpers
    function showError(msg) {
        messageEl.className = 'form-message is-error';
        messageEl.textContent = msg;
    }
    function showSuccess(msg) {
        messageEl.className = 'form-message is-success';
        messageEl.textContent = msg;
    }
    function hideMessage() {
        messageEl.className = 'form-message';
        messageEl.textContent = '';
    }
});