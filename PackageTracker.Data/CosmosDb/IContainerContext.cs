using Microsoft.Azure.Cosmos;
using PackageTracker.Data.Models;

namespace PackageTracker.Data.CosmosDb
{
	public interface IContainerContext<T> where T : Entity
	{
		string ContainerName { get; }
		string GenerateId(T entity);
		PartitionKey GetPartitionKeyFromString(string partitionKey);
		string ResolvePartitionKeyString(string unresolvedPartitionKey);
	}
}
