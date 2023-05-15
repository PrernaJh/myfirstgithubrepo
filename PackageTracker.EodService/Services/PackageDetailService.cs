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
using IPackageDetailProcessor = PackageTracker.EodService.Interfaces.IPackageDetailProcessor;

namespace PackageTracker.EodService.Services
{
	public class PackageDetailService : IPackageDetailService
	{
		private readonly ILogger<PackageDetailService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IEodService eodService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly IPackageDetailProcessor packageDetailProcessor;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public PackageDetailService(ILogger<PackageDetailService> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IEmailService emailService,
			IEodService eodService,
			IFileShareHelper fileShareHelper,
			IPackageDetailProcessor packageDetailProcessor,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.eodService = eodService;
			this.fileShareHelper = fileShareHelper;
			this.packageDetailProcessor = packageDetailProcessor;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ExportPackageDetailFile(WebJobSettings webJobSettings, string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Package Detail incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: Package Detail File Export for site: {siteName}");

			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.PackageDetailExportJobType, username))
				{
					return;
				}

				var fileDetails = new List<FileDetail>();
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.PackageDetailExportJobType,
					JobName = "Package Detail File Export",
					Message = "Package Detail File Export started",
					Username = username
				});					

				var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var isHistorical = siteCreateDate.Date != dateToProcess.Date;
				var addPackageRatingElements = webJobSettings.GetParameterBoolValue("AddPackageRatingElements", true);

				var fileExportPath = config.GetSection("PackageDetailFileExport").Value;
				var fileName = $"600014_RPT_D301_{site.SiteName}_{siteCreateDate:yyyyMMddHHmm}";
				var response = await packageDetailProcessor.GeneratePackageDetailFile(
					site, dateToProcess, webJobRun.Id, isHistorical, addPackageRatingElements);

				if (response.FileContents.Any())
				{
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Package Detail File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					var fileShareName = config.GetSection("PackageDetailFileExportFileShare").Value;
					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Package Detail File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
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
					Message = "Package Detail File Export complete",
					FileDetails = fileDetails
				});
				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export Package Detail File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Package Detail File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}

		}
	}
}
