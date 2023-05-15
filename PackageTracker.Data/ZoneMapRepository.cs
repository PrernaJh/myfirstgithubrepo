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
	public class ZoneMapRepository : CosmosDbRepository<ZoneMap>, IZoneMapRepository
	{
		public ZoneMapRepository(ILogger<ZoneMapRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ZoneMaps;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<ZoneMap> GetZoneMapAsync(string zipFirstThree, string activeGroupId)
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} z WHERE z.activeGroupId = @activeGroupId                                                            
                                                            AND z.zipFirstThree = @zipFirstThree";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId)
													.WithParameter("@zipFirstThree", zipFirstThree);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.FirstOrDefault() ?? new ZoneMap();
		}

        public async Task<IEnumerable<ZoneMap>> GetZoneMapsByActiveGroupIdAsync(string activeGroupId)
        {
			var query = $@"SELECT * FROM {ContainerName} z WHERE z.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}
    }
}
