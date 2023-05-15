'use strict';

window.PPG = window.PPG || {};

PPG.ForgotPassword = function ($) {

    function getFormInstance() {
        return $('#forgotPasswordForm').dxForm('instance');
    }

    const onClick = function (e) {
        const form = getFormInstance();
        const isValid = form.validate().isValid;
        if (isValid)
            e.component.option('disabled', true);

        return isValid;

    };

    const init = function () { };

    return {
        init: init,
        onClick: onClick
    };

}
PPG.forgotPassword = new PPG.ForgotPassword(jQuery);