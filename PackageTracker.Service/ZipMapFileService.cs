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
	public class ZipMapFileService : IZipMapFileService
	{
		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly ILogger<ZipMapFileService> logger;
		private readonly IZipMapFileProcessor fileProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public ZipMapFileService(IBlobHelper blobHelper, IConfiguration config, IEmailService emailService, ILogger<ZipMapFileService> logger, IZipMapFileProcessor fileProcessor, IWebJobRunProcessor webJobRunProcessor)
		{
			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.logger = logger;
			this.fileProcessor = fileProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ProcessZipMapFileAsync(Stream fileStream, string fileName)
		{
			try
			{
				var fileImportPath = config.GetSection("ZipMapFileImport").Value;
				var fileArchivePath = config.GetSection("ZipMapFileArchive").Value;

				logger.Log(LogLevel.Information, $"Processing file: { fileName }");

				var response = await fileProcessor.ImportZipMaps(fileStream, fileName);
				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					SiteName = SiteConstants.AllSites,
					JobName = "Zip Map File Import",
					JobType = WebJobConstants.ZipMapImportJobType,
					Username = "System",
					Message = string.Empty,
					FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName } },
					IsSuccessful = response.IsSuccessful
				});
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("ZipMap File Import", response)}");
				await blobHelper.ArchiveBlob(fileName, fileImportPath, fileArchivePath);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to process ZipMap File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("ZipMap File Processor", ex.ToString());
			}
		}
	}
}
