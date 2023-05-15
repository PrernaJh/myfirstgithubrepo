using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models.JobOptions;
using PackageTracker.Data.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class JobOptionRepository : CosmosDbRepository<JobOption>, IJobOptionRepository
	{
		public JobOptionRepository(ILogger<JobOptionRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.JobOptions;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

		public async Task<JobOption> GetJobOptionsBySiteAsync(string siteName) // add job
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} j WHERE j.siteName = @siteName
                            AND j.isEnabled = true";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results.FirstOrDefault() ?? new JobOption();
		}
	}
}
