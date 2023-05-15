'use strict';

window.PPG = window.PPG || {};

PPG.ManageBusinessRules = function ($) {

    let _extendedRulesGrid, _grid, _serviceOverrideCard, _popup, _exportButton, _ajaxingGrid = null;
    let _customerName, _selectedActiveGroupId, _downloadFileName = null;

    const init = function () {
        _serviceOverrideCard = $('#serviceOverrideCard');
        _serviceOverrideCard.hide();
        _popup = $('#uploadServiceRulesPopup').dxPopup('instance');
        _grid = $('#serviceRulesGrid').dxDataGrid('instance');
        _extendedRulesGrid = $('#extendedServiceRulesGrid').dxDataGrid('instance');
        _exportButton = $('#exportButton').dxButton('instance');
        _ajaxingGrid = new PPG.Ajaxing('serviceOverrideCard');
    };

    const uploadButton_onClick = function (e) {
        unselectGridRow();
        _popup.show();
    };

    const extendedUploadButton_onClick = function (e) {
        unselectExtendedGridRow();
        _popup.show();
    };

    const getSelectedCustomer = function (e) {
        return _customerName;
    };

    const grid_onLoading = function (e) {
        if (!_customerName) return;
    };

    const grid_onLoaded = function (e) {
    };

    const onExtendedCustomerValueChanged = function (e) {
        _customerName = e.value;
        _serviceOverrideCard.fadeIn();
        _extendedRulesGrid.option('visible', true);
        _extendedRulesGrid.refresh();
        unselectExtendedGridRow();
    };

    const onCustomerValueChanged = function (e) { 
        _customerName = e.value;
        _serviceOverrideCard.fadeIn();
        _grid.option('visible', true);
        _grid.refresh();
        unselectGridRow();
    };

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    };

    function unselectExtendedGridRow() {
        _extendedRulesGrid.clearSelection();
        _extendedRulesGrid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    };

    const popup_onShowing = function (e) {

    }

    const popup_onHiding = function (e) {
        _grid.refresh();
    };

    const extendedPopup_onHiding = function (e) {
        _extendedRulesGrid.refresh();
    };

    const exportButton_onClick = function (e) {

        var selectedActiveGroupId = _grid.option("focusedRowKey");
        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetBusinessRulesByActiveGroupId', { id: selectedActiveGroupId, name: _customerName });
        ajax.done(function (response) {
            $('#exportGrid').dxDataGrid('instance').option('dataSource', response);
        });
        ajax.fail(function (xhr, status) { });
        ajax.always(function () {
            setTimeout(function () {
                var $el = $('.dx-icon.dx-icon-export-excel-button');
                _ajaxingGrid.stop();
                $el.trigger('click');
            }, 250);
        });

    };

    const extendedExportButton_onClick = function (e) {

        var selectedActiveGroupId = _extendedRulesGrid.option("focusedRowKey");
        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetExtendedServiceRulesByActiveGroupId', { id: selectedActiveGroupId, name: _customerName });
        ajax.done(function (response) {
            $('#exportGrid').dxDataGrid('instance').option('dataSource', response);
        });
        ajax.fail(function (xhr, status) { });
        ajax.always(function () {
            setTimeout(function () {
                var $el = $('.dx-icon.dx-icon-export-excel-button');
                _ajaxingGrid.stop();
                $el.trigger('click');
            }, 250);
        });

    };

    const exportGrid_onExporting = function (e) {
        e.fileName = _downloadFileName;
    };

    const grid_onFocusedRowChanged = function (e) {
        if (e.rowIndex == -1) return;

        console.log("grid_onFocusedRowChanged", e);
        _selectedActiveGroupId = e.row.data.Id;


        const sd = new Date(e.row.data.StartDate);
        const year = sd.getFullYear();
        let day = sd.getDate();
        let month = sd.getMonth() + 1;
        if (month.toString().length < 2)
            month = `0${month}`;
        if (day.toString().length < 2)
            day = `0${day}`;
        let selectedStartDate = `${month}/${day}/${year}`;

        _downloadFileName = `${_customerName}_business_rules_${month}${day}${year}`;
        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${selectedStartDate}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    const extendedGrid_onFocusedRowChanged = function (e) {
        if (e.rowIndex == -1) return;

        console.log("extendedGrid_onFocusedRowChanged", e);
        _selectedActiveGroupId = e.row.data.Id;


        const sd = new Date(e.row.data.StartDate);
        const year = sd.getFullYear();
        let day = sd.getDate();
        let month = sd.getMonth() + 1;
        if (month.toString().length < 2)
            month = `0${month}`;
        if (day.toString().length < 2)
            day = `0${day}`;
        let selectedStartDate = `${month}/${day}/${year}`;

        _downloadFileName = `${_customerName}_extended_service_rules_${month}${day}${year}`;
        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${selectedStartDate}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }

    return {
        init: init,
        getSelectedCustomer: getSelectedCustomer,
        uploadButton_onClick: uploadButton_onClick,
        extendedUploadButton_onClick: extendedUploadButton_onClick,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        onCustomerValueChanged: onCustomerValueChanged,
        onExtendedCustomerValueChanged: onExtendedCustomerValueChanged,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        extendedPopup_onHiding: extendedPopup_onHiding,
        exportButton_onClick: exportButton_onClick,
        extendedExportButton_onClick: extendedExportButton_onClick,
        exportGrid_onExporting: exportGrid_onExporting,
        extendedGrid_onFocusedRowChanged: extendedGrid_onFocusedRowChanged,
        grid_onFocusedRowChanged: grid_onFocusedRowChanged
    };

}

PPG.manageBusinessRules = new PPG.ManageBusinessRules(jQuery);