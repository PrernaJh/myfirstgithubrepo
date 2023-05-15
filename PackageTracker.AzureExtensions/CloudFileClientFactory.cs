using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace PackageTracker.AzureExtensions
{
	public class CloudFileClientFactory : ICloudFileClientFactory
	{
		private readonly CloudStorageAccount storageAccount;

		public CloudFileClientFactory(CloudStorageAccount storageAccount)
		{
			this.storageAccount = storageAccount;
		}

		public CloudFileClient GetCloudFileClient()
		{
			return storageAccount.CreateCloudFileClient();
		}
	}
}
