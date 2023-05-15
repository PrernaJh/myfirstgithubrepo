using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
    public interface IPackageRepository : IRepository<Package>
    {
        // scans
        Task<Package> GetProcessedPackageByPackageId(string packageId, string siteName);
        Task<Package> GetImportedOrProcessedPackage(string packageId, string siteName);
        Task<Package> GetProcessedOrReleasedPackage(string packageId, string siteName);
        Task<Package> GetCreatedPackage(string packageId, string siteName);
        Task<Package> GetReturnPackage(string packageId, string siteName);
        Task<IEnumerable<Package>> GetPackagesByPackageId(string packageId, string siteName);
        Task<Package> GetPackageForConfirmParcelData(string packageId);

        // recall/release
        Task<IEnumerable<Package>> GetRecalledPackages(string subClientName);
        Task<IEnumerable<Package>> GetReleasedPackages(string subClientName);
        Task<IEnumerable<Package>> GetPackagesToRecallByPartial(string subClientName, string partialPackageId);
        Task<BatchDbResponse<Package>> UpdatePackagesForRecallRelease(IEnumerable<Package> packages);

        // eod
        Task<IEnumerable<Package>> GetPackagesEodOverview(string siteName, DateTime targetDatwe);
        Task<IEnumerable<Package>> GetPackagesForEndOfDayProcess(string siteName, DateTime lastScanDateTime);
        Task<IEnumerable<Package>> GetPackagesForSqlEndOfDayProcess(string siteName, DateTime targetDate, DateTime lastScanDateTime);
        Task<IEnumerable<Package>> GetPackagesForUspsEvsFile(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate);
        Task<IEnumerable<Package>> GetPackagesToResetEod(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate);
        Task<BatchDbResponse<Package>> UpdatePackagesEodProcessed(IEnumerable<Package> packages);
        Task<BatchDbResponse<Package>> UpdatePackagesSqlEodProcessed(IEnumerable<Package> packages);
        Task<BatchDbResponse<Package>> UpdatePackagesForRateUpdate(IEnumerable<Package> packages);
        Task<BatchDbResponse<Package>> UpdatePackagesForCreatePackagePostProcess(IEnumerable<Package> packages);

        // post
        Task<IEnumerable<Package>> GetPackagesForConsumerDetailFile(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime);
        Task<IEnumerable<Package>> GetPackagesForCreatePackagePostProcessing(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime);
        Task<IEnumerable<Package>> GetPackagesForPackageDatasetsAsync(string siteId, int firstTimestamp, int lastTimeStamp);
        Task<IEnumerable<int>> GetPackageTimeStampsAsync(string siteName, DateTime lastScanDateTime, DateTime nextScanDateTime);
        Task<bool> HavePackagesChangedForSiteAsync(string siteName, DateTime lastScanDateTime);
        Task<IEnumerable<Package>> GetPackagesForRateAssignment(string subClientName);
        Task<Package> GetPackageByTrackingNumberAndSiteName(string barcode, string siteName);
        Task<IEnumerable<Package>> GetPackagesForDuplicateAsnChecker(string siteName);
        Task<IEnumerable<Package>> GetImportedPackagesBySite(string siteName);
        Task<IEnumerable<Package>> GetPackagesWithOutdatedBinData(int daysToLookback, int maxCount, string subClientName, string binGroupId, string binMapGroupId);
        Task<BatchDbResponse<Package>> UpdatePackagesSetBinData(IEnumerable<Package> packages);
        Task<BatchDbResponse<Package>> UpdatePackagesSetServiceRuleGroupIds(IEnumerable<Package> packages);
        Task<IEnumerable<Package>> GetPackagesForRateUpdate(string subClientName, int daysToLookback);
        Task<IEnumerable<Package>> GetImportedOrReleasedPackagesBySubClient(string subClientName, int daysToLookback);
        Task<IEnumerable<Package>> GetPackagesByContainerAsync(string containerId, string siteName);
        Task<IEnumerable<Package>> GetPackageIdsByContainerAsync(string containerId, string siteName);
        Task<IEnumerable<Package>> GetProcessedPackagesByDate(DateTime targetDate,
            string clientName = null, string siteName = null, string subClientName = null, string shippingCarrier = null, int count = 0);
        Task<int> CountPackagesForContainerAsync(string containerId, string siteName);
        Task<BatchDbResponse<Package>> UpdatePackagesSetContainer(IEnumerable<Package> packages);
        Task<BatchDbResponse<Package>> UpdatePackagesSetIsSecondaryContainer(IEnumerable<Package> packages);
    }
}
