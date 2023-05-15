'use strict';

window.PPG = window.PPG || {};

PPG.ManageUspsEvsCodes = function ($) {
    let _grid, _popup, _ajaxing, _fileName, _downloadFileName, _exportButton, _createDate = null;

    const init = function () {
        _grid = $('#gridContainer').dxDataGrid('instance'); 
        _popup = $('#uploadUspsEvsCodes-popup').dxPopup('instance');
        _ajaxing = new PPG.Ajaxing(); 
        _exportButton = $('#exportButton').dxButton('instance');
    };

    const uploadButton_onClick = function (e) {
        _popup.show(); 
    };

    function unselectGridRow() {
        _grid.clearSelection();
        _grid.option('focusedRowIndex', -1);
        
    };

    // todo add blob container download file
    const downloadFile = function (e) {  
        $.ajax({
            url: '/ServiceManagement/DownloadUspsEvsCodesFile',
            data: { createDate: encodeURI(_createDate) },
            method: 'GET',
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data) {
                console.log(data);
                let a = document.createElement('a'),
                    url = window.URL.createObjectURL(data);

                a.href = url;
                a.download = _downloadFileName;
                document.body.appendChild(a);

                // fire click event from new element in dom
                a.click();
                _exportButton.option('disabled', true);
                setTimeout(function () {
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                    unselectGridRow();
                }, 100);                 
            }
        });
    }

    const grid_onFocusedRowChanged = function (e) { 
        _downloadFileName = e.row.data.FileName;        
        _createDate = e.row.data.CreateDate;
        $('#exportButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Download ${_downloadFileName}`);
            }
        });
        _exportButton.option('disabled', false);
    };

    const fileUploader_onValueChanged = function (e) {
        let files = e.value;

        if (files.length > 0)
            _fileName = files[0].name; 

        const f = $("#uploadForm"); 
        var formData = new FormData(f[0]); 
        _ajaxing.start();

        let ajax = PPG.ajaxer.postFormData('/ServiceManagement/ImportUspsEvsCodes', formData);

        ajax.done(function (response) {
            if (response.IsSuccessful) { 
                _grid.refresh(); 
                _popup.hide();
                $('#fileUploader').val('');
                PPG.toastr.success("Successfully imported " + _fileName + ".");
            } else {
                _grid.refresh();
                _popup.hide();
                PPG.toastr.error(response.Message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('USPS Evs Codes upload failed.');
        });
        ajax.always(function () {
            _ajaxing.stop();
        });
    }
    
    return {
        init: init,
        fileUploader_onValueChanged: fileUploader_onValueChanged,
        downloadFile: downloadFile,
        uploadButton_onClick: uploadButton_onClick,
        grid_onFocusedRowChanged: grid_onFocusedRowChanged,
        unselectGridRow: unselectGridRow
    };
}

PPG.manageUspsEvsCodes = new PPG.ManageUspsEvsCodes(jQuery);