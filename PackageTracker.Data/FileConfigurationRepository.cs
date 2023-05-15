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
	public class FileConfigurationRepository : CosmosDbRepository<FileConfiguration>, IFileConfigurationRepository
	{
		public FileConfigurationRepository(ILogger<FileConfigurationRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.FileConfigurations;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

		public async Task<IEnumerable<FileConfiguration>> GetAllFileConfigurations(string siteName, string scheduleType)
		{
			var query = $@"SELECT * FROM {ContainerName} fc WHERE fc.siteName = @siteName 
                            AND fc.scheduleType = @scheduleType
                            AND fc.isEnabled = true";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@scheduleType", scheduleType);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results;
		}
	}
}
