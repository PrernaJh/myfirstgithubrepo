using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IExpenseProcessor
	{
		Task<FileExportResponse> GenerateExpenseFile(SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId);
		ExpenseRecord CreatePackageExpenseRecord(Package package);
		ExpenseRecord CreateContainerExpenseRecord(ShippingContainer container);
		string[] BuildExpenseHeader();
		eDataTypes[] GetExcelDataTypes();
	}
}
