'use strict';

window.PPG = window.PPG || {};

PPG.ServiceOverride = function ($) {

    let _ajaxingGrid, _ajaxingForm, _popup, _grid, _serviceOverrideButton, _serviceOverrideCard = null;
    let _oldShippingCarrier, _newShippingCarrier, _oldShippingMethod, _newShippingMethod = null;
    let _customerName, _startDate, _endDate = null;

    const init = function () {
        _serviceOverrideCard = $('#serviceOverrideCard');
        _serviceOverrideCard.hide();
        moveGridFilter();
        _ajaxingGrid = new PPG.Ajaxing('serviceOverrideCard');
        _ajaxingForm = new PPG.Ajaxing('form');
        _popup = $('#serviceOverride-popup').dxPopup('instance');
        _grid = $('#serviceOverridesGrid').dxDataGrid('instance');        
    };

    const startDate_onValueChanged = function (e) {
        _startDate = e.value;
        manageButtonState();
    }

    const endDate_onValueChanged = function (e) {
        _endDate = e.value;
        manageButtonState();
    }

    const oldShippingCarrier_onValueChanged = function (e) {
        _oldShippingCarrier = e.value;
        manageButtonState();
    };

    const newShippingCarrier_onValueChanged = function (e) {
        _newShippingCarrier = e.value;
        manageButtonState();
    };

    const oldShippingMethod_onValueChanged = function (e) {
        _oldShippingMethod = e.value;
        manageButtonState();
    };

    const newShippingMethod_onValueChanged = function (e) {
        _newShippingMethod = e.value;
        manageButtonState();
    };

    const addNewButton_onClick = function (e) {
        _popup.show();
    };

    const popup_onShowing = function (e) {
        _serviceOverrideButton = $('#serviceOverrideButton').dxButton('instance');
    }

    const popup_onHiding = function (e) {
        _grid.refresh();
    };

    const grid_onLoading = function (e) {
        if (!_customerName) return;
        _ajaxingGrid.start();
    };

    const grid_onLoaded = function (e) {
        _ajaxingGrid.stop();
    };

    const serviceOverrideButton_onClick = function (e) {
        var _checkStartDate = new Date(_startDate).toLocaleDateString("en-US");
        var _checkEndDate = new Date(_endDate).toLocaleDateString("en-US");

        if (_checkStartDate == _checkEndDate) {
            PPG.toastr.error('Start Date and End Date cannot be on the same date');
        } else {
            var dxFormInstance = $("#ServiceOverridePostForm").dxForm("instance");
            var validationResult = dxFormInstance.validate();

            if (!validationResult.isValid) {
                let brokenRules = validationResult.brokenRules;
                let messages = [];
                brokenRules.forEach(brokenRule => messages.push(brokenRule.message));
                PPG.toastr.warning(messages.toString());
                return;
            }

            let f = $('#ServiceOverridePostHtmlForm')[0];

            let formData = new FormData(f);
            formData.append("CustomerName", _customerName);
            //for (let pair of formData.entries())
            //    console.log(pair[0] + ', ' + pair[1]);

            _ajaxingForm.start();

            let ajax = PPG.ajaxer.postFormData('/ServiceOverride/Post', formData)

            ajax.done(function (response) {
                if (response.success) {
                    dxFormInstance.resetValues();
                    _popup.hide();
                    _grid.refresh();
                } else {
                    PPG.toastr.error(response.message);
                }
            });

            ajax.fail(function (xhr, status) {
                PPG.toastr.error('Override failed.');
            });

            ajax.always(function () {
                _ajaxingForm.stop();
            });
        }
    };

    const onCustomerValueChanged = function (e) {
        _customerName = e.value;


        _grid.option('visible', true);
        _serviceOverrideCard.show();
        _grid.refresh();
    };

    const getSelectedCustomer = function (e) {
        return _customerName;
    }

    const getSelectedOldShippingCarrier = function (e) {
        return _oldShippingCarrier;
    }

    const getSelectedNewShippingCarrier = function (e) {
        return _newShippingCarrier;
    }

    function manageButtonState() {
        if (!_serviceOverrideButton) return;

        let disable = !_startDate || !_endDate || !_oldShippingCarrier || !_newShippingCarrier || !_oldShippingMethod || !_newShippingMethod;

        _serviceOverrideButton.option('disabled', disable);
    }

    function moveGridFilter() {
        const $el = $('#serviceOverridesGrid > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div > div > div');
        $el.appendTo($('#gridFilterContainer'));
        $('.dx-datagrid-header-panel').remove();
    }

    const clearFilters = function () {
        $("#serviceOverridesGrid").dxDataGrid("instance").clearFilter();
    }

    const reloadOldShippingMethods = function () {
        $("#oldShippingMethod").dxSelectBox({
            value: null
        })
        $("#oldShippingMethod").dxSelectBox("getDataSource").reload();
    }

    const reloadNewShippingMethods = function () {
        $("#newShippingMethod").dxSelectBox({
            value: null
        })
        $("#newShippingMethod").dxSelectBox("getDataSource").reload();
    }

    return {
        init: init,
        startDate_onValueChanged: startDate_onValueChanged,
        endDate_onValueChanged: endDate_onValueChanged,
        oldShippingCarrier_onValueChanged: oldShippingCarrier_onValueChanged,
        newShippingCarrier_onValueChanged: newShippingCarrier_onValueChanged,
        oldShippingMethod_onValueChanged: oldShippingMethod_onValueChanged,
        newShippingMethod_onValueChanged: newShippingMethod_onValueChanged,
        addNewButton_onClick: addNewButton_onClick,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        serviceOverrideButton_onClick: serviceOverrideButton_onClick,
        onCustomerValueChanged: onCustomerValueChanged,
        getSelectedCustomer: getSelectedCustomer,
        clearFilters: clearFilters,
        getSelectedOldShippingCarrier: getSelectedOldShippingCarrier,
        getSelectedNewShippingCarrier: getSelectedNewShippingCarrier,
        reloadOldShippingMethods: reloadOldShippingMethods,
        reloadNewShippingMethods: reloadNewShippingMethods
    };

}

PPG.serviceOverride = new PPG.ServiceOverride(jQuery);