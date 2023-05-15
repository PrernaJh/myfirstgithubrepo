using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PackageTracker.AzureExtensions
{
	public class BlobClientFactory : IBlobClientFactory
	{
		private readonly CloudStorageAccount storageAccount;

		public BlobClientFactory(CloudStorageAccount storageAccount)
		{
			this.storageAccount = storageAccount;
		}

		public CloudBlobClient GetClient()
		{
			return storageAccount.CreateCloudBlobClient();
		}
	}
}
