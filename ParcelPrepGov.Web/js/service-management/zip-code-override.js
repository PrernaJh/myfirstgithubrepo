'use strict';

window.PPG = window.PPG || {};

PPG.zipCodeOverride = function ($) {

    let _exportButton, _ajaxingGrid, _grid, _popup, _ajaxing, _ZipCodeOverrideCard = null;
    let _selectedActiveGroupId, _downloadFileName;
    let _startDate = null, _uploadStartDate = null, _locationLocalDate = null;
    let _ZipCodeOverrideSubClient = null;

    let _fiveDigitFile = [];
    let _importZipCodeOverrideButton = null;

    const init = function () {
        _ZipCodeOverrideCard = $('#ZipCodeOverrideCard');
        _ZipCodeOverrideCard.hide();
        _exportButton = $('#exportButton').dxButton('instance');
        _ajaxingGrid = new PPG.Ajaxing('ZipCodeOverrideCard');
        _grid = $('#ZipCodeOverrideActiveGroupsGrid').dxDataGrid('instance');
        _popup = $('#uploadBinMaps-popup').dxPopup('instance');
        _ajaxing = new PPG.Ajaxing();
    };

    const grid_onFocusedRowChanged = function (e) {
        if (e.rowIndex == -1) return;
        console.log("grid_onFocusedRowChanged", e);
        _selectedActiveGroupId = e.row.data.Id;
        if (!_selectedActiveGroupId) return;

        const sd = new Date(e.row.data.StartDate);
        const year = sd.getFullYear();
        let day = sd.getDate();
        let month = sd.getMonth() + 1;
        if (month.toString().length < 2)
            month = `0${month}`;
        if (day.toString().length < 2)
            day = `0${day}`;
        let selectedStartDate = `${month}/${day}/${year}`;

        _downloadFileName = `${_ZipCodeOverrideSubClient}_zip_code_overrides_${month}${day}${year}`;

        if (e.row.data.Filename) {
            _downloadFileName = e.row.data.Filename;
            _downloadFileName = _downloadFileName.split('.').slice(0, -1).join('.');
        }

        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${selectedStartDate}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    const exportButton_onClick = function (e) {

        var selectedActiveGroupId = _grid.option("focusedRowKey");

        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetZipCodeOverrideByActiveGroupId', { id: selectedActiveGroupId });
        ajax.done(function (response) {
            debugger;
            $('#exportGrid').dxDataGrid('instance').option('dataSource', response);
        });
        ajax.fail(function (xhr, status) {
            debugger;
        });
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

    const uploadButton_onClick = function (e) {
        _popup.show();
        unselectGridRow();
    };

    const popup_onShowing = function (e) {
        //$("#SelectedSubClient").val(_ZipCodeOverrideSubClient);        
    };

    const popup_onHiding = function (e) { };

    const getSelectedZipCodeOverrideSubClient = function (e) {
        return _ZipCodeOverrideSubClient;
    }

    const ZipCodeOverrideSubClientSelectBox_onValueChanged = function (e) {
        _ZipCodeOverrideSubClient = e.value;
        setLocationLocalDate();
        _ZipCodeOverrideCard.fadeIn();
        _grid.option('visible', true);
        _grid.refresh();
        unselectGridRow();
    }

    const fiveDigitZipFileUploader_onValueChanged = function (e) {
        _fiveDigitFile = e.value;
        console.log("_fiveDigitFile", _fiveDigitFile);
        //enableImportZipCodeOverridesButton();
    }

    const importZipCodeOverrideButton_onClick = function (e) {

        var dxFormInstance = $("#ManageZipCodeOverrideForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = $('#formManageZipCodeOverride');

        var formData = new FormData(f[0]);

        formData.append('SelectedSubClient', getSelectedZipCodeOverrideSubClient());

        for (var pair of formData.entries())
            console.log(pair[0] + ', ' + pair[1]);


        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZipCodeOverride', formData);


        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                _grid.refresh();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });
        ajax.always(function () {
            _ajaxing.stop();
        });

    }

    const popup_OnContentReady = function (e) {
        _importZipCodeOverrideButton = $('#importZipCodeOverrideButton').dxButton('instance');
    };

    const startDate_onValueChanged = function (e) {
        _startDate = e.value;
        console.log("_startDate", _startDate);
        //enableImportZipCodeOverridesButton();
    }

    const setLocationLocalDate = function (e) {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetLocationLocalDate?subClientName=' + _ZipCodeOverrideSubClient);
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
    }


    const cancelButton_onClick = function () {
        var dxFormInstance = $("#ManageZipCodeOverrideForm").dxForm("instance");
        dxFormInstance.resetValues();
        _popup.hide();
    }

    function enableImportZipCodeOverridesButton() {
        if (!_startDate || (_fiveDigitFile.length == 0) || !_ZipCodeOverrideSubClient)
            _importZipCodeOverrideButton.option('disabled', true);
        else
            _importZipCodeOverrideButton.option('disabled', false);
    }

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }

    return {
        init: init,
        ZipCodeOverrideSubClientSelectBox_onValueChanged: ZipCodeOverrideSubClientSelectBox_onValueChanged,
        getSelectedZipCodeOverrideSubClient: getSelectedZipCodeOverrideSubClient,
        grid_onFocusedRowChanged: grid_onFocusedRowChanged,
        exportButton_onClick: exportButton_onClick,
        exportGrid_onExporting: exportGrid_onExporting,
        uploadButton_onClick: uploadButton_onClick,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        fiveDigitZipFileUploader_onValueChanged: fiveDigitZipFileUploader_onValueChanged,
        importZipCodeOverrideButton_onClick: importZipCodeOverrideButton_onClick,
        popup_OnContentReady: popup_OnContentReady,
        startDate_onValueChanged: startDate_onValueChanged,
        startDate_onInitialized: startDate_onInitialized,
        cancelButton_onClick: cancelButton_onClick
    };

}

PPG.zipCodeOverride = new PPG.zipCodeOverride(jQuery);