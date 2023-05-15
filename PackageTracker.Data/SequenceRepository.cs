using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class SequenceRepository : CosmosDbRepository<Sequence>, ISequenceRepository
	{
		public SequenceRepository(ILogger<SequenceRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Sequences;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

		public async Task<Sequence> GetSequenceAsync(string siteName, string sequenceType)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } q WHERE q.siteName = @siteName 
                            AND q.sequenceType = @sequenceType";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@sequenceType", sequenceType);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results.FirstOrDefault() ?? new Sequence();
		}
	}
}
