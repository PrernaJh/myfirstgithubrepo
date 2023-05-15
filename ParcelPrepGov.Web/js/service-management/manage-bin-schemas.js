'use strict';

window.PPG = window.PPG || {};

PPG.ManageBinSchemas = function ($) {

    let _exportButton, _ajaxingGrid, _grid, _popup, _ajaxing, _binSchemasCard = null;
    let _selectedSiteName, _selectedActiveGroupId, _downloadFileName;
    let _startDate = null, _uploadStartDate = null, _siteLocalDate = null;
    let _binUploadFile = null;
    let _importBinSchemasButton = null;

    const init = function () {
        _binSchemasCard = $('#binSchemasCard');
        _binSchemasCard.hide();
        _exportButton = $('#exportButton').dxButton('instance');
        _ajaxingGrid = new PPG.Ajaxing('binSchemasCard');
        _grid = $('#binSchemaActiveGroupsGrid').dxDataGrid('instance');
        _popup = $('#uploadBins-popup').dxPopup('instance');
        _ajaxing = new PPG.Ajaxing();
    };

    const onSiteNameValueChanged = function (e) {
        _selectedSiteName = e.value;
        setSiteLocalDate();
        _binSchemasCard.fadeIn();
        _grid.option('visible', true);
        _grid.refresh();
        unselectGridRow();
    };

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    };

    const getSelectedSite = function (e) {
        return _selectedSiteName;
    };

    const getSiteLocalDate = function (e) {
        return _siteLocalDate;
    };

    const setSiteLocalDate = function (e) {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetSiteLocalDate?siteName=' + _selectedSiteName);
        ajax.done(function (response) {
            _siteLocalDate = new Date(response);
            if (_uploadStartDate !== null) {
                startDate_onInitialized(_uploadStartDate);
            }
        });
    };    

    const grid_onFocusedRowChanged = function (e) {
        if (e.rowIndex == -1) return;

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

        //Used for testing gridDetail();

        _downloadFileName = `${_selectedSiteName}_bin_schemas_${month}${day}${year}`;

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

    const gridDetail = function (e) {
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetBinsByActiveGroupId', { id: _selectedActiveGroupId });
        ajax.done(function (response) {
            $('#exportGrid').dxDataGrid('instance').option('dataSource', response);
        });
    }

    const exportButton_onClick = function (e) {
        if (!_grid.option("focusedRowKey")) {
            return;
        }

        var selectedActiveGroupId = _grid.option("focusedRowKey");

        _ajaxingGrid.start();
        
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetBinsByActiveGroupId', { id: selectedActiveGroupId });
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

    const popup_onShowing = function (e) { };

    const popup_onHiding = function (e) { };

    const binSchemaFileUploader_onValueChanged = function (e) {
        _binUploadFile = e.value;
        //enableImportBinSchemasButton();
    }

    const importBinSchemasButton_onClick = function (e) {

        var dxFormInstance = $("#ManageBinSchemasForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = $('#formManageBinSchemas');

        var formData = new FormData(f[0]);

        formData.append('SelectedSite', getSelectedSite());

        for (var pair of formData.entries())
            console.log(pair[0] + ', ' + pair[1]);

        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportBinSchemas', formData);


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
        _importBinSchemasButton = $('#importBinSchemasButton').dxButton('instance');
    };

    const startDate_onValueChanged = function (e) {
        _startDate = e.value;
        //enableImportBinSchemasButton();
    }

    const startDate_onInitialized = function (e) {
        _uploadStartDate = e;
        PPG.utility.setStartDate(e, _siteLocalDate);
    }

    const cancelButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ManageBinSchemasForm").dxForm("instance");
        dxFormInstance.resetValues();
    }

    function enableImportBinSchemasButton() {
        if (!_startDate || !_binUploadFile || !_selectedSiteName)
            _importBinSchemasButton.option('disabled', true);
        else
            _importBinSchemasButton.option('disabled', false);
    }

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }    

    return {
        init: init,
        onSiteNameValueChanged: onSiteNameValueChanged,
        getSelectedSite: getSelectedSite,
        getSiteLocalDate: getSiteLocalDate,
        grid_onFocusedRowChanged: grid_onFocusedRowChanged,
        exportButton_onClick: exportButton_onClick,
        exportGrid_onExporting: exportGrid_onExporting,
        uploadButton_onClick: uploadButton_onClick,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        binSchemaFileUploader_onValueChanged: binSchemaFileUploader_onValueChanged,
        importBinSchemasButton_onClick: importBinSchemasButton_onClick,
        popup_OnContentReady: popup_OnContentReady,
        startDate_onValueChanged: startDate_onValueChanged,
        startDate_onInitialized: startDate_onInitialized,
        cancelButton_onClick: cancelButton_onClick
    };

}

PPG.manageBinSchemas = new PPG.ManageBinSchemas(jQuery);