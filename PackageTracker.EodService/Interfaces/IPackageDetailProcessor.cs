using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IPackageDetailProcessor
	{
		Task<FileExportResponse> GeneratePackageDetailFile(
			Site site, DateTime dateToProcess, string webJobId, bool isHistorical, bool addPackageRatingElements);
		PackageDetailRecord CreatePackageDetailRecord(Package package);
	}
}
