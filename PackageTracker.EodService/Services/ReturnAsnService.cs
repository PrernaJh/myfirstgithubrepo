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
using IReturnAsnProcessor = PackageTracker.EodService.Interfaces.IReturnAsnProcessor;

namespace PackageTracker.EodService.Services
{
	public class ReturnAsnService : IReturnAsnService
	{
		private readonly ILogger<ReturnAsnService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IEodService eodService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly IReturnAsnProcessor returnAsnProcessor;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;


		public ReturnAsnService(ILogger<ReturnAsnService> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IEmailService emailService,
			IEodService eodService,
			IFileShareHelper fileShareHelper,			
			IReturnAsnProcessor returnAsnProcessor,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.eodService = eodService;
			this.fileShareHelper = fileShareHelper;
			this.returnAsnProcessor = returnAsnProcessor;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;

		}

		public async Task ExportReturnAsnFiles(WebJobSettings webJobSettings, string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Return ASN incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";
			var addPackageRatingElements = webJobSettings.GetParameterBoolValue("AddPackageRatingElements", true);

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: Return ASN File Export for site: {siteName}");

			try
			{
				if ((! force) && await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.ReturnAsnExportJobType, username))
				{
					return;
				}

				var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);			
				var subClients = await subClientProcessor.GetSubClientsBySiteNameAsync(site.SiteName);
				foreach (var subClient in subClients)
				{
					await ExportReturnAsnFile(site, subClient, username, dateToProcess, siteCreateDate, addPackageRatingElements);
				}
			}
			finally
			{
				semaphore.Release();
			}
		}

		private async Task ExportReturnAsnFile(
			Site site, SubClient subClient, string username, DateTime dateToProcess, DateTime siteCreateDate, bool addPackageRatingElements)
		{
			try
			{
				var fileName = AsnFileUtility.FormatExportFileName(subClient, subClient.AsnExportFileNameFormat, dateToProcess);				
				var isSimplified = subClient.ClientName == ClientSubClientConstants.DalcClientName;
				var fileDetails = new List<FileDetail>();

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest 
				{
					Site = site,
					SubClientName = subClient.Name,
					ProcessedDate = dateToProcess,
					WebJobTypeConstant = WebJobConstants.ReturnAsnExportJobType,
					JobName = "Return ASN File Export",
					Message = "Return ASN File Export started",
					Username = username
				});				

				var response = await returnAsnProcessor.GenerateReturnAsnFile(
					site, subClient, dateToProcess, webJobRun.Id, isSimplified, addPackageRatingElements);

				if (response.FileContents.Any())
				{
					var fileExportPath = $"{config.GetSection("ReturnAsnFileExport").Value}/{subClient.ClientName}/{subClient.SiteName}";
					await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("ReturnAsn File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					if (StringHelper.Exists(config.GetSection("FinancialReturnAsn").Value))
					{
						var financialExportPath = $"{config.GetSection("FinancialReturnAsn").Value}/{subClient.ClientName}/{subClient.SiteName}";
						await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, financialExportPath, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("ReturnAsn File Export to Container", $"{financialExportPath}/{fileName}", response)}");
					}

					var fileShareName = subClient.AsnExportLocation;
					if (StringHelper.Exists(fileShareName))
					{
						fileName += ".txt";
						await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);

						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("ReturnAsn File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
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
					Message = $"Return ASN File Export complete for subclient: {subClient.Name}",
					FileDetails = fileDetails
				});

				await eodService.CheckEodComplete(site, dateToProcess);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export ReturnAsn File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("ReturnAsn File Processor", ex.ToString());
			}
		}
	}
}
