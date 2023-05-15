'use strict';

window.PPG = window.PPG || {};

PPG.EndOfDayFiles = function ($) {
    let _ajaxing = null;
    let _popup, _resultsPopup = null;

    const init = function () {
        _ajaxing = new PPG.Ajaxing('endOfDayFilesCard');
        _popup = $('#confirmPopup').dxPopup('instance');
        _resultsPopup = $('#result-popup').dxPopup('instance');
    };

    const generateFilesButton_onClick = function (e) {
        _popup.show();
    }

    const confirmCancel = function () {
        location.reload();
    }

    const confirmContinue = function () {
        _ajaxing.start();
        _popup.hide();

        _ajaxing.start();

        var ajax = PPG.ajaxer.post('/FileManagement/GenerateFiles')

        ajax.done(function (response) {
            if (response.success) {
                displaySearchResults(response.displayMessages);
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

    function displaySearchResults(data) {
        _resultsPopup.show();

        $('#resultList').dxList({
            dataSource: data,
            height: '100%',
            itemTemplate: function (data, index) {
                var result = $('<div>').addClass('file');
                $('<i>').addClass('icon dx-icon-check').appendTo(result);
                $('<span>').text(data).appendTo(result);
                return result;
            }
        }).dxList("instance");
    }

    return {
        init: init,
        confirmCancel: confirmCancel,
        confirmContinue: confirmContinue,
        generateFilesButton_onClick: generateFilesButton_onClick
    };

}

PPG.endOfDayFiles = new PPG.EndOfDayFiles(jQuery);