using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace PackageTracker.AzureExtensions
{
	public class QueueClientFactory : IQueueClientFactory
	{
		private readonly CloudStorageAccount storageAccount;

		public QueueClientFactory(CloudStorageAccount storageAccount)
		{
			this.storageAccount = storageAccount;
		}

		public CloudQueueClient GetClient()
		{
			return storageAccount.CreateCloudQueueClient();
		}
	}
}
