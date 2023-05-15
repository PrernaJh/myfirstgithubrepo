'use strict';

window.PPG = window.PPG || {};

PPG.Release = function ($) {

    let _ajaxing, _ajaxingForm, _releaseReleaseBlock, _packageReleaseGrid, _releasedPackagesGrid, _releasePackagePopup, _releaseFilePopup, _releaseButton = null;
    let _selectedSubClientName, _packageIdForrelease, _packageIdForRelease, _releaseFile = null;

    const init = function (defaultSubClient) {
        $("#releaseFileSubClientValue").val(defaultSubClient);
        _selectedSubClientName = defaultSubClient;
        _ajaxing = new PPG.Ajaxing('releaseCard');
        _ajaxingForm = new PPG.Ajaxing('uploadFormRelease');
        //_releasePackagePopup = $('#releasePackagePopup').dxPopup('instance');
        _releaseFilePopup = $('#releaseFilePopup').dxPopup('instance');
        _packageReleaseGrid = $('#packageReleaseGrid').dxDataGrid('instance');  
        _releasedPackagesGrid = $("#releasedPackagesGrid").dxDataGrid('instance');

        _releaseButton = $('#releaseFileButton').dxButton('instance');
        _releaseReleaseBlock = $('#ReleaseBlock');

        if (!!_selectedSubClientName) {
            _releaseReleaseBlock.fadeIn();
            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
        } else {
            _releaseReleaseBlock.hide()
        }
    };

    const onSubClientNameValueChanged = function (e) {
        _selectedSubClientName = e.value;
        if (!!_selectedSubClientName) {
            _releaseReleaseBlock.fadeIn();
            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
        } else {
            _releaseReleaseBlock.hide()
        }
    };

    const getSelectedSubClient = function (e) {
        return _selectedSubClientName;
    };

    const releaseButton_onItemClick = function (e) { 
                _releaseFilePopup.show();
            
        
    };

    const grid_onLoading = function (e) {
        _ajaxing.start();
    };

    const grid_onLoaded = function (e) {
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
    };

    const releasePackagePopup_onShowing = function (e) {
        $('#releaseFileSubClient').val(_selectedSubClientName);
    };

    const releasePackagePopup_onHiding = function (e) {
        _packageReleaseGrid.refresh();
    };

    const releaseFilePopup_onShowing = function (e) {
        //$('#releaseFileSubClientValue').val(_selectedSubClientName);
    };

    const releaseFilePopup_onHiding = function (e) {
        _packageReleaseGrid.refresh();
    };

    const packageId_onValueChanged = function (e) {
        _packageIdForrelease = e.value;
    }

    const submitPackagereleaseButton_onClick = function (e) {
        if (!_packageIdForrelease) {
            PPG.toastr.error('No Package Id has been entered.');
            return;
        }

        let result = DevExpress.ui.dialog.confirm(`release package: <strong>${_packageIdForrelease}</strong>`, 'Confirm release');
        result.done(function (dialogResult) {
            if (dialogResult) {
                _releasePackagePopup.hide();
                _ajaxing.start();

                var data = { 'packageId': _packageIdForrelease, 'releaseFileSubClientValue': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/recallRelease/releasePackage', data);

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);
                    } else {
                        PPG.toastr.error(response.message);
                    }
                });

                ajax.fail(function (xhr, status) {
                    PPG.toastr.error('release failed.');
                });

                ajax.always(function () {
                    $('#packageIdAutocomplete').dxAutocomplete('instance').option('value', '');
                    _packageIdForrelease = null;
                    _packageReleaseGrid.refresh();
                    _releasedPackagesGrid.refresh();
                    _packa.refresh();
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

                var data = { 'packageId': _packageIdForRelease, 'releaseFileSubClientValue': _selectedSubClientName };

                var ajax = PPG.ajaxer.post('/RecallRelease/ReleasePackage', data)

                ajax.done(function (response) {
                    if (response.success) {
                        PPG.toastr.success(response.message);

                    } else {
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
                    _ajaxing.stop();
                });
            }
        });
    }

    const releaseFileUploader_onValueChanged = function (e) {
        _releaseFile = e.value;
        $('#submitFilereleaseButton').dxButton('instance').option('disabled', !_releaseFile);
    };

    const submitFilereleaseButton_onClick = function (e) {
        _ajaxingForm.start();

        const f = $("#uploadFormRelease");         

        let formData = new FormData(f[0]);
        //alert($("#releaseFileSubClientValue").val());

        formData.append('releaseFileSubClientValue', $("#releaseFileSubClientValue").val());

        let ajax = PPG.ajaxer.postFormData('/RecallRelease/ReleaseFile', formData)

        ajax.done(function (response) {
            if (response.success) {
                _releaseFilePopup.hide();
                PPG.toastr.success(response.message);
            } else {
                PPG.toastr.error(response.message);
            }
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('release failed.');
        });

        ajax.always(function () {
            $('#releaseFileUploader').dxFileUploader('instance').option('value', '');
            _releaseFile = null;
            $('#submitFilereleaseButton').dxButton('instance').option('disabled', true);
            _packageReleaseGrid.refresh();
            _releasedPackagesGrid.refresh();
            _ajaxingForm.stop();
        });
    };

    return {
        init: init,
        releaseButton_onItemClick: releaseButton_onItemClick,
        grid_onLoading: grid_onLoading,
        grid_onLoaded: grid_onLoaded,
        onSubClientNameValueChanged: onSubClientNameValueChanged,
        getSelectedSubClient: getSelectedSubClient,
        grid_onSelectionChanged: grid_onSelectionChanged,
        releasePackagePopup_onShowing: releasePackagePopup_onShowing,
        releasePackagePopup_onHiding: releasePackagePopup_onHiding,
        releaseFilePopup_onShowing: releaseFilePopup_onShowing,
        releaseFilePopup_onHiding: releaseFilePopup_onHiding,
        submitPackagereleaseButton_onClick: submitPackagereleaseButton_onClick,
        packageId_onValueChanged: packageId_onValueChanged,
        releaseButton_onClick: releaseButton_onClick,
        releaseFileUploader_onValueChanged: releaseFileUploader_onValueChanged,
        submitFilereleaseButton_onClick: submitFilereleaseButton_onClick
    };

}

PPG.release = new PPG.Release(jQuery);