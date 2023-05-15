using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IDatasetProcessor
	{
		Task<IEnumerable<ShippingContainer>> GetContainersForContainerDatasetsAsync(string siteName, DateTime lastScanDateTime);
		Task<IEnumerable<Job>> GetJobsForJobDatasetsAsync(string siteName);
        Task<IEnumerable<int>> GetPackageTimeStampsAsync(string siteName, DateTime lastScanDateTime, DateTime nextScanDateTime);
		Task<IEnumerable<Package>> GetPackagesForPackageDatasetsAsync(string siteId, int firstTimestamp, int lastTimeStamp);
    }
}
