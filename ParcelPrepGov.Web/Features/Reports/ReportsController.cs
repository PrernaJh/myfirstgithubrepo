using AutoMapper;
using DevExpress.Compatibility.System.Web;
using DevExpress.Spreadsheet;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Reports.Utility;
using ParcelPrepGov.Web.Features.Reports.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkbookExtensions = ParcelPrepGov.Reports.Utility.WorkbookExtensions;

namespace ParcelPrepGov.Web.Features.Reports
{
    /// <summary>
    /// default to controller authorize all actions
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IMapper mapper;
        private readonly IReportsRepository reportsRepository;
        private readonly IPackageDatasetRepository packageDatasetRepository;
        private readonly ILogger<ReportsController> logger;

        public ReportsController(IReportsRepository reportsRepository, IMapper mapper,
            IPackageDatasetRepository packageDatasetRepository,
            ILogger<ReportsController> logger
            )
        {
            this.mapper = mapper;
            this.reportsRepository = reportsRepository;
            this.packageDatasetRepository = packageDatasetRepository;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var reportItems = GenerateReportList();
            return View(reportItems);
        }

        /// <summary>
        /// todo singleton
        /// get a static list of reports based on acl
        /// compound role access check
        /// </summary>
        /// <returns></returns>
        private List<ReportItem> GenerateReportList()
        {
            List<ReportItem> reportsList = new List<ReportItem>();

            if (User.IsClientWebAdministrator() || User.IsClientWebUser() ||
                User.IsSubClientWebAdministrator() || User.IsSubClientWebUser() ||
                User.IsClientWebFinancialUser())
            {
                if (User.GetClient().Equals("OPTUM"))
                {
                    reportsList.AddRange(
                        new List<ReportItem> {
                            new ReportItem(1, "Client Package Summary Report","Client Package Summary Report","ClientPackageSummaryReport"),               // getRptClientDailyPackageSummary.sql                            
                            new ReportItem(2, "Daily Piece Detail", "Daily Piece Detail", "DailyPieceDetailReport"),
                            new ReportItem(3, "No Stop-the-Clock Report", "No Stop-the-Clock Report", "UndeliveredReport"),
                            new ReportItem(4, "USPS Greater Than 5 Days Report", "USPS Greater Than 5 Days Report", "UspsGtr5Report"),
                            new ReportItem(5, "Postal Performance Summary", "Postal Performance Summary", "PostalPerformanceSummary"),
                            new ReportItem(6, "ASN Reconciliation Report", "ASN Reconciliation Report", "AsnReconciliationReport") // getRptASNReconcilationDetail_master.sql
                        });
                }
                else
                {
                    reportsList.AddRange(
                    new List<ReportItem> {
                        new ReportItem(1, "Postal Performance Summary", "Postal Performance Summary", "PostalPerformanceSummary"),
                        new ReportItem(2, "USPS Carrier Detail", "USPS Carrier Detail", "UspsCarrierDetail"),
                        new ReportItem(3, "USPS Drop Point Status", "USPS Drop Point Status", "UspsDropPointStatus"),
                        new ReportItem(4, "Daily Piece Detail", "Daily Piece Detail", "DailyPieceDetailReport"),                        
                        new ReportItem(7, "USPS Undeliverable Report", "USPS Undeliverable Report", "UspsUndeliverable"),
                        new ReportItem(8, "No Stop-the-Clock Report", "No Stop-the-Clock Report", "UndeliveredReport"),
                        new ReportItem(9, "USPS Greater Than 5 Days Report", "USPS Greater Than 5 Days Report", "UspsGtr5Report"),
                        new ReportItem(10, "USPS Delivery Summary by Location", "Usps Delivery Summary by Location", "UspsLocationDeliverySummary"),
                        new ReportItem(11, "USPS Delivery Summary by Product", "Usps Delivery Summary by Product", "UspsProductDeliverySummary"),
                        new ReportItem(12, "USPS Delivery Summary by VISN", "Usps Delivery Summary by VISN", "UspsVisnDeliverySummary"),
                        new ReportItem(13, "USPS Delivery Tracking by Location", "Usps Delivery Tracking by Location", "UspsLocationTrackingSummary"),
                        new ReportItem(14, "USPS Delivery Tracking by VISN", "Usps Delivery Tracking by VISN", "UspsVisnTrackingSummary"),
                        new ReportItem(15, "Recall / Release Summary", "Recall / Release Summary", "RecallReleaseSummary"),
                        new ReportItem(16, "ASN Reconciliation Report", "ASN Reconciliation Report", "AsnReconciliationReport"), // getRptASNReconcilationDetail_master.sql
                        new ReportItem(17, "Client Package Summary Report","Client Package Summary Report","ClientPackageSummaryReport") // getRptClientDailyPackageSummary.sql
                    }
                );
                }                
            }
            else if (User.IsCustomerService())
            {
                reportsList.AddRange(
                    new List<ReportItem> {
                        new ReportItem(1, "No Stop-the-Clock Report", "No Stop-the-Clock Report", "UndeliveredReport"),
                        new ReportItem(2, "Postal Performance Summary", "Postal Performance Summary", "PostalPerformanceSummary"),
                        new ReportItem(3, "USPS Carrier Detail", "USPS Carrier Detail", "UspsCarrierDetail"),
                        new ReportItem(4, "Daily Piece Detail", "Daily Piece Detail", "DailyPieceDetailReport"),                        
                        new ReportItem(5, "USPS Drop Point Status", "USPS Drop Point Status", "UspsDropPointStatus"),
                        new ReportItem(6, "USPS Drop Point Status By Container", "USPS Drop Point Status By Container", "UspsDropPointStatusByContainer"),  // getRptUspsDropPointStatusByContainer_master.sql, getRptUspsDropPointStatusByContainer_detail.sql
                        new ReportItem(7, "Basic Container Package Nesting Report", "Basic Container Package Nesting Report", "BasicContainerPackageNesting"), //getRptBasicContainerPackageNesting.sql
                        new ReportItem(8, "Daily Container Report", "Daily Container Report", "DailyContainerReport"),  // getRptDailyContainer_master.sql, getRptDailyContainer_master.sql
                        new ReportItem(9, "Carrier Report", "Carrier Report", "CarrierReport")              // getRptClientDailyPackageSummary.sql
                    });
            }

            if (User.IsAdministrator() || User.IsSystemAdministrator() || 
                User.IsTransportationUser() || User.IsGeneralManager() || 
                User.IsFscWebFinancialUser() || User.IsSupervisor())
            {
                reportsList.AddRange(
                    new List<ReportItem>(){
                        new ReportItem(1, "Postal Performance Summary","Postal Performance Summary","PostalPerformanceSummary"),     // getRptPostalPerformanceSummary_master.sql
                        new ReportItem(2, "USPS Carrier Detail","USPS Carrier Detail","UspsCarrierDetail"),                          // getRptUspsCarrierDetail_master.sql, getRptUspsCarrierDetail_detail.sql
                        new ReportItem(3, "USPS Drop Point Status","USPS Drop Point Status","UspsDropPointStatus"),                  // getRptUspsDropPointStatus_master.sql, getRptUspsDropPointStatus_detail.sql                        
                        new ReportItem(4, "Daily Piece Detail", "Daily Piece Detail", "DailyPieceDetailReport"),
                        new ReportItem(7, "USPS Undeliverable Report","USPS Undeliverable Report", "UspsUndeliverable"),             // getRptUspsUndeliverables_master.sql, getRptUspsUndeliverables_detail.sql
                        new ReportItem(8, "No Stop-the-Clock Report", "No Stop-the-Clock Report", "UndeliveredReport"),              // getRptUndeliveredReport.sql
                        new ReportItem(9, "USPS Greater Than 5 Days Report", "USPS Greater Than 5 Days Report", "UspsGtr5Report"),   // getRptUspsGtr5Detail.sql
                        new ReportItem(10, "Package Summary Report","Package Summary Report","PackageSummaryReport"),                 // getRptDailyPackageSummary.sql 
                        new ReportItem(11, "Client Package Summary Report","Client Package Summary Report","ClientPackageSummaryReport"),                 // getRptClientDailyPackageSummary.sql
                        new ReportItem(12, "USPS Delivery Summary by Location", "Usps Delivery Summary by Location", "UspsLocationDeliverySummary"),    // getRptUspsLocationDeliverySummary.sql
                        new ReportItem(13, "USPS Delivery Summary by Product", "Usps Delivery Summary by Product", "UspsProductDeliverySummary"),       // getRptUspsProductDeliverySummary.sql
                        new ReportItem(14, "USPS Delivery Summary by VISN", "Usps Delivery Summary by VISN", "UspsVisnDeliverySummary"),                // getRptUspsVisnDeliverySummary.sql
                        new ReportItem(15, "USPS Delivery Tracking by Location", "Usps Delivery Tracking by Location", "UspsLocationTrackingSummary"),  // getRptUspsLocationTrackingSummary.sql
                        new ReportItem(16, "USPS Delivery Tracking by VISN", "Usps Delivery Tracking by VISN", "UspsVisnTrackingSummary"),              // getRptUspsVisnTrackingSummary.sql
                        new ReportItem(17, "Recall / Release Summary", "Recall / Release Summary", "RecallReleaseSummary"),
                        new ReportItem(18, "ASN Reconciliation Report", "ASN Reconciliation Report", "AsnReconciliationReport"), // getRptASNReconcilationDetail_master.sql
                        new ReportItem(19, "Carrier Report", "Carrier Report", "CarrierReport"),
                        new ReportItem(20, "Basic Container Package Nesting Report", "Basic Container Package Nesting Report", "BasicContainerPackageNesting"), //getRptBasicContainerPackageNesting.sql
                        new ReportItem(21, "USPS Drop Point Status By Container","USPS Drop Point Status By Container","UspsDropPointStatusByContainer"),  // getRptUspsDropPointStatusByContainer_master.sql, getRptUspsDropPointStatusByContainer_detail.sql
                        new ReportItem(22, "Daily Container Report","Daily Container Report","DailyContainerReport")  // getRptDailyContainer_master.sql, getRptDailyContainer_master.sql                        
                    }
                );
            }

            if(User.IsAdministrator() || User.IsSystemAdministrator())
            {
                reportsList.Add(
                    new ReportItem(23, "USPS Monthly Delivery Performance Summary", "USPS Monthly Delivery Performance Summary", "USPSMonthlyDeliveryPerformanceSummary")
                );
            }

            return reportsList;
        }

        public IActionResult Get(string id)
        {
            return View(id);
        }

        /// <summary>
        /// export data to xlsx 
        /// </summary>        
        [HttpGet]
        public async Task<IActionResult> ExportRecall(string subClient, string startDate, string endDate)
        {
            try
            {
                Workbook workbook = new Workbook();
                Worksheet recallSheet = workbook.Worksheets[0];
                recallSheet.Name = "Recalled Released Packages";
                workbook.Unit = DevExpress.Office.DocumentUnit.Point;
                // get recalled packages and convert to datatable to get header info
                var data = await packageDatasetRepository.GetRecallReleasePackages(subClient, startDate, endDate);
                recallSheet.Import(data.ToDataTable<PackageFromStatus>(), true, 1, 0);
                // save as binary to return mvc file
                byte[] doc = await workbook.SaveDocumentAsync(DocumentFormat.Xlsx);
                string fileName = $"{DateTime.Now.Date:yyyyMMddHHmmss}_{"RecallRelease"}_{DateTime.Now.Date:yyyyMMdd}";
                return File(doc, "application/ms-excel", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in Export Recall/Release is : {ex.Message.Substring(0, 100)}");
                throw;
            }
        }

        private async Task<IActionResult> ExportSpreadsheetAsync<T1, T2>(string host, string reportId, string locations, string startDate, string endDate,
            IEnumerable<T1> masterDataSource, IEnumerable<T2> detailsDataSource = null, bool addTotalsToMaster = false, bool addTotalsToDetail = false)
        {
            var role = User.GetRoles().FirstOrDefault();
            var workbook = WorkbookExtensions.CreateWorkbook();
            var workSheetIndex = 0;
            var workSheetsProcessed = 0;
            workbook.ImportDataToWorkSheets(ref workSheetIndex, masterDataSource);
            while (workSheetsProcessed < workbook.Worksheets.Count())
            {
                workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<T1>(host, role, addTotalsToMaster);
            }

            if (detailsDataSource != null)
            {
                workbook.ImportDataToWorkSheets(ref workSheetIndex, detailsDataSource);
                while (workSheetsProcessed < workbook.Worksheets.Count())
                {
                    workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<T2>(host, role, addTotalsToDetail);
                }
            }

            workbook.Calculate();
            byte[] doc = await workbook.SaveDocumentAsync(DocumentFormat.Xlsx);
            DateTime.TryParse(startDate, out var date1);
            DateTime.TryParse(endDate, out var date2);
            var now = DateTime.Now;
            string fileName = $"{now.Date:yyyyMMddHHmmss}_{reportId}_{date1.Date:yyyyMMdd}";

            if (date1 != date2)
            {
                fileName += $"_{date2.Date:yyyyMMdd}";
            }

            fileName += $"_{reportId}.xlsx";

            return base.File(doc, "application/ms-excel", fileName);
        }

        private async Task<IActionResult> ExportAdvancedDailyWarningAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            AdvancedDailyWarningDetail.HOST = host;
            return await ExportSpreadsheetAsync<AdvancedDailyWarningMaster, AdvancedDailyWarningDetail>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetAdvancedDailyWarningDetailMasters(subClientNames, startDate, endDate),
                await reportsRepository.GetAdvancedDailyWarningDetailDetails(subClientNames, startDate, endDate)
            );
        }

        private async Task<IActionResult> ExportDailyRevenueFileAsync(string host, string reportId, string subClientNames, string manifestDate)
        {
            return await ExportSpreadsheetAsync<DailyRevenueFile, DailyRevenueFile>(host,
                reportId, subClientNames, manifestDate, manifestDate,
                await reportsRepository.GetDailyRevenueFile(subClientNames, manifestDate)
            );
        }

        private async Task<IActionResult> ExportPostalPerformanceSummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate,
            IDictionary<string, string> filterBy)
        {
            return await ExportSpreadsheetAsync<PostalPerformanceSummary, PostalPerformanceSummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetPostalPerformanceSummary(subClientNames, startDate, endDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsCarrierDetailAsync(string host, string reportId, string subClientName, string startDate, string endDate,
            IDictionary<string, string> filterBy)
        {
            return await ExportSpreadsheetAsync<UspsCarrierDetailMaster, UspsCarrierDetailDetail>(host,
                reportId, subClientName, startDate, endDate,
                await reportsRepository.GetUspsCarrierDetailMaster(subClientName, startDate, endDate, filterBy),
                await reportsRepository.GetUspsCarrierDetailDetails(subClientName, startDate, endDate, null, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsDropPointStatusAsync(string host, string reportId, string subClientNames, string startDate, string endDate,
           IDictionary<string, string> filterBy)
         {
            UspsDropPointStatusDetail.HOST = host;
            return await ExportSpreadsheetAsync<UspsDropPointStatusMaster, UspsDropPointStatusDetail>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsDropPointStatusMaster(subClientNames, startDate, endDate, filterBy),
                await reportsRepository.GetUspsDropPointStatusDetails(subClientNames, startDate, endDate, null, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsDropPointStatusByContainerAsync(string host, string reportId, string siteName, string startDate, string endDate,
           IDictionary<string, string> filterBy)
        {
            UspsDPSByContainerDetail.HOST = host;
            return await ExportSpreadsheetAsync<UspsDPSByContainerMaster, UspsDPSByContainerDetail>(host,
                reportId, siteName, startDate, endDate,
                await reportsRepository.GetUspsDropPointStatusByContainerMaster(siteName, startDate, endDate, filterBy),
                await reportsRepository.GetUspsDropPointStatusByContainerDetails(siteName, startDate, endDate, null, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsDailyPieceDetailReportAsync(string host, string reportId, string subClientNames, string manifestDate, 
            IDictionary<string, string> filterBy)
        {
            UspsDailyPieceDetail.HOST = host;
            return await ExportSpreadsheetAsync<UspsDailyPieceDetail, UspsDailyPieceDetail>(host,
                reportId, subClientNames, manifestDate, manifestDate,
                await reportsRepository.GetUspsDailyPieceDetail(subClientNames, manifestDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportUpsDailyPieceDetailReportAsync(string host, string reportId, string subClientNames, string manifestDate,
            IDictionary<string, string> filterBy)
        {
            UpsDailyPieceDetail.HOST = host;
            return await ExportSpreadsheetAsync<UpsDailyPieceDetail, UpsDailyPieceDetail>(host,
                reportId, subClientNames, manifestDate, manifestDate,
                await reportsRepository.GetUpsDailyPieceDetail(subClientNames, manifestDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportDailyPieceDetailReportAsync(string host, string reportId, string subClientNames, string manifestDate,
       IDictionary<string, string> filterBy)
        {
            DailyPieceDetail.HOST = host;
            return await ExportSpreadsheetAsync<DailyPieceDetail, DailyPieceDetail>(host,
                reportId, subClientNames, manifestDate, manifestDate,
                await reportsRepository.GetDailyPieceDetail(subClientNames, manifestDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportFedExDailyPieceDetailReportAsync(string host, string reportId, string subClientNames, string manifestDate,
            IDictionary<string, string> filterBy)
        {
            FedExDailyPieceDetail.HOST = host;
            return await ExportSpreadsheetAsync<FedExDailyPieceDetail, FedExDailyPieceDetail>(host,
                reportId, subClientNames, manifestDate, manifestDate,
                await reportsRepository.GetFedExDailyPieceDetail(subClientNames, manifestDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportWeeklyInvoiceFileAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<WeeklyInvoiceFile, WeeklyInvoiceFile>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetWeeklyInvoiceFile(subClientNames, startDate, endDate)
            );
        }

        private async Task<IActionResult> ExportPackageSummaryReportAsync(string host, string reportId, string siteName, string manifestDate)
        {
            return await ExportSpreadsheetAsync<DailyPackageSummary, DailyPackageSummary>(host,
                reportId, siteName, manifestDate, manifestDate,
                await reportsRepository.GetDailyPackageSummary(siteName, manifestDate)
            );
        }

        private async Task<IActionResult> ExportClientPackageSummaryReportAsync(string host, string reportId, string siteName, string manifestDate)
        {
            return await ExportSpreadsheetAsync<ClientDailyPackageSummary, ClientDailyPackageSummary>(host,
                reportId, siteName, manifestDate, manifestDate,
                await reportsRepository.GetClientDailyPackageSummary(siteName, manifestDate)
            );
        }

        private async Task<IActionResult> ExportUspsUndeliverableAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            UspsUndeliverableDetail.HOST = host;
            return await ExportSpreadsheetAsync<UspsUndeliverableMaster, UspsUndeliverableDetail>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsUndeliverableMaster(subClientNames, startDate, endDate),
                await reportsRepository.GetUspsUndeliverableDetails(subClientNames, startDate, endDate)
            );
        }

        private async Task<IActionResult> ExportUndeliveredReportAsync(string host, string reportId, string subClientNames, string startDate, string endDate,
           IDictionary<string, string> filterBy)
        {
            Undelivered.HOST = host;
            return await ExportSpreadsheetAsync<Undelivered, Undelivered>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetPostalPerformanceNoStc(subClientNames, startDate, endDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsGtr5DetailAsync(string host, string reportId, string subClientNames, string startDate, string endDate,
            IDictionary<string, string> filterBy)
        {
            UspsGtr5Detail.HOST = host;
            return await ExportSpreadsheetAsync<UspsGtr5Detail, UspsGtr5Detail>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsGtr5Details(subClientNames, startDate, endDate, filterBy)
            );
        }

        private async Task<IActionResult> ExportUspsLocationDeliverySummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<UspsLocationDeliverySummary, UspsLocationDeliverySummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsLocationDeliverySummary(subClientNames, startDate, endDate),
                null, true
            );
        }

        private async Task<IActionResult> ExportUspsProductDeliverySummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<UspsProductDeliverySummary, UspsProductDeliverySummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsProductDeliverySummary(subClientNames, startDate, endDate),
                null, true
            );
        }

        private async Task<IActionResult> ExportUspsVisnDeliverySummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<UspsVisnDeliverySummary, UspsVisnDeliverySummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsVisnDeliverySummary(subClientNames, startDate, endDate),
                null, true
            );
        }

        private async Task<IActionResult> ExportUspsLocationTrackingSummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<UspsLocationTrackingSummary, UspsLocationTrackingSummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsLocationTrackingSummary(subClientNames, startDate, endDate),
                null, true
            );
        }

        private async Task<IActionResult> ExportUspsVisnTrackingSummaryAsync(string host, string reportId, string subClientNames, string startDate, string endDate)
        {
            return await ExportSpreadsheetAsync<UspsVisnTrackingSummary, UspsVisnTrackingSummary>(host,
                reportId, subClientNames, startDate, endDate,
                await reportsRepository.GetUspsVisnTrackingSummary(subClientNames, startDate, endDate),
                null, true
            );
        }

        private async Task<IActionResult> ExportCarrierDetailReportAsync(string host, string reportId, string siteName, string startDate, string endDate,
           IDictionary<string, string> filterBy)
        {
            return await ExportSpreadsheetAsync<CarrierDetail, CarrierDetail>(host,
                reportId, siteName, startDate, endDate,
                await reportsRepository.GetCarrierDetail(siteName, startDate, endDate, filterBy));
        }

        private async Task<IActionResult> ExportAsnReconciliationReportAsync(string host, string reportId, string subClientName, string startDate, string endDate)
        {
            AsnReconciliationDetailMaster.HOST = host;
            return await ExportSpreadsheetAsync<AsnReconciliationDetailMaster, AsnReconciliationDetailMaster>(host,
                reportId, subClientName, startDate, endDate,
                await reportsRepository.GetAsnReconciliationDetailMaster(subClientName, startDate, endDate),
                null
            );
        }

        private async Task<IActionResult> ExportBasicContainerPackageNestingReportAsync(string host, string reportId, string siteName, string manifestDate, IDictionary<string, string> filterBy)
        {
            BasicContainerPackageNesting.HOST = host;
            return await ExportSpreadsheetAsync<BasicContainerPackageNesting, BasicContainerPackageNesting>(host,
                reportId, siteName, manifestDate, manifestDate,
                await reportsRepository.GetBasicContainerPackageNesting(siteName, manifestDate, filterBy)
            );
        }
        
        private async Task<IActionResult> ExportUSPSMonthlyDeliveryPerformanceSummary(string host, string reportId,
            string subclientNames, string startDate, string endDate, IDictionary<string, string> filterBy)
        {
            //USPSMonthlyDeliveryPerformanceSummary.HOST = host; for when you need urls in the excel
            return await ExportSpreadsheetAsync<USPSMonthlyDeliveryPerformanceSummary, USPSMonthlyDeliveryPerformanceSummary>(host,
                reportId, subclientNames, startDate, endDate,
                await reportsRepository.GetUSPSMonthlyDeliveryPerformanceSummary(subclientNames, startDate, endDate)// filterBy
            );
        }

        private async Task<IActionResult> ExportDailyContainerReportAsync(string host, string reportId, string siteName, string manifestDate, IDictionary<string, string> filterBy)
        {
            DailyContainerDetail.HOST = host;
            DailyContainerMaster.HOST = host;
            return await ExportSpreadsheetAsync<DailyContainerMaster, DailyContainerDetail>(host,
                reportId, siteName, manifestDate, manifestDate,
                await reportsRepository.GetDailyContainerMaster(siteName, manifestDate, filterBy),
                await reportsRepository.GetDailyContainerDetails(siteName, manifestDate, null, filterBy)
            );
        }

        [HttpGet]
        public async Task<IActionResult> Export(string reportName, string siteName, string subClientNames, string subClientName, string manifestDate, string startDate, string endDate, string filterBy)
        {
            var filterByColumns = FilterObject.ParseFilterString(filterBy);
            var report = GenerateReportList().FirstOrDefault(r => r.ReportName == reportName);

            if(report == null)
            {
                report = GenerateReportList().FirstOrDefault(r => r.ReportName.Contains(reportName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (report != null)
            {
                if (report.UpdateID == "AdvancedDailyWarning")
                    return await ExportAdvancedDailyWarningAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "DailyRevenueFile")
                    return await ExportDailyRevenueFileAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate);
                else if (report.UpdateID == "PostalPerformanceSummary")
                    return await ExportPostalPerformanceSummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "UspsCarrierDetail")
                    return await ExportUspsCarrierDetailAsync(Request.Host.Value, report.UpdateID, subClientName, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "UspsDropPointStatus")
                    return await ExportUspsDropPointStatusAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "UspsDailyPieceDetailReport")
                    return await ExportUspsDailyPieceDetailReportAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate, filterByColumns);
                else if (report.UpdateID == "DailyPieceDetailReport")
                    return await ExportDailyPieceDetailReportAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate, filterByColumns);
                else if (report.UpdateID == "UpsDailyPieceDetailReport")
                    return await ExportUpsDailyPieceDetailReportAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate, filterByColumns);
                else if (report.UpdateID == "FedExDailyPieceDetailReport")
                    return await ExportFedExDailyPieceDetailReportAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate, filterByColumns);
                else if (report.UpdateID == "WeeklyInvoiceFile")
                    return await ExportWeeklyInvoiceFileAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "PackageSummaryReport")
                    return await ExportPackageSummaryReportAsync(Request.Host.Value, report.UpdateID, siteName, manifestDate);
                else if (report.UpdateID == "ClientPackageSummaryReport")
                    return await ExportClientPackageSummaryReportAsync(Request.Host.Value, report.UpdateID, subClientNames, manifestDate);
                else if (report.UpdateID == "UspsUndeliverable")
                    return await ExportUspsUndeliverableAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "UndeliveredReport")
                    return await ExportUndeliveredReportAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "UspsGtr5Report")
                    return await ExportUspsGtr5DetailAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "UspsLocationDeliverySummary")
                    return await ExportUspsLocationDeliverySummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "UspsProductDeliverySummary")
                    return await ExportUspsProductDeliverySummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "UspsVisnDeliverySummary")
                    return await ExportUspsVisnDeliverySummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "UspsLocationTrackingSummary")
                    return await ExportUspsLocationTrackingSummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "UspsVisnTrackingSummary")
                    return await ExportUspsVisnTrackingSummaryAsync(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate);
                else if (report.UpdateID == "CarrierReport")
                    return await ExportCarrierDetailReportAsync(Request.Host.Value, report.UpdateID, siteName, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "AsnReconciliationReport")
                    return await ExportAsnReconciliationReportAsync(Request.Host.Value, report.UpdateID, subClientName, startDate, endDate);
                else if (report.UpdateID == "BasicContainerPackageNesting")
                    return await ExportBasicContainerPackageNestingReportAsync(Request.Host.Value, report.UpdateID, siteName, manifestDate, filterByColumns);
                else if (report.UpdateID == "UspsDropPointStatusByContainer")
                    return await ExportUspsDropPointStatusByContainerAsync(Request.Host.Value, report.UpdateID, siteName, startDate, endDate, filterByColumns);
                else if (report.UpdateID == "DailyContainerReport")
                    return await ExportDailyContainerReportAsync(Request.Host.Value, report.UpdateID, siteName, manifestDate, filterByColumns);
                else if (report.UpdateID == "USPSMonthlyDeliveryPerformanceSummary")
                    return await ExportUSPSMonthlyDeliveryPerformanceSummary(Request.Host.Value, report.UpdateID, subClientNames, startDate, endDate, null);

            }
            return Ok();
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetAdvancedDailyWarningData))]
        public async Task<JsonResult> GetAdvancedDailyWarningData([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var masters = await reportsRepository.GetAdvancedDailyWarningDetailMasters(subClientNames, startDate, endDate);
            return new JsonResult(masters);
        }
        [AjaxOnly]
        [HttpGet(Name = nameof(GetAdvancedDailyWarningDetails))]
        public async Task<JsonResult> GetAdvancedDailyWarningDetails([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetAdvancedDailyWarningDetailDetails(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }
        [AjaxOnly]
        [HttpGet(Name = nameof(GetAdvancedDailyWarningDetailBySingleId))]
        public async Task<JsonResult> GetAdvancedDailyWarningDetailBySingleId([FromQuery] string subClientNames, string manifestDate, string id)
        {
            var details = await reportsRepository.GetAdvancedDailyWarningDetailDetails(subClientNames, manifestDate, manifestDate, id);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetDailyRevenueFile))]
        public async Task<JsonResult> GetDailyRevenueFile([FromQuery] string subClientNames, string manifestDate)
        {
            var details = await reportsRepository.GetDailyRevenueFile(subClientNames, manifestDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetPostalPerformanceSummaryMaster))]
        public async Task<JsonResult> GetPostalPerformanceSummaryMaster(string subClientNames, string startDate, string endDate)
        {
            var postalPerformanceSummaryList = await reportsRepository.GetPostalPerformanceSummary(subClientNames, startDate, endDate);
            return new JsonResult(postalPerformanceSummaryList);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsGtr5Details))]
        public async Task<JsonResult> GetUspsGtr5Details(string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsGtr5Details(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetPostalPerformanceSummaryDetailsBySingleId))]
        public async Task<JsonResult> GetPostalPerformanceSummaryDetailsBySingleId([FromQuery] string id)
        {
            var postalPerformanceSummaryDetail = new PostalPerformanceSummaryDetail();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var threeDigitsAndFiveDigits = await reportsRepository.GetPostalPerformanceSummary3DigitAnd5Digit(id);

                foreach (var threeDigit in threeDigitsAndFiveDigits.ThreeDigitDetails)
                {
                    postalPerformanceSummaryDetail.PostalPerformance3Digit = threeDigit;

                    var fiveDigits = new List<PostalPerformance5Digit>();
                    var fiveDigitsMatched = threeDigitsAndFiveDigits.FiveDigitDetails.Where(x => x.ID3 == threeDigit.ID3);

                    foreach (var fiveDigitReport in fiveDigitsMatched)
                    {
                        fiveDigits.Add(mapper.Map<PostalPerformance5Digit>(fiveDigitReport));
                    }

                    postalPerformanceSummaryDetail.PostalPerformance5Digit.AddRange(fiveDigits);
                }
            }

            return new JsonResult(postalPerformanceSummaryDetail);
        }
        [AjaxOnly]
        [HttpGet(Name = nameof(GetPostalPerformanceGtr6))]
        public async Task<JsonResult> GetPostalPerformanceGtr6([FromQuery] string siteName, string startDate, string endDate, string uspsRegion, string entryType)
        {
            var postalPerformanceGtr6List = await reportsRepository.GetPostalPerformanceGtr6(siteName, startDate, endDate);
            return new JsonResult(postalPerformanceGtr6List);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsCarrierDetailData))]
        public async Task<JsonResult> GetUspsCarrierDetailData([FromQuery] string subClientName, string startDate, string endDate)
        {
            var masters = await reportsRepository.GetUspsCarrierDetailMaster(subClientName, startDate, endDate);
            return new JsonResult(masters);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUSPSMonthlyDeliveryPerformanceSummary))]
        public async Task<JsonResult> GetUSPSMonthlyDeliveryPerformanceSummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var masters = await reportsRepository.GetUSPSMonthlyDeliveryPerformanceSummary(subClientNames, startDate, endDate);
            return new JsonResult(masters);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsCarrierDetailDetails))]
        public async Task<JsonResult> GetUspsCarrierDetailDetails([FromQuery] string subClientName, string startDate, string endDate)
        {
            var uspsCarrierDetailDetails = await reportsRepository.GetUspsCarrierDetailDetails(subClientName, startDate, endDate);
            return new JsonResult(uspsCarrierDetailDetails);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsCarrierDetailBySingleId))]
        public async Task<JsonResult> GetUspsCarrierDetailBySingleId([FromQuery] string subClientName, string startDate, string endDate, string id)
        {
            var uspsCarrierDetailDetails = await reportsRepository.GetUspsCarrierDetailDetails(subClientName, startDate, endDate, id);
            return new JsonResult(uspsCarrierDetailDetails);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDropPointStatusReport))]
        public async Task<JsonResult> GetUspsDropPointStatusReport([FromQuery] string subClientNames, string startDate, string endDate)
        {
            //if (string.IsNullOrEmpty(subClientNames))
            //    return new JsonResult(null);
            var masters = await reportsRepository.GetUspsDropPointStatusMaster(subClientNames, startDate, endDate);
            return new JsonResult(masters);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDropPointStatusDetails))]
        public async Task<JsonResult> GetUspsDropPointStatusDetails([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details =
                await reportsRepository.GetUspsDropPointStatusDetails(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDropPointStatusDetailsBySingleId))]
        public async Task<JsonResult> GetUspsDropPointStatusDetailsBySingleId([FromQuery] string subClientNames, string manifestDate, string id)
        {
            var details =
                await reportsRepository.GetUspsDropPointStatusDetails(subClientNames, manifestDate, manifestDate, id);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDropPointStatusByContainerReport))]
        public async Task<JsonResult> GetUspsDropPointStatusByContainerReport([FromQuery] string siteName, string startDate, string endDate)
        {
            var byContainers = await reportsRepository.GetUspsDropPointStatusByContainerMaster(siteName, startDate, endDate);
            return new JsonResult(byContainers);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDropPointStatusByContainerDetailsBySingleId))]
        public async Task<JsonResult> GetUspsDropPointStatusByContainerDetailsBySingleId([FromQuery] string siteName, string startDate, string endDate, string id)
        {
            var details =
                await reportsRepository.GetUspsDropPointStatusByContainerDetails(siteName, startDate, endDate, id);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetPostalPerformanceNoStc))]
        public async Task<JsonResult> GetPostalPerformanceNoStc([FromQuery] string siteName, string startDate, string endDate, string uspsRegion, string entryType)
        {
            var postalPerformanceNoStcList = await reportsRepository.GetPostalPerformanceNoStc(siteName, startDate, endDate);
            return new JsonResult(postalPerformanceNoStcList);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsDailyPieceDetailReportData))]
        public async Task<JsonResult> GetUspsDailyPieceDetailReportData([FromQuery] string subClientNames, string manifestDate)
        {
            var reports = await reportsRepository.GetDailyPieceDetail(subClientNames, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetDailyPieceDetailReportData))]
        public async Task<JsonResult> GetDailyPieceDetailReportData([FromQuery] string subClientNames, string manifestDate)
        {
            var reports = await reportsRepository.GetDailyPieceDetail(subClientNames, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUpsDailyPieceDetailReportData))]
        public async Task<JsonResult> GetUpsDailyPieceDetailReportData([FromQuery] string subClientNames, string manifestDate)
        {
            var reports = await reportsRepository.GetUpsDailyPieceDetail(subClientNames, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetFedExDailyPieceDetailReportData))]
        public async Task<JsonResult> GetFedExDailyPieceDetailReportData([FromQuery] string subClientNames, string manifestDate)
        {
            var reports = await reportsRepository.GetFedExDailyPieceDetail(subClientNames, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetWeeklyInvoiceFile))]
        public async Task<JsonResult> GetWeeklyInvoiceFile([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetWeeklyInvoiceFile(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }


        [AjaxOnly]
        [HttpGet(Name = nameof(GetPackageSummaryReport))]
        public async Task<JsonResult> GetPackageSummaryReport([FromQuery] string siteName, string manifestDate)
        {
            var reports = await reportsRepository.GetDailyPackageSummary(siteName, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetClientPackageSummaryReport))]
        public async Task<JsonResult> GetClientPackageSummaryReport([FromQuery] string subClientNames, string manifestDate)
        {
            var reports = await reportsRepository.GetClientDailyPackageSummary(subClientNames, manifestDate);
            return new JsonResult(reports);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsUndeliverableReport))]
        public async Task<JsonResult> GetUspsUndeliverableReport([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var masters = await reportsRepository.GetUspsUndeliverableMaster(subClientNames, startDate, endDate);
            return new JsonResult(masters);
        }
        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsUndeliverableReportDetails))]
        public async Task<JsonResult> GetUspsUndeliverableReportDetails([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsUndeliverableDetails(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }
        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsUndeliverableReportDetailsBySingleId))]
        public async Task<JsonResult> GetUspsUndeliverableReportDetailsBySingleId([FromQuery] string subClientNames, string startDate, string endDate, string id)
        {
            var details = await reportsRepository.GetUspsUndeliverableDetails(subClientNames, startDate, endDate, id);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUndeliveredReport))]
        public async Task<JsonResult> GetUndeliveredReport([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetPostalPerformanceNoStc(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsLocationDeliverySummary))]
        public async Task<JsonResult> GetUspsLocationDeliverySummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsLocationDeliverySummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsProductDeliverySummary))]
        public async Task<JsonResult> GetUspsProductDeliverySummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsProductDeliverySummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsVisnDeliverySummary))]
        public async Task<JsonResult> GetUspsVisnDeliverySummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsVisnDeliverySummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsLocationTrackingSummary))]
        public async Task<JsonResult> GetUspsLocationTrackingSummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsLocationTrackingSummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetUspsVisnTrackingSummary))]
        public async Task<JsonResult> GetUspsVisnTrackingSummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetUspsVisnTrackingSummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetRecallReleaseSummary))]
        public async Task<JsonResult> GetRecallReleaseSummary([FromQuery] string subClientNames, string startDate, string endDate)
        {
            var details = await reportsRepository.GetRecallReleaseSummary(subClientNames, startDate, endDate);
            return new JsonResult(details);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetCarrierDetailData))]
        public async Task<JsonResult> GetCarrierDetailData([FromQuery] string siteName, string startDate, string endDate)
        {
            var data = await reportsRepository.GetCarrierDetail(siteName, startDate, endDate);
            return new JsonResult(data);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetAsnReconciliationDetailMaster))]
        public async Task<JsonResult> GetAsnReconciliationDetailMaster([FromQuery] string subClientName, string startDate, string endDate)
        {
            var asnReconciliationData = await reportsRepository.GetAsnReconciliationDetailMaster(subClientName, startDate, endDate);
            return new JsonResult(asnReconciliationData);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetBasicContainerPackageNesting))]
        public async Task<JsonResult> GetBasicContainerPackageNesting([FromQuery] string siteName, string manifestDate)
        {
            var basicContainerPackageNesting = await reportsRepository.GetBasicContainerPackageNesting(siteName, manifestDate);
            return new JsonResult(basicContainerPackageNesting);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetDailyContainerReport))]
        public async Task<JsonResult> GetDailyContainerReport([FromQuery] string siteName, string manifestDate)
        {
            var byContainers = await reportsRepository.GetDailyContainerMaster(siteName, manifestDate);
            return new JsonResult(byContainers);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetDailyContainerReportDetailsBySingleId))]
        public async Task<JsonResult> GetDailyContainerReportDetailsBySingleId([FromQuery] string siteName, string manifestDate, string id)
        {
            var details =
                await reportsRepository.GetDailyContainerDetails(siteName, manifestDate, id);
            return new JsonResult(details);
        }
    }
}

