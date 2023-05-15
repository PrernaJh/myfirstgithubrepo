using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IEodPackageRepository
	{
		Task<int> CountOldEodPackagesAsync(string siteName, DateTime cutoff);
		Task<BatchDbResponse> DeleteOldEodPackagesAsync(string siteName, DateTime cutoff, int chunkSize);
		Task<BatchDbResponse> DeleteEodPackages(string siteName, DateTime localProcessedDate);
		Task<BatchDbResponse> UpsertEodPackagesAsync(IEnumerable<EodPackage> eodPackages, int chunkSize = 500);
		Task<EodPackage> GetEodPackageByGuidId(string guidPackageId);
		Task<IEnumerable<EodPackage>> GetEodPackages(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodPackage>> GetEodOverview(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodPackage>> GetEvsEodPackages(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodPackage>> GetExpenseRecords(string siteName, DateTime localProcessedDate, string subClientName);
		Task<IEnumerable<EodPackage>> GetInvoiceRecords(string siteName, DateTime localProcessedDate, string subClientName);
		Task<IEnumerable<EodPackage>> GetPackageDetails(string siteName, DateTime localProcessedDate);
		Task<IEnumerable<EodPackage>> GetReturnAsns(string siteName, DateTime localProcessedDate, string subClientName);
	}
}
