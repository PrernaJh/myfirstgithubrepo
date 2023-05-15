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
	public class ServiceRuleRepository : CosmosDbRepository<ServiceRule>, IServiceRuleRepository
	{
		public ServiceRuleRepository(ILogger<ServiceRuleRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.ServiceRules;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<ServiceRule> GetServiceRuleAsync(Package package, bool useOverridenMailCode = false)
		{
			var inputMailCode = useOverridenMailCode ? package.OverrideMailCode : package.MailCode;
			var query = @$"SELECT sr.id, sr.shippingCarrier, sr.shippingMethod, sr.serviceLevel, sr.isQCRequired FROM { ContainerName } sr 
                            WHERE sr.activeGroupId = @activeGroupId
                            AND sr.mailCode = @mailCode
                            AND sr.isOrmd = @isOrmd
                            AND sr.isPoBox = @isPoBox
                            AND sr.isOutside48States = @isOutside48States
                            AND sr.isUpsDas = @isUpsDas
                            AND sr.isSaturday = @isSaturday
                            AND sr.isDduScfBin = @isDduScfBin
                            AND sr.minWeight <= @weight
                            AND sr.maxWeight >= @weight
                            AND sr.minLength <= @length
                            AND sr.maxLength >= @length
                            AND sr.minHeight <= @height
                            AND sr.maxHeight >= @height
                            AND sr.minWidth <= @width
                            AND sr.maxWidth >= @width
                            AND sr.minTotalDimensions <= @totalDimensions
                            AND sr.maxTotalDimensions >= @totalDimensions
                            AND sr.zoneMin <= @zone
                            AND sr.zoneMax >= @zone";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", package.ServiceRuleGroupId)
													.WithParameter("@mailCode", inputMailCode)
													.WithParameter("@isOrmd", package.IsOrmd)
													.WithParameter("@isPoBox", package.IsPoBox)
													.WithParameter("@isOutside48States", package.IsOutside48States)
													.WithParameter("@isUpsDas", package.IsUpsDas)
													.WithParameter("@isSaturday", package.IsSaturday)
													.WithParameter("@isDduScfBin", package.IsDduScfBin)
													.WithParameter("@weight", package.Weight)
													.WithParameter("@length", package.Length)
													.WithParameter("@height", package.Depth)
													.WithParameter("@width", package.Width)
													.WithParameter("@totalDimensions", package.TotalDimensions)
													.WithParameter("@zone", package.Zone);
			var results = await GetItemsAsync(queryDefinition, package.ServiceRuleGroupId);
			return results.FirstOrDefault() ?? new ServiceRule();
		}

		public async Task<IEnumerable<ServiceRule>> GetServiceRulesByActiveGroupIdAsync(string activeGroupId)
		{
			var query = $@"SELECT * FROM {ContainerName} sr
                           WHERE sr.activeGroupId = @activeGroupId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@activeGroupId", activeGroupId);
			var results = await GetItemsAsync(queryDefinition, activeGroupId);
			return results;
		}
	}
}