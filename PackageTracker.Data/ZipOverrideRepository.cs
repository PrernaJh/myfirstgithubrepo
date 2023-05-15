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
	public class ZipOverrideRepository : CosmosDbRepository<ZipOverride>, IZipOverrideRepository
	{
		public ZipOverrideRepository(ILogger<ZipOverrideRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)

		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ZipOverrides;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<ZipOverride> GetZipOverrideByZipCodeAsync(string zipCode, string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} z WHERE z.activeGroupId = @activeGroupId                                                            
                                                            AND z.zipCode = @zipCode";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId)
													.WithParameter("@zipCode", zipCode);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.FirstOrDefault() ?? new ZipOverride();
		}

		public async Task<IEnumerable<ZipOverride>> GetZipOverridesByActiveGroupId(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} zo
                           WHERE zo.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}

		public async Task<ZipOverride> GetZipCarrierOverrideAsync(Package package, ServiceRule serviceRule, string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} z WHERE z.activeGroupId = @activeGroupId                                                            
                                                            AND z.zipCode = @zipCode
															AND z.fromShippingCarrier = @shippingCarrier
															AND z.fromShippingMethod = @shippingMethod";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId)
													.WithParameter("@shippingCarrier", serviceRule.ShippingCarrier)
													.WithParameter("@shippingMethod", serviceRule.ShippingMethod)
													.WithParameter("@zipCode", package.Zip);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results.FirstOrDefault() ?? new ZipOverride();
		}
	}
}
