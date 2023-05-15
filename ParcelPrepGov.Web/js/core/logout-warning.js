'use strict';

window.PPG = window.PPG || {};

PPG.logoutWarning = (function () {

    let _ajaxing, _popup = null;
    let _interval = 5 * 1000;
    let startRun = true;

    function init() { 
        _ajaxing = new PPG.Ajaxing('signin-popup-form');
        _popup = $('#signInPopup').dxPopup('instance');
        //checkExpirationCallback();
        
        window.setInterval(checkExpirationCallback, _interval);
        
    }

    function formatTime(milliseconds) {
        let seconds = Math.floor(milliseconds / 1000)
        const m = Math.floor((seconds) / 60);
        const s = Math.round(seconds % 60);
        if (m > 1) {
            return m + " minutes";
        } else if (m == 1) {
            return m + " minute " + s + " seconds";
        } else {
            return s + " seconds";
        }
    }

    function signInButton_onClick(e) {
        var dxFormInstance = $("#signin-popup-form").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = $('#signInPopupForm');

        var formData = new FormData(f[0]);

        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/Account/AjaxSignIn', formData);

        ajax.setRequestHeader('RequestVerificationToken', $('input:hidden[name="__RequestVerificationToken"]').val());

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            //window.location.href = "/";
            //PPG.toastr.error('Sign In failed.');
        });
        ajax.always(function () {
            _ajaxing.stop();
        });
    }

    function signInWithFedexButton_onClick(e) {
        PPG.externalLogin.initiateExternalLogin($('#signInPopupForm'));
    }

    function cancelButton_onClick(e) {
        _popup.hide();
    }

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) {
            return parts.pop().split(';').shift();
        }
    }

    function checkExpirationCallback() {
        let cookie = decodeURIComponent(getCookie("LoginExpiration")); 
        let expiration = new Date(cookie);
        let now = new Date();
        let difference_ms = expiration - now;
        let seconds = Math.floor(difference_ms / 1000);
        let minutes = Math.floor((seconds) / 60); 

        //console.log("Logout Warning", difference_ms, formatTime(difference_ms)); 

        if (minutes < 5) {
            PPG.toastr.warning('Login is set to expire within ' + formatTime(difference_ms) + '.');
            if (seconds < 10) {  
                startRun = false;
                window.location.href = "/Account/SignOut";
                //cookie = "expires=Thu, 01 Jan 1970 00:00:00 UTC;"; 
            } else {
                // keeps hitting this check every second
                if (_popup == null) {
                    return;
                } else {
                    _popup.show();
                }
            }
        } 
    }

    return {
        init: init,
        signInButton_onClick: signInButton_onClick,
        signInWithFedexButton_onClick,
        cancelButton_onClick: cancelButton_onClick
    };

}());


window.addEventListener('load', function documentLoad() {
    this.removeEventListener('load', documentLoad);
    PPG.logoutWarning.init();
});