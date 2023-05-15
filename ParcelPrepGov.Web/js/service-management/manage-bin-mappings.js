'use strict';

window.PPG = window.PPG || {};

PPG.manageBinMappings = function ($) {

    let _exportButton, _ajaxingGrid, _grid, _popup, _ajaxing, _binMappingsCard = null;
    let _selectedActiveGroupId, _downloadFileName;
    let _startDate = null, _uploadStartDate = null, _locationLocalDate = null;
    let _binMappingsClient, _binMappingsSubClient = null;

    let _fiveDigitFile = [];
    let _threeDigitFile = [];
    let _importBinMappingsButton = null;

    const init = function () {
        _binMappingsCard = $('#binMappingsCard');
        _binMappingsCard.hide();
        _exportButton = $('#exportButton').dxButton('instance');
        _ajaxingGrid = new PPG.Ajaxing('binMappingsCard');
        _grid = $('#binMappingsActiveGroupsGrid').dxDataGrid('instance');
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

        _downloadFileName = `${_binMappingsSubClient}_bin_mappings_${month}${day}${year}`;

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
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetBinMapsByActiveGroupId', { id: selectedActiveGroupId });
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

    const uploadButton_onClick = function (e) {
        _popup.show();
        unselectGridRow();
    };

    const popup_onShowing = function (e) {
        //$("#SelectedSubClient").val(_binMappingsSubClient);        
    };

    const popup_onHiding = function (e) { };

    const getSelectedBinMappingsSubClient = function (e) {
        return _binMappingsSubClient;
    }

    //const binMappingsClientSelectBox_onValueChanged = function (e) {
    //    _binMappingsClient = e.value;

    //    let subClientSelectBox = $("#binMappingsSubClientSelectBox").dxSelectBox("instance");

    //    let dataSource = subClientSelectBox.getDataSource();
    //    dataSource.filter(["ClientName", "=", _binMappingsClient]);
    //    dataSource.load();
    //    subClientSelectBox.option("value", null);
    //}

    const binMappingsSubClientSelectBox_onValueChanged = function (e) {
        _binMappingsSubClient = e.value;
        setLocationLocalDate();
        _binMappingsCard.fadeIn();
        _grid.option('visible', true);
        _grid.refresh();
        unselectGridRow();
    }

    const setLocationLocalDate = function (e) {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetLocationLocalDate?subClientName=' + _binMappingsSubClient);
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

    const fiveDigitZipFileUploader_onValueChanged = function (e) {
        _fiveDigitFile = e.value;
        console.log("_fiveDigitFile", _fiveDigitFile);
        //enableImportBinSchemasButton();
    }

    const threeDigitZipFileUploader_onValueChanged = function (e) {
        _threeDigitFile = e.value;
        console.log("_threeDigitFile", _threeDigitFile);
        //enableImportBinSchemasButton();
    }

    const importBinMappingsButton_onClick = function (e) {

        var dxFormInstance = $("#ManageBinMappingsForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = $('#formManageBinMaps');

        var formData = new FormData(f[0]);

        formData.append('SelectedSubClient', getSelectedBinMappingsSubClient());

        for (var pair of formData.entries())
            console.log(pair[0] + ', ' + pair[1]);
        

        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportBinMappings', formData);


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
        _importBinMappingsButton = $('#importBinMappingsButton').dxButton('instance');
    };

    const startDate_onValueChanged = function (e) {
        _startDate = e.value;
        console.log("_startDate", _startDate);
        //enableImportBinSchemasButton();
    }

    const cancelButton_onClick = function () {
        var dxFormInstance = $("#ManageBinMappingsForm").dxForm("instance");
        dxFormInstance.resetValues();
        _popup.hide();
    }

    function enableImportBinSchemasButton() {
        if (!_startDate || (_fiveDigitFile.length == 0) || (_threeDigitFile.length == 0) || !_binMappingsSubClient)
            _importBinMappingsButton.option('disabled', true);
        else
            _importBinMappingsButton.option('disabled', false);
    }

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }    

    return {
        init: init,
        binMappingsSubClientSelectBox_onValueChanged: binMappingsSubClientSelectBox_onValueChanged,        
        getSelectedBinMappingsSubClient: getSelectedBinMappingsSubClient,
        grid_onFocusedRowChanged: grid_onFocusedRowChanged,
        exportButton_onClick: exportButton_onClick,
        exportGrid_onExporting: exportGrid_onExporting,
        uploadButton_onClick: uploadButton_onClick,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        fiveDigitZipFileUploader_onValueChanged: fiveDigitZipFileUploader_onValueChanged,
        threeDigitZipFileUploader_onValueChanged: threeDigitZipFileUploader_onValueChanged,
        importBinMappingsButton_onClick: importBinMappingsButton_onClick,
        popup_OnContentReady: popup_OnContentReady,
        startDate_onValueChanged: startDate_onValueChanged,
        startDate_onInitialized: startDate_onInitialized,
        cancelButton_onClick: cancelButton_onClick
    };

}

PPG.manageBinMappings = new PPG.manageBinMappings(jQuery);