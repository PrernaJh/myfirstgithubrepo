using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IContainerRepository : IRepository<ShippingContainer>
	{
		Task<ShippingContainer> GetContainerByContainerId(string siteName, string containerId);
		Task<ShippingContainer> GetActiveContainerByContainerIdAsync(string containerId, string siteName);
		Task<ShippingContainer> GetClosedContainerByContainerIdAsync(string containerId, string siteName);
		Task<ShippingContainer> GetActiveOrClosedContainerByContainerIdAsync(string containerId, string siteName);
		Task<ShippingContainer> GetActiveContainerByBinCodeAsync(string siteName, string binCode, DateTime localTime);
		Task<IEnumerable<ShippingContainer>> GetActiveContainersForSiteAsync(string siteName, DateTime localTime);
		Task<IEnumerable<ShippingContainer>> GetContainersForRateAssignmentAsync(string siteName);
		Task<IEnumerable<ShippingContainer>> GetContainersForRateUpdate(string siteName, int daysToLookback);
		Task<IEnumerable<ShippingContainer>> GetContainersForContainerDatasetsAsync(string siteName, DateTime lastScanDateTime);
        Task<IEnumerable<ShippingContainer>> GetOutOfDateContainersAsync(string siteName, DateTime localTime);

		// eod
		Task<IEnumerable<ShippingContainer>> GetContainersForEndOfDayProcess(string siteName, DateTime lastScanDateTime);
		Task<IEnumerable<ShippingContainer>> GetContainersForSqlEndOfDayProcess(string siteName, DateTime targetDate, DateTime lastScanDateTime);
		Task<IEnumerable<ShippingContainer>> GetContainersForUspsEvsFileAsync(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate);
		Task<IEnumerable<ShippingContainer>> GetClosedContainersForPackageEvsFile(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate);
		Task<IEnumerable<ShippingContainer>> GetContainersToResetEod(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate);
		Task<bool> HaveContainersChangedForSiteAsync(string siteName, DateTime lastScanDateTime);
        Task<IEnumerable<ShippingContainer>> GetClosedContainersByDate(DateTime targetDate, string siteName);
		Task<BatchDbResponse<ShippingContainer>> UpdateContainersEodProcessed(IEnumerable<ShippingContainer> containers);
		Task<BatchDbResponse<ShippingContainer>> UpdateContainersSqlEodProcessed(IEnumerable<ShippingContainer> containers);
		Task<BatchDbResponse<ShippingContainer>> UpdateContainersForRateUpdate(IEnumerable<ShippingContainer> containers);
		Task<BatchDbResponse<ShippingContainer>> UpdateContainersSetRateAssigned(IEnumerable<ShippingContainer> containers);
	}
}
