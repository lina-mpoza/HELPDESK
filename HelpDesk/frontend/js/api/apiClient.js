window.ApiClient = (function () {
    const BASE = '';

    async function request(method, url, body) {
        const options = {
            method,
            headers: { 'Content-Type': 'application/json' }
        };
        if (body !== undefined) options.body = JSON.stringify(body);

        const res = await fetch(BASE + url, options);

        let data = null;
        try { data = await res.json(); } catch { }

        return { ok: res.ok, status: res.status, data };
    }

    return {
        post: (url, body) => request('POST', url, body),
        get: (url) => request('GET', url),
        put: (url, body) => request('PUT', url, body)
    };
})();