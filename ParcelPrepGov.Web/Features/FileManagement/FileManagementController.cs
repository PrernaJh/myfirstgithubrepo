using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.AzureExtensions;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain.Interfaces;
using ParcelPrepGov.Web.Infrastructure;
using System;
using Microsoft.AspNetCore.Mvc;

namespace ParcelPrepGov.Web.Features.FileManagement
{
	public partial class FileManagementController : Controller
	{
		private readonly IConfiguration config;
		private readonly IFileConfigurationProcessor fileConfigurationProcessor;
		private readonly IMemoryCache cache;
		private readonly ISiteProcessor siteProcessor;
		private readonly IQueueClientFactory queueFactory;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly ISubClientRepository subClientRepository;
		private readonly ILogger<FileManagementController> logger;

		public FileManagementController(
			IConfiguration config,
			IFileConfigurationProcessor fileConfigurationProcessor,
			IMemoryCache cache,
			ISiteProcessor siteProcessor,
			ISubClientRepository subClientRepository,
			IQueueClientFactory queueFactory,
			IWebJobRunProcessor webJobRunProcessor,
			ILogger<FileManagementController> logger)
		{
			this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
			this.config = config ?? throw new ArgumentNullException(nameof(config));
			this.fileConfigurationProcessor = fileConfigurationProcessor ?? throw new ArgumentNullException(nameof(fileConfigurationProcessor));
			this.siteProcessor = siteProcessor ?? throw new ArgumentNullException(nameof(siteProcessor));
			this.queueFactory = queueFactory ?? throw new ArgumentNullException(nameof(queueFactory));
			this.webJobRunProcessor = webJobRunProcessor ?? throw new ArgumentNullException(nameof(webJobRunProcessor));
			this.subClientRepository = subClientRepository ?? throw new ArgumentNullException(nameof(subClientRepository));
			this.logger = logger;
		}

	}
}