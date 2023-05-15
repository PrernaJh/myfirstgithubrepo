'use strict';

window.PPG = window.PPG || {};

PPG.manageZoneMaps = function ($) {

    let _ajaxingGrid, _grid, _popup, _uploadButton, _exportButton = null;
    let _selectedActiveGroupId = null, _downloadFileName = null;
    let _file = null, _siteLocalDate = null;
    let _date = null;

    const init = function () {
        customizeGridToolbar();
        _ajaxingGrid = new PPG.Ajaxing('manageZoneMapsCard');
        _grid = $('#zoneMapsGrid').dxDataGrid('instance');
        _popup = $('#uploadZones-popup').dxPopup('instance');
        _uploadButton = $('#upload-button').dxButton('instance');
        _exportButton = $('#export-button').dxButton('instance');
        setSiteLocalDate();
    };

    const grid_onLoading = function (e) {
    };

    const grid_onLoaded = function (e) {
    };

    const uploadButton_onClick = function (e) {
        unselectGridRow();
        _popup.show();
    };

    const exportButton_onClick = function (e) {
        _ajaxingGrid.start();
        var ajax = PPG.ajaxer.post('/ServiceManagement/GetZoneMapsByActiveGroupId', { id: _selectedActiveGroupId });
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
            filename = "ZONE_MAPS"
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

    const popup_onShowing = function (e) {
    };

    const popup_onHiding = function (e) {
        _grid.refresh();
    };

    const fileUploader_onValueChanged = function (e) {
        _file = e.value;
    };

    const dateBox_onValueChanged = function (e) {
        _date = e.value;
    }

    const dateBox_onInitialized = function (e) {
        PPG.utility.setStartDate(e, _siteLocalDate);
        _date = _siteLocalDate;
    }

    const setSiteLocalDate = function () {
        var ajax = PPG.ajaxer.get('/ServiceManagement/GetSiteLocalDate?siteName=TUCSON'); // Westernmost site ...
        ajax.done(function (response) {
            _siteLocalDate = new Date(response);
        });
    };

    const importZonesFormButton_onClick = function (e) {

        var dxFormInstance = $("#ZoneMapsForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();
        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }


        let f = $('#formZones');

        var formData = new FormData(f[0]);

        console.log(formData);

        _ajaxingGrid.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportZoneMaps', formData);

        ajax.done(function (response) {
            if (response.success) {
                dxFormInstance.resetValues();
                PPG.toastr.success(response.message);
                $('#zoneMapsGrid').dxDataGrid('getDataSource').reload();
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

    const cancelZonesFormButton_onClick = function () {
        _popup.hide();
        var dxFormInstance = $("#ZoneMapsForm").dxForm("instance");
        dxFormInstance.resetValues();
    };

    function customizeGridToolbar() {
        //const $filter = $('#zoneMapsGrid > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div.dx-item.dx-toolbar-item.dx-toolbar-button');
        //$filter.appendTo($('#gridFilterContainer'));
        $('#zoneMapsGrid .dx-datagrid-header-panel').remove();
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
        uploadButton_onClick: uploadButton_onClick,
        exportButton_onClick: exportButton_onClick,
        grid_onSelectionChanged: grid_onSelectionChanged,
        exportGrid_onExporting: exportGrid_onExporting,
        popup_onShowing: popup_onShowing,
        popup_onHiding: popup_onHiding,
        fileUploader_onValueChanged: fileUploader_onValueChanged,
        dateBox_onValueChanged: dateBox_onValueChanged,
        dateBox_onInitialized: dateBox_onInitialized,
        importZonesFormButton_onClick: importZonesFormButton_onClick,
        cancelZonesFormButton_onClick: cancelZonesFormButton_onClick,
    };

}

PPG.manageZoneMaps = new PPG.manageZoneMaps(jQuery);