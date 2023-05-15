'use strict';

window.PPG = window.PPG || {};

PPG.ChangePassword = function ($) {

    function getFormInstance() {
        return $('#changePasswordForm').dxForm('instance');
    }

    const isFormValid = function () {
        var form = getFormInstance();
        return form.validate().isValid;
    };

    const init = function () { };

    return {
        init: init,
        isFormValid: isFormValid
    };

}
PPG.changePassword = new PPG.ChangePassword(jQuery);