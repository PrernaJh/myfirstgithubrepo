using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class ClientRepository : CosmosDbRepository<Client>, IClientRepository
	{
		public ClientRepository(ILogger<ClientRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Clients;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateDefaultPartitionKeyString();

		public async Task<Client> GetClientByNameAsync(string name)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } c WHERE c.name = @name";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@name", name);
			var results = await GetItemsAsync(queryDefinition, name);
			return results.FirstOrDefault() ?? new Client();
		}

		public async Task<IEnumerable<Client>> GetClientsAsync()
		{
			var query = $@"SELECT * FROM { ContainerName } c WHERE c.isEnabled = true";
			var queryDefinition = new QueryDefinition(query);
			var results = await GetItemsAsync(queryDefinition, null);
			return results;
		}
	}
}
