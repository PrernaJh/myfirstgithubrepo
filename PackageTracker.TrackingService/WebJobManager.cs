using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.TrackingService.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.TrackingService
{
	public class WebJobManager
	{
		private const string MessagePrefix = "WebJobManager |";

		private readonly ILogger<WebJobManager> logger;
		private readonly IReportService reportService;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ITrackPackageService trackPackageService;

		private readonly IDictionary<string, WebJobSettings> webJobSettings;

		public WebJobManager(IConfiguration configuration,
			ILogger<WebJobManager> logger,
			IReportService reportService,
			ISemaphoreManager semaphoreManager,
			ITrackPackageService trackPackageService)
		{
			this.logger = logger;
			this.reportService = reportService;
			this.semaphoreManager = semaphoreManager;
			this.trackPackageService = trackPackageService;
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

		[FunctionName("UpdateDatasetsJob")]
		public async Task UpdateDatasetsJob([TimerTrigger("%WebJobSettings:UpdateDatasetsJob:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("UpdateDatasetsJob");
				if (webJobSettings.IsEnabled)
				{
					await reportService.UpdateBinDatasets();
					await reportService.UpdateSubClientDatasets();
					await reportService.UpdateShippingContainerDatasets();
					await reportService.UpdateJobDatasets();
					await reportService.UpdatePackageDatasets();
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}

		[FunctionName("MonitorEodPackagesJob")]
		public async Task MonitorEodPackagesJob([QueueTrigger("%MonitorEodPackagesQueue%")] string message)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("MonitorEodPackagesJob");
				if (webJobSettings.IsEnabled)
				{
					await reportService.MonitorEodPackages(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}

		[FunctionName("FedExScanDataFileImportWatcher")]
		public async Task FedExScanDataFileImportWatcher([TimerTrigger("%WebJobSettings:FedExScanDataFileImportWatcher:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("FedExScanDataFileImportWatcher");
				if (webJobSettings.IsEnabled)
				{
					await trackPackageService.ProcessFedExScanDataFilesAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}

		[FunctionName("UpsTrackPackageJob")]
		public async Task UpsTrackPackageJob([TimerTrigger("%WebJobSettings:UpsTrackPackageJob:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("UpsTrackPackageJob");
				if (webJobSettings.IsEnabled)
				{
					await trackPackageService.ProcessUpsTrackPackageDataAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}

		[FunctionName("UspsScanDataFileImportWatcher")]
		public async Task UspsScanDataFileImportWatcher([TimerTrigger("%WebJobSettings:UspsScanDataFileImportWatcher:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("UspsScanDataFileImportWatcher");
				if (webJobSettings.IsEnabled)
				{
					await trackPackageService.ProcessUspsScanDataFilesAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}

		[FunctionName("UspsScanDataUpdater")]
		public async Task UspsScanDataUpdater([TimerTrigger("%WebJobSettings:UspsScanDataUpdater:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"UPDATE_DATASETS");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("UspsScanDataUpdater");
				if (webJobSettings.IsEnabled)
				{
					await trackPackageService.UpdateMissingPackageUspsScanDataAsync(webJobSettings);
					await trackPackageService.UpdateMissingContainerUspsScanDataAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
