using Microsoft.Azure.Cosmos;

namespace PackageTracker.Data.CosmosDb
{
	public interface ICosmosDbContainer
	{
		Container Container { get; }
	}
}
