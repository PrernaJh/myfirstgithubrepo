'use strict';

window.PPG = window.PPG || {};

PPG.externalLogin = (function () {
    function initiateExternalLogin(jqueryLoginForm) {
        jqueryLoginForm.attr('action', '/External/SignIn')
        jqueryLoginForm.submit();
    }

    return {
        initiateExternalLogin
    }
})();
