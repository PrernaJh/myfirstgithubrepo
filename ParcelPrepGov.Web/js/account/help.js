'use strict';

window.PPG = window.PPG || {};

PPG.Help = function () {

    const onHidden = function () {
        window.location.href = '/account/signin'
    };

    return {
        onHidden: onHidden
    };

}
PPG.help = new PPG.Help();