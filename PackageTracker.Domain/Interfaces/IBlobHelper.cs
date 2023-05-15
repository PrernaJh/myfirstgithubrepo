using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PackageTracker.Domain.Utilities;
namespace PackageTracker.Domain.Interfaces
{
	public interface IBlobHelper
	{
		Task<(bool IsSuccess, string Message)> ArchiveToBlob(Stream fileStream, string fileName, string destinationContainer);
		Task UploadListOfStringsToBlobAsync(List<string> input, string containerName, string fileName);
		Task UploadExcelToBlobAsync(ExcelWorkSheet excel, string containerName, string fileName);
		Task<string> ArchiveBlob(string fileName, string sourceContainer, string destinationContainer);
		Task<string> ArchiveBlob(CloudBlockBlob sourceBlob, CloudBlobContainer destinationContainer);
		CloudBlobContainer GetBlobContainerReference(string containerName);
		CloudBlockBlob DownloadBlob(string containerName, string fileName);
		Task<byte[]> DownloadBlobAsByteArray(string containerName, string fileName);
		string GenerateArchiveFileName(string fileName);
	}
}
