'use strict';

window.PPG = window.PPG || {};

PPG.PackageSearch = function ($) {

    let _ajaxing, _idAutoComplete, _ajaxingForm, _packageListViewModel, _selectedPackageId, _idType = null;

    // knockout
    function PackageListViewModel(data) {
        this.PackageList = ko.observableArray(data);
    };

    const init = function () {
        _ajaxing = new PPG.Ajaxing('packageSearchCard');
        _ajaxingForm = new PPG.Ajaxing('uploadForm');
        _idAutoComplete = $('#packageIdAutocomplete').dxAutocomplete('instance');

        // knockout
        _packageListViewModel = new PackageListViewModel([]);
        ko.applyBindings(_packageListViewModel);

        // handle querystring
        let packageIdQueryString = _idAutoComplete.option('value');
        if (packageIdQueryString) {
            _idAutoComplete.option('value', packageIdQueryString);
            getSinglePackage(packageIdQueryString);
        }
    };

    $('#packageIdAutocomplete').keydown(function (e) {
        if (e.which == 13) {
            searchPackages_onClick();
        }
    });

    const formatShippingBarCodeLink = function (data) {
        if (data.ShippingBarcode === "" || data.ShippingBarcode == null) {
            return "No Data Available"
        }
        var url = data.ShippingBarcodeHyperlink;
        // if cannot format hyperlink from unknown carrier, return as string
        if (url === data.ShippingBarcode) {
            return url;
        }
        var aTag = document.createElement('a');
        aTag.href = url;
        aTag.innerHTML = data.ShippingBarcode;
        aTag.target = "_blank";
        return aTag;
    }

    function getSinglePackage(ids) {
        _ajaxing.start();

        let data = { ids: ids };

        let url = '/PackageSearch/SingleSearch';

        var ajax = PPG.ajaxer.post(url, data);

        ajax.done(function (response) {

            // data coming in will have to match the knockout viewmodel
            if (response.data != null) {
                if (response.data.length > 1) {
                    // generate tabs
                    var tabPanel = $("#tabpanel-container").dxTabPanel({
                        height: 600,
                        dataSource: response.data,
                        selectedIndex: 0,
                        loop: false,
                        animationEnabled: true,
                        itemTitleTemplate: $("#title"),
                        itemTemplate: $("#packagesToRender"),
                        onSelectionChanged: function (e) {
                            $(".selected-index").text(e.component.option("selectedIndex") + 1);

                            var value = formatShippingBarCodeLink(response.data[e.component.option("selectedIndex")]);
                            $(".bindShippingBarCode").html(value);
                            // load the grid by this data index on change
                            loadGrid(response.data[e.component.option("selectedIndex")].PackageTracking);

                            // bind checkboxes
                            bindCheckBoxes(response.data[e.component.option("selectedIndex")]);

                        }
                    }).dxTabPanel("instance");

                    // initial bind load
                    loadGrid(response.data[0].PackageTracking);
                    // set hyperlink
                    var value = formatShippingBarCodeLink(response.data[0]);
                    $(".bindShippingBarCode").html(value);
                    // bind checkboxes
                    bindCheckBoxes(response.data[0]);

                    // bind bin code details popover
                    bindBinCode(response.data[0].BinGroupId, response.data[0].BinCode);

                    $("#search-results").hide();
                    $("#btnExportPdf").dxButton({
                        visible: false
                    });
                    $("#tabpanel-container").show();

                } else {
                    $("#tabpanel-container").hide();
                    $("#search-results").show(); 
                    $("#btnExportPdf").dxButton({
                        visible: true
                    });

                    // begin knockout dependency
                    displaySearchResults(response.data);
                    // end knockout dependency

                    // begin format
                    var value = formatShippingBarCodeLink(response.data[0]);
                    $(".bindShippingBarCode").html(value);
                 
                    // bind bin code details popover
                    bindBinCode(response.data[0].BinGroupId, response.data[0].BinCode);                     
                }
            } else {
                PPG.toastr.warning("No results found.");
                $("#multipleRowGrid").hide();
                $("#search-results").hide();
                $("#btnExportPdf").dxButton({
                    visible: false
                });                 
            }

            // hide multi rows if shown
            $("#multipleRowGrid").hide(); 

        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Search failed.');
        });

        ajax.always(function () {
            $("#btnExport").dxButton({
                visible: false
            });

            // prevent click of any checkbox within the element mainWrapper
            var checkBoxes = $("#mainWrapper").find(':checkbox');
            checkBoxes.click((e) => {
                e.stopPropagation();
                return false;
            });

            _ajaxing.stop();
        });
    }

    function bindCheckBoxes(data) {       
        $(".poBoxCheck").dxCheckBox({
            value: data.IsPoBox,
            disabled: true
        });
        $(".isRuralCheck").dxCheckBox({
            value: data.IsRural,
            disabled: true
        });
        $(".ormdCheck").dxCheckBox({
            value: data.IsOrmd,
            disabled: true
        });
        // checkbox
        $(".outsideFortyEightStates").dxCheckBox({
            value: data.IsOutside48States,
            disabled: true
        });
        $(".dduScfBin").dxCheckBox({
            value: data.IsDduScfBin,
            disabled: true
        });
        $(".upDasCheck").dxCheckBox({
            value: data.IsUpsDas,
            disabled: true
        });
        $(".saturdayCheck").dxCheckBox({
            value: data.IsSaturday,
            disabled: true
        });
        $(".isStopTheClockCheck").dxCheckBox({
            value: data.IsStopTheClock,
            disabled: true
        });
        $(".isUndeliverableCheck").dxCheckBox({
            value: data.IsUndeliverable,
            disabled: true
        });        
    }    

    function formatSecondary(property) {
        return property ? '[Secondary]' + property : '';
    }

    function bindBinCode(groupId, binCode) {
        if (groupId && binCode) {
            $.ajax({
                type: "POST",
                url: "/packagesearch/searchbindataset",
                data: { "activeGroupId": groupId, "binCode": binCode },
                success: function (response) {
                    $(".bindBinCode").html(`<a class='binPopover'>
                                                Label Description: ${response.data.LabelListDescription} <br/>
                                                Origin Point Description: ${response.data.OriginPointDescription} <br/>
                                                Drop Ship Key: ${response.data.DropShipSiteKeyPrimary} ${formatSecondary(response.data.DropShipSiteKeySecondary)}<br/>
                                                Drop Ship Site Description: ${response.data.DropShipSiteDescriptionPrimary}  ${formatSecondary(response.data.DropShipSiteDescriptionSecondary)}<br/>
                                                Drop Ship Site Address: ${response.data.DropShipSiteAddressPrimary}  ${formatSecondary(response.data.DropShipSiteAddressSecondary)} <br/>
                                                Drop Ship Site Csz: ${response.data.DropShipSiteCszPrimary}   ${formatSecondary(response.data.DropShipSiteCszSecondary)} </br>
                                                Shipping Carrier: ${response.data.ShippingCarrierPrimary} <br/>
                                                Shipping Method: ${response.data.ShippingMethodPrimary}  ${formatSecondary(response.data.ShippingMethodSecondary)}<br/>
                                                Container Type: ${response.data.ContainerTypePrimary}  ${formatSecondary(response.data.ContainerTypeSecondary)}<br/>
                                                Label Type: ${response.data.LabelTypePrimary}  ${formatSecondary(response.data.LabelTypeSecondary)}<br/>
                                                Days of the Week: ${response.data.DaysOfTheWeekPrimary}  ${formatSecondary(response.data.DaysOfTheWeekSecondary)}<br/>
                                                Regional Carrier Hub: ${response.data.RegionalCarrierHubPrimary} ${formatSecondary(response.data.RegionalCarrierHubSecondary)}<br/>
                                                Scac: ${response.data.ScacPrimary}  ${formatSecondary(response.data.ScacSecondary)}<br/>
                                                AccountId: ${response.data.AccountIdPrimary}  ${formatSecondary(response.data.AccountIdSecondary)}<br/>
                                            </a><span id='binData'><a>Details ...</a></span>`);
                    $(".binPopover").dxPopover({
                        target: "#binData",
                        showEvent: "mouseenter",
                        hideEvent: "mouseleave",
                        position: "top",
                        width: 400,
                        animation: {
                            show: {
                                type: "pop",
                                from: { scale: 0 },
                                to: { scale: 1 }
                            },
                            hide: {
                                type: "fade",
                                from: 1,
                                to: 0
                            }
                        }
                    });
                }
            });
        }
    }

    // handle list of string or comma separated
    function getPackages(ids) {
        $("#btnExportPdf").dxButton({
            visible: false
        });   

        _ajaxing.start();

        var data = { "Ids": ids };

        let ajax = $.ajax({
            type: "POST",
            url: "/PackageSearch/Search",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            async: true,
            cache: false
        });

        ajax.done(function (response) {
            $("#btnExport").dxButton({
                visible: true
            });

            DataBindGrid(response);

            $("#multipleRowGrid").show();
            $("#tabpanel-container").hide();
            $("#search-results").hide();
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Search failed.');
        });

        ajax.always(function () {
            _ajaxing.stop();
        });
    }

    function loadGrid(data) {
        $(".packageTrackingGrid").dxDataGrid({
            dataSource: data,
            rowAlternationEnabled: true,
            columns: [
                "ShippingCarrier",
                "TrackingNumber",
                {
                    dataField: "EventDate",
                    dataType: "datetime"
                },
                "EventDescription",
                "EventLocation",
                "UserId",
                "EventCode",
                "EventZip"
            ],
            showBorders: true,
            allowColumnResizing: true,
            columnAutoWidth: true,
            wordWrapEnabled: true,
            height: function () {
                return window.innerHeight / 4;
            },
            scrolling: {
                mode: "virtual",
                columnRenderingMode: "virtual"
            }, 
        });

        // resize the parent tab container to fit
        $("#tabpanel-container").css({
            'width': 'auto',
            'height': 'auto',
            'display': 'table'
        });
       
    }

    function DataBindGrid(response) {
        // todo fix the grid to match the image file
        $("#multipleRowGrid").dxDataGrid({
            width: "100%",
            height: function () {
                return window.innerHeight / 2;
            },
            dataSource: response,
            showBorders: true,
            allowColumnResizing: true,
            columnResizeMode: "widget",
            columnAutoWidth: true,
            rowAlternationEnabled: true,
            paging: {
                enabled: false
            },
            scrolling: {
                mode: "virtual",
                columnRenderingMode: "virtual"
            }, // open columns at your own risk
            onCellPrepared: function (options) {
                options.cellElement.html(options.value);
            },
            columns: [
                {
                    caption: "PackageId",
                    dataField: "PACKAGE_ID",
                    calculateCellValue: function (data) {
                        if (data.PACKAGE_ID === "" || data.PACKAGE_ID == null) {
                            return "No Data Available"
                        }
                        var url = window.location.protocol + "//" + window.location.host + "/PackageSearch?packageId=" + data.PACKAGE_ID;
                        var aTag = document.createElement('a');
                        aTag.href = url;
                        aTag.innerHTML = data.PACKAGE_ID;
                        aTag.target = "_blank";
                        return aTag;
                    }
                },
                {
                    caption: "InquiryId",
                    dataField: "INQUIRY_ID",
                    calculateCellValue: function (data) {
                        
                        var url = data.INQUIRY_ID_HYPERLINK_UIONLY;
                        var aTag = document.createElement('a');
                        aTag.href = url;
                        aTag.target = "_blank";

                        if (data.INQUIRY_ID == "" || data.INQUIRY_ID == null) {
                            aTag.innerHTML = "Create new inquiry";
                        }
                        else {
                            aTag.innerHTML = data.INQUIRY_ID;

                        }

                        return aTag;
                    }
                },
                {
                    caption: "SR#",
                    dataField: "SERVICE_REQUEST_NUMBER"
                },
                {
                    caption: "Tracking Number",
                    dataField: "TRACKING_NUMBER",
                    calculateCellValue: function (data) {
                        if (data.TRACKING_NUMBER === "" || data.TRACKING_NUMBER === null) {
                            return "No Data Available";
                        }
                        var url = "";
                        if (data.CARRIER == "FedEx") {
                            //url = "https://www.fedex.com/fedextrack/no-results-found?trknbr=" + data.TRACKING_NUMBER;
                        } else if (data.CARRIER == "USPS") {
                            url = "https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1=" + data.TRACKING_NUMBER;
                        } else if (data.CARRIER == "UPS") {
                            url = "https://www.ups.com/track?loc=null&tracknum=" + data.TRACKING_NUMBER + "&requester=WT/trackdetails";
                        } else {
                            url = "https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1=" + data.TRACKING_NUMBER;
                        }
                        var aTag = document.createElement('a');
                        aTag.href = url;
                        aTag.innerHTML = data.TRACKING_NUMBER;
                        aTag.target = "_blank";
                        return aTag;
                    }
                },
                {
                    caption: "Carrier",
                    dataField: "SHIPPING_CARRIER"
                },
                {
                    caption: "Package Status",
                    dataField: "PACKAGE_STATUS"
                },
                {
                    caption: "Customer Location",
                    dataField: "CUST_LOCATION"
                },
                {
                    caption: "Product",
                    dataField: "PRODUCT"
                },
                {
                    caption: "Destination Zip",
                    dataField: "DEST_ZIP"
                },
                {
                    caption: "Date Shipped",
                    dataField: "DATE_SHIPPED"
                },
                {
                    caption: "Entry Unit Name",
                    dataField: "ENTRY_UNIT_NAME"
                },
                {
                    caption: "Entry Unit CSZ",
                    dataField: "ENTRY_UNIT_CSZ"
                },
                {
                    caption: "Last Known Description",
                    dataField: "LAST_KNOWN_DESC"
                },
                {
                    caption: "Last Known Date",
                    dataField: "LAST_KNOWN_DATE"
                },
                {
                    caption: "Last Known Location",
                    dataField: "LAST_KNOWN_LOCATION"
                },
                {
                    caption: "Last Known Location Zip",
                    dataField: "LAST_KNOWN_ZIP"
                }
            ]
        });
    }

    const searchPackages_onClick = function (e) {
        // get selected package id
        var uploader = $("#fileUploader").dxFileUploader("instance");
        _selectedPackageId = _idAutoComplete.option("value");
        uploader.reset();

        $("#multipleRowGrid").hide();
        if (_selectedPackageId != null) {
            var trimmedIds = _selectedPackageId.trim();
            var indexFound = trimmedIds.indexOf(" ", 0);
            if (indexFound != -1) {
                getPackages(_selectedPackageId);
            } else {
                if (_selectedPackageId.split(",").length == 1) {
                    getSinglePackage(_selectedPackageId);

                } else {
                    // if more than one input, call different data routine
                    getPackages(_selectedPackageId);
                }
            }
        }
    }
    const printFile = function () {
        var prtContent = document.getElementById("section-to-print");
        var iframe = document.getElementById("printf");

        iframe.contentWindow.document.open();
        iframe.contentWindow.document.write(prtContent.innerHTML);
        iframe.contentWindow.print()
        iframe.contentWindow.document.close();

    }

    const fileUploader_onValueChanged = function () {

        const f = $("#uploadForm");
        let formData = new FormData(f[0]);

        let currentFile = formData.get("searchFile").name;
        if (currentFile === '') {
            return;
        }

        $("#tabpanel-container").hide();
        _idAutoComplete.option('value', null);

        _ajaxingForm.start();

        let ajax = PPG.ajaxer.postFormData('/PackageSearch/SearchByFile', formData)

        ajax.done(function (response) {
            $("#search-results").hide();

            if (response && response.length > 0) {
                DataBindGrid(response);
                $("#multipleRowGrid").show();
                $("#btnExport").dxButton({
                    visible: true
                });
            } else {
                PPG.toastr.warning("No records found.");
                $("#multipleRowGrid").hide();
                $("#btnExport").dxButton({
                    visible: false
                });
            }       
        });

        ajax.fail(function (xhr, status) {
            PPG.toastr.error('File upload failed.');
            _ajaxingForm.stop();
        });

        ajax.always(function () {
            _ajaxingForm.stop();
        });
    };

    const exportFile = function () {
        // export disable
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "exportxlsx"
        });

        // get values from file
        const f = $("#uploadForm");
        let formData = new FormData(f[0]);

        // add values from input to the form, if any or not and we check in GetValues
        formData.append("values", _selectedPackageId);

        try {
            let ajax = PPG.ajaxer.postFormData('/PackageSearch/GetValues', formData);
            // get all ids regardless if from file or from search input
            ajax.done(function (response) {
                // params are generated in ctor and controls events
                var turl = "/PackageSearch/Export";
                var paramdata = { Ids: response, IdType: _idType };

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
                        var nowYear = now.getFullYear();
                        var fileName = "Package_search_" + now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + "_" + addZero(now.getHours()) + "" + addZero(now.getMinutes()) + "" + addZero(now.getSeconds()) + ".xlsx";

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
                    error: function (msg) {
                        PPG.toastr.error('Export error. Please contact your administrator.');
                    }
                });

            });
        } catch (e) {
            PPG.toastr.error(e);
        }
    }

    function addZero(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    }

    // knockout
    function displaySearchResults(data) {
        //console.log(data);
        updateViewModel(data);
    }

    // knockout
    function updateViewModel(data) {
        var datamodels = data.map(x => new PackageViewModel(x));
        _packageListViewModel.PackageList(datamodels);
    }

    // knockout
    function PackageViewModel(data) {
        this.PackageId = ko.observable(data.PackageId);
        this.InquiryId = ko.observable(data.InquiryId);
        this.InquiryIdHyperLink = ko.observable(data.InquiryIdHyperLink);
        this.ServiceRequestNumber = ko.observable(data.ServiceRequestNumber);
        this.Barcode = ko.observable(data.Barcode);
        this.Carrier = ko.observable(data.Carrier);
        this.RecipientAddress = ko.observable(data.RecipientAddress);
        this.RecipientName = ko.observable(data.RecipientName);
        this.ScanDateTimeUsps = ko.observable(data.ScanDateTimeUsps);
        this.Type = ko.observable(data.Type);
        this.UspsLocation = ko.observable(data.UspsLocation);
        this.Weight = ko.observable(data.Weight);
        this.Status = ko.observable(data.Status);
        this.TrackingNumber = ko.observable(data.TrackingNumber);
        this.CreateDate = ko.observable(data.CreateDate);
        this.PackageTracking = ko.observableArray(data.PackageTracking);
        this.AddressLine1 = ko.observable(data.AddressLine1);
        this.AddressLine2 = ko.observable(data.AddressLine2);
        this.ShippingBarcode = ko.observable(data.ShippingBarcode);
        this.FscJob = ko.observable(data.FscJob);
        this.ProcessedDate = ko.observable(data.ProcessedDate);
        this.PackageStatus = ko.observable(data.PackageStatus);
        this.BinCode = ko.observable(data.BinCode);
        this.BinGroupId = ko.observable(data.BinGroupId);
        this.SiteName = ko.observable(data.SiteName);
        this.Zone = ko.observable(data.Zone);
        this.ServiceLevel = ko.observable(data.ServiceLevel);
        this.IsPoBox = ko.observable(data.IsPoBox);
        this.IsRural = ko.observable(data.IsRural);
        this.IsOrmd = ko.observable(data.IsOrmd);
        this.IsUpsDas = ko.observable(data.IsUpsDas);
        this.IsSaturday = ko.observable(data.IsSaturday);
        this.IsOutside48States = ko.observable(data.IsOutside48States);
        this.IsDduScfBin = ko.observable(data.IsDduScfBin);
        this.IsStopTheClock = ko.observable(data.IsStopTheClock);
        this.IsUndeliverable = ko.observable(data.IsUndeliverable);
        this.MailCode = ko.observable(data.MailCode);
        this.MarkupType = ko.observable(data.MarkupType);
        this.CosmosCreateDate = ko.observable(data.CosmosCreateDate);
        this.ShippingCarrier = ko.observable(data.ShippingCarrier);
        this.ShippingMethod = ko.observable(data.ShippingMethod);
        this.State = ko.observable(data.State);
        this.City = ko.observable(data.City);
        this.Zip = ko.observable(data.Zip);
        this.ContainerId = ko.observable(data.ContainerId);
        this.MedicalCenterId = ko.observable(data.MedicalCenterId);
        this.MedicalCenterName = ko.observable(data.MedicalCenterName);
        this.MedicalCenterAddress1 = ko.observable(data.MedicalCenterAddress1);
        this.MedicalCenterAddress2 = ko.observable(data.MedicalCenterAddress2);
        this.MedicalCenterCsz = ko.observable(data.MedicalCenterCsz);
        this.ShippingBarcodeHyperlink = ko.observable(data.ShippingBarcodeHyperlink);
        this.BinCodeDescription = ko.observable(data.BinCodeDescription);
        this.ClientName = ko.observable(data.ClientName);
        this.LastKnownDate = ko.observable(data.LastKnownDate);
        this.LastKnownDescription = ko.observable(data.LastKnownDescription);
        this.LastKnownLocation = ko.observable(data.LastKnownLocation);
        this.LastKnownZip = ko.observable(data.LastKnownZip);
        this.StopTheClockDate = ko.observable(data.StopTheClockDate);
    };

    return {
        init: init,
        searchPackages_onClick: searchPackages_onClick,
        fileUploader_onValueChanged: fileUploader_onValueChanged,
        exportFile: exportFile,
        printFile: printFile,
        bindBinCode: bindBinCode
    };
}

PPG.packageSearch = new PPG.PackageSearch(jQuery);