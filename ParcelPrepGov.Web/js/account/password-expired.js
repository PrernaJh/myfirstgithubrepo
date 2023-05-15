"use strict";

window.PPG = window.PPG || {};

PPG.PasswordExpired = function ($) {

    function getFormInstance() {
        return $("#passwordExpiredForm").dxForm("instance");
    }

    const isFormValid = function () {
        var form = getFormInstance();
        return form.validate().isValid;
    };

    const init = function () {};

    return {
        init: init,
        isFormValid: isFormValid
    };

}

PPG.passwordExpired = new PPG.PasswordExpired(jQuery);