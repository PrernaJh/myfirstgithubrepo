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
	public class ContainerRepository : CosmosDbRepository<ShippingContainer>, IContainerRepository
	{
		public ContainerRepository(ILogger<ContainerRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Containers;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<ShippingContainer> GetContainerByContainerId(string siteName, string containerId)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT TOP 1 * FROM {ContainerName} c 
							WHERE c.containerId = @containerId
							AND c.siteName = @siteName
							AND c.createDate > @lookback
							ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, containerId);
			return results.FirstOrDefault() ?? new ShippingContainer();
		}

		public async Task<ShippingContainer> GetActiveContainerByContainerIdAsync(string containerId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var active = ContainerEventConstants.Active;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} c 
								WHERE c.containerId = @containerId
									AND c.siteName = @siteName
									AND c.status = @active
									AND c.createDate> @lookback
								ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@active", active)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, containerId);
			return results.FirstOrDefault() ?? new ShippingContainer();
		}

		public async Task<ShippingContainer> GetClosedContainerByContainerIdAsync(string containerId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var closed = ContainerEventConstants.Closed;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} c 
								WHERE c.containerId = @containerId
									AND c.siteName = @siteName
									AND c.status = @closed
									AND c.createDate> @lookback
								ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@closed", closed)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, containerId);
			return results.FirstOrDefault() ?? new ShippingContainer();
		}

		public async Task<ShippingContainer> GetActiveOrClosedContainerByContainerIdAsync(string containerId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var active = ContainerEventConstants.Active;
			var closed = ContainerEventConstants.Closed;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} c 
								WHERE c.containerId = @containerId
									AND c.siteName = @siteName
									AND c.status IN (@active, @closed)
									AND c.createDate> @lookback
								ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@active", active)
													.WithParameter("@closed", closed)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, containerId);
			return results.FirstOrDefault() ?? new ShippingContainer();
		}

		public async Task<ShippingContainer> GetActiveContainerByBinCodeAsync(string siteName, string binCode, DateTime localTime)
		{
			var active = ContainerEventConstants.Active;
			var localDateLastMidnight = localTime.Date;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} c
                            WHERE c.siteCreateDate > @localDateLastMidnight
								AND c.siteName = @siteName
								AND c.status = @active
								AND c.binCode = @binCode
                            ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@active", active)
													.WithParameter("@binCode", binCode)
													.WithParameter("@localDateLastMidnight", localDateLastMidnight);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results.FirstOrDefault() ?? new ShippingContainer();
		}

		public async Task<IEnumerable<ShippingContainer>> GetActiveContainersForSiteAsync(string siteName, DateTime localTime)
		{
			var active = ContainerEventConstants.Active;
			var localDateLastMidnight = localTime.Date;
			var query = $@"SELECT * FROM {ContainerName} c
                            WHERE c.siteCreateDate > @localDateLastMidnight
								AND c.siteName = @siteName
								AND c.status = @active";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@active", active)
													.WithParameter("@localDateLastMidnight", localDateLastMidnight);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForRateAssignmentAsync(string siteName)
		{
			var lookback = DateTime.Now.AddDays(-14);
			var closed = ContainerEventConstants.Closed;
			var shippingMethods = new List<string>()
			{
				ContainerConstants.PmodBag,
				ContainerConstants.PmodPallet
			};
			var query = $@"SELECT * FROM {ContainerName} c
							WHERE c.createDate > @lookback
								AND c.siteName = @siteName
								AND c.status = @closed
								AND ARRAY_CONTAINS(@shippingMethods, c.binLabelType)
								AND c.isRateAssigned = false";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@shippingMethods", shippingMethods)
													.WithParameter("@closed", closed)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForRateUpdate(string siteName, int daysToLookback)
		{
			var closed = ContainerEventConstants.Closed;
			var shippingMethods = new List<string>()
			{
				ContainerConstants.PmodBag,
				ContainerConstants.PmodPallet
			};
			var lookback = DateTime.Now.AddDays(-daysToLookback);
			var query = $@"SELECT * FROM {ContainerName} c
							WHERE c.createDate > @lookback
							AND c.siteName = @siteName
							AND c.status = @closed
							AND ARRAY_CONTAINS(@shippingMethods, c.binLabelType)";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@shippingMethods", shippingMethods)
													.WithParameter("@closed", closed)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForUspsEvsFileAsync(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate)
		{
			var closed = ContainerEventConstants.Closed;
			var carrier = ShippingCarrierConstants.Usps.ToUpper();
			var shippingMethods = new List<string>()
			{
				ContainerConstants.PmodBag,
				ContainerConstants.PmodPallet
			};
			var lookback = DateTime.Now.AddDays(-7);
			var query = $@"SELECT * FROM {ContainerName} c 
                           WHERE c.siteName = @siteName 
                            AND c.isUspsEvsFileProcessed = false 
							AND c.status = @closed
							AND c.shippingCarrier = @carrier
                            AND c.localProcessedDate > @lookbackStartDate
							AND c.localProcessedDate < @lookbackEndDate                            
                            AND ARRAY_CONTAINS(@shippingMethods, c.binLabelType)";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@shippingMethods", shippingMethods)
													.WithParameter("@closed", closed)
													.WithParameter("@lookbackStartDate", lookbackStartDate)
													.WithParameter("@lookbackEndDate", lookbackEndDate)
													.WithParameter("@carrier", carrier);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForEndOfDayProcess(string siteName, DateTime lastScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-14).Date;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 500 * FROM {ContainerName} c
			                        WHERE c.siteName = @siteName
									AND c._ts >= @unixTimeAtLastScan
									AND c.localProcessedDate > @lookback
			                        AND c.eodUpdateCounter > c.eodProcessCounter";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForSqlEndOfDayProcess(string siteName, DateTime targetDate, DateTime lastScanDateTime)
		{
			var startDate = targetDate.Date;
			var endDate = startDate.AddDays(1);
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = startDate;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 500 * FROM {ContainerName} c
			                        WHERE c.siteName = @siteName
									AND c._ts >= @unixTimeAtLastScan
									AND c.localProcessedDate >= @startDate 
									AND c.localProcessedDate < @endDate
			                        AND ((IS_DEFINED(c.sqlEodProcessCounter) = true AND c.eodUpdateCounter > c.sqlEodProcessCounter)
										OR   (IS_DEFINED(c.sqlEodProcessCounter) = false AND c.eodUpdateCounter > 0))";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}
		public async Task<IEnumerable<ShippingContainer>> GetContainersForContainerDatasetsAsync(string siteName, DateTime lastScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-30);
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT * FROM {ContainerName} c
								WHERE c.siteName = @siteName
									AND c._ts >= @unixTimeAtLastScan
									AND c.createDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetClosedContainersForPackageEvsFile(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate)
		{
			var closed = ContainerEventConstants.Closed;
			var query = $@"SELECT * FROM {ContainerName} c
								WHERE c.siteName = @siteName 
									AND c.status = @closed
									AND c.localProcessedDate > @lookbackStartDate
									AND c.localProcessedDate < @lookbackEndDate";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@closed", closed)
													.WithParameter("@lookbackStartDate", lookbackStartDate)
													.WithParameter("@lookbackEndDate", lookbackEndDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersToResetEod(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate)
		{
			var closed = ContainerEventConstants.Closed;
			var query = $@"SELECT * FROM {ContainerName} c 
			                        WHERE c.siteName = @siteName
									AND c.localProcessedDate > @lookbackStartDate
									AND c.localProcessedDate < @lookbackEndDate
			                        AND c.status = @closed";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@closed", closed)
													.WithParameter("@lookbackStartDate", lookbackStartDate)
													.WithParameter("@lookbackEndDate", lookbackEndDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

        public async Task<IEnumerable<ShippingContainer>> GetOutOfDateContainersAsync(string siteName, DateTime localTime)
        {
			var active = ContainerEventConstants.Active;
			var localDateLastMidnight = localTime.Date;
			var query = $@"SELECT * FROM { ContainerName } c 
							WHERE c.siteName = @siteName 
                            AND c.siteCreateDate < @localDateLastMidnight
                            AND c.status = @active
							ORDER BY c.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@localDateLastMidnight", localDateLastMidnight)
													.WithParameter("@active", active);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<bool> HaveContainersChangedForSiteAsync(string siteName, DateTime lastScanDateTime)
		{
			if (lastScanDateTime.Year == 1)
				return true;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 1 c._ts, c.siteName FROM {ContainerName} c
									WHERE c.siteName = @siteName
										AND c._ts >= @unixTimeAtLastScan";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan);
			var results = await this.GetTimestampsAsync(queryDefinition);
			return results.Any();
		}

        public async Task<IEnumerable<ShippingContainer>> GetClosedContainersByDate(DateTime targetDate, string siteName)
        {
			var closed = ContainerEventConstants.Closed;
			var startDate = targetDate.Date;
			var endDate = startDate.AddDays(1);
			var query = $@"SELECT * FROM {ContainerName} c
							WHERE c.status = @closed
									AND c.localProcessedDate >= @startDate 
									AND c.localProcessedDate < @endDate
									AND c.siteName = @siteName";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@closed", closed)
													.WithParameter("@siteName", siteName)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}


		public async Task<BatchDbResponse<ShippingContainer>> UpdateContainersEodProcessed(IEnumerable<ShippingContainer> containers)
		{
			var updateItems = new Dictionary<ShippingContainer, ICollection<PatchOperation>>();
			foreach (var container in containers)			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/eodProcessCounter", container.EodProcessCounter),
					PatchOperation.Set("/isRateAssigned", container.IsRateAssigned),
					PatchOperation.Set("/cost", container.Cost),
					PatchOperation.Set("/charge", container.Charge),
					PatchOperation.Set("/billingWeight", container.BillingWeight ?? "0"),
					PatchOperation.Set("/rateId", container.RateId ?? string.Empty),
					PatchOperation.Set("/rateGroupId", container.RateGroupId ?? string.Empty),
					PatchOperation.Set("/historicalRateIds", container.HistoricalRateIds),
					PatchOperation.Set("/historicalRateGroupIds", container.HistoricalRateGroupIds),
					PatchOperation.Set("/webJobIds", container.WebJobIds)
				};
				updateItems[container] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<ShippingContainer>> UpdateContainersSqlEodProcessed(IEnumerable<ShippingContainer> containers)
		{
			var updateItems = new Dictionary<ShippingContainer, ICollection<PatchOperation>>();
			foreach (var container in containers)			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/sqlEodProcessCounter", container.SqlEodProcessCounter),
					PatchOperation.Set("/isRateAssigned", container.IsRateAssigned),
					PatchOperation.Set("/cost", container.Cost),
					PatchOperation.Set("/charge", container.Charge),
					PatchOperation.Set("/billingWeight", container.BillingWeight ?? "0"),
					PatchOperation.Set("/rateId", container.RateId ?? string.Empty),
					PatchOperation.Set("/rateGroupId", container.RateGroupId ?? string.Empty),
					PatchOperation.Set("/historicalRateIds", container.HistoricalRateIds),
					PatchOperation.Set("/historicalRateGroupIds", container.HistoricalRateGroupIds),
					PatchOperation.Set("/webJobIds", container.WebJobIds)
				};
				updateItems[container] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}
		public async Task<BatchDbResponse<ShippingContainer>> UpdateContainersForRateUpdate(IEnumerable<ShippingContainer> containers)
		{
			var updateItems = new Dictionary<ShippingContainer, ICollection<PatchOperation>>();
			foreach (var container in containers)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isRateAssigned", container.IsRateAssigned),
					PatchOperation.Set("/cost", container.Cost),
					PatchOperation.Set("/charge", container.Charge),
					PatchOperation.Set("/billingWeight", container.BillingWeight ?? "0"),
					PatchOperation.Set("/rateId", container.RateId ?? string.Empty),
					PatchOperation.Set("/rateGroupId", container.RateGroupId ?? string.Empty),
					PatchOperation.Set("/historicalRateIds", container.HistoricalRateIds),
					PatchOperation.Set("/historicalRateGroupIds", container.HistoricalRateGroupIds),
					PatchOperation.Set("/webJobIds", container.WebJobIds)
				};
				updateItems[container] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

        public async Task<BatchDbResponse<ShippingContainer>> UpdateContainersSetRateAssigned(IEnumerable<ShippingContainer> containers)
        {
			var updateItems = new Dictionary<ShippingContainer, ICollection<PatchOperation>>();
			foreach (var container in containers)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isRateAssigned", container.IsRateAssigned),
					PatchOperation.Set("/cost", container.Cost),
					PatchOperation.Set("/charge", container.Charge),
					PatchOperation.Set("/billingWeight", container.BillingWeight ?? "0"),
					PatchOperation.Set("/rateId", container.RateId ?? string.Empty),
					PatchOperation.Set("/rateGroupId", container.RateGroupId ?? string.Empty),
					PatchOperation.Set("/historicalRateIds", container.HistoricalRateIds),
					PatchOperation.Set("/historicalRateGroupIds", container.HistoricalRateGroupIds),
					PatchOperation.Set("/webJobIds", container.WebJobIds)
				};
				updateItems[container] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}
	}
}
