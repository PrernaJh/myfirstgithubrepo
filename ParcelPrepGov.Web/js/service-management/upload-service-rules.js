'use strict';

window.PPG = window.PPG || {};

PPG.UploadServiceRules = function ($) {
    let _grid, _extendedRulesGrid, _popup, _ajaxing, _fileName = null;

    const init = function () {
        _grid = $('#serviceRulesGrid').dxDataGrid('instance');
        _extendedRulesGrid = $('#extendedServiceRulesGrid').dxDataGrid('instance');
        _popup = $('#uploadServiceRulesPopup').dxPopup('instance');
        _ajaxing = new PPG.Ajaxing();
    };

    const extendedUploadFileButton_onClick = function (e) {

        var dxFormInstance = $("#UploadExtendedServiceRulesForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = getFormInstance();

        const subClientName = $('input[name="SubClientName"]').val();

        var formData = new FormData(f[0]);
        formData.append('subClientName', subClientName);

        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/UploadExtendedRules', formData);

        ajax.done(function (response) {

            if (response.success) {
                _popup.hide();
                _extendedRulesGrid.refresh();
                dxFormInstance.resetValues();
                PPG.toastr.success(_fileName);
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Extended service rules upload failed.');
        });
        ajax.always(function () { _ajaxing.stop(); });

    }

    const uploadFileButton_onClick = function (e) {

        var dxFormInstance = $("#UploadServiceRulesForm").dxForm("instance");
        var validationResult = dxFormInstance.validate();

        if (!validationResult.isValid) {
            let brokenRules = validationResult.brokenRules;
            let messages = brokenRules.map(brokenRule => brokenRule.message);
            PPG.toastr.warning(messages.join(' '));
            return;
        }

        let f = getFormInstance();

        const customerName = $('input[name="CustomerName"]').val();

        var formData = new FormData(f[0]);
        formData.append('customerName', customerName);

        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/UploadFile', formData);

        ajax.done(function (response) {

            if (response.success) {
                _popup.hide();
                _grid.refresh();
                dxFormInstance.resetValues();
                PPG.toastr.success(_fileName);
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Service rules upload failed.');
        });
        ajax.always(function () { _ajaxing.stop(); });

    }

    const fileUploader_onValueChanged = function (e) {
        let files = e.value;

        if (files.length > 0)
            _fileName = files[0].name;

        //$('#uploadFileButton').dxButton('instance').option('disabled', files.length > 0 ? false : true);
    }

    const cancelButton_onClick = function () {
        var dxFormInstance = $("#UploadServiceRulesForm").dxForm("instance");
        dxFormInstance.resetValues();
        _popup.hide();        
    };

    const extendedCancelButton_onClick = function () {
        var dxFormInstance = $("#UploadExtendedServiceRulesForm").dxForm("instance");
        dxFormInstance.resetValues();
        _popup.hide();
    };
    

    function getFormInstance() {
        return $("#uploadForm");
    }

    return {
        init: init,
        uploadFileButton_onClick: uploadFileButton_onClick,
        extendedUploadFileButton_onClick: extendedUploadFileButton_onClick,
        fileUploader_onValueChanged: fileUploader_onValueChanged,
        cancelButton_onClick: cancelButton_onClick,
        extendedCancelButton_onClick: extendedCancelButton_onClick
    };
}

PPG.uploadServiceRules = new PPG.UploadServiceRules(jQuery);