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
	public class UpsGeoDescFileService : IUpsGeoDescFileService
	{
		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly ILogger<UpsGeoDescFileService> logger;
		private readonly IUpsGeoDescFileProcessor fileProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly ISiteProcessor siteProcessor;


		public UpsGeoDescFileService(IBlobHelper blobHelper, IConfiguration config, IEmailService emailService, ILogger<UpsGeoDescFileService> logger, IUpsGeoDescFileProcessor fileProcessor, IWebJobRunProcessor webJobRunProcessor, ISiteProcessor siteProcessor)
		{
			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.logger = logger;
			this.fileProcessor = fileProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
			this.siteProcessor = siteProcessor;
		}

		public async Task ProcessUpsGeoDescFileAsync(Stream fileStream, string fileName)
		{
			try
			{
				var fileImportPath = config.GetSection("UpsGeoDescFileImport").Value;
				var fileArchivePath = config.GetSection("UpsGeoDescFileArchive").Value;

				logger.Log(LogLevel.Information, $"Processing file: { fileName }");

				var site = await GetSiteFromFileName(fileName);

				var response = await fileProcessor.ImportUpsGeoDescFileToDatabase(fileStream, site);

				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					SiteName = site.SiteName,
					ClientName = string.Empty,
					SubClientName = string.Empty,
					JobName = "UPS Geo Descriptor File Import",
					JobType = WebJobConstants.UpsGeoDescFileImportJobType,
					Username = "System",
					Message = string.Empty,
					FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName } },
					IsSuccessful = response.IsSuccessful
				});

				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("UPS Geo Descriptor File Import", response)}");

				await blobHelper.ArchiveBlob(fileName, fileImportPath, fileArchivePath);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to process Geo Descriptor File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Geo Descriptor File Processor", ex.ToString());
			}
		}

		private async Task<Site> GetSiteFromFileName(string fileName)
		{
			var name = fileName.Split("_")[0];
			var site = await siteProcessor.GetSiteBySiteNameAsync(name);


			if (StringHelper.DoesNotExist(site.Id))
			{
				logger.Log(LogLevel.Error, "Invalid Site Name in File Name");
			}
			return site;
		}
	}
}
