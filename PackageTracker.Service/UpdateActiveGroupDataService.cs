using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	class UpdateActiveGroupDataService : IUpdateActiveGroupDataService
	{
		private readonly ILogger<UpdateActiveGroupDataService> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinProcessor binProcessor;
		private readonly IPackagePostProcessor packagePostProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public UpdateActiveGroupDataService(ILogger<UpdateActiveGroupDataService> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IBinProcessor binProcessor,
			IPackagePostProcessor packagePostProcessor,
			IPackageRepository packageRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;
			this.activeGroupProcessor = activeGroupProcessor;
			this.binProcessor = binProcessor;
			this.packagePostProcessor = packagePostProcessor;
			this.packageRepository = packageRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task UpdatePackageBinsAndBinMapsAsync(WebJobSettings webJobSettings)
		{
			try
			{
				var successCount = 0L;
				var failureCount = 0L;
				var sites = await siteProcessor.GetAllSitesAsync();
				var subClients = await subClientProcessor.GetSubClientsAsync();

				var lookback = webJobSettings.GetParameterIntValue("Lookback", 7);				// days
				var chunk = webJobSettings.GetParameterIntValue("ChunkSize", 10000);

				var webJob = new WebJobRunRequest
				{
					Id = Guid.NewGuid().ToString(),
					SiteName = SiteConstants.AllSites,
					JobName = "Update Package Bins and Bin Maps",
					JobType = WebJobConstants.UpdatePackageBinsAndBinMapsJobType,
					Username = "System"
				};

				for (; ;)
                {
					// Process 'chunk' records for each sub-client, until there is nothing left to process.
					var processed = 0;
					foreach (var subClient in subClients)
					{
						var site = sites.FirstOrDefault(s => s.SiteName == subClient.SiteName);
						// This is set to run each hour during the early morning hours.
						// This will allow the more easterly sites to get started earlier, 
						//		because their new bins will become active first after midnight local time.
						if (webJobSettings.IsDuringScheduledHours(site))
						{
							var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(subClient.SiteName, site.TimeZone);
							var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
							var packagesToUpdate = (await packagePostProcessor
								.GetPackagesWithOutdatedBinData(lookback, chunk, subClient.Name, binGroupId, binMapGroupId)).ToList();
							if (!packagesToUpdate.Any())
								continue;
							logger.Log(LogLevel.Information, $"Update package bins and bin maps: {packagesToUpdate.Count()} packages for subClient {subClient.Name}.");

							var isUpdate = true;
							await binProcessor.AssignBinsForListOfPackagesAsync(packagesToUpdate, binGroupId, binMapGroupId, isUpdate);
							var bulkUpdateResponse = await packageRepository.UpdatePackagesSetBinData(packagesToUpdate);
							logger.LogInformation($"Request Units Consumed by UpdatePackagesSetBinData: {bulkUpdateResponse.RequestCharge}");
							if (bulkUpdateResponse.IsSuccessful)
							{
								successCount += bulkUpdateResponse.Count;
								logger.Log(LogLevel.Information, $"Update package bins and bin maps updated {bulkUpdateResponse.Count} packages for subClient {subClient.Name}.");
							}
							else
							{
								failureCount += bulkUpdateResponse.FailedCount;
								logger.Log(LogLevel.Error, $"Update package bins and bin maps failed to update {bulkUpdateResponse.FailedCount} packages for subClient {subClient.Name}.");
								break; // Try again later?
							}
							processed = packagesToUpdate.Count();
						}
					}
					if (processed == 0)
						break;
                }

				webJob.IsSuccessful = failureCount == 0;

				await webJobRunProcessor.AddWebJobRunAsync(webJob);
			}
			catch (Exception ex)
			{
				var message = $"Update package bins and bin maps failed, Exception: {ex.Message}";
				logger.Log(LogLevel.Error, message);
			}
		}

		public async Task UpdateServiceRuleGroupIds(string message)
		{
			try
			{
				logger.LogInformation($"Begin update of service rule group ids. Queue message: {message}");
				var webJobId = Guid.NewGuid().ToString();
				var updateQueueMessage = QueueUtility.ParseUpdateQueueMessage(message);
				
				var isSuccessful = await packagePostProcessor.UpdatePackageServiceRuleGroupIds(updateQueueMessage.Name, updateQueueMessage.DaysToLookback, webJobId);

				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					Id = webJobId,
					SubClientName = updateQueueMessage.Name,
					JobName = "Update Package Service Rule Group Ids",
					JobType = WebJobConstants.UpdateServiceRuleGroupsType,
					Username = "System",
					IsSuccessful = isSuccessful
				});
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to update service rule group ids. Queue message: {message} Exception: {ex}");
				throw;
			}
		}
	}
}
