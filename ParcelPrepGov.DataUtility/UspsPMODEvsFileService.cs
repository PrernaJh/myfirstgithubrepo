using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public class UspsPmodEvsFileService : IUspsPmodEvsFileService
	{
		private readonly ILogger<UspsEvsFileService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly IUspsEvsProcessor uspsEvsFileProcessor;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly ISequenceProcessor sequenceProcessor;

		public UspsPmodEvsFileService(ILogger<UspsEvsFileService> logger, 
			IBlobHelper blobHelper, 
			IConfiguration config, 
			IFileShareHelper fileShareHelper, 
			IUspsEvsProcessor uspsEvsFileProcessor, 
			IEmailService emailService,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor, 
			IWebJobRunProcessor webJobRunProcessor, 
			ISequenceProcessor sequenceProcessor)
		{
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.config = config;
			this.uspsEvsFileProcessor = uspsEvsFileProcessor;
			this.emailService = emailService;
			this.fileShareHelper = fileShareHelper;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
			this.sequenceProcessor = sequenceProcessor;
		}

		public async Task ExportUspsEvsFileForPMODContainers(string message)
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
			logger.LogInformation($"EOD: USPS eVs PMOD File Export for site: {siteName}");

			try
			{
				var response = await uspsEvsFileProcessor.ExportUspsEvsFileForPMODContainers(site, endOfDayQueueMessage);
				var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var fileExportPath = config.GetSection("UspsEvsFileExport").Value;
				var sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(siteName, SequenceTypeConstants.EvsFileName, SequenceTypeConstants.FourDigitMaxSequence);
				var fileName = $"USPS_eVs_{dateToProcess:yyyMMdd}{sequence.Number.ToString().PadLeft(4, '0')}.ssf.manifest";

				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					SiteName = siteName,
					ProcessedDate = dateToProcess,
					JobName = "USPS eVs PMOD File Export",
					JobType = WebJobConstants.UspsEvsPmodExportJobType,
					Username = endOfDayQueueMessage.Username,
					Message = string.Empty,
					FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName } },
					IsSuccessful = response.IsSuccessful,
					LocalCreateDate = siteCreateDate
				});

				if (response.FileContents.Any())
				{
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

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("USPS eVs File For PMOD Export to FileShare", $"{fileShareName}/{fileName}", response)}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export USPS eVs File For PMOD. Exception: { ex }");
				emailService.SendServiceErrorNotifications("UspsEvs File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
