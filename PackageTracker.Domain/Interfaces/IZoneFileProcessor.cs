using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IZoneFileProcessor
	{
		Task<FileImportResponse> ImportZoneFileToDatabase(Stream fileStream);
		Task<ICollection<ZoneMap>> ReadZoneFileStreamAsync(Stream stream);
	}
}
