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
	public class ZipMapRepository : CosmosDbRepository<ZipMap>, IZipMapRepository
	{
		public ZipMapRepository(ILogger<ZipMapRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ZipMaps;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<ZipMap> GetZipMapValueAsync(string activeGroupId, string zipCode)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } zm WHERE zm.activeGroupId = @activeGroupId 
                            AND zm.zipCode = @zipCode";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId)
													.WithParameter("@zipCode", zipCode);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.FirstOrDefault() ?? new ZipMap();
		}

        public async Task<IEnumerable<ZipMap>> GetZipMapsByActiveGroupIdAsync(string activeGroupId)
        {
 			var query = $@"SELECT * FROM { ContainerName } zm WHERE zm.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}
    }
}
