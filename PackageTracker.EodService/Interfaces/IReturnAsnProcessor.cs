using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IReturnAsnProcessor
	{
		Task<FileExportResponse> GenerateReturnAsnFile(
			Site site, SubClient subClient, DateTime dateToProcess, string webJobId, bool isSimplified, bool addPackageRatingElements);
		public ReturnAsnRecord CreateReturnAsnRecord(Package package);
		public ReturnAsnRecord CreateReturnAsnShortRecord(Package package);
	}
}
