using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.ArchiveService.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PackageTracker.Domain.Utilities;

namespace PackageTracker.ArchiveService
{
	public class WebJobManager
	{
		private const string MessagePrefix = "WebJobManager |";

		private readonly ILogger<WebJobManager> logger;
		private readonly IArchiveDataService archiveDataService;
		private readonly IHistoricalDataService historicalDataService;
		private readonly IReportService reportService;
		private readonly ISemaphoreManager semaphoreManager;

		private readonly IDictionary<string, WebJobSettings> webJobSettings;

		public WebJobManager(IConfiguration configuration,
			ILogger<WebJobManager> logger,
			IArchiveDataService archiveDataService,
			IHistoricalDataService historicalDataService,
			IReportService reportService,
			ISemaphoreManager semaphoreManager
			)
		{
			this.logger = logger;
			this.archiveDataService = archiveDataService;
			this.historicalDataService = historicalDataService;
			this.reportService = reportService;
			this.semaphoreManager = semaphoreManager;
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

		[FunctionName("ArchiveDataExporter")]
		public async Task ArchiveDataExporter([TimerTrigger("%WebJobSettings:ArchiveDataExporter:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"ARCHIVE_DATA");
			await semaphore.WaitAsync(); 
			try
			{
				var webJobSettings = GetSettingsForWebJob("ArchiveDataExporter");
				if (webJobSettings.IsEnabled)
				{
					await archiveDataService.ArchivePackagesAsync(webJobSettings);
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

		[FunctionName("HistoricalDataFileImportWatcher")]
		public async Task HistoricalDataFileImportWatcher([TimerTrigger("%WebJobSettings:HistoricalDataFileImportWatcher:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"ARCHIVE_DATA");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("HistoricalDataFileImportWatcher");
				if (webJobSettings.IsEnabled)
				{
					await historicalDataService.FileImportWatcher(webJobSettings);
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

		[FunctionName("ReportService")]
		public async Task ReportService([TimerTrigger("%WebJobSettings:ReportService:JobTimer%")] TimerInfo jobTimer)
		{
			var semaphore = semaphoreManager.GetSemaphore($"REPORT_GENERATION");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("ReportService");
				if (webJobSettings.IsEnabled)
				{
					var targetDate = DateTime.UtcNow.AddDays(-1).Date;
					await reportService.CreateDailyContainerPackageNestingReport(targetDate, "System");
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

		[FunctionName("DailyContainerNestingReportJob")]
		public async Task DailyContainerNestingReportJob([QueueTrigger("%DailyContainerNestingReportJobQueue%")] string message)
		{
			var semaphore = semaphoreManager.GetSemaphore($"REPORT_GENERATION");
			await semaphore.WaitAsync();
			try
			{
				var webJobSettings = GetSettingsForWebJob("DailyContainerNestingReportJob");
				if (webJobSettings.IsEnabled)
				{
					var queueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
					await reportService.CreateDailyContainerPackageNestingReport(queueMessage.TargetDate, queueMessage.Username);
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
