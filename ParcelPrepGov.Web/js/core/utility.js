'use strict';

window.PPG = window.PPG || {};

PPG.utility = (function () {

    const setStartDate = function (e, siteLocalDate) {
        var tomorrow = new Date(siteLocalDate);
        tomorrow.setDate(tomorrow.getDate() + 1);
        e.component.option('min', tomorrow);
        e.component.option('value', tomorrow);
    }

    return {
        setStartDate: setStartDate
    };

}());