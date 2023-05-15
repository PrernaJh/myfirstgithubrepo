'use strict';

window.PPG = window.PPG || {};

PPG.CustomerSelector = function ($) {

    let _customerName, _manageServiceRulesWrapper = null;

    const init = function () {
        _manageServiceRulesWrapper = $('#manageServiceRulesWrapper');
        _manageServiceRulesWrapper.hide();
    };

    const onCustomerValueChanged = function (e) {
        _customerName = e.value;
        _manageServiceRulesWrapper.fadeIn();
    };

    const getSelectedCustomer = function (e) {
        return _customerName;
    }

    return {
        init: init,
        onCustomerValueChanged: onCustomerValueChanged,
        getSelectedCustomer: getSelectedCustomer
    };

}

PPG.customerSelector = new PPG.CustomerSelector(jQuery);