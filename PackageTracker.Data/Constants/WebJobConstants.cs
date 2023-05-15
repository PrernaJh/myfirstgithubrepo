namespace PackageTracker.Data.Constants
{
    public static class WebJobConstants
    {
        // job types

        // fsc end of day
        public const string ContainerDetailExportJobType = "CONTAINER_DETAIL";
        public const string PmodContainerDetailExportJobType = "PMOD_CONTAINER_DETAIL";
        public const string PackageDetailExportJobType = "PACKAGE_DETAIL";
        public const string ReturnAsnExportJobType = "RETURN_ASN";
        public const string UspsEvsExportJobType = "USPS_EVS";
        public const string UspsEvsPmodExportJobType = "USPS_EVS_PMOD";
        public const string EodPackageProcess = "EOD_PACKAGE";
        public const string EodContainerProcess = "EOD_CONTAINER";
        public const string SqlEodPackageProcess = "SQL_EOD_PACKAGE";
        public const string SqlEodContainerProcess = "SQL_EOD_CONTAINER";
        public const string EodPackageDuplicateCheck = "EOD_PACKAGE_DUPLICATE_CHECK";
        public const string EodContainerDuplicateCheck = "EOD_CONTAINER_DUPLICATE_CHECK";
        public const string InvoiceExportJobType = "INVOICE";
        public const string ExpenseExportJobType = "EXPENSE";
        public const string InvoiceWeeklyExportJobType = "INVOICE_WEEKLY";
        public const string ExpenseWeeklyExportJobType = "EXPENSE_WEEKLY";
        public const string InvoiceMonthlyExportJobType = "INVOICE_MONTHLY";
        public const string ExpenseMonthlyExportJobType = "EXPENSE_MONTHLY";
        public const string MonitorEodJobType = "MONITOR_EOD";
        public const string CheckEodJobType = "CHECK_EOD";
        public const string RunEodJobType = "RUN_EOD";

        // triggered jobs
        public const string PackageRecallJobType = "PACKAGE_RECALL";
        public const string UpdateServiceRuleGroupsType = "UPDATE_SERVICERULE_GROUPS";
        public const string UpdatePackageRates = "UPDATE_PACKAGE_RATES";
        public const string UpdateContainerRates = "UPDATE_CONTAINER_RATES";

        // timed jobs
        public const string AsnImportJobType = "ASN_IMPORT";
        public const string ConsumerDetailExportJobType = "CONSUMER_DETAIL";
        public const string RateAssignmentJobType = "RATE_ASSIGNMENT";
        public const string FedExTrackPackageJobType = "FEDEX_TRACKPACKAGE";
        public const string HistoricalDataJobType = "HISTORICAL_PACKAGE_IMPORT";
        public const string UpsTrackPackageJobType = "UPS_TRACKPACKAGE";
        public const string UspsTrackPackageJobType = "USPS_TRACKPACKAGE";
        public const string MonitorAsnFileImportJobType = "MONITOR_ASN";
        public const string CheckForDuplicateAsnsJobType = "CHECK_FOR_DUPLICATE_ASNS";
        public const string UpdatePackageBinsAndBinMapsJobType = "UPDATE_PACKAGE_BINS_AND_BINMAPS";
        public const string PackageArchiveJobType = "PACKAGE_ARCHIVE";
        public const string PostProcessCreatedPackagesJobType = "POST_PROCESS_CREATED_PACKAGES";
        public const string DailyContainerNestingReportGenerationJobType = "DAILY_CONTAINER_NESTING_REPORT_GENERATION";

        // domain imports
        public const string ServiceRuleExtensionImportJobType = "SERVICERULE_EXTENSION";
        public const string ServiceRuleImportJobType = "SERVICERULE";
        public const string RateFileImportJobType = "RATE";
        public const string RecallReleaseImportJobType = "RECALLRELEASE";
        public const string UpsGeoDescFileImportJobType = "UPSGEODESC";
        public const string UpsDasImportJobType = "UPSDAS";
        public const string ZipCarrierOverrideImportJobType = "ZIPCARRIEROVERRIDE";
        public const string ZipMapImportJobType = "ZIPMAP";
        public const string ZoneImportJobType = "ZONE";

        // import to reports DB
        public const string BinDatasetJobType = "BINDATASET";
        public const string JobDatasetJobType = "JOBDATASET";
        public const string PackageDatasetJobType = "PACKAGEDATASET";
        public const string EodPackageMonitorJobType = "EODPACKAGEMONITOR";
        public const string TrackPackageDatasetJobType = "TRACKPACKAGEDATASET";
        public const string ShippingContainerDatasetJobType = "SHIPPINGCONTAINERDATASET";
        public const string SubClientDatasetJobType = "SUBCLIENTDATASET";

        // import USPS files
        public const string UspsRegionsImportJobType = "IMPORT_USPS_REGIONS";
        public const string UspsHolidaysImportJobType = "IMPORT_USPS_HOLIDAYS";
        public const string UspsEvsCodesImportJobType = "IMPORT_USPS_EVSCODE";
        public const string UspsVisnSiteImportJobType = "IMPORT_USPS_VISNSITE";
    }
}
