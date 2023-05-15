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
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	public class WebJobRunsService : IWebJobRunsService
	{
		private readonly ILogger<WebJobRunsService> logger;
		private readonly IConfiguration config;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IEmailConfiguration emailConfiguration;
		private readonly IEmailService emailService;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly IDictionary<string, DateTime> lastWarning = new Dictionary<string, DateTime>();

		public WebJobRunsService(ILogger<WebJobRunsService> logger,
			IConfiguration config,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService, 
			ISiteProcessor siteProcessor, 
			ISubClientProcessor subClientProcessor, 
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;
			this.config = config;
			this.emailConfiguration = emailConfiguration;
			this.emailService = emailService;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task MonitorRecentAsnImportsAsync(WebJobSettings webJobSettings)
		{
			try
			{
				var subClients = await subClientProcessor.GetSubClientsAsync();
				var sites = await siteProcessor.GetAllSitesAsync();

				foreach (var subClient in subClients)
				{
					var site = sites.FirstOrDefault(s => s.SiteName == subClient.SiteName);
					if (webJobSettings.IsDuringScheduledHours(site, subClient))
					{
						var warningDurationHours = webJobSettings.GetParameterIntValue("WarningDurationHours", 2);
						var shouldNotify = await webJobRunProcessor.CheckForRecentAsnFileImportAsync(site.SiteName, subClient.Name, warningDurationHours);
						await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
						{
							SiteName = site.SiteName,
							ClientName = subClient.ClientName,
							SubClientName = subClient.Name,
							JobName = "Monitor Recent ASN Imports",
							JobType = WebJobConstants.MonitorAsnFileImportJobType,
							Username = "System",
							Message = string.Empty,
							IsSuccessful = true
						});

						if (shouldNotify && lastWarning.TryGetValue(subClient.Name, out var lastSent))
						{
							var warningIntervalMinutes = webJobSettings.GetParameterIntValue("WarningIntervalMinutes", 15);
							var timeElapsed = DateTime.Now - lastSent;
							shouldNotify = timeElapsed.TotalMinutes > warningIntervalMinutes;
						}

						if (shouldNotify)
						{
							lastWarning[subClient.Name] = DateTime.Now;
							emailService.SendServiceAlertNotifications($"Alert: ASN File Import: ASN file not received in last {warningDurationHours} hours",
								$"ASN file not received in last {warningDurationHours} hours for: {subClient.Name}.",
								emailConfiguration.ExceptionsEmailContactList,
								emailConfiguration.ExceptionsEmailContactList);

							logger.Log(LogLevel.Information, $"ASN File Import Alert sent for subClient: {subClient.Name}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to process webJobRuns. Exception: { ex }");
			}
		}
	}
}
