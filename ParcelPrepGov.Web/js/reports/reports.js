'use strict';
window.PPG = window.PPG || {};

PPG.Reports = function ($) {

    let _reportsCard, _grid = null;
    let _subClientName = null;
    let _subClientList = [];
    let _siteName = null;
    let _manifestDate = null;
    let _manifestStartDate = null;
    let _manifestEndDate = null;
    let _reportName = null;
    let _packageStatus = null;

    

    const init = function (reportName) {
        _reportsCard = $('#reportsCard');
        _reportName = reportName;
        //_subClientName = $('#subClientName');
        _manifestDate = new Date().toLocaleDateString("en-US");
        var manStart = new Date();
        manStart.setDate(manStart.getDate() - 6);
        _manifestStartDate = manStart.toLocaleDateString("en-US");
        _manifestEndDate = new Date().toLocaleDateString("en-US");
        _reportsCard.hide();
        _grid = $('#reportsGrid').dxDataGrid('instance');
    };

    function CalculateDays(startDate, endDate) {
        var startDay = new Date(startDate);
        var endDay = new Date(endDate);
        var msPerDay = 1000 * 60 * 60 * 24;
        var msDiff = startDay.getTime() - endDay.getTime();
        var days = Math.abs(Math.floor(msDiff / msPerDay));
        return days;
    }

    const onSubClientListChanged = function (e) {
        debugger
        _subClientList = e.value;
    };

    const onSubClientValueChanged = function (e) {
        debugger
        console.log(e.value);
        _subClientName = e.value;
    };

    const onSiteValueChanged = function (e) {
        debugger
        _siteName = e.value;
    };

    const getSelectedSite = function () {
        return _siteName;
    };

    const getSelectedSubClientList = function () {
        //var res = _subClientList.split(', ');
        //return res;
        return _subClientList;
    }

    const getSelectedSubClient = function () {
        debugger;
        return _subClientName;
    };

    const onManifestDateChanged = function (e) {
        if (e != null && e.value != null) {
            console.log(e.value);
            _manifestDate = new Date(e.value).toLocaleDateString("en-US");
        } else {
            _manifestDate = null;
        }
    };

    const onManifestStartDateChanged = function (e) {
        if (e != null && e.value != null) {
            console.log(e.value);
            _manifestStartDate = new Date(e.value).toLocaleDateString("en-US");
        } else {
            _manifestStartDate = null;
        }
    };

    const onManifestEndDateChanged = function (e) {
        if (e != null && e.value != null) {
            _manifestEndDate = new Date(e.value).toLocaleDateString("en-US");
        } else {
            _manifestEndDate = null;
        }
    };

    const getManifestDate = function () {
        return _manifestDate;
    };

    const getManifestStartDate = function () {
        return _manifestStartDate;
    };

    const getManifestEndDate = function () {
        return _manifestEndDate;
    };

    const onRunUSPSUndeliverable = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const onRunPostPerformanceSummaryReport = function (e) {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const onRunDropPointReport = function (e) {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    };

    const onRunDropPointByContainerReport = function (e) {
        debugger;
        _manifestDate = null;
        _subClientName = null;

        if (_siteName.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    };

    const onRunWeeklyInvoiceReport = function (e) {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const onRunPackageSummaryReport = function (e) {
        debugger;
        _subClientName = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    }

    const onRunClientPackageSummaryReport = function (e) {
        debugger;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    }

    const onRunUspsDailyPieceDetailReport = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunDailyPieceDetailReport = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunUpsDailyPieceDetailReport = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunFedExDailyPieceDetailReport = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunRecallReleaseReport = function () {
        debugger;
        $('.dx-datagrid-header-panel').hide();
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            $("#reportsCardDetails").hide();
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunGtr5Report = function (e) {
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    }

    const onRunDailyRevenue = function () {
        _siteName = null;

        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    }

    const onRunUndeliveredReport = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const onRunDailyWarningReport = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {

            var days = CalculateDays(_manifestStartDate, _manifestEndDate);

            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsLocationDeliverySummary = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsProductDeliverySummary = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsVisnDeliverySummary = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsLocationTrackingSummary = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsVisnTrackingSummary = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const onRunUspsCarrierDetailReport = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientName == null || _manifestStartDate == null || _manifestEndDate == null ||
            _subClientName == "" || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should before end date.');
        }
        else if (msDt < new Date(_manifestEndDate).setDate(meDt.getDate() - 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else if (meDt > new Date(_manifestStartDate).setDate(msDt.getDate() + 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunCarrierDetailReport = function () {
        debugger;
        _manifestDate = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientList == null || _manifestStartDate == null || _manifestEndDate == null || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should before end date.');
        }
        else if (msDt < new Date(_manifestEndDate).setDate(meDt.getDate() - 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else if (meDt > new Date(_manifestStartDate).setDate(msDt.getDate() + 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunAsnReconciliationReport = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientName == null || _subClientName == "" || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should be before end date.');
        }
        else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunBasicContainerPackageNestingReport = function () {
        debugger;
        _subClientName = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    const onRunDailyContainerReport = function () {
        debugger;
        _subClientName = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {
            DisableButtonForGridWithPromiseEnable("#runReportButton");
        }
    };

    // might want to have a group to disable instead of per element
    const DisableButtonForGridWithPromiseEnable = function (buttonId) {
        // run disable
        $(buttonId).dxButton({
            text: "Loading",
            disabled: true,
        });
        // export disable
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "xlsxfile"
        });

        _reportsCard.fadeIn('slow');
        _grid.refresh().then(function () {

            // run enable
            $(buttonId).dxButton({
                text: "Run Report",
                disabled: false
            });
            //export enable
            $("#btnExport").dxButton({
                text: "Export",
                type: "success",
                icon: "xlsxfile",
                disabled: false
            });
        });
    };

    const runAndExportButtonReenable = function ()
    {
        // run enable
        $('#runReportButton').dxButton({
            text: "Run Report",
            disabled: false
        });
        //export enable
        $("#btnExport").dxButton({
            text: "Export",
            type: "success",
            icon: "xlsxfile",
            disabled: false
        });
    }

    const onTrackingNumberRender = function (itemIndex, itemData, itemElement) {
        if (itemData !== undefined && itemData.data !== undefined) {
            var link = itemData.data.TRACKING_NUMBER; //'<div><a href="https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1='+ itemData.data.TRACKING_NUMBER+'" Target=_blank>' + itemData.data.TRACKING_NUMBER + '</a></div>';
            return link;
        }
        return '';
    };

    const exporting = function (e) {
        var workbook = new ExcelJS.Workbook();
        var worksheet = workbook.addWorksheet('Employees');

        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet: worksheet,
            autoFilterEnabled: true
        }).then(function () {
            workbook.xlsx.writeBuffer().then(function (buffer) {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Employees.xlsx');
            });
        });
        e.cancel = true;
    }

    const exportRecallRelease = function () {
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "exportxlsx"
        });

        // params are generated in ctor and controls events
        var url = "/Reports/ExportRecall?subClient=" + _subClientList +
            "&startDate=" + _manifestStartDate +
            "&endDate=" + _manifestEndDate;

        //alert(url);
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
                var siteOrSubclient = _siteName != null ? _siteName : _subClientList;
                var now = new Date();
                var start = _manifestDate != null ? new Date(_manifestDate) : new Date(_manifestStartDate);
                var end = _manifestDate != null ? new Date(_manifestDate) : new Date(_manifestEndDate);
                var nowMonth = now.getMonth() + 1;
                var startMonth = start.getMonth() + 1;
                var endMonth = end.getMonth() + 1;
                var fileName = now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + "" + addZero(now.getHours()) + "" + addZero(now.getMinutes()) + "" + addZero(now.getSeconds()) +
                    "_" + siteOrSubclient + "_" + start.getFullYear() + "" + addZero(startMonth) + "" + addZero(start.getDate()) + "";
                if (start.getFullYear() != end.getFullYear() || start.getMonth() != end.getMonth() || start.getDate() != end.getDate()) {
                    fileName += "_" + end.getFullYear() + "" + addZero(endMonth) + "" + addZero(end.getDate());
                }
                fileName += "_" + _reportName.replace(/\s/g, '') + ".xlsx";

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
            }
        });
    }

    function getColumnFieldName(dataGridInstance, getter) {
        var column, i;
        if ($.isFunction(getter)) {
            for (i = 0; i < dataGridInstance.columnCount(); i++) {
                column = dataGridInstance.columnOption(i);
                if (column.calculateCellValue.guid === getter.guid) {
                    return column.dataField;
                }
            }
        } else {
            return getter;
        }
    }

    const exportFile = function () {
        // export disable
        $("#btnExport").dxButton({
            text: "Processing",
            disabled: true,
            icon: "exportxlsx"
        });

        var combinedFilter = $("#reportsGrid").dxDataGrid("getCombinedFilter", true);

        // params are generated in ctor and controls events
        var url = "/Reports/Export?reportName=" + _reportName +
            "&siteName=" + _siteName +
            "&subclientNames=" + _subClientList +
            "&subclientName=" + _subClientName +
            "&manifestDate=" + _manifestDate +
            "&startDate=" + _manifestStartDate +
            "&endDate=" + _manifestEndDate + "&filterBy=" + JSON.stringify(combinedFilter);

        //alert(url);
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
                var siteOrSubclient = _siteName != null ? _siteName : _subClientList;
                //let trimmedItems = ''; 
                //for (let i = 0; i < siteOrSubclient.length; i++) {
                //    trimmedItems += siteOrSubclient[i].substring(1,2) + siteOrSubclient[i].substring(4,2);
                //} 
                //alert(trimmedItems);

                var now = new Date();
                var start = _manifestDate != null ? new Date(_manifestDate) : new Date(_manifestStartDate);
                var end = _manifestDate != null ? new Date(_manifestDate) : new Date(_manifestEndDate);
                var nowMonth = now.getMonth() + 1;
                var startMonth = start.getMonth() + 1;
                var endMonth = end.getMonth() + 1;
                var fileName = now.getFullYear() + "" + addZero(nowMonth) + "" + addZero(now.getDate()) + "" + addZero(now.getHours()) + "" + addZero(now.getMinutes()) + "" + addZero(now.getSeconds()) +
                    "_" + _reportName.replace(/\s/g, '') + "_" + start.getFullYear() + "" + addZero(startMonth) + "" + addZero(start.getDate()) + "";
                if (start.getFullYear() != end.getFullYear() || start.getMonth() != end.getMonth() || start.getDate() != end.getDate()) {
                    fileName += "_" + end.getFullYear() + "" + addZero(endMonth) + "" + addZero(end.getDate());
                }
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
            }
        });

    }

    const uspsCarrierDetailExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientName == null || _manifestStartDate == null || _manifestEndDate == null ||
            _subClientName == "" || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should before end date.');
        }
        else if (msDt < new Date(_manifestEndDate).setDate(meDt.getDate() - 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else if (meDt > new Date(_manifestStartDate).setDate(msDt.getDate() + 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else {
            exportFile();
        }
    }

    const advancedDailyWarningExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {

            var days = CalculateDays(_manifestStartDate, _manifestEndDate);

            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const asnReconciliationExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientName == null || _subClientName == "" || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should be before end date.');
        }
        else {
            exportFile();
        }
    }

    const basicContainerPackageNestingExportFile = function () {
        debugger;
        _subClientName = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {
            exportFile();
        }
    }

    const clientPackageSummaryExportFile = function () {
        debugger;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const dailyRevenueExportFile = function () {
        _siteName = null;

        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {
            exportFile();
        }
    }

    const fedExDailyPieceDetailExportFile = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const packageSummaryExportFile = function () {
        debugger;
        _subClientName = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const postalPerformanceSummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const recallReleaseSummaryExportFile = function () {
        debugger;
        $('.dx-datagrid-header-panel').hide();
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportRecallRelease();
        }
    }

    const carrierDetailExportFile = function () {
        debugger;
        _manifestDate = null;
        var msDt = new Date(_manifestStartDate);
        var meDt = new Date(_manifestEndDate);
        if (_subClientList == null || _manifestStartDate == null || _manifestEndDate == null || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else if (msDt > meDt) {
            PPG.toastr.error('Manifest start date should before end date.');
        }
        else if (msDt < new Date(_manifestEndDate).setDate(meDt.getDate() - 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else if (meDt > new Date(_manifestStartDate).setDate(msDt.getDate() + 31)) {
            PPG.toastr.error('Manifest start and end dates gap should not be more than 31 days.');
        }
        else {
            exportFile();
        }
    }
    
    const undeliveredReportExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const upsDailyPieceDetailExportFile = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const uspsDailyPieceDetailExportFile = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const dailyPieceDetailExportFile = function () {
        debugger;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const uspsDropPointStatusExportFile = function () {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const uspsDropPointStatusByContainerExportFile = function () {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_siteName.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const uspsGtr5ExportFile = function () {
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == null || _manifestEndDate == null) {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            exportFile();
        }
    }

    const uspsLocationDeliverySummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const uspsLocationTrackingSummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const uspsProductDeliverySummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const uspsUndeliverableExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const uspsVisnDeliverySummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const uspsVisnTrackingSummaryExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days");
            }
        }
    }

    const dailyContainerReportExportFile = function () {
        debugger;
        _subClientList = null;
        if (_siteName == null || _manifestDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        }
        else {
            exportFile();
        }
    }

    const weeklyInvoiceExportFile = function () {
        debugger;
        _manifestDate = null;
        _siteName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const formatDecimal = function (num, precision) {
        if (num == null || typeof num === undefined)
            return 0;
        return ((num * 1).toFixed(precision) + "").toLocaleString();
    }

    const formatPercent = function (num, precision) {
        if (num == null || typeof num === undefined) {
            return "0.00%";
        } else {
            // get our new value with precision 
            return (num * 100).toFixed(precision) + "%";
        }
    }


    const formatPackageHyperLink = function (data) {
        if (data.PACKAGE_ID === "" || data.PACKAGE_ID == null) {
            return "No Data Available"
        }
        var aTag = document.createElement('a');
        aTag.href = data.PACKAGE_ID_HYPERLINK;
        aTag.innerHTML = data.PACKAGE_ID;
        aTag.target = "_blank";
        return aTag;
    }

    const formatTrackingHyperLink = function (data) {
        if (data.TRACKING_NUMBER === "" || data.TRACKING_NUMBER == null) {
            return "No Data Available"
        }
        var url = data.TRACKING_NUMBER_HYPERLINK;
        if (url === data.TRACKING_NUMBER) {
            return url;
        }
        var aTag = document.createElement('a');
        aTag.href = url;
        aTag.innerHTML = data.TRACKING_NUMBER;
        aTag.target = "_blank";
        return aTag;
    }

    const formatContainerHyperLink = function (data) {        
        if (data.CONTAINER_ID === "" || data.CONTAINER_ID == null) {
            return "No Data Available"
        }

        var url = data.CONTAINER_ID_HYPERLINK;
        var aTag = document.createElement('a');
        aTag.href = url;
        aTag.innerHTML = data.CONTAINER_ID;
        aTag.target = "_blank";
        return aTag;
    }

    const formatContainerTrackingHyperLink = function (data) {
        if (data.CONTAINER_TRACKING_NUMBER === "" || data.CONTAINER_TRACKING_NUMBER == null) {
            return "No Data Available"
        }
        var url = data.CONTAINER_TRACKING_NUMBER_HYPERLINK;
        if (url === data.CONTAINER_TRACKING_NUMBER) {
            return url;
        }
        var aTag = document.createElement('a');
        aTag.href = url;
        aTag.innerHTML = data.CONTAINER_TRACKING_NUMBER;
        aTag.target = "_blank";
        return aTag;
    }

    function calculateSummary(options, genericList) {
        if (typeof options !== undefined) {
            for (var i = 0; i < genericList.length; i++) {
                calculateSummaryForRow(options, genericList[i]);
            }
        }
    }

    function calculateSummaryForRow(options, genericParam) {
        if (options.name === genericParam.Name) {
            // step 1: start the process
            if (options.summaryProcess === "start") {
                options.totalValue = 0.00;
                options.recordSum = 0;
                options.totalPieces = 0;
            }
            // step 2: calculate each row
            if (options.summaryProcess === "calculate") {
                options.recordSum++;
                options.totalPieces += options.value[genericParam.Divisor];
                // know kind of calc, percent, avg, 
                if (genericParam.Type == "PCT") {
                    options.totalValue = options.totalValue + options.value[genericParam.Column];
                } else if (genericParam.Type == "AVG") { // Weighted average
                    options.totalValue = options.totalValue + (options.value[genericParam.Column] * options.value[genericParam.Divisor]);
                }
            }
            // step 3: profit
            if (options.summaryProcess === "finalize") {
                // only finalize summary if we have records
                if (options.recordSum > 0) {
                    if (genericParam.Type == "PCT") {
                        if (options.totalPieces != 0) {
                            options.totalValue = formatPercent(options.totalValue / options.totalPieces, 2);
                        } else {
                            options.totalValue = formatPercent(0, 2);
                        }
                    } else if (genericParam.Type == "AVG") {
                        if (options.totalPieces != 0) {
                            options.totalValue = formatDecimal(options.totalValue / options.totalPieces, 2);
                        } else {
                            options.totalValue = formatDecimal(0, 2);
                        }
                    }
                }
            }
        }
    }

    /// to extend, add new variable and add to the listOfColumns at any index
    const calculateCustomSummary = function (options) {
        // begin uspsvisntracking
        var noStcPercent = {
            Column: "NO_STC_PCS", Name: "NoSTCPercent", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var deliveredPercent = {
            Column: "DELIVERED_PCS", Name: "DeliveredTotalPercent", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var postalDaysAverage = {
            Column: "AVG_POSTAL_DAYS", Name: "PostalDaysAverage", Divisor: "DELIVERED_PCS", Type: "AVG"
        }
        var calendarDaysAverage = {
            Column: "AVG_CAL_DAYS", Name: "CalendarDaysAverage", Divisor: "DELIVERED_PCS", Type: "AVG"
        }
        var signaturedPercent = {
            Column: "SIGNATURE_DELIVERED_PCS", Name: "SignatureDeliveredTotalPercent", Divisor: "SIGNATURE_PCS", Type: "PCT"
        };
        var signatureAvgPostallDays = {
            Column: "SIGNATURE_AVG_POSTAL_DAYS", Name: "SignatureAvgPostalDays", Divisor: "SIGNATURE_DELIVERED_PCS", Type: "AVG"
        }
        var signatureAvgCalDays = {
            Column: "SIGNATURE_AVG_CAL_DAYS", Name: "SignatureAvgCalDays", Divisor: "SIGNATURE_DELIVERED_PCS", Type: "AVG"
        }


        // begin uspsvisndelivery
        var day3Percent = {
            Column: "DAY3_PCS", Name: "Day3PCT", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var day4Percent = {
            Column: "DAY4_PCS", Name: "Day4PCT", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var day5Percent = {
            Column: "DAY5_PCS", Name: "Day5PCT", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var day6Percent = {
            Column: "DAY6_PCS", Name: "Day6PCT", Divisor: "TOTAL_PCS", Type: "PCT"
        }
        var delayedPercent = {
            Column: "DELAYED_PCS", Name: "DelayedPCT", Divisor: "TOTAL_PCS", Type: "PCT"
        }

        // all other reports reuse same columns thus far (6.9.2021)

        var listOfColumns = [noStcPercent, deliveredPercent,
            postalDaysAverage, calendarDaysAverage,
            signaturedPercent, signatureAvgPostallDays, signatureAvgCalDays,
            day3Percent, day4Percent, day5Percent, day6Percent, delayedPercent];

        calculateSummary(options, listOfColumns);
    }

    const onSelectionChanged = function (row) {
        var data = row.selectedRowsData[0];
        _packageStatus = data.Status;
        //if (_packageStatus == "RECALLED" || _packageStatus == "RELEASED") {
        $.ajax({
            url: "/RecallRelease/GetPackagesFromStatus",
            data: {
                subClient: encodeURI(_subClientList),
                packageStatus: encodeURI(_packageStatus),
                startDate: encodeURI(_manifestStartDate),
                endDate: encodeURI(_manifestEndDate)
            },
            type: "GET",
            success: function (response) {
                $("#reportsCardDetails").show();
                $("#packageReleaseGrid").dxDataGrid({
                    dataSource: response,
                    keyExpr: "PackageId"
                });
            }
        });
        //}
        var datagrid = row.component;
        var keys = datagrid.getSelectedRowKeys();
        datagrid.deselectRows(keys);
    }

    const getPackageStatus = function () {
        return _packageStatus;
    }

    const clearFilters = function () {
        $("#reportsGrid").dxDataGrid("instance").clearFilter();
    }

    function addZero(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    }

    const uspsMonthlyDeliveryPerformanceSummaryExport = function () {
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                exportFile();
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    const getUSPSMonthlyDeliveryPerformanceSummary = function () {
        debugger;
        _manifestDate = null;
        _subClientName = null;
        if (_subClientList.length == 0 || _manifestStartDate == "" || _manifestEndDate == "") {
            PPG.toastr.error('Please select all search criteria to run report.');
        } else {
            var days = CalculateDays(_manifestStartDate, _manifestEndDate);
            if (days <= 31) {
                DisableButtonForGridWithPromiseEnable("#runReportButton");
            } else {
                PPG.toastr.warning("Please select a range within 31 days.");
            }
        }
    }

    return {
        init: init,
        clearFilters: clearFilters,
        onRunDropPointReport: onRunDropPointReport,
        onRunDropPointByContainerReport: onRunDropPointByContainerReport,
        onSubClientValueChanged: onSubClientValueChanged,
        onSiteValueChanged: onSiteValueChanged,
        onManifestDateChanged: onManifestDateChanged,
        getSelectedSubClient: getSelectedSubClient,
        getSelectedSite: getSelectedSite,
        getManifestDate: getManifestDate,
        getSelectedSubClientList: getSelectedSubClientList,
        onTrackingNumberRender: onTrackingNumberRender,
        onRunDailyRevenue: onRunDailyRevenue,
        onRunUSPSUndeliverable: onRunUSPSUndeliverable,
        onRunDailyWarningReport: onRunDailyWarningReport,
        onRunPackageSummaryReport: onRunPackageSummaryReport,
        onRunClientPackageSummaryReport: onRunClientPackageSummaryReport,
        onRunPostPerformanceSummaryReport: onRunPostPerformanceSummaryReport,
        onRunWeeklyInvoiceReport: onRunWeeklyInvoiceReport,
        onRunUspsCarrierDetailReport: onRunUspsCarrierDetailReport,
        onRunCarrierDetailReport: onRunCarrierDetailReport,
        onRunUspsLocationDeliverySummary: onRunUspsLocationDeliverySummary,
        onRunUspsProductDeliverySummary: onRunUspsProductDeliverySummary,
        onRunUspsVisnDeliverySummary: onRunUspsVisnDeliverySummary,
        onRunUspsVisnTrackingSummary: onRunUspsVisnTrackingSummary,
        onRunUspsLocationTrackingSummary: onRunUspsLocationTrackingSummary,
        onManifestStartDateChanged: onManifestStartDateChanged,
        onRunUspsDailyPieceDetailReport: onRunUspsDailyPieceDetailReport,
        onRunUpsDailyPieceDetailReport: onRunUpsDailyPieceDetailReport,
        onRunFedExDailyPieceDetailReport: onRunFedExDailyPieceDetailReport,
        onRunRecallReleaseReport: onRunRecallReleaseReport,
        onSubClientListChanged: onSubClientListChanged,
        onRunUndeliveredReport: onRunUndeliveredReport,
        onRunGtr5Report: onRunGtr5Report,
        onRunAsnReconciliationReport: onRunAsnReconciliationReport,
        getManifestStartDate: getManifestStartDate,
        onManifestEndDateChanged: onManifestEndDateChanged,
        getManifestEndDate: getManifestEndDate,
        exportFile: exportFile,
        exporting: exporting,
        formatPackageHyperLink: formatPackageHyperLink,
        formatTrackingHyperLink: formatTrackingHyperLink,
        formatContainerHyperLink: formatContainerHyperLink,
        formatContainerTrackingHyperLink: formatContainerTrackingHyperLink,
        formatPercent: formatPercent,
        formatDecimal: formatDecimal,
        calculateCustomSummary: calculateCustomSummary,
        onSelectionChanged: onSelectionChanged,
        exportRecallRelease: exportRecallRelease,
        getPackageStatus: getPackageStatus,
        onRunBasicContainerPackageNestingReport: onRunBasicContainerPackageNestingReport,
        uspsCarrierDetailExportFile: uspsCarrierDetailExportFile,
        advancedDailyWarningExportFile: advancedDailyWarningExportFile,
        asnReconciliationExportFile: asnReconciliationExportFile,
        basicContainerPackageNestingExportFile: basicContainerPackageNestingExportFile,
        clientPackageSummaryExportFile: clientPackageSummaryExportFile,
        dailyRevenueExportFile: dailyRevenueExportFile,
        fedExDailyPieceDetailExportFile: fedExDailyPieceDetailExportFile,
        packageSummaryExportFile: packageSummaryExportFile,
        postalPerformanceSummaryExportFile: postalPerformanceSummaryExportFile,
        recallReleaseSummaryExportFile: recallReleaseSummaryExportFile,
        carrierDetailExportFile: carrierDetailExportFile,
        undeliveredReportExportFile: undeliveredReportExportFile,
        upsDailyPieceDetailExportFile: upsDailyPieceDetailExportFile,
        uspsDailyPieceDetailExportFile: uspsDailyPieceDetailExportFile,
        uspsDropPointStatusExportFile: uspsDropPointStatusExportFile,
        uspsDropPointStatusByContainerExportFile: uspsDropPointStatusByContainerExportFile,
        uspsGtr5ExportFile: uspsGtr5ExportFile,
        uspsLocationDeliverySummaryExportFile: uspsLocationDeliverySummaryExportFile,
        uspsLocationTrackingSummaryExportFile: uspsLocationTrackingSummaryExportFile,
        uspsProductDeliverySummaryExportFile: uspsProductDeliverySummaryExportFile,
        uspsUndeliverableExportFile: uspsUndeliverableExportFile,
        uspsVisnDeliverySummaryExportFile: uspsVisnDeliverySummaryExportFile,
        uspsVisnTrackingSummaryExportFile: uspsVisnTrackingSummaryExportFile,
        weeklyInvoiceExportFile: weeklyInvoiceExportFile,
        dailyContainerReportExportFile: dailyContainerReportExportFile,
        onRunDailyContainerReport: onRunDailyContainerReport,
        onRunDailyPieceDetailReport: onRunDailyPieceDetailReport,
        dailyPieceDetailExportFile: dailyPieceDetailExportFile,
        runAndExportButtonReenable: runAndExportButtonReenable,
        getUSPSMonthlyDeliveryPerformanceSummary: getUSPSMonthlyDeliveryPerformanceSummary,
        uspsMonthlyDeliveryPerformanceSummaryExport: uspsMonthlyDeliveryPerformanceSummaryExport
    };
}

PPG.reports = new PPG.Reports(jQuery);
