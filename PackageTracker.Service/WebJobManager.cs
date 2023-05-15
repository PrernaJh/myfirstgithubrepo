using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	public class WebJobManager
	{
		private const string MessagePrefix = "WebJobManager |";

		private readonly ILogger<WebJobManager> logger;
		private readonly IConsumerDetailFileService consumerDetailFileService;
		private readonly ICreatedPackagePostProcessService createdPackagePostProcessService;
		private readonly IDuplicateAsnCheckerService duplicateAsnCheckerService;
		private readonly IPackageRecallJobService packageRecallJobService;
		private readonly IQueueManager queueManager;
		private readonly IUpdateActiveGroupDataService updateService;
		private readonly IUpsGeoDescFileService upsGeoDescFileService;
		private readonly IWebJobRunsService webJobRunsService;
		private readonly IZipMapFileService zipMapFileService;
		private readonly IZoneFileService zoneFileService;

		private readonly IDictionary<string, WebJobSettings> webJobSettings;

		public WebJobManager(ILogger<WebJobManager> logger,
			IConfiguration configuration,
			IConsumerDetailFileService consumerDetailFileService,
			ICreatedPackagePostProcessService createdPackagePostProcessService,
			IDuplicateAsnCheckerService duplicateAsnCheckerService,
			IPackageRecallJobService packageRecallJobService,
			IQueueManager queueManager,
			IUpdateActiveGroupDataService updateService,
			IUpsGeoDescFileService upsGeoDescFileService,
			IWebJobRunsService webJobRunsService,
			IZipMapFileService zipMapFileService,
			IZoneFileService zoneFileService)
		{
			this.logger = logger;
			this.consumerDetailFileService = consumerDetailFileService;
			this.createdPackagePostProcessService = createdPackagePostProcessService;
			this.duplicateAsnCheckerService = duplicateAsnCheckerService;
			this.packageRecallJobService = packageRecallJobService;
			this.updateService = updateService;
			this.queueManager = queueManager;
			this.upsGeoDescFileService = upsGeoDescFileService;
			this.webJobRunsService = webJobRunsService;
			this.zipMapFileService = zipMapFileService;
			this.zoneFileService = zoneFileService;

			webJobSettings = configuration.GetSection("WebJobSettings").Get<Dictionary<string, WebJobSettings>>();
		}

		private WebJobSettings GetSettingsForWebJob(string name)
		{
			if (!webJobSettings.TryGetValue(name, out var settings))
			{
				settings = new WebJobSettings { IsEnabled = false };
			}

			return settings;
		}


		[FunctionName("UpsGeoDescFileWatcher")]
		public async Task UpsGeoDescFileWatcher([BlobTrigger("%UpsGeoDescFileImport%/{fileName}", Connection = "AzureWebJobsStorage")] Stream fileStream, string fileName)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("UpsGeoDescFileWatcher");
				if (webJobSettings.IsEnabled)
				{
					using (fileStream)
					{
						await upsGeoDescFileService.ProcessUpsGeoDescFileAsync(fileStream, fileName);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}


		[FunctionName("ZipMapFileWatcher")]
		public async Task ZipMapFileWatcher([BlobTrigger("%ZipMapFileImport%/{fileName}", Connection = "AzureWebJobsStorage")] Stream fileStream, string fileName)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("ZipMapFileWatcher");
				if (webJobSettings.IsEnabled)
				{
					using (fileStream)
					{
						await zipMapFileService.ProcessZipMapFileAsync(fileStream, fileName);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("ZoneFileWatcher")]
		public async Task ZoneFileWatcher([BlobTrigger("%ZoneFileImport%/{fileName}", Connection = "AzureWebJobsStorage")] Stream fileStream, string fileName)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("ZoneFileWatcher");
				if (webJobSettings.IsEnabled)
				{
					using (fileStream)
					{
						await zoneFileService.ProcessZoneFileAsync(fileStream, fileName);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("ConsumerDetailFileExportJob")]
		public async Task ConsumerDetailFileExportJob([TimerTrigger("%WebJobSettings:ConsumerDetailFileExportJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("ConsumerDetailFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await consumerDetailFileService.ExportConsumerDetailFile();
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("PostProcessCreatedPackagesJob")]
		public async Task PostProcessCreatedPackagesJob([TimerTrigger("%WebJobSettings:PostProcessCreatedPackagesJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("PostProcessCreatedPackagesJob");
				if (webJobSettings.IsEnabled)
				{
					await createdPackagePostProcessService.PostProcessCreatedPackages();
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("MonitorAsnFileImport")]
		public async Task MonitorAsnFileImport([TimerTrigger("%WebJobSettings:MonitorAsnFileImport:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("MonitorAsnFileImport");
				if (webJobSettings.IsEnabled)
				{
					await webJobRunsService.MonitorRecentAsnImportsAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("MonitorDuplicateAsns")]
		public async Task MonitorDuplicateAsns([TimerTrigger("%WebJobSettings:MonitorDuplicateAsns:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("MonitorDuplicateAsns");
				if (webJobSettings.IsEnabled)
				{
					await duplicateAsnCheckerService.CheckForDuplicateAsns(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("UpdatePackageBinsAndBinMapsJob")]
		public async Task UpdatePackageBinsAndBinMapsJob([TimerTrigger("%WebJobSettings:UpdatePackageBinsAndBinMapsJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("UpdatePackageBinsAndBinMapsJob");
				if (webJobSettings.IsEnabled)
				{
					await updateService.UpdatePackageBinsAndBinMapsAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("PackageRecallJob")]
		public async Task PackageRecallJob([QueueTrigger("%PackageRecallJobQueue%")] string message)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("PackageRecallJob");
				if (webJobSettings.IsEnabled)
				{
					await packageRecallJobService.ProcessRecalledPackages(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("UpdateServiceRuleGroupIds")]
		public async Task UpdateServiceRuleGroupIds([QueueTrigger("%UpdatePackageServiceRuleGroupQueue%")] string message)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("UpdatePackageServiceRuleGroupJob");
				if (webJobSettings.IsEnabled)
				{
					await updateService.UpdateServiceRuleGroupIds(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}
	}
}
