using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IZipOverrideProcessor
	{
		Task AssignZipOverridesForListOfPackages(List<Package> packages, List<string> activeGroupIds);		

		Task<FileImportResponse> ImportZipCarrierOverrideFileToDatabase(Stream fileStream, string fileName, string subClientName);
	}
}
