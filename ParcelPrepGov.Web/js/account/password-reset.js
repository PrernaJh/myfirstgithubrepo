'use strict';

window.PPG = window.PPG || {};

PPG.PasswordReset = function ($) {

    function getFormInstance() {
        return $('#passwordResetForm').dxForm('instance');
    }

    const submitButton_onClick = function () {
        var form = getFormInstance();
        return form.validate().isValid;
    };

    const init = function () { };

    const onHidden = function () {
        window.location.href = '/account/signin'
    };

    return {
        init: init,
        submitButton_onClick: submitButton_onClick,
        onHidden: onHidden
    };

}

PPG.passwordReset = new PPG.PasswordReset(jQuery);