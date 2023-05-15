using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IInvoiceProcessor
	{
		Task<FileExportResponse> GenerateInvoiceFile(
			SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId, bool addPackageRatingElements);
		InvoiceRecord CreateInvoiceRecord(Package package);
		string[] BuildInvoiceHeader(bool addPackageRatingElements);
		eDataTypes[] GetExcelDataTypes(bool addPackageRatingElements);
	}
}
