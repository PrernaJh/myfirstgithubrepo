using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService
{
	public class WebJobManager
	{
		private const string MessagePrefix = "WebJobManager |";

		private readonly ILogger<WebJobManager> logger;
		private readonly ICleanupService cleanupService;
		private readonly IContainerDetailService containerDetailFileService;
		private readonly IEodService eodService;
		private readonly IEvsFileService evsFileService;
		private readonly IInvoiceExpenseService invoiceExpenseService;
		private readonly IPackageDetailService packageDetailFileService;
		private readonly IQueueManager queueManager;
		private readonly IReturnAsnService returnAsnFileService;

		private readonly IDictionary<string, WebJobSettings> webJobSettings;

		private readonly int endOfDayBlockMinutes = 0;

		public WebJobManager(ILogger<WebJobManager> logger,
			ICleanupService cleanupService,
			IConfiguration configuration,
			IContainerDetailService containerDetailFileService,
			IEodService eodService,
			IEvsFileService evsFileService,
			IInvoiceExpenseService invoiceExpenseService,
			IPackageDetailService packageDetailFileService,
			IQueueManager queueManager,
			IReturnAsnService returnAsnFileService
			)
		{
			this.logger = logger;
			this.cleanupService = cleanupService;
			this.containerDetailFileService = containerDetailFileService;
			this.eodService = eodService;
			this.evsFileService = evsFileService;
			this.invoiceExpenseService = invoiceExpenseService;
			this.packageDetailFileService = packageDetailFileService;
			this.queueManager = queueManager;
			this.returnAsnFileService = returnAsnFileService;

			webJobSettings = configuration.GetSection("WebJobSettings").Get<Dictionary<string, WebJobSettings>>();
			int.TryParse(configuration.GetSection("EndOfDayBlockMinutes").Value, out endOfDayBlockMinutes);
		}

		private WebJobSettings GetSettingsForWebJob(string name)
		{
			if (!webJobSettings.TryGetValue(name, out var settings))
			{
				settings = new WebJobSettings { IsEnabled = false };
			}

			return settings;
		}

#if true // Set true after removing Obsolete functions from Service
		[FunctionName("ReturnAsnFileExportJob")]
		public async Task ReturnAsnFileExportJob([QueueTrigger("%ReturnAsnFileJobQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("ReturnAsnFileJobQueue", message, endOfDayBlockMinutes))
					return;
				var webJobSettings = GetSettingsForWebJob("ReturnAsnFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await returnAsnFileService.ExportReturnAsnFiles(webJobSettings, message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("ContainerDetailFileExportJob")]
		public async Task ContainerDetailFileExportJob([QueueTrigger("%ContainerDetailFileJobQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("ContainerDetailFileJobQueue", message, endOfDayBlockMinutes))
					return;
				var webJobSettings = GetSettingsForWebJob("ContainerDetailFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await containerDetailFileService.ExportContainerDetailFile(message);
					await containerDetailFileService.ExportPmodContainerDetailFile(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("PackageDetailExportJob")]
		public async Task PackageDetailExportJob([QueueTrigger("%PackageDetailFileJobQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("PackageDetailFileJobQueue", message, endOfDayBlockMinutes))
					return;
				var webJobSettings = GetSettingsForWebJob("PackageDetailFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await packageDetailFileService.ExportPackageDetailFile(webJobSettings, message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("InvoiceExpenseExportJob")]
		public async Task InvoiceExpenseExportJob([QueueTrigger("%InvoiceExpenseFileJobQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("InvoiceExpenseFileJobQueue", message, endOfDayBlockMinutes))
					return;
				var webJobSettings = GetSettingsForWebJob("InvoiceExpenseFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await invoiceExpenseService.ProcessInvoiceFiles(webJobSettings, message);
					await invoiceExpenseService.ProcessExpenseFiles(webJobSettings, message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("UspsEvsFileExportJob")]
		public async Task UspsEvsFileExportJob([QueueTrigger("%UspsEvsFileJobQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("UspsEvsFileJobQueue", message, endOfDayBlockMinutes))
					return;
				var webJobSettings = GetSettingsForWebJob("UspsEvsFileExportJob");
				if (webJobSettings.IsEnabled)
				{
					await evsFileService.ExportEvsFile(message);
					await evsFileService.ExportPmodEvsFile(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("ResetPackageEod")]
		public async Task ResetPackageEod([QueueTrigger("%ResetPackageEodQueue%")] string message)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("ResetPackageEodJob");
				if (webJobSettings.IsEnabled)
				{
					await eodService.ResetPackageEod(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("ResetContainerEod")]
		public async Task ResetContainerEod([QueueTrigger("%ResetContainerEodQueue%")] string message)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("ResetContainerEodJob");
				if (webJobSettings.IsEnabled)
				{
					await eodService.ResetContainerEod(message);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("MonitorEodJob")]
		public async Task MonitorEodJob([TimerTrigger("%WebJobSettings:MonitorEodJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("MonitorEodJob");
				if (webJobSettings.IsEnabled)
				{
					await eodService.MonitorEod(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("WeeklyFinancialReportsJob")]
		public async Task WeeklyFinancialReportsJob([TimerTrigger("%WebJobSettings:WeeklyFinancialReportsJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("WeeklyFinancialReportsJob");
				if (webJobSettings.IsEnabled)
				{
					await invoiceExpenseService.ProcessPeriodicExpenseFiles(webJobSettings);
					await invoiceExpenseService.ProcessPeriodicInvoiceFiles(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("TriggerEodJobsJob")]
		public async Task TriggerEodJobsJob([QueueTrigger("%TriggerEodJobsQueue%")] string message)
		{
			try
			{
				if (queueManager.IsDuplicateQueueMessage("TriggerEodJobsQueue", message, endOfDayBlockMinutes))
					return;
				if (await eodService.StartEod(message))
                {
					await UspsEvsFileExportJob(message);
					await ContainerDetailFileExportJob(message);
					await PackageDetailExportJob(message);
					await InvoiceExpenseExportJob(message);
					await ReturnAsnFileExportJob(message);
                }
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}
#endif


#if true // Set true after removing Obsolete functions from Service
		[FunctionName("CreateSqlEodCollectionsJob")]
		public async Task CreateSqlEodCollectionsJob([TimerTrigger("%WebJobSettings:CreateSqlEodCollectionsJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("CreateSqlEodCollectionsJob");
				if (webJobSettings.IsEnabled)
				{
					await eodService.ProcessEndOfDayContainers();
					await eodService.ProcessEndOfDayPackages();
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}

		[FunctionName("CleanupSqlEodCollectionsJob")]
		public async Task CleanupSqlEodCollections([TimerTrigger("%WebJobSettings:CleanupSqlEodCollectionsJob:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("CleanupSqlEodCollectionsJob");
				if (webJobSettings.IsEnabled)
				{
					await cleanupService.CleanupEodCollectionsAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}
#endif
	}
}
