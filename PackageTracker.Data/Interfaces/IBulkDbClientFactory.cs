namespace PackageTracker.Data.Interfaces
{
	public interface IBulkDbClientFactory
	{
		IBulkDbClient GetClient(string collectionName);
	}
}
