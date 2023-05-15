'use strict';

window.PPG = window.PPG || {};

PPG.RecallRelease = function ($) {

    let _ajaxing, _ajaxingForm, _recallReleaseBlock, _packageReleaseGrid, _releasedPackagesGrid, _recallPackagePopup, _recallFilePopup,
        _releaseButton = null, _releaseGridRecallButton = null, _deleteButton = null;
    let _selectedSubClientName, _packageIdForRecall, _packageIdForRelease, _releaseGridPackageIdForRecall, _recallFile = null;

    const init = function (defaultSubClient) {
        _selectedSubClientName = defaultSubClient;
        _ajaxing = new PPG.Ajaxing('recallReleaseCard');
        _ajaxingForm = new PPG.Ajaxing('uploadForm');
        _recallPackagePopup = $('#recallPackagePopup').dxPopup('instance');
        _recallFilePopup = $('#recallFilePopup').dxPopup('instance');
        _packageReleaseGrid = $('#packageReleaseGrid').dxDataGrid('instance');
        _releasedPackagesGrid = $("#releasedPackagesGrid").dxDataGrid('instance');

        _releaseButton = $('#releaseButton').dxButton('instance');
        _deleteButton = $('#deleteButton').dxButton('instance');
        _releaseGridRecallButton = $('#releaseGridRecallButton').dxButton('instance');
        _recallReleaseBlock = $('#recallReleaseBlock');

        if (!!_selectedSubClientName) {
            _recallReleaseBlock.fadeIn();
            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
        } else {
            _recallReleaseBlock.hide()
        }
    };

    const onSubClientNameValueChanged = function (e) {
        _selectedSubClientName = e.value;
        $("#releaseFileSubClientValue").val(_selectedSubClientName); // for release to work

        if (!!_selectedSubClientName) {
            _recallReleaseBlock.fadeIn();

            $("#btnExport").dxButton({ 
                visible: true
            }); 

            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
        } else {
            _recallReleaseBlock.hide()
        }
    };

    const getSelectedSubClient = function (e) {
        return _selectedSubClientName;
    };

    const recallButton_onItemClick = function (e) {
        if (!e.itemData) return;
        var recallType = e.itemData.toLowerCase();

        switch (recallType) {
            case 'single package':
                _recallPackagePopup.show();
                break;
            case 'multiple packages':
                _recallFilePopup.show();
                break;
            default:
        }
    };

    const grid_onLoading = function (e) {
        _ajaxing.start();
        console.log('loading start');
    };

    const grid_onLoaded = function (e) {
        console.log('loading stop');

        _ajaxing.stop();
    };

    const grid_onSelectionChanged = function (e) {
        _packageIdForRelease = e.selectedRowKeys[0];

        $('#releaseButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Release ${_packageIdForRelease}`);
            }
        });

        _releaseButton.option('disabled', false);

        if (e.selectedRowsData != null && e.selectedRowsData.length > 0
            && e.selectedRowsData[0].RecallStatus != null
            && e.selectedRowsData[0].RecallStatus != 'IMPORTED'
            && e.selectedRowsData[0].RecallStatus != 'SCANNED'
            && e.selectedRowsData[0].RecallStatus != 'PROCESSED'
        ) {
            $('#deleteButtonTooltip').dxTooltip({
                contentTemplate: function (data) {
                    data.html(`Delete ${_packageIdForRelease}`);
                }
            });

            if (_deleteButton != null && _deleteButton.option != null) {
                _deleteButton.option('disabled', false);
            }
        }
        else
        {
            if (_deleteButton != null && _deleteButton.option != null) {
                _deleteButton.option('disabled', true);
            }
        }
    };

    const releasegrid_onSelectionChanged = function (e) {
        _releaseGridPackageIdForRecall = e.selectedRowKeys[0];

        $('#releaseGridRecallButtonTooltip').dxTooltip({
            contentTemplate: function (data) {
                data.html(`Recall ${_releaseGridPackageIdForRecall}`);
            }
        });

        _releaseGridRecallButton.option('disabled', false);
    };

    const recallPackagePopup_onShowing = function (e) {
        $('#recallPackageSubClient').val(_selectedSubClientName);
    };

    const recallPackagePopup_onHiding = function (e) {
        _packageReleaseGrid.refresh();
    };

    const recallFilePopup_onShowing = function (e) {
        $('#recallFileSubClient').val(_selectedSubClientName);
    };

    const recallFilePopup_onHiding = function (e) {
        _packageReleaseGrid.refresh();
    };
    const packageId_onValueChanged = function (e) {
        _packageIdForRecall = e.value;
    }
    const exportFile = function () {
        // export disable
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "exportxlsx"
        });        

        try { 

            var turl = "/RecallRelease/Export";
            var paramdata = { subClient: _selectedSubClientName };

            // panel to cover id=b
            var loadPanel = $(".loadPanel").dxLoadPanel({
                shadingColor: "rbga(0,0,0,0.4)",
                position: { of: "#b" },
                visible: false,
                showIndicator: true,
                showPane: true,
                shading: true
            }).dxLoadPanel("instance");

            loadPanel.show();

            // get the file and update ui 
            $.ajax({
                url: turl,
                method: 'POST',
                data: paramdata,
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data) {
                    let a = document.createElement('a'),
                        url = window.URL.createObjectURL(data);

                    a.href = url;
                    var now = new Date();
                    var nowMonth = now.getMonth() + 1; 
                    var fileName = "Recall_Release" + now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + "_" + addZero(now.getHours()) + "" + addZero(now.getMinutes()) + "" + addZero(now.getSeconds()) + ".xlsx";

                    a.download = fileName;
                    document.body.appendChild(a);

                    // fire click event from new element in dom
                    a.click();

                    setTimeout(function () {
                        window.URL.revokeObjectURL(url);
                        document.body.removeChild(a);
                    }, 100);

                    $("#btnExport").dxButton({
                        text: "Export Recalled and Released Packages",
                        disabled: false,
                        icon: "xlsxfile"
                    });
                    loadPanel.hide();
                },
                error: function (msg) {
                    PPG.toastr.error('Export error. Please contact your administrator.');
                }
            });             
        } catch (e) {
            PPG.toastr.error(e);
        }
    }

    // repeating myself with addZero function
    function addZero(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    }

    const validatePackageRecallRequest = function (e) {
        var IsValid = true;        

        if (!_packageIdForRecall) {
            PPG.toastr.error('No Package Id has been entered.');
            IsValid = false;
        }
        else if (_packageIdForRecall.length > 51) {
            PPG.toastr.error('Package Id cannot exceed 51 characters.'); // Alternatively we can make it say Package Id is invalid
            IsValid = false;
        }
        else if (_packageIdForRecall.indexOf(',') > -1) {
            PPG.toastr.error('Package Id cannot contain commas.'); // Alternatively we can make it say Package Id is invalid
            IsValid = false;
        }

        return IsValid;
    }

    const submitPackageRecallButton_onClick = function (e) {
        if (validatePackageRecallRequest() === false) {
            return;
        }        

        let result = DevExpress.ui.dialog.confirm(`Recall package: <strong>${_packageIdForRecall}</strong>`, 'Confirm Recall');
        result.done(function (dialogResult) {
            if (dialogResult) {
                _recallPackagePopup.hide();
                _ajaxing.start();

                var data = { 'packageId': _packageIdForRecall, 'subClient': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/RecallRelease/RecallPackage', data);

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);
                    } else {
                        PPG.toastr.error(response.message);
                    }
                });

                ajax.fail(function (xhr, status) {
                    PPG.toastr.error('Recall failed.');
                });

                ajax.always(function () {
                    $('#packageIdAutocomplete').dxAutocomplete('instance').option('value', '');
                    _packageIdForRecall = null;
                    _packageReleaseGrid.refresh();
                    _releasedPackagesGrid.refresh();                    
                    _ajaxing.stop();
                });
            }
        });
    };

    const releaseButton_onClick = function (e) {
        let result = DevExpress.ui.dialog.confirm(`Release package: <strong>${_packageIdForRelease}</strong>`, 'Confirm Release');
        result.done(function (dialogResult) {
            if (dialogResult) {

                _ajaxing.start();

                var data = { 'packageId': _packageIdForRelease, 'subClient': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/RecallRelease/ReleasePackage', data)

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);

                    } else {
                        _packageIdForRelease = null;
                        _packageReleaseGrid.option('focusedRowIndex', -1);
                        _releaseButton.option('disabled', true);

                        if (_deleteButton != null && _deleteButton.option != null) {
                            _deleteButton.option('disabled', true);
                        }

                        _packageReleaseGrid.refresh();
                        _releasedPackagesGrid.refresh();
                        _ajaxing.stop();
                        _packageReleaseGrid.clearSelection();
                        PPG.toastr.error(response.message);
                    }
                });

                ajax.fail(function (xhr, status) {
                    PPG.toastr.error('Release failed.');
                });

                ajax.always(function () {
                    _packageIdForRelease = null;
                    _packageReleaseGrid.clearSelection();
                    _packageReleaseGrid.option('focusedRowIndex', -1);
                    _packageReleaseGrid.refresh();
                    _releasedPackagesGrid.refresh();
                    _releaseButton.option('disabled', true);

                    if (_deleteButton != null && _deleteButton.option != null) {
                        _deleteButton.option('disabled', true);
                    }

                    _ajaxing.stop();
                });
            }
        });
    }

    const deleteButton_onClick = function (e) {
        let result = DevExpress.ui.dialog.confirm(`Delete package: <strong>${_packageIdForRelease}</strong>`, 'Confirm Delete');
        result.done(function (dialogResult) {
            if (dialogResult) {

                _ajaxing.start();

                var data = { 'packageId': _packageIdForRelease, 'subClient': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/RecallRelease/DeleteRecallPackage', data)

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);

                    } else {
                        _packageIdForRelease = null;
                        _packageReleaseGrid.option('focusedRowIndex', -1);
                        _releaseButton.option('disabled', true);                        
                        _deleteButton.option('disabled', true);                       

                        _packageReleaseGrid.refresh();
                        _releasedPackagesGrid.refresh();
                        _ajaxing.stop();
                        _packageReleaseGrid.clearSelection();
                        PPG.toastr.error(response.message);
                    }
                });

                ajax.fail(function (xhr, status) {
                    PPG.toastr.error('Delete failed.');
                });

                ajax.always(function () {
                    _packageIdForRelease = null;
                    _packageReleaseGrid.clearSelection();
                    _packageReleaseGrid.option('focusedRowIndex', -1);
                    _packageReleaseGrid.refresh();
                    _releasedPackagesGrid.refresh();
                    _releaseButton.option('disabled', true);
                    _deleteButton.option('disabled', true);
                    _ajaxing.stop();
                });
            }
        });
    }

    const releaseGridRecallButton_onClick = function (e) {
        let result = DevExpress.ui.dialog.confirm(`Recall package: <strong>${_releaseGridPackageIdForRecall}</strong>`, 'Confirm Recall');
        result.done(function (dialogResult) {
            if (dialogResult) {

                _ajaxing.start();

                var data = { 'packageId': _releaseGridPackageIdForRecall, 'subClient': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/RecallRelease/RecallPackage', data)

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);

                    } else {
                        PPG.toastr.error(response.message);
                    }
                });

                ajax.fail(function (xhr, status) {
                    PPG.toastr.error('Recall failed.');
                });

                ajax.always(function () {
                    _packageIdForRecall = null;
                    _releasedPackagesGrid.clearSelection();
                    _releasedPackagesGrid.option('focusedRowIndex', -1);
                    _packageReleaseGrid.refresh();
                    _releasedPackagesGrid.refresh();
                    _releaseGridRecallButton.option('disabled', true);
                    _ajaxing.stop();
                });
            }
        });
    }

    const recallFileUploader_onValueChanged = function (e) {
        _recallFile = e.value;
        $('#submitFileRecallButton').dxButton('instance').option('disabled', !_recallFile);
    };

    const submitFileRecallButton_onClick = function (e) {
        _ajaxingForm.start();

        const f = $("#uploadForm");

        let formData = new FormData(f[0]);

        let ajax = PPG.ajaxer.postFormData('/RecallRelease/RecallFile', formData)

        ajax.done(function (response) {
            if (response.success) {
                _recallFilePopup.hide();
                PPG.toastr.success(response.message);
            } else {
                PPG.toastr.error(response.message);
            }
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Recall failed.');
        });

        ajax.always(function () {
            $('#recallFileUploader').dxFileUploader('instance').option('value', '');
            _recallFile = null;
            $('#submitFileRecallButton').dxButton('instance').option('disabled', true);
            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
            _ajaxingForm.stop();
        });
    };

    return {
        init: init,
        recallButton_onItemClick: recallButton_onItemClick,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        onSubClientNameValueChanged: onSubClientNameValueChanged,
        getSelectedSubClient: getSelectedSubClient,
        grid_onSelectionChanged: grid_onSelectionChanged,
        releasegrid_onSelectionChanged: releasegrid_onSelectionChanged,
        recallPackagePopup_onShowing: recallPackagePopup_onShowing,
        recallPackagePopup_onHiding: recallPackagePopup_onHiding,
        recallFilePopup_onShowing: recallFilePopup_onShowing,
        recallFilePopup_onHiding: recallFilePopup_onHiding,
        submitPackageRecallButton_onClick: submitPackageRecallButton_onClick,
        packageId_onValueChanged: packageId_onValueChanged,
        releaseButton_onClick: releaseButton_onClick,
        releaseGridRecallButton_onClick: releaseGridRecallButton_onClick,
        recallFileUploader_onValueChanged: recallFileUploader_onValueChanged,
        submitFileRecallButton_onClick: submitFileRecallButton_onClick,
        exportFile: exportFile,
        deleteButton_onClick: deleteButton_onClick
    };

}

PPG.recallRelease = new PPG.RecallRelease(jQuery);