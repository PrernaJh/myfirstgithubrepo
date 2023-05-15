using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace PackageTracker.AzureExtensions
{
	public static class StorageAccountSetup
	{
		public static IServiceCollection AddQueueStorageAccount(this IServiceCollection services, string storageAccountSettings)
		{
			var storageAccount = CloudStorageAccount.Parse(storageAccountSettings);

			var queueClient = new QueueClientFactory(storageAccount);

			services.AddSingleton<IQueueClientFactory>(queueClient);

			return services;
		}

		public static IServiceCollection AddBlobStorageAccount(this IServiceCollection services, string storageAccountSettings)
		{
			var storageAccount = CloudStorageAccount.Parse(storageAccountSettings);

			var blobClient = new BlobClientFactory(storageAccount);

			services.AddSingleton<IBlobClientFactory>(blobClient);

			return services;
		}

		public static IServiceCollection AddFileShareStorageAccount(this IServiceCollection services, string storageAccountSettings)
		{
			var storageAccount = CloudStorageAccount.Parse(storageAccountSettings);

			var fileClient = new CloudFileClientFactory(storageAccount);

			services.AddSingleton<ICloudFileClientFactory>(fileClient);

			return services;
		}
	}
}
