using Microsoft.Azure.Cosmos;
using PackageTracker.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data.CosmosDb
{
	public class CosmosDbContainerFactory : ICosmosDbContainerFactory
	{
		private readonly CosmosClient cosmosClient;
		private readonly string databaseName;
		private readonly List<ContainerInfo> containers;

		public CosmosDbContainerFactory(CosmosClient cosmosClient,
								   string databaseName,
								   List<ContainerInfo> containers)
		{
			this.databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
			this.containers = containers ?? throw new ArgumentNullException(nameof(containers));
			this.cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
		}

		public ICosmosDbContainer GetContainer(string containerName)
		{
			if (containers.Where(x => x.Name == containerName) == null)
			{
				throw new ArgumentException($"Unable to find container: {containerName}");
			}

			return new CosmosDbContainer(cosmosClient, databaseName, containerName);
		}

		public async Task EnsureDbSetupAsync()
		{
			var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

			foreach (var container in containers)
			{
				await database.Database.CreateContainerIfNotExistsAsync(container.Name, $"{container.PartitionKey}");
			}
		}
	}
}
