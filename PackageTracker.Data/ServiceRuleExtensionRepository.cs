using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace PackageTracker.Data
{
	public class ServiceRuleExtensionRepository : CosmosDbRepository<ServiceRuleExtension>, IServiceRuleExtensionRepository
	{
		public ServiceRuleExtensionRepository(ILogger<ServiceRuleExtensionRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ServiceRuleExtensions;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);
		public async Task<IEnumerable<ServiceRuleExtension>> GetFortyEightStatesRulesByActiveGroupIdAsync(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} sre
                           WHERE sre.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}

		public async Task<ServiceRuleExtension> GetFortyEightStatesRuleAsync(Package package)
		{
			var inFedExList = package.ZipOverrides.Contains(ActiveGroupTypeConstants.ZipsFedExHawaii);
			var inUpsList = package.ZipOverrides.Contains(ActiveGroupTypeConstants.ZipsUpsSat48);
			var query = @$"SELECT sre.id, sre.shippingCarrier, sre.shippingMethod, sre.serviceLevel 
                            FROM { ContainerName } sre 
                            WHERE sre.activeGroupId = @activeGroupId
							AND sre.mailCode = @mailCode
                            AND sre.stateCode = @stateCode
                            AND sre.inFedExList = @inFedExList
                            AND sre.inUpsList = @inUpsList
							AND sre.isSaturdayDelivery = @isSaturdayDelivery
							AND sre.minWeight <= @weight
                            AND sre.maxWeight >= @weight";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", package.ServiceRuleExtensionGroupId)
													.WithParameter("@mailCode", package.MailCode)
													.WithParameter("@stateCode", package.State)
													.WithParameter("@inFedExList", inFedExList)
													.WithParameter("@inUpsList", inUpsList)
													.WithParameter("@isSaturdayDelivery", package.IsSaturday)
													.WithParameter("@weight", package.Weight);
			var results = await GetItemsAsync(queryDefinition, package.ServiceRuleExtensionGroupId);
			return results.FirstOrDefault() ?? new ServiceRuleExtension();
		}

		public async Task<ServiceRuleExtension> GetDefaultFortyEightStatesRuleAsync(Package package)
		{
			var query = @$"SELECT sre.id, sre.shippingCarrier, sre.shippingMethod, sre.serviceLevel
                            FROM { ContainerName } sre 
                            WHERE sre.activeGroupId = @activeGroupId
                            AND sre.isDefault = true
							AND sre.minWeight <= @weight
                            AND sre.maxWeight >= @weight";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", package.ServiceRuleExtensionGroupId)
													.WithParameter("@weight", package.Weight);
			var results = await GetItemsAsync(queryDefinition, package.ServiceRuleExtensionGroupId);
			return results.FirstOrDefault() ?? new ServiceRuleExtension();
		}
	}
}
