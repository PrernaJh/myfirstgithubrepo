'use strict';

window.PPG = window.PPG || {};

PPG.UserManagement = function ($) {

    let _selectedUserId, _selectedSite, _selectedUsername, _siteName, _grid, _form = null;

    const cellTemplate = function (container, options) {
        var noBreakSpace = '\u00A0',
            text = (options.value || []).map(element => {
                return options.column.lookup.calculateCellValue(element);
            }).join(', ');
        container.text(text || noBreakSpace).attr('title', text);
    }

    const onKeyDown = function (e) {
        if (e.event.keyCode !== 13)
            return;
        const selectedRowKeys = e.component.getSelectedRowKeys();
        const currentKey = selectedRowKeys[0];
        const dataIndex = e.component.getRowIndexByKey(currentKey);
        e.component.editRow(dataIndex);
    }

    const onToolbarPreparing = function (e) {
        let toolbarItems = e.toolbarOptions.items;  
        let addRowButton = toolbarItems.find(x => x.name === "addRowButton");
        addRowButton.options.text = "Create User";
        addRowButton.showText = "always";
    }

    const init = function () {
        _grid = $('#gridUsersContainer').dxDataGrid('instance');
        //$("#gridUsersContainer").hide();  Why did we do this?
    };

    const addNewButton_onClick = function () {
        $('#gridUsersContainer > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div.dx-item.dx-toolbar-item.dx-toolbar-button.dx-toolbar-item-auto-hide.dx-toolbar-text-auto-hide > div > div').trigger('click');
    };

    const changeSiteButton_onClick = function (e) {
        $('#changePlantPopup').dxPopup('instance').show();
    };

    const grid_onSelectionChanged = function (e) {
        _selectedUsername = e.selectedRowsData[0].UserName;
        _selectedUserId = e.selectedRowKeys[0];
    };

    const siteSelectBox_onSelectionChanged = function (e) {
        _selectedSite = e.selectedItem.SiteName;
    };

    const submitButton_onClick = function (e) {

        const data = {
            'UserId': _selectedUserId,
            'Site': _selectedSite,
            'Username': _selectedUsername
        }

        let ajax = PPG.ajaxer.post('/usermanagement/changesite', data)

        ajax.done(function (response) {
            if (response.success) {
                location.reload();
            } else {
                PPG.toastr.error(response.message);
            }
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Plant change failed.');
        });

        ajax.always(function () {
            _ajaxingForm.stop();
        });
    };

    function moveGridFilter() {
        const $el = $('#gridUsersContainer > div > div.dx-datagrid-header-panel > div > div > div.dx-toolbar-after > div:nth-child(2)');
        $el.appendTo($('#gridFilterContainer'));
        $('.dx-datagrid-header-panel').hide();
    }

    const gridPopupAdmin_onFieldDataChanged = function (e) {
        console.log(e);
    };

    const RolesSelectBox_OnSelectionChanged = function (e) {
        console.log(e);

        let role = e.selectedItem;


        var siteSelectBox = $("#SiteSelectBox").dxSelectBox("instance");
        var clientSelectBox = $("#ClientSelectBox").dxSelectBox("instance");
        var subClientSelectBox = $("#SubClientSelectBox").dxSelectBox("instance");

        if (!siteSelectBox || !clientSelectBox || !subClientSelectBox) {
            return;
        }

        let siteDataSource = siteSelectBox.getDataSource();
        let clientDataSource = clientSelectBox.getDataSource();
        let subClientSource = subClientSelectBox.getDataSource();

        let isAdmin = ['SYSTEMADMINISTRATOR', 'ADMINISTRATOR'].indexOf(role) >= 0;
        let isRoleSiteBased = ['SUPERVISOR', 'OPERATOR', 'QUALITYASSURANCE', 'AUTOMATIONSTATION'].indexOf(role) >= 0;

        if (isAdmin) {
            siteSelectBox.option("disabled", true);
            siteSelectBox.option("value", null);
            clientSelectBox.option("disabled", true);
            clientSelectBox.option("value", null);
            subClientSelectBox.option("disabled", true);
            subClientSelectBox.option("value", null);
        } else if (isRoleSiteBased) {
            siteSelectBox.option("disabled", false);
            //siteSelectBox.option("value", null);
            clientSelectBox.option("disabled", true);
            clientSelectBox.option("value", null);
            subClientSelectBox.option("disabled", true);
            subClientSelectBox.option("value", null);
        } else {
            siteSelectBox.option("disabled", true);
            siteSelectBox.option("value", null);
            clientSelectBox.option("disabled", false);
            //clientSelectBox.option("value", null);
            subClientSelectBox.option("disabled", false);
            //subClientSelectBox.option("value", null);
        }
    };

    const SiteSelectBox_OnSelectionChanged = function (e) {
        console.log(e);
    };

    const ClientSelectBox_OnSelectionChanged = function (e) {
        console.log(e);

        var subClientSelectBox = $("#SubClientSelectBox").dxSelectBox("instance");

        if (!subClientSelectBox) {
            return;
        }

        let subClientSource = subClientSelectBox.getDataSource();

        
        subClientSource.filter("ClientName", "=", e.selectedItem.Name);
        subClientSource.load();
    };

    const SubClientSelectBox_OnSelectionChanged = function (e) {
        console.log(e);
    };

    const onSiteValueChanged = function (e) {
        _siteName = e.value;
        $("#gridUsersContainer").show();
        _grid.refresh();
    };
     
    const onInitNewRow = function (e) {
        e.data.SiteName = _siteName;
    }
    const getSelectedSite = function (e) {
        return _siteName;
    };

    return {
        init: init,
        cellTemplate: cellTemplate,
        onKeyDown: onKeyDown,
        onToolbarPreparing: onToolbarPreparing,
        addNewButton_onClick: addNewButton_onClick,
        changeSiteButton_onClick: changeSiteButton_onClick,
        grid_onSelectionChanged: grid_onSelectionChanged,
        submitButton_onClick: submitButton_onClick,
        siteSelectBox_onSelectionChanged: siteSelectBox_onSelectionChanged,
        gridPopupAdmin_onFieldDataChanged: gridPopupAdmin_onFieldDataChanged,
        RolesSelectBox_OnSelectionChanged: RolesSelectBox_OnSelectionChanged,
        SiteSelectBox_OnSelectionChanged: SiteSelectBox_OnSelectionChanged,
        ClientSelectBox_OnSelectionChanged: ClientSelectBox_OnSelectionChanged,
        SubClientSelectBox_OnSelectionChanged: SubClientSelectBox_OnSelectionChanged, 
        onSiteValueChanged: onSiteValueChanged,
        getSelectedSite: getSelectedSite, 
        onInitNewRow: onInitNewRow
    };
}

PPG.userManagement = new PPG.UserManagement(jQuery);