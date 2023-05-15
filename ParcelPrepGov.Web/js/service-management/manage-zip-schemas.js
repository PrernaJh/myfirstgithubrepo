'use strict';

window.PPG = window.PPG || {};

PPG.manageZipSchemas = function ($) {

    let _ajaxingGrid, _grid, _popup, _uploadButton, _exportButton = null;
    let _selectedActiveGroupType, _selectedActiveGroupId, _downloadFileName = null;
    let _fedExHawaiiFile, _upsNdaSat48File, _upsDasFile = null, _uspsRuralFile = null;
    let _fedExHawaiiDate, _upsNdaSat48Date, _upsDasDate = null, _uspsRuralDate = null, _siteLocalDate = null;

    const init = function () {
        customizeGridToolbar();
        _ajaxingGrid = new PPG.Ajaxing('manageZipSchemasCard');
        _grid = $('#zipsGrid').dxDataGrid('instance');
        _popup = $('#uploadZips-popup').dxPopup('instance');
        _uploadButton = $('#upload-button').dxButton('instance');
        _exportButton = $('#exportButton').dxButton('instance');
        setSiteLocalDate();
    };

    const selectBox_valueChanged = function (e) {
        _selectedActiveGroupType = e.value;
        _uploadButton.option('disabled', false);
        _grid.option('visible', true);
        unselectGridRow();
        _exportButton.option('disabled', true);
        $('#zipsGrid').dxDataGrid('getDataSource').reload();

        switch (_selectedActiveGroupType) {
            case 'ZipsFedExHawaii':
                _popup.option("contentTemplate", $('#uploadZips-form-template-ZipsFedExHawaii'));
                break;
            case 'ZipsUpsSat48':
                _popup.option("contentTemplate", $('#uploadZips-form-template-ZipsUpsSat48'));
                break;
            case 'ZipsUpsDas':
                _popup.option("contentTemplate", $('#uploadZips-form-template-ZipsUpsDas'));
                break;
           case 'ZipsUspsRural':
                _popup.option("contentTemplate", $('#uploadZips-form-template-ZipsUspsRural'));
                break;            default:
        }
    };

    const getSelectedActiveGroupType = function () {
        return _selectedActiveGroupType; 
    }

    const grid_onLoading = function (e) {
        if (!_selectedActiveGroupType) return;
    };

    const grid_onLoaded = function (e) {
    };

    const uploadButton_onClick = function (e) {
        unselectGridRow();
        _popup.show();
    };

    const exportButton_onClick = function (e) {
        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetZipsByActiveGroupId', { id: _selectedActiveGroupId });
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
        let selectedStartDate = `${month}/${day}/${year}`;
        let filename = e.selectedRowsData[0].Filename;
        if (filename == null)
            filename = "";
        filename = filename.replace(".xlsx", "");
        debugger;
        if (filename == "") {
            switch (_selectedActiveGroupType) {
                case 'ZipsFedExHawaii':
                    filename = 'FEDEX'
                    break;
                case 'ZipsUpsSat48':
                    filename = 'UPS_NDA'
                    break;
                case 'ZipsUpsDas':
                    filename = 'UPS_DAS'
                    break;
                case 'ZipsUspsRural':
                    filename = 'USPS_RURAL'
                    break;
                default:
            }
        }

        _downloadFileName = `${filename}_download_${month}${day}${year}`;
        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${selectedStartDate}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    const exportGrid_onExporting = function (e) {
        e.fileName = _downloadFileName;
    };

    const popup_onShowing = function (e) {};

    const popup_onHiding = function (e) {};

    const fedexHawaiiZipsFileUploader_onValueChanged = function (e) {
        _fedExHawaiiFile = e.value;
    };

    const upsNdaSat48FileUploader_onValueChanged = function (e) {
        _upsNdaSat48File = e.value;
    };

    const upsDasFileUploader_onValueChanged = function (e) {
        _upsDasFile = e.value;
    }

    const uspsRuralFileUploader_onValueChanged = function (e) {
        _uspsRuralFile = e.value;
    }

    const fedExHawaiiDateBox_onValueChanged = function (e) {
        _fedExHawaiiDate = e.value;
    }

    const upsNdaSat48DateBox_onValueChanged = function (e) {
        _upsNdaSat48Date = e.value;
    }

    const upsDasDateBox_onValueChanged = function (e) {
        _upsDasDate = e.value;
    }

    const uspsRuralDateBox_onValueChanged = function (e) {
        _uspsRuralDate = e.value;
    }

    const setSiteLocalDate = function () {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetSiteLocalDate?siteName=TUCSON'); // Westernmost site ...
        ajax.done(function (response) {
            _siteLocalDate = new Date(response);
        });
    };

    const fedExHawaiiDateBox_onInitialized = function (e) {
        PPG.utility.setStartDate(e, _siteLocalDate);
        _fedExHawaiiDate = _siteLocalDate;
    }

    const upsNdaSat48DateBox_onInitialized = function (e) {
        PPG.utility.setStartDate(e, _siteLocalDate);
        _upsNdaSat48Date = _siteLocalDate;
    }

    const upsDasDateBox_onInitialized = function (e) {
        PPG.utility.setStartDate(e, _siteLocalDate);
        _upsDasDate = _siteLocalDate;
    }

    const uspsRuralDateBox_onInitialized = function (e) {
        PPG.utility.setStartDate(e, _siteLocalDate);
        _uspsRuralDate = _siteLocalDate;
    }

    const importZipsFedExHawaiiFormButton_onClick = function (e) {

        var dxFormInstance = $("#ZipsFedExHawaiiForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();
        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }


        let f = $('#formZipsFedExHawaii');

        var formData = new FormData(f[0]);

        console.log(formData);

        _ajaxingGrid.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZipSchemas', formData);

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                PPG.toastr.success(response.message);
                $('#zipsGrid').dxDataGrid('getDataSource').reload();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });
        ajax.always(function () {
            _ajaxingGrid.stop();
        });
    }

    const cancelZipsFedExHawaiiFormButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ZipsFedExHawaiiForm").dxForm("instance");
        dxFormInstance.resetValues();
    };

    const importZipsUpsSat48FormButton_onClick = function (e) {

        var dxFormInstance = $("#ZipsUpsSat48Form").dxForm("instance");
        var validationResult = dxFormInstance.validate();
        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }


        let f = $('#formZipsUpsSat48');

        var formData = new FormData(f[0]);

        console.log(formData);

        _ajaxingGrid.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZipSchemas', formData);

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                PPG.toastr.success(response.message);
                $('#zipsGrid').dxDataGrid('getDataSource').reload();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });
        ajax.always(function () {
            _ajaxingGrid.stop();
        });
    }

    const cancelZipsUpsSat48FormButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ZipsUpsSat48Form").dxForm("instance");
        dxFormInstance.resetValues();
    };

    const importZipsUpsDasFormButton_onClick = function (e) {

        var dxFormInstance = $("#ZipsUpsDasForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();
        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }


        let f = $('#formZipsUpsDas');

        var formData = new FormData(f[0]);

        console.log(formData);

        _ajaxingGrid.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZipSchemas', formData);

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                PPG.toastr.success(response.message);
                $('#zipsGrid').dxDataGrid('getDataSource').reload();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });
        ajax.always(function () {
            _ajaxingGrid.stop();
        });
    }

    const cancelZipsUpsDasFormButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ZipsUpsDasForm").dxForm("instance");
        dxFormInstance.resetValues();
    };

    const importZipsUspsRuralFormButton_onClick = function (e) {

        var dxFormInstance = $("#ZipsUspsRuralForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();
        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = $('#formZipsUspsRural');

        var formData = new FormData(f[0]);

        console.log(formData);

        _ajaxingGrid.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZipSchemas', formData);

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                PPG.toastr.success(response.message);
                $('#zipsGrid').dxDataGrid('getDataSource').reload();
                _popup.hide();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Upload failed.');
        });
        ajax.always(function () {
            _ajaxingGrid.stop();
        });
    }

    const cancelZipsUspsRuralFormButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ZipsUspsRuralForm").dxForm("instance");
        dxFormInstance.resetValues();
    };

    function customizeGridToolbar() {
        //const $filter = $('#zipsGrid > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div.dx-item.dx-toolbar-item.dx-toolbar-button');
        //$filter.appendTo($('#gridFilterContainer'));
        $('#zipsGrid .dx-datagrid-header-panel').remove();
    }

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        _exportButton.option('disabled', true);
    }

    return {
        init: init,
        getSelectedActiveGroupType: getSelectedActiveGroupType,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        uploadButton_onClick: uploadButton_onClick,
        exportButton_onClick: exportButton_onClick,
        selectBox_valueChanged: selectBox_valueChanged,
        grid_onSelectionChanged: grid_onSelectionChanged,
        exportGrid_onExporting: exportGrid_onExporting,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        fedexHawaiiZipsFileUploader_onValueChanged: fedexHawaiiZipsFileUploader_onValueChanged,
        upsNdaSat48FileUploader_onValueChanged: upsNdaSat48FileUploader_onValueChanged,
        upsDasFileUploader_onValueChanged: upsDasFileUploader_onValueChanged,
        uspsRuralFileUploader_onValueChanged: uspsRuralFileUploader_onValueChanged,
        fedExHawaiiDateBox_onValueChanged: fedExHawaiiDateBox_onValueChanged,
        upsNdaSat48DateBox_onValueChanged: upsNdaSat48DateBox_onValueChanged,
        upsDasDateBox_onValueChanged: upsDasDateBox_onValueChanged,
        uspsRuralDateBox_onValueChanged: uspsRuralDateBox_onValueChanged,
        fedExHawaiiDateBox_onInitialized: fedExHawaiiDateBox_onInitialized,
        upsNdaSat48DateBox_onInitialized: upsNdaSat48DateBox_onInitialized,
        upsDasDateBox_onInitialized: upsDasDateBox_onInitialized,
        uspsRuralDateBox_onInitialized: uspsRuralDateBox_onInitialized,
        importZipsFedExHawaiiFormButton_onClick: importZipsFedExHawaiiFormButton_onClick,
        cancelZipsFedExHawaiiFormButton_onClick: cancelZipsFedExHawaiiFormButton_onClick,
        importZipsUpsSat48FormButton_onClick: importZipsUpsSat48FormButton_onClick,
        cancelZipsUpsSat48FormButton_onClick: cancelZipsUpsSat48FormButton_onClick,
        importZipsUpsDasFormButton_onClick: importZipsUpsDasFormButton_onClick,
        cancelZipsUpsDasFormButton_onClick: cancelZipsUpsDasFormButton_onClick,
        importZipsUspsRuralFormButton_onClick: importZipsUspsRuralFormButton_onClick,
        cancelZipsUspsRuralFormButton_onClick: cancelZipsUspsRuralFormButton_onClick,
    };

}

PPG.manageZipSchemas = new PPG.manageZipSchemas(jQuery);