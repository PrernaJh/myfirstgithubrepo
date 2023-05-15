'use strict';

window.PPG = window.PPG || {};

PPG.SignIn = function ($) {

    const init = function () { };

    const signinButton_onClick = function (e) {
        e.component.option('disabled', true);
        $('#signin-form').dxForm('instance').option('readOnly', true); 

        // show progress
        var loadPanel = $(".loadPanel").dxLoadPanel({ 
            position: { of: "#signin-form" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading:true
        }).dxLoadPanel("instance");

        loadPanel.show();

    }

    const signinWithFedexButton_onClick = function (e) {
        PPG.externalLogin.initiateExternalLogin($('form'));
    }

    const helpButton_onClick = function () {
        window.location.href = '/account/help';
    }

    return {
        init: init,
        signinButton_onClick: signinButton_onClick,
        signinWithFedexButton_onClick,
        helpButton_onClick: helpButton_onClick
    };

}

PPG.signIn = new PPG.SignIn(jQuery);