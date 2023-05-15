'use strict';

window.PPG = window.PPG || {};

document.addEventListener('DOMContentLoaded', function documentReady() {
    this.removeEventListener('DOMContentLoaded', documentReady);
    const options = {
        displayTime: 10000,
        closeOnClick: true,
        closeOnOutsideClick: true,
        position: { my: 'center', at: 'center' },
        shading: true,
        onHidden: function (e) { window.location.href = '/account/signin'; }
    };
    PPG.toastr.toast("Account Deactivated", options);
});