using PackageTracker.Data.Models;
using PackageTracker.Data.Models.Archive;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface IPackageDatasetRepository
	{
		Task<PackageDataset> GetPackageDatasetAsync(string packageIdOrBarcode);
		Task<IList<PackageDataset>> GetPackageDatasetsForSearchAsync(string packageIdOrBarcodes);
		Task<IList<PackageDataset>> GetPackageDatasetsForContainerSearchAsync(string containerIdOrBarcode, string subclientName);
		Task<IList<PackageSearchExportModel>> GetPackageDataForExportAsync(string packageIdOrBarcodes, string ticketHost);
		Task<byte[]> ExportPackageDataToSpreadSheet(string host, string ticketHost, string packageIdOrBarcodes);
		Task<IList<PackageDataset>> GetDatasetsByTrackingNumberAsync(List<TrackPackage> trackPackages);
		Task<IList<PackageDataset>> GetDatasetsByTrackingNumberAsync(List<PackageDataset> packages);
		Task<IList<PackageDataset>> GetProcessedPackagesBySiteAndPackageIdAsync(string siteName, List<PackageDataset> packages);
		Task<IList<PackageDataset>> GetDatasetsByCosmosIdNumberAsync(List<PackageDataset> packages);
  		Task<bool> ExecuteBulkInsertOrUpdateAsync(List<PackageDataset> packageDatasets);
  		Task<bool> ExecuteBulkUpdateAsync(List<PackageDataset> packageDatasets);
		Task<IList<PackageDataset>> GetDatasetsWithNoStopTheClockScans(Site site, int lookbackMin, int lookbackMax);
		Task<IEnumerable<PackageFromStatus>> GetPackagesFromStatusAsync(string subClient, string packageStatus, string startDate, string endDate); 
        Task<IEnumerable<PackageFromStatus>> GetRecallReleasePackages(string subClient, string startDate, string endDate);
		Task<PackageDataset> FindOldestPackageForArchiveAsync(string subClient, DateTime startDate);
		Task<IList<PackageForArchive>> GetPackagesForArchiveAsync(string subClient, DateTime manifestDate);
        Task DeleteArchivedPackagesAsync(string subClient, DateTime manifestDate);
        Task DeleteOlderPackagesAsync(string subClient, bool isPackageProcessed, DateTime date);
    }
}
