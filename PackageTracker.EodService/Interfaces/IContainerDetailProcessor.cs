using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IContainerDetailProcessor
	{
		Task<FileExportResponse> ExportContainerDetailFile(Site site, DateTime dateToProcess, string webJobId);
		Task<FileExportResponse> ExportPmodContainerDetailFile(Site site, DateTime dateToProcess, string webJobId);
		ContainerDetailRecord CreateContainerDetailRecord(Site site, Bin bin, ShippingContainer container);
		PmodContainerDetailRecord CreatePmodContainerDetailRecord(ShippingContainer container);
	}
}
