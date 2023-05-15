using Microsoft.Azure.Cosmos;
using PackageTracker.Data.Interfaces;

namespace PackageTracker.Data.CosmosDb
{
	public class CosmosDbContainer : ICosmosDbContainer
	{
		public Container Container { get; }

		public CosmosDbContainer(CosmosClient cosmosClient,
								 string databaseName,
								 string containerName)
		{
			Container = cosmosClient.GetContainer(databaseName, containerName);
		}
	}
}
