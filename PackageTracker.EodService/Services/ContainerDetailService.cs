using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IContainerDetailProcessor = PackageTracker.EodService.Interfaces.IContainerDetailProcessor;

namespace PackageTracker.EodService.Services
{
	public class ContainerDetailService : IContainerDetailService
	{		
		private readonly ILogger<ContainerDetailService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IContainerDetailProcessor containerDetailProcessor;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IEodService eodService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public ContainerDetailService(ILogger<ContainerDetailService> logger,
			IBlobHelper blobHelper,
			IContainerDetailProcessor containerDetailProcessor,
			IConfiguration config,
			IEmailService emailService,
			IEodService eodService,
			IFileShareHelper fileShareHelper,			
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{			
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.containerDetailProcessor = containerDetailProcessor;
			this.config = config;
			this.emailService = emailService;
			this.eodService = eodService;
			this.fileShareHelper = fileShareHelper;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ExportContainerDetailFile(string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Container Detail incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: Container Detail File Export for site: {siteName}");
			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.ContainerDetailExportJobType, username))
				{
					return;
				}

				var fileDetails = new List<FileDetail>();
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.ContainerDetailExportJobType,
					JobName = "Container Detail File Export",
					Message = "Container Detail File Export started",
					Username = username
				});

				var fileExportPath = config.GetSection("ContainerDetailFileExport").Value;
				var fileName = $"ASN_{site.SiteName}_{dateToProcess:yyyyMMddHHmm}";
					
				var response = await containerDetailProcessor.ExportContainerDetailFile(site, dateToProcess, webJobRun.Id);
				if (response.FileContents.Any())
				{
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Container Detail File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					var fileShareName = config.GetSection("ContainerDetailFileExportFileShare").Value;

					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Container Detail File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
					}

					fileDetails.Add(new FileDetail
					{
						FileName = response.NumberOfRecords != 0 ? fileName : string.Empty,
						FileArchiveName = response.NumberOfRecords != 0 ? fileName : string.Empty,
						NumberOfRecords = response.NumberOfRecords
					});
				}

				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfRecords,
					Message = "Container Detail File Export complete",
					FileDetails = fileDetails
				});

				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export ContainerDetail File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("ContainerDetail File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}

		public async Task ExportPmodContainerDetailFile(string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: PMOD Container Detail incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: PMOD Container Detail File Export for site: {siteName}");

			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.PmodContainerDetailExportJobType, username))
				{
					return;
				}

				var fileDetails = new List<FileDetail>();
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.PmodContainerDetailExportJobType,
					JobName = "PMOD Container Detail File Export",
					Message = "PMOD Container Detail File Export started",
					Username = username
				});

				var fileExportPath = config.GetSection("PmodContainerDetailFileExport").Value;
				var fileName = $"600014_RPT_D301_{site.SiteName}_{dateToProcess:yyyyMMdd}9999";

				var response = await containerDetailProcessor.ExportPmodContainerDetailFile(site, dateToProcess, webJobRun.Id);
				if (response.FileContents.Any())
				{
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("PMOD Container Detail File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					var fileShareName = config.GetSection("PmodContainerDetailFileExportFileShare").Value;

					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("PMOD Container Detail File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
					}

					fileDetails.Add(new FileDetail
					{
						FileName = response.NumberOfRecords != 0 ? fileName : string.Empty,
						FileArchiveName = response.NumberOfRecords != 0 ? fileName : string.Empty,
						NumberOfRecords = response.NumberOfRecords
					});
				}

				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfRecords,
					Message = "PMOD Container Detail File Export complete",
					FileDetails = fileDetails
				});

				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export PMOD Container Detail File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("PMOD Container Detail File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
