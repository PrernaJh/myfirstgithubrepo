using PackageTracker.Domain.Models.FileProcessing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IZipMapFileProcessor
	{
		Task<FileImportResponse> ImportZipMaps(Stream fileStream, string fileName);
	}
}
