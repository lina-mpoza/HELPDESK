
(function () {
    'use strict';

    // Footer year
    function setYear() {
        const el = document.getElementById('year');
        if (el) el.textContent = new Date().getFullYear();
    }

    // Boot
    document.addEventListener('DOMContentLoaded', () => {
        setYear();
    });

    console.log('[Helpdesk] main.js loaded');
})();