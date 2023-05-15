using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class RateRepository : CosmosDbRepository<Rate>, IRateRepository
	{
		public RateRepository(ILogger<RateRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Rates;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<IEnumerable<Rate>> GetRatesByActiveGroupId(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} r
                           WHERE r.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}
	}
}
