using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class SubClientRepository : CosmosDbRepository<SubClient>, ISubClientRepository
	{
		public SubClientRepository(ILogger<SubClientRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.SubClients;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateDefaultPartitionKeyString();


		public async Task<SubClient> GetSubClientByNameAsync(string name)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } c WHERE c.name = @name";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@name", name);
			var results = await GetItemsAsync(queryDefinition, name);
			return results.FirstOrDefault() ?? new SubClient();
		}

		public async Task<SubClient> GetSubClientByKeyAsync(string key)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } c WHERE c.key = @key";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@key", key);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results.FirstOrDefault() ?? new SubClient();
		}

		public async Task<IEnumerable<SubClient>> GetSubClientsBySiteNameAsync(string siteName)
		{
			var query = $@"SELECT * FROM { ContainerName } c WHERE c.siteName = @siteName";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<SubClient>> GetSubClientsAsync()
		{
			var query = $@"SELECT * FROM { ContainerName } c WHERE c.isEnabled = true";
			var queryDefinition = new QueryDefinition(query);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<bool> HaveSubClientsChangedAsync(DateTime lastScanDateTime)
		{
			if (lastScanDateTime.Year == 1)
				return true;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 1 s._ts FROM {ContainerName} s
										WHERE s._ts >= @unixTimeAtLastScan";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan);
			var results = await GetTimestampsAsync(queryDefinition);
			return results.Any();
		}
	}
}
