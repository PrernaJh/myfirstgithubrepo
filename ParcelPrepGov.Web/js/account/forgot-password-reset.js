'use strict';

window.PPG = window.PPG || {};

PPG.ForgotPasswordReset = function ($) {

    function getFormInstance() {
        return $('#forgotPasswordResetForm').dxForm('instance');
    }

    const onClick = function (e) {
        const form = getFormInstance();
        const isValid = form.validate().isValid;
        if(isValid)
            e.component.option('disabled', true);

        return isValid;

    };

    const onHidden = function () {
        window.location.href = '/account/signin'
    };

    const init = function () { };

    return {
        init: init,
        onClick: onClick,
        onHidden: onHidden
    };

}
PPG.forgotPasswordReset = new PPG.ForgotPasswordReset(jQuery);