document.addEventListener('DOMContentLoaded', () => {
    const stepOneForm = document.getElementById('stepOneForm');
    const stepTwoForm = document.getElementById('stepTwoForm');
    const pageTitle = document.getElementById('pageTitle');
    const pageSubtitle = document.getElementById('pageSubtitle');
    const emailEl = document.getElementById('email');
    const otpEl = document.getElementById('otp');
    const newPasswordEl = document.getElementById('newPassword');
    const confirmEl = document.getElementById('confirmPassword');
    const stepOneMsg = document.getElementById('stepOneMessage');
    const stepTwoMsg = document.getElementById('stepTwoMessage');
    const backBtn = document.getElementById('backToStepOne');

    let currentEmail = '';

    // Step 1
    stepOneForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideStepOneMessage();

        const email = emailEl.value.trim();
        if (!email) return showStepOneError('Please enter your email address.');

        const btn = document.getElementById('sendOtpBtn');
        btn.disabled = true;
        btn.textContent = 'Sending…';

        try {
            const res = await ApiClient.post('/api/password/forgot', { email });

            if (res.ok && res.data?.success) {
                currentEmail = email;
                // Move to step 2
                stepOneForm.style.display = 'none';
                stepTwoForm.style.display = 'block';
                pageTitle.textContent = 'Reset your password';
                pageSubtitle.textContent = `We sent a 6-digit code to ${email}. It expires in 10 minutes.`;
                otpEl.focus();
            } else {
                showStepOneError(res.data?.message || 'Could not send the OTP.');
            }
        } catch (err) {
            console.error(err);
            showStepOneError('Network error. Please try again.');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Send OTP';
        }
    });

    // Step 2
    stepTwoForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        hideStepTwoMessage();

        const otp = otpEl.value.trim();
        const pwd = newPasswordEl.value;
        const confirm = confirmEl.value;

        if (!/^\d{6}$/.test(otp)) return showStepTwoError('Please enter the 6-digit code.');
        if (pwd.length < 6) return showStepTwoError('Password must be at least 6 characters.');
        if (pwd !== confirm) return showStepTwoError('Passwords do not match.');

        const btn = document.getElementById('resetBtn');
        btn.disabled = true;
        btn.textContent = 'Resetting…';

        try {
            const res = await ApiClient.post('/api/password/reset', {
                email: currentEmail,
                otp,
                newPassword: pwd,
                confirmPassword: confirm
            });

            if (res.ok && res.data?.success) {
                showStepTwoSuccess('Password reset successful! Redirecting to sign in…');
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 1500);
            } else {
                showStepTwoError(res.data?.message || 'Could not reset password.');
            }
        } catch (err) {
            console.error(err);
            showStepTwoError('Network error.');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Reset password';
        }
    });

    // Back
    backBtn.addEventListener('click', () => {
        stepTwoForm.style.display = 'none';
        stepOneForm.style.display = 'block';
        pageTitle.textContent = 'Forgot your password?';
        pageSubtitle.textContent = 'Enter your registered email and we\'ll send you a one-time code.';
        otpEl.value = '';
        newPasswordEl.value = '';
        confirmEl.value = '';
        hideStepTwoMessage();
    });

    // Helpers
    function showStepOneError(m) {
        stepOneMsg.className = 'form-message is-error';
        stepOneMsg.textContent = m;
    }
    function hideStepOneMessage() {
        stepOneMsg.className = 'form-message';
        stepOneMsg.textContent = '';
    }
    function showStepTwoError(m) {
        stepTwoMsg.className = 'form-message is-error';
        stepTwoMsg.textContent = m;
    }
    function showStepTwoSuccess(m) {
        stepTwoMsg.className = 'form-message is-success';
        stepTwoMsg.textContent = m;
    }
    function hideStepTwoMessage() {
        stepTwoMsg.className = 'form-message';
        stepTwoMsg.textContent = '';
    }
});