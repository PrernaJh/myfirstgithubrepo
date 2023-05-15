using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEvsFileProcessor = PackageTracker.EodService.Interfaces.IEvsFileProcessor;

namespace PackageTracker.EodService.Services
{
	public class EvsFileService : IEvsFileService
	{
		private readonly ILogger<EvsFileService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IEodService eodService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly IEvsFileProcessor evsFileProcessor;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly ISequenceProcessor sequenceProcessor;

		public EvsFileService(ILogger<EvsFileService> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IFileShareHelper fileShareHelper,
			IEvsFileProcessor evsFileProcessor,
			IEmailService emailService,
			IEodService eodService,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			IWebJobRunProcessor webJobRunProcessor,
			ISequenceProcessor sequenceProcessor)
		{
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.config = config;
			this.evsFileProcessor = evsFileProcessor;
			this.emailService = emailService;
			this.eodService = eodService;
			this.fileShareHelper = fileShareHelper;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
			this.sequenceProcessor = sequenceProcessor;
		}

		public async Task ExportEvsFile(string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message, true);
			logger.LogInformation($"EOD: eVs File incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.UspsEvsExportJobType, username))
				{
					return;
				}

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.UspsEvsExportJobType,
					JobName = "USPS eVs File Export",
					Message = "USPS eVs File Export started",
					Username = username
				}); 
				
				var response = await evsFileProcessor.GenerateEvsEodFile(site, dateToProcess);
				var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var fileExportPath = config.GetSection("UspsEvsFileExport").Value;
				var fileDetails = new List<FileDetail>();

				if (response.FileContents.Any())
				{
					var fileName = response.FileName;
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("USPS eVs File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					var fileShareName = config.GetSection("UspsEvsFileExportFileShare").Value;
					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("USPS eVs File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
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
					Message = $"USPS eVs File Export complete for site: {site.SiteName}",
					FileDetails = fileDetails
				});

				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export USPS eVs File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Usps eVs File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}

		public async Task ExportPmodEvsFile(string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message, true);
			logger.LogInformation($"EOD: eVs PMOD file incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.UspsEvsPmodExportJobType, username))
				{
					return;
				}

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.UspsEvsPmodExportJobType,
					JobName = "USPS eVs PMOD File Export",
					Message = "USPS eVs PMOD File Export started",
					Username = username
				}); 
				
				var response = await evsFileProcessor.GeneratePmodEvsEodFile(site, dateToProcess);
				var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var fileExportPath = config.GetSection("UspsEvsFileExport").Value;
				var fileDetails = new List<FileDetail>();

				if (response.FileContents.Any())
				{
					var fileName = response.FileName;
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("USPS eVs PMOD File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					var fileShareName = config.GetSection("UspsEvsFileExportFileShare").Value;
					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("USPS eVs File PMOD Export to FileShare", $"{fileShareName}/{fileName}", response)}");
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
					Message = $"USPS eVs PMOD File Export complete for site: {site.SiteName}",
					FileDetails = fileDetails
				});

				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export USPS PMOD eVs File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Usps eVs PMOD File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
