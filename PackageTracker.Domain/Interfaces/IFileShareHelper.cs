using PackageTracker.Domain.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IFileShareHelper
	{
		public Task UploadListOfStringsToFileShareAsync(List<string> input, string accountName, string keyValue, string fileShareName, string fileName);
		public Task UploadWorkSheet(ExcelWorkSheet ws, string accountName, string keyValue, string fileShareName, string fileName, long maxSize);
		Task UploadExcelToFileShare(ExcelWorkSheet excel, string accountName, string keyValue, string fileShareName, string fileName);
	}
}
