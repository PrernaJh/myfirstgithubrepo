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
	public class BinRepository : CosmosDbRepository<Bin>, IBinRepository
	{
		public BinRepository(ILogger<BinRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)

		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Bins;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<IEnumerable<Bin>> GetBinsByActiveGroupIdAsync(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} b WHERE b.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}

		public async Task<IEnumerable<Bin>> GetBinCodesAsync(string activeGroupId)
		{
			var query = $@"SELECT b.binCode FROM { ContainerName } b WHERE b.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}

		public async Task<Bin> GetBinByBinCodeAsync(string binCode, string activeGroupId)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } b WHERE b.activeGroupId = @activeGroupId
                            AND b.binCode = @binCode";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@binCode", binCode)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.FirstOrDefault() ?? new Bin();
		}
	}
}
