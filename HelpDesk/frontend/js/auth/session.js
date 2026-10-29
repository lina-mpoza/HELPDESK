/** @type {any} */
window.Session = (function () {
    const KEY = 'helpdesk.user';

    function save(user) {
        sessionStorage.setItem(KEY, JSON.stringify(user));
    }

    function get() {
        const raw = sessionStorage.getItem(KEY);
        if (!raw) return null;

        try {
            return JSON.parse(raw);
        } catch {
            return null;
        }
    }

    function clear() {
        sessionStorage.removeItem(KEY);
    }

    function isLoggedIn() {
        return get() !== null;
    }

    function requireRole(...allowedRoles) {
        const user = get();

        if (!user) {
            window.location.href = '/pages/login.html';
            return null;
        }

        if (allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
            redirectToDashboard(user.role);
            return null;
        }

        return user;
    }

    function redirectToDashboard(role) {
        switch (role) {
            case 'Admin':
                window.location.href = '/pages/admin/dashboard.html';
                break;

            case 'Technician':
                window.location.href = '/pages/technician/dashboard.html';
                break;

            default:
                window.location.href = '/pages/staff/dashboard.html';
                break;
        }
    }

    return {
        save,
        get,
        clear,
        isLoggedIn,
        requireRole,
        redirectToDashboard
    };
})();
// Session helpers — shared by login, logout, and every dashboard.
window.Session = (function () {
    const KEY = 'helpdesk.user';

    function save(user) {
        sessionStorage.setItem(KEY, JSON.stringify(user));
    }

    function get() {
        const raw = sessionStorage.getItem(KEY);
        if (!raw) return null;
        try { return JSON.parse(raw); } catch { return null; }
    }

    function clear() {
        sessionStorage.removeItem(KEY);
    }

    function isLoggedIn() {
        return get() !== null;
    }

    function requireRole(...allowedRoles) {
        const user = get();
        if (!user) {
            window.location.href = '/pages/login.html';
            return null;
        }
        if (allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
            // Wrong role — send them to their own dashboard
            redirectToDashboard(user.role);
            return null;
        }
        return user;
    }

    function redirectToDashboard(role) {
        switch (role) {
            case 'Admin': window.location.href = '/pages/admin/dashboard.html'; break;
            case 'Technician': window.location.href = '/pages/technician/dashboard.html'; break;
            default: window.location.href = '/pages/staff/dashboard.html'; break;
        }
    }

    return { save, get, clear, isLoggedIn, requireRole, redirectToDashboard };
})();