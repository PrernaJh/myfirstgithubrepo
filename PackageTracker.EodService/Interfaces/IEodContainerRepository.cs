using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
    public interface IEodContainerRepository
    {
		Task<int> CountOldEodContainersAsync(string siteName, DateTime cutoff);
		Task<BatchDbResponse> DeleteOldEodContainersAsync(string siteName, DateTime cutoff, int chunkSize);
        Task<BatchDbResponse> DeleteEodContainers(string siteName, DateTime localProcessedDate);
        Task<BatchDbResponse> UpsertEodContainersAsync(IEnumerable<EodContainer> eodContainers, int chunkSize = 500);
		Task<EodContainer> GetEodContainerByGuidId(string guidContainerId);
		Task<IEnumerable<EodContainer>> GetEodContainers(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetEodOverview(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetContainerDetails(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetEvsEodContainers(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetExpenseRecords(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetPmodContainerDetails(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodContainer>> GetReferencedContainers(string siteName, DateTime localProcessedDate);
    }
}
