using PackageTracker.Domain.Models.FileProcessing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IUpsDasFileProcessor
	{
		Task<FileImportResponse> ImportUpsDasFileToDatabase(Stream fileStream);
	}
}
