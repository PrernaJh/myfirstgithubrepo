using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using PackageTracker.AzureExtensions;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Utilities
{
	public class FileShareHelper : IFileShareHelper
	{
		private readonly ICloudFileClientFactory cloudFileClientFactory;
		private readonly ILogger<FileShareHelper> logger;

		public FileShareHelper(ICloudFileClientFactory cloudFileClientFactory, ILogger<FileShareHelper> logger)
		{
			this.cloudFileClientFactory = cloudFileClientFactory;
			this.logger = logger;
		}

		public async Task UploadListOfStringsToFileShareAsync(List<string> input, string accountName, string keyValue, string fileShareName, string fileName)
		{
			var fileClient = cloudFileClientFactory.GetCloudFileClient();
			var directory = fileClient.GetShareReference(fileShareName).GetRootDirectoryReference();
			var fileUri = new Uri($"{directory.Uri}/{fileName}");
			var storageCredentials = new StorageCredentials(accountName, keyValue);
			var cloudFile = new CloudFile(fileUri, storageCredentials);
			var byteArray = input.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
			using (var fileStream = await cloudFile.OpenWriteAsync(byteArray.Length))
			{
				await fileStream.WriteAsync(byteArray);
				await fileStream.CommitAsync();
			}
		}

		public async Task UploadExcelToFileShare(ExcelWorkSheet excel, string accountName, string keyValue, string fileShareName, string fileName)
		{
			var fileClient = cloudFileClientFactory.GetCloudFileClient();
			var directory = fileClient.GetShareReference(fileShareName).GetRootDirectoryReference();
			var fileUri = new Uri($"{directory.Uri}/{fileName}");
			var storageCredentials = new StorageCredentials(accountName, keyValue);
			var cloudFile = new CloudFile(fileUri, storageCredentials);

			var byteArray = await excel.GetContentsAsync();

			using (var fileStream = await cloudFile.OpenWriteAsync(byteArray.Length))
			{
				await fileStream.WriteAsync(byteArray);
				await fileStream.CommitAsync();
			}
		}

		public async Task UploadWorkSheet(ExcelWorkSheet ws, string accountName, string keyValue, string fileShareName, string fileName, long maxSize)
        {
			var fileClient = cloudFileClientFactory.GetCloudFileClient();
			var directory = fileClient.GetShareReference(fileShareName).GetRootDirectoryReference();
			var fileUri = new Uri($"{directory.Uri}/{fileName}");
			var storageCredentials = new StorageCredentials(accountName, keyValue);
			var cloudFile = new CloudFile(fileUri, storageCredentials);
			long fileSize = maxSize;
			using (var exportFileStream = await cloudFile.OpenWriteAsync(fileSize))
			{
				await ws.WriteAsync(exportFileStream);
				fileSize = exportFileStream.Position;
				exportFileStream.Close();
			}
			await cloudFile.ResizeAsync(fileSize);
		}
	}
}
