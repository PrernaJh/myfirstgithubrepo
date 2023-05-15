using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	public class ZoneFileService : IZoneFileService
	{
		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly ILogger<ZoneFileService> logger;
		private readonly IZoneFileProcessor fileProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		public ZoneFileService(IBlobHelper blobHelper, IConfiguration config, IEmailService emailService, ILogger<ZoneFileService> logger, IZoneFileProcessor fileProcessor, IWebJobRunProcessor webJobRunProcessor)
		{
			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.logger = logger;
			this.fileProcessor = fileProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ProcessZoneFileAsync(Stream fileStream, string fileName)
		{
			try
			{
				var fileImportPath = config.GetSection("ZoneFileImport").Value;
				var fileArchivePath = config.GetSection("ZoneFileArchive").Value;

				logger.Log(LogLevel.Information, $"Processing file: { fileName }");

				var response = await fileProcessor.ImportZoneFileToDatabase(fileStream);
				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					SiteName = SiteConstants.AllSites,
					JobName = "Zone File Import",
					JobType = WebJobConstants.ZoneImportJobType,
					Username = "System",
					Message = string.Empty,
					FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName } },
					IsSuccessful = response.IsSuccessful
				});
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Zone File Import", response)}");
				await blobHelper.ArchiveBlob(fileName, fileImportPath, fileArchivePath);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to process Zone File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Zone File Processor", ex.ToString());
			}
		}
	}
}
