using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public interface IUspsEvsProcessor
	{
		Task<FileExportResponse> ExportUspsEvsFile(Site site, EndOfDayQueueMessage queueMessage);
		Task<FileExportResponse> ExportUspsEvsFileForPMODContainers(Site site, EndOfDayQueueMessage queueMessage);
		Task<FileExportResponse> CreateUspsRecords(Site site, IEnumerable<Package> packages, IEnumerable<ShippingContainer> containers);
		Task<FileExportResponse> CreateUspsRecordsForContainers(Site site, IEnumerable<ShippingContainer> containers);
	}
}
