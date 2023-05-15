'use strict';

window.PPG = window.PPG || {};

PPG.ManageCostsAndCharges = function ($) {

    let _ajaxingGrid, _ajaxingForm, _popup, _submitButton, _exportButton, _grid, _manageCostsAndChargesCard = null;
    let _startDate = null, _uploadStartDate = null, _locationLocalDate = null;
    let _files, _selectedActiveGroupId, _selectedStartDate, _downloadFileName, _customerName = null;

    const init = function () {
        customizeGridToolbar();
        _manageCostsAndChargesCard = $('#manageCostsAndChargesCard');
        _manageCostsAndChargesCard.hide();
        _ajaxingGrid = new PPG.Ajaxing('manageCostsAndChargesCard');
        _ajaxingForm = new PPG.Ajaxing('form');
        _popup = $('#uploadRates-popup').dxPopup('instance');
        _exportButton = $('#exportButton').dxButton('instance');
        _grid = $('#currentRatesGrid').dxDataGrid('instance');
    };

    const grid_onLoading = function (e) {
        if (!_customerName) return;
    };

    const grid_onLoaded = function (e) {
    };

    const uploadButton_onClick = function (e) {
        unselectGridRow();
        _popup.show();
    };

    const popup_onShowing = function (e) {
        _submitButton = $('#submitButton').dxButton('instance');
    };

    const popup_onHiding = function (e) {

    };

    const startDate_onValueChanged = function (e) {
        _startDate = e.value;
        manageButtonState();
    };

    const setLocationLocalDate = function (e) {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetLocationLocalDate?subClientName=' + _customerName);
        ajax.done(function (response) {
            _locationLocalDate = new Date(response);
            if (_uploadStartDate !== null) {
                startDate_onInitialized(_uploadStartDate);
            }
        });
    };

    const startDate_onInitialized = function (e) {
        _uploadStartDate = e;
        PPG.utility.setStartDate(e, _locationLocalDate);
        _startDate = _locationLocalDate;
    }


    const fileUploader_onValueChanged = function (e) {
        _files = e.value;
        manageButtonState();
    };

    const submitButton_onClick = function (e) {

        _ajaxingForm.start();

        const f = $('form');

        let formData = new FormData(f[0]);
        formData.append('customerName', getSelectedCustomer());

        let ajax = PPG.ajaxer.postFormData('/servicemanagement/UploadPackageRates', formData)

        ajax.done(function (response) {
            if (response.success) {
                PPG.toastr.success(response.message);
                _popup.hide();
                _grid.refresh();
            } else {
                PPG.toastr.error(response.message);
            }
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });

        ajax.always(function () {
            _ajaxingForm.stop();
        });

    };

    const grid_onSelectionChanged = function (e) {
        _selectedActiveGroupId = e.selectedRowKeys[0];
        if (!_selectedActiveGroupId) return;
        const sd = new Date(e.selectedRowsData[0].StartDate);
        const year = sd.getFullYear();
        let day = sd.getDate();
        let month = sd.getMonth() + 1;
        if (month.toString().length < 2)
            month = `0${month}`;
        if (day.toString().length < 2)
            day = `0${day}`;
        _selectedStartDate = `${month}/${day}/${year}`;
        _downloadFileName = `rates_download_${_customerName}_${month}${day}${year}`;
        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${_selectedStartDate}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    const exportButton_onClick = function (e) {
        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetRatesByActiveGroupId', { id: _selectedActiveGroupId });
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

    const onCustomerValueChanged = function (e) {
        _customerName = e.value;
        setLocationLocalDate();
        _grid.option('visible', true);
        _manageCostsAndChargesCard.fadeIn();
        _grid.refresh();
    };

    const cancelButton_onClick = function () {
        _popup.hide();
        location.reload();
    };

    const getSelectedCustomer = function (e) {
        return _customerName;
    };

    function manageButtonState() {
        if (!_submitButton) return;
        if (!_startDate) return;
        if (!_files || _files.length == 0) return;

        _submitButton.option('disabled', false);
    }

    function customizeGridToolbar() {
        //const $filter = $('#currentRatesGrid > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div.dx-item.dx-toolbar-item.dx-toolbar-button');
        //$filter.appendTo($('#gridFilterContainer'));
        $('#currentRatesGrid .dx-datagrid-header-panel').remove();
    }

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }

    return {
        init: init,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        startDate_onValueChanged: startDate_onValueChanged,
        startDate_onInitialized: startDate_onInitialized,
        fileUploader_onValueChanged: fileUploader_onValueChanged,
        uploadButton_onClick: uploadButton_onClick,
        submitButton_onClick: submitButton_onClick,
        grid_onSelectionChanged: grid_onSelectionChanged,
        exportButton_onClick: exportButton_onClick,
        exportGrid_onExporting: exportGrid_onExporting,
        onCustomerValueChanged: onCustomerValueChanged,
        getSelectedCustomer: getSelectedCustomer,
        cancelButton_onClick: cancelButton_onClick
    };

}

PPG.manageCostsAndCharges = new PPG.ManageCostsAndCharges(jQuery);