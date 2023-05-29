'use strict'

const navs = document.querySelectorAll('.nav-link');
const basePath = window.location.pathname.split('/');
const currentUrl = window.location.origin + `/${basePath[1]}`;

const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

navs.forEach(item => {
    if (item.href === currentUrl) {
        item.classList.add('active');
    }
});
