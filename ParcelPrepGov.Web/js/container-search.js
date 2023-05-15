'use strict';

window.PPG = window.PPG || {};

PPG.ContainerSearch = function ($) {

    let _idAutoComplete, _selectedContainersId, _ajaxing = null;   

    var vm = {
        ID: ko.observable(),
        CONTAINER_ID: ko.observable(),
        MANIFEST_DATE_STRING: ko.observable(),
        STATUS: ko.observable(),
        CONTAINER_TYPE: ko.observable(),
        SHIPPING_CARRIER: ko.observable(),
        SHIPPING_METHOD: ko.observable(),
        TRACKING_NUMBER: ko.observable(),
        TRACKING_NUMBER_HYPERLINK: ko.observable(),
        BIN_CODE: ko.observable(),
        FSC_SITE: ko.observable(),
        DROP_SHIP_SITE_KEY: ko.observable(),
        ZONE: ko.observable(),
        ENTRY_UNIT_NAME: ko.observable(),
        ENTRY_UNIT_CSZ: ko.observable(),
        ENTRY_UNIT_TYPE: ko.observable(),
        CONTAINER_WEIGHT: ko.observable(),
        PIECES_IN_CONTAINER: ko.observable(),
        IS_OUTSIDE_48_STATES: ko.observable(),
        IS_RURAL: ko.observable(),
        IS_SATURDAY: ko.observable(),
        LAST_KNOWN_DATE_STRING: ko.observable(),
        LAST_KNOWN_DESCRIPTION: ko.observable(),
        LAST_KNOWN_LOCATION: ko.observable(),
        LAST_KNOWN_ZIP: ko.observable(),
        IS_SECONDARY_CARRIER: ko.observable(),
        PACKAGES: ko.observableArray(),
        EVENTS: ko.observableArray()
    }

   

    const init = function () {
        _ajaxing = new PPG.Ajaxing('containerSearchCard');        
        _idAutoComplete = $('#containerIdAutocomplete').dxAutocomplete('instance');
        var packagesContent = $("#packagesToRender");
        var eventsContent = $("#eventsToRender");

        // Load templates into the view
        $("#containerEvents").html(eventsContent.html());
        $("#containerPackages").html(packagesContent.html());
        $("#containerPackages").hide();
        $("#mainWrapper").hide();

        $("#containerEvents").show();        

        // handle querystring
        let containerIdQueryString = _idAutoComplete.option('value');
        if (containerIdQueryString) {
            _idAutoComplete.option('value', containerIdQueryString);
            getSingleContainer(containerIdQueryString);
        }
        
        ko.applyBindings(vm);
    }

    const doShowContainerInformationHyperLink = function () {        
        return doShowHyperLink(vm.TRACKING_NUMBER_HYPERLINK());
    }


    const doShowHyperLink = function (url) {
        return url !== '';
    }

    const searchContainer_onClick = function () {
        _selectedContainersId = _idAutoComplete.option("value");

        var validationResult = PPG.validation.isContainerOrTrackingValid(_selectedContainersId);
        if (validationResult.IsValid === false) {
            PPG.toastr.error(validationResult.Message);
        }
        else {
            getSingleContainer(_selectedContainersId);
        }
    }

    function ContainerViewModel(data) {
        debugger;
        vm.ID(data.ID);
        vm.CONTAINER_ID(data.CONTAINER_ID);
        vm.MANIFEST_DATE_STRING(data.MANIFEST_DATE_STRING);
        vm.STATUS(data.STATUS);
        vm.CONTAINER_TYPE(data.CONTAINER_TYPE);
        vm.SHIPPING_CARRIER(data.SHIPPING_CARRIER);
        vm.SHIPPING_METHOD(data.SHIPPING_METHOD);
        vm.TRACKING_NUMBER(data.TRACKING_NUMBER);
        vm.TRACKING_NUMBER_HYPERLINK(data.TRACKING_NUMBER_HYPERLINK);
        vm.BIN_CODE(data.BIN_CODE);
        vm.ZONE(data.ZONE);
        vm.FSC_SITE(data.FSC_SITE);
        vm.DROP_SHIP_SITE_KEY(data.DROP_SHIP_SITE_KEY);
        vm.ENTRY_UNIT_NAME(data.ENTRY_UNIT_NAME);
        vm.ENTRY_UNIT_CSZ(data.ENTRY_UNIT_CSZ);
        vm.ENTRY_UNIT_TYPE(data.ENTRY_UNIT_TYPE);
        vm.CONTAINER_WEIGHT(data.CONTAINER_WEIGHT);
        vm.PIECES_IN_CONTAINER(data.PIECES_IN_CONTAINER);
        vm.IS_OUTSIDE_48_STATES(data.IS_OUTSIDE_48_STATES);
        vm.IS_RURAL(data.IS_RURAL);
        vm.IS_SATURDAY(data.IS_SATURDAY);
        vm.LAST_KNOWN_DATE_STRING(data.LAST_KNOWN_DATE_STRING);
        vm.LAST_KNOWN_DESCRIPTION(data.LAST_KNOWN_DESCRIPTION);
        vm.LAST_KNOWN_LOCATION(data.LAST_KNOWN_LOCATION);
        vm.LAST_KNOWN_ZIP(data.LAST_KNOWN_ZIP);
        vm.IS_SECONDARY_CARRIER(data.IS_SECONDARY_CARRIER);
        vm.PACKAGES(data.PACKAGES);
        vm.EVENTS(data.EVENTS);        
    }

    const getSingleContainer = function (barcode) {
        // barcode can be tracking number or container id
        _ajaxing.start();
        let url = '/ContainerSearch/SingleSearch';
        var ajax = PPG.ajaxer.post(url, {Barcode: barcode});

        ajax.done(function (response) {
            if (response.data) {
                if (response.data["CONTAINER_ID"] !== null) {
                    _selectedContainersId = barcode;
                    $("#mainWrapper").show();

                    ContainerViewModel(response.data);

                    $("#btnExport").dxButton({
                        visible: true
                    });
                }
                else {
                    _selectedContainersId = null;
                    PPG.toastr.warning("No results found.");
                    $("#mainWrapper").hide();
                    $("#btnExport").dxButton({
                        visible: false
                    });
                }
            }
            else {                
                $("#btnExport").dxButton({
                    visible: false
                });
            }
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Search failed.');
        });

        ajax.always(function () {
            _ajaxing.stop();
        });
    }

    const tabs_itemClick = function (e) {
        var selectBox = $("#selectbox").dxSelectBox("instance");
        selectBox.option("value", e.itemData.id);
    }

    const selectBox_valueChanged = function (e) {        
        var tabsInstance = $("#tabs-container").dxTabs("instance");        
        tabsInstance.option("selectedIndex", e.value);        
        
        if (e.value === 1) {
            $("#containerEvents").hide();
            $("#containerPackages").show();
        }
        else {
            $("#containerEvents").show();
            $("#containerPackages").hide();
        }        
    }

    const exportFile = function () {
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "exportxlsx"
        });

        var url = "/ContainerSearch/Export?barcode=" + _selectedContainersId;

        var loadPanel = $(".loadPanel").dxLoadPanel({
            shadingColor: "rbga(0,0,0,0.4)",
            position: { of: "#b" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true
        }).dxLoadPanel("instance");

        loadPanel.show();

        $.ajax({
            url: url,
            method: 'GET',
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data) {
                console.log(data);
                let a = document.createElement('a'),
                    url = window.URL.createObjectURL(data);

                a.href = url;                
                var now = new Date();                
                var nowMonth = now.getMonth() + 1;
                                
                var fileName = now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + ""
                    + addZero(now.getHours()) + "" + addZero(now.getMinutes()) + "" + addZero(now.getSeconds()) +
                    "_" + 'ContainerSearch' + "_" + now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + "";
          
                fileName += ".xlsx";

                a.download = fileName;
                document.body.appendChild(a);

                // fire click event from new element in dom
                a.click();

                setTimeout(function () {
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                }, 100);

                $("#btnExport").dxButton({
                    text: "Export",
                    disabled: false,
                    icon: "xlsxfile"
                });
                loadPanel.hide();
            },
            error: function (data) {
                toastr.error("Export error. Please contact your administrator.");
            },
            always: function () {
                $("#btnExport").dxButton({
                    text: "Export",
                    disabled: false,
                    icon: "xlsxfile"
                });
                loadPanel.hide();
            }
        });
    }

    function addZero(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    }

    return {
        init: init,        
        searchContainer_onClick: searchContainer_onClick,
        exportFile: exportFile,        
        tabs_itemClick: tabs_itemClick,
        selectBox_valueChanged: selectBox_valueChanged,
        getSingleContainer: getSingleContainer,
        doShowContainerInformationHyperLink: doShowContainerInformationHyperLink
    };
}


PPG.containerSearch = new PPG.ContainerSearch(jQuery);