using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using PackageTracker.AzureExtensions;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Utilities
{
    public class BlobHelper : IBlobHelper
	{
		private readonly IBlobClientFactory blobClientFactory;
		private readonly ILogger<BlobHelper> logger;

		public BlobHelper(IBlobClientFactory blobClientFactory, ILogger<BlobHelper> logger)
		{
			this.blobClientFactory = blobClientFactory;
			this.logger = logger;
		}
		 

        public async Task UploadListOfStringsToBlobAsync(List<string> input, string containerName, string fileName)
		{
			var blobClient = blobClientFactory.GetClient();
			var blobContainer = blobClient.GetContainerReference(containerName);
			var blob = blobContainer.GetBlockBlobReference(fileName);
			var byteArray = input.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
			await blob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);
		}

		public async Task UploadExcelToBlobAsync(ExcelWorkSheet excel, string containerName, string fileName)
		{
			var blobClient = blobClientFactory.GetClient();
			var blobContainer = blobClient.GetContainerReference(containerName);
			var blob = blobContainer.GetBlockBlobReference(fileName);
			var byteArray = await excel.GetContentsAsync();
			await blob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);
		}

		public string GenerateArchiveFileName(string fileName)
		{
			return $"{ DateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) }_{fileName}";
		}

		public async Task<(bool IsSuccess, string Message)> ArchiveToBlob(Stream fileStream, string fileName, string destinationContainer)
		{
			try
			{
				var archiveFileName = GenerateArchiveFileName(fileName);
				var destinationBlobContainerReference = GetBlobContainerReference(destinationContainer);
				var archiveBlob = destinationBlobContainerReference.GetBlockBlobReference(archiveFileName);
				await archiveBlob.UploadFromStreamAsync(fileStream);

				return (true, archiveFileName);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error while archiving filename: {fileName}. Exception: {ex}");
				return (false, ex.ToString());
			}
		}

		public async Task<string> ArchiveBlob(string fileName, string sourceContainer, string destinationContainer)
		{
			var sourceBlobFileName = fileName;
			var sourceBlobContainerReference = GetBlobContainerReference(sourceContainer);
			var destinationBlobContainerReference = GetBlobContainerReference(destinationContainer);
			var sourceBlob = sourceBlobContainerReference.GetBlockBlobReference(sourceBlobFileName);
			return await ArchiveBlob(sourceBlob, destinationBlobContainerReference);
		}

		public async Task<string> ArchiveBlob(CloudBlockBlob sourceBlob, CloudBlobContainer destinationContainer)
		{
			try
			{
				using (var memoryStream = new MemoryStream())
				{
					await sourceBlob.DownloadToStreamAsync(memoryStream);

					var archiveFileName = GenerateArchiveFileName(sourceBlob.Name);
					var destinationBlob = destinationContainer.GetBlockBlobReference(archiveFileName);
					await destinationBlob.StartCopyAsync(sourceBlob);

					await sourceBlob.DeleteAsync();

					logger.Log(LogLevel.Information, $"File archived: {archiveFileName}");
					return archiveFileName;
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to archive file: {sourceBlob.Name} Exception: {ex}");
			}
			return string.Empty;
		}

		public CloudBlobContainer GetBlobContainerReference(string containerName)
		{
			var blobClient = blobClientFactory.GetClient();
			var blobContainer = blobClient.GetContainerReference(containerName);

			return blobContainer;
		}

        public CloudBlockBlob DownloadBlob(string containerName, string fileName)
        {
			var blobClient = blobClientFactory.GetClient();
			var blobContainer = blobClient.GetContainerReference(containerName);  
			var blob = blobContainer.GetBlockBlobReference(fileName);
			return blob;
		}
        public async Task<byte[]> DownloadBlobAsByteArray(string containerName, string fileName)
        {
			var value = DownloadBlob(containerName, fileName);
			Stream blobStream = await value.OpenReadAsync();
			using (var memoryStream = new MemoryStream())
			{
				blobStream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}    
	}
}
