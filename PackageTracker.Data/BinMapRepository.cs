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
	public class BinMapRepository : CosmosDbRepository<BinMap>, IBinMapRepository
	{
		public BinMapRepository(ILogger<BinMapRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)

		{ }

		public override string ContainerName { get; } = CollectionNameConstants.BinMaps;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<BinMap> GetBinMapByZip(string zipCode, string activeGroupId)
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} b
                           WHERE b.zipCode = @zipCode
                           AND b.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@zipCode", zipCode)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.Any() ? results.FirstOrDefault() : new BinMap();
		}

		public async Task<IEnumerable<BinMap>> GetBinMapsByActiveGroupIdAsync(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} b WHERE b.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}
	}
}
