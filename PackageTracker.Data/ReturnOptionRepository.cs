using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models.ReturnOptions;
using PackageTracker.Data.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class ReturnOptionRepository : CosmosDbRepository<ReturnOption>, IReturnOptionRepository
	{
		public ReturnOptionRepository(ILogger<ReturnOptionRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ReturnOptions;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

		public async Task<ReturnOption> GetReturnOptionsBySiteAsync(string siteName)
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} r WHERE r.siteName = @siteName
                            AND r.isEnabled = true";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results.FirstOrDefault() ?? new ReturnOption();
		}
	}
}