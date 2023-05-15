using Microsoft.WindowsAzure.Storage.Queue;

namespace PackageTracker.AzureExtensions
{
	public interface IQueueClientFactory
	{
		CloudQueueClient GetClient();
	}
}
