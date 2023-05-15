using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IPackagePostProcessor
	{
		Task<bool> UpdatePackageServiceRuleGroupIds(string subClientName, int daysToLookback, string webJobId);
		Task<bool> IsContainerAssignedToPackages(string containerId, string siteName);
		Task<Package> GetPackageByPackageId(string packageId, string siteName);
		Task<IEnumerable<Package>> GetPackagesForConsumerDetailFile(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime);
		Task<IEnumerable<Package>> GetPackagesForCreatePackagePostProcessing(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime);
		Task<IEnumerable<Package>> GetPackagesWithOutdatedBinData(int daysToLookback, int maxCount, string subClientName, string binGroupId, string binMapGroupId);
		Task<IEnumerable<Package>> GetPackagesByShippingContainerAsync(string containerId, string siteName);
		Task<int> CountPackagesForContainerAsync(string containerId, string siteName);
	}
}
