using Microsoft.WindowsAzure.Storage.File;

namespace PackageTracker.AzureExtensions
{
	public interface ICloudFileClientFactory
	{
		CloudFileClient GetCloudFileClient();
	}
}
