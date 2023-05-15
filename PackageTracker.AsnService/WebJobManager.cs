using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.AsnService
{
	public class WebJobManager
	{
		private const string MessagePrefix = "WebJobManager |";

		private readonly ILogger<WebJobManager> logger;
		private readonly IAsnFileService asnFileService;


		private readonly IDictionary<string, WebJobSettings> webJobSettings;

		public WebJobManager(IConfiguration configuration,
			ILogger<WebJobManager> logger,
			IAsnFileService asnFileService)
		{
			this.logger = logger;
			this.asnFileService = asnFileService;
			webJobSettings = configuration.GetSection("WebJobSettings").Get<Dictionary<string, WebJobSettings>>();
		}

		private WebJobSettings GetSettingsForWebJob(string name)
		{
			if (!webJobSettings.TryGetValue(name, out var settings))
			{
				settings = new WebJobSettings { IsEnabled = false };
			}

			return settings;
		}

		[FunctionName("AsnFileImportWatcher")]
		public async Task AsnFileImportWatcher([TimerTrigger("%WebJobSettings:AsnFileImportWatcher:JobTimer%")] TimerInfo jobTimer)
		{
			try
			{
				var webJobSettings = GetSettingsForWebJob("AsnFileImportWatcher");
				if (webJobSettings.IsEnabled)
				{
					await asnFileService.ProcessAsnFilesAsync(webJobSettings);
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"{ MessagePrefix } High Level Exception: { ex }");
			}
		}
	}
}
