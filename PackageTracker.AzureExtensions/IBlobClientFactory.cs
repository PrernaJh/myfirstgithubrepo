using Microsoft.WindowsAzure.Storage.Blob;

namespace PackageTracker.AzureExtensions
{
	public interface IBlobClientFactory
	{
		CloudBlobClient GetClient();
	}
}
