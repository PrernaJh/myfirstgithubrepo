using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IRateFileProcessor
	{		
		Task<FileImportResponse> ImportRatesFileToDatabase(Stream fileStream, SubClient subClient);		
	}
}
