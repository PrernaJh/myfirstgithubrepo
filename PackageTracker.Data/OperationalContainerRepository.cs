using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class OperationalContainerRepository : CosmosDbRepository<OperationalContainer>, IOperationalContainerRepository
	{
		public OperationalContainerRepository(ILogger<OperationalContainerRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.OperationalContainers;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

		public async Task<OperationalContainer> GetMostRecentOperationalContainerAsync(string siteName, string binCode, string partitionKeyString)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } oc 
							WHERE oc.siteName = @siteName 
                            AND oc.binCode = @binCode	
							ORDER BY oc.createDate DESC";

			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@binCode", binCode);
			var results = await GetItemsAsync(queryDefinition, partitionKeyString);
			return results.FirstOrDefault() ?? new OperationalContainer();
		}

		public async Task<OperationalContainer> GetActiveOperationalContainerAsync(string siteName, string binCode, string partitionKeyString)
		{

			var query = $@"SELECT TOP 1 * FROM {ContainerName} oc 
							WHERE oc.siteName = @siteName 
                            AND oc.binCode = @binCode	
							AND oc.status = @status
							ORDER BY oc.createDate DESC";

			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@status", ContainerEventConstants.Active)
													.WithParameter("@binCode", binCode);

			var results = await GetItemsAsync(queryDefinition, partitionKeyString);
			return results.FirstOrDefault() ?? new OperationalContainer();
		}

		public async Task<OperationalContainer> GetOperationalContainerAsync(string siteName, string containerId, string partitionKeyString)
		{
			var query = $@"SELECT TOP 1 * FROM { ContainerName } oc 
							WHERE oc.siteName = @siteName 
                            AND oc.containerId = @containerId	
							ORDER BY oc.createDate DESC";

			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@containerId", containerId);
			var results = await GetItemsAsync(queryDefinition, partitionKeyString);
			return results.FirstOrDefault() ?? new OperationalContainer();
		}

		public async Task<IEnumerable<OperationalContainer>> GetOutOfDateOperationalContainersAsync(string siteName, DateTime localTime, int localTimeOffset)
        {
			var active = ContainerEventConstants.Active;
			var gmtDateLastMidnight = localTime.Date.AddMinutes(localTimeOffset);
			var query = $@"SELECT * FROM { ContainerName } oc 
							WHERE oc.siteName = @siteName 
                            AND oc.createDate < @gmtDateLastMidnight
                            AND oc.status = @active
							ORDER BY oc.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@gmtDateLastMidnight", gmtDateLastMidnight)
													.WithParameter("@active", active);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}
    }
}
