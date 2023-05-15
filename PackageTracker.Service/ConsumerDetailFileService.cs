using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	public class ConsumerDetailFileService : IConsumerDetailFileService
	{
		private readonly ILogger<ConsumerDetailFileService> logger;

		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IConsumerDetailFileProcessor fileProcessor;
		private readonly IEmailService emailService;
		private readonly IFileShareHelper fileShareHelper;
		private readonly IPackageRepository packageRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public ConsumerDetailFileService(ILogger<ConsumerDetailFileService> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IConsumerDetailFileProcessor fileProcessor,
			IEmailService emailService,
			IFileShareHelper fileShareHelper,
			IPackageRepository packageRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;

			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.fileShareHelper = fileShareHelper;
			this.fileProcessor = fileProcessor;
			this.packageRepository = packageRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ExportConsumerDetailFile()
		{
			var fileExportPath = config.GetSection("ConsumerDetailFileExport").Value;
			var subClients = (await subClientProcessor.GetSubClientsAsync())
				.Where(s => s.ClientName == ClientSubClientConstants.CmopClientName && s.SendConsumerDetailFile);
			foreach (var subClient in subClients)
			{
				var mostRecentRun = await webJobRunProcessor.GetMostRecentJobRunBySubClientAndJobType(
					subClient.SiteName, subClient.Name, WebJobConstants.ConsumerDetailExportJobType, true);
				if (!await packageRepository.HavePackagesChangedForSiteAsync(subClient.SiteName, mostRecentRun.CreateDate))
					continue;
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					SubClientName = subClient.Name,
					WebJobTypeConstant = WebJobConstants.ConsumerDetailExportJobType,
					JobName = "Consumer Detail File Export",
					Message = "Consumer Detail File Export started"
				});
				var formattedTime = webJobRun.CreateDate.ToString("MMddyyyhh24mm");
				var fileName = $"400012_RTN1_{subClient.Key}_{formattedTime}"; // this is a legacy fileName from BestWay that we have not been given a replacement schema for
				var response = new FileExportResponse();
				try
				{
					response = await fileProcessor.GetConsumerDetailFileAsync(subClient, webJobRun.Id, mostRecentRun.CreateDate, webJobRun.CreateDate);
					if (response.FileContents.Any())
					{
						await blobHelper.UploadListOfStringsToBlobAsync(response.FileContents, fileExportPath, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Consumer Detail File Export to blob Container", $"{fileExportPath}/{fileName}", response)}");

						var fileShareName = StringHelper.Exists(subClient.ConsumerDetailFileExportLocation)
							? subClient.ConsumerDetailFileExportLocation
							: config.GetSection("ConsumerDetailFileExportFileShare").Value;
						if (StringHelper.Exists(fileShareName))
						{
							await fileShareHelper.UploadListOfStringsToFileShareAsync(response.FileContents,
								config.GetSection("AzureFileShareAccountName").Value,
								config.GetSection("AzureFileShareKey").Value,
								fileShareName, fileName + ".txt");

							logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Consumer Detail File Export to FileShare", $"{fileShareName}/{fileName}.txt", response)}");
						}
					}
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, $"Failed to export ConsumerDetail File for subClient: {subClient.Name}. Exception: { ex }");
					emailService.SendServiceErrorNotifications("ConsumerDetail File Processor", ex.ToString());
					response.IsSuccessful = false;
				}
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfRecords,
					Message = response.IsSuccessful ? "Consumer Detail File Export complete" : "Consumer Detail File Export failed",
					FileDetails = new List<FileDetail>
					{
						new FileDetail
						{
							FileName = response.NumberOfRecords != 0 ? fileName + ".txt" : string.Empty,
							FileArchiveName = response.NumberOfRecords != 0 ? fileName : string.Empty,
							NumberOfRecords = response.NumberOfRecords
						}
					},
				});
			}
		}
	}
}
