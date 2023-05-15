using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEodProcessor = PackageTracker.EodService.Interfaces.IEodProcessor;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Services
{
	public class EodJob
    {
		public string Type { get; set; }
		public string Description { get; set; }
		public bool BySubClient { get; set; }		
	}

	public class EodService : IEodService
	{
		private readonly ILogger<EodService> logger;

		private readonly IConfiguration config;
		private readonly IContainerRepository containerRepository;
		private readonly IEmailConfiguration emailConfiguration;
		private readonly IEmailService emailService;
		private readonly IEodContainerRepository eodContainerRepository;
		private readonly IEodPackageRepository eodPackageRepository;
		private readonly IEodProcessor eodProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly IQueueClientFactory queueFactory;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly PackageTrackerIdentityDbContext identityDbContext;

		private static IDictionary<string, EodJob> eodJobTypes = new Dictionary<string, EodJob>
		{
			{ WebJobConstants.RunEodJobType, new EodJob {
				Type = WebJobConstants.RunEodJobType, Description = "Eod Started", BySubClient = false } },
			{ WebJobConstants.ContainerDetailExportJobType, new EodJob {
				Type = WebJobConstants.ContainerDetailExportJobType, Description = "Container Detail File Export", BySubClient = false } },
			{ WebJobConstants.PmodContainerDetailExportJobType, new EodJob {
				Type = WebJobConstants.PmodContainerDetailExportJobType, Description = "PMOD Container Detail File Export", BySubClient = false } },
			{ WebJobConstants.PackageDetailExportJobType, new EodJob {
				Type = WebJobConstants.PackageDetailExportJobType, Description = "Package Detail File Export", BySubClient = false } },
			{ WebJobConstants.ReturnAsnExportJobType, new EodJob {
				Type = WebJobConstants.ReturnAsnExportJobType, Description = "Return ASN File Export", BySubClient = true } },
			{ WebJobConstants.UspsEvsExportJobType, new EodJob {
				Type = WebJobConstants.UspsEvsExportJobType, Description = "USPS eVs File Export", BySubClient = false } },
			{ WebJobConstants.UspsEvsPmodExportJobType, new EodJob {
				Type = WebJobConstants.UspsEvsPmodExportJobType, Description = "USPS eVs PMOD File Export", BySubClient = false } },
			{ WebJobConstants.InvoiceExportJobType, new EodJob {
				Type = WebJobConstants.InvoiceExportJobType, Description = "Invoice File Export", BySubClient = true } },
			{ WebJobConstants.ExpenseExportJobType, new EodJob {
				Type = WebJobConstants.ExpenseExportJobType, Description = "Expense File Export", BySubClient = true } },
		};

		public EodService(ILogger<EodService> logger,
			IConfiguration config,
			IContainerRepository containerRepository,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IEodContainerRepository eodContainerRepository,
            IEodPackageRepository eodPackageRepository,
			IEodProcessor eodProcessor,
			IPackageRepository packageRepository,
			IQueueClientFactory queueFactory,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor,
			PackageTrackerIdentityDbContext identityDbContext)
		{
			this.logger = logger;

			this.config = config;
			this.containerRepository = containerRepository;
			this.emailConfiguration = emailConfiguration;
			this.emailService = emailService;
			this.eodContainerRepository = eodContainerRepository;
			this.eodPackageRepository = eodPackageRepository;
			this.eodProcessor = eodProcessor;
			this.packageRepository = packageRepository;
			this.queueFactory = queueFactory;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
			this.identityDbContext = identityDbContext;
		}

		public async Task ProcessEndOfDayPackages()
		{
			var sites = await siteProcessor.GetAllSitesAsync();
			foreach (var site in sites)
			{
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{siteLocalTime.Date}");
				await semaphore.WaitAsync();
				logger.LogInformation($"EOD: Process Eod Packages for site: {site.SiteName}");
				try
				{
					await ProcessEndOfDayPackagesBySite(site, siteLocalTime.Date);
				}
				finally
				{
					semaphore.Release();
				}
			}
		}

		public async Task ProcessEndOfDayContainers()
		{
			var sites = await siteProcessor.GetAllSitesAsync();
			foreach (var site in sites)
			{
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{siteLocalTime.Date}");
				await semaphore.WaitAsync();
				logger.LogInformation($"EOD: Process Eod Containers for site: {site.SiteName}");

				try
				{
					await ProcessEndOfDayContainersBySite(site, siteLocalTime.Date);
				}
				finally
				{
					semaphore.Release();
				}
			}
		}

		public async Task ProcessEndOfDayPackagesBySite(Site site, DateTime targetDate, bool force = false)
		{
			try
			{
				var mostRecentRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodPackageProcess, true);
				if ((!force) && !await packageRepository.HavePackagesChangedForSiteAsync(site.SiteName, mostRecentRun.CreateDate))
					return;

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					WebJobTypeConstant = WebJobConstants.SqlEodPackageProcess,
					ProcessedDate = targetDate.Date,
					JobName = "Assign End Of Day Package Data By Site",
					Message = "EoD package process started"
				});
				var isSuccessful = true;
				var numberOfRecords = 0;
				var maxIterations = 10;
				while (isSuccessful)
                {
					var response = await eodProcessor.ProcessEndOfDayPackagesAsync(site, webJobRun.Id, targetDate, mostRecentRun.CreateDate);
					isSuccessful = response.IsSuccessful;
					numberOfRecords += response.NumberOfRecords;
					if ((!force) && response.NumberOfRecords < 500) // This must match "TOP 500" in GetPackagesForEndOfDayProcess
						break;
					if (force && response.NumberOfRecords == 0)
						break;
					if ((!force) && --maxIterations <= 0)
					{
						isSuccessful = false;
						break;  // will pickup where it left off, later.
					}
				}
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = isSuccessful,
					NumberOfRecords = numberOfRecords,
					Message = "EoD package process complete"
				});

				if (isSuccessful)
				{
					logger.Log(LogLevel.Information, $"Eod Package Process successful for Site: {site.SiteName}. Total Records Processed: {numberOfRecords}");
				}
				else if (maxIterations > 0)
				{
					logger.Log(LogLevel.Error, $"Failed to complete Eod Package Process for Site: {site.SiteName}.");
					//emailService.SendServiceErrorNotifications("Eod Package Process", $"Failed to complete Eod Package Process for Site: {site.SiteName} Total Records Processed: {numberOfRecords}");
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Package Process. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Eod Package Process", ex.ToString());
			}
		}

		public async Task ProcessEndOfDayContainersBySite(Site site, DateTime targetDate, bool force = false)
		{
			try
			{
				var mostRecentRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodContainerProcess, true);
				if ((!force) && !await containerRepository.HaveContainersChangedForSiteAsync(site.SiteName, mostRecentRun.CreateDate))
					return;

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					WebJobTypeConstant = WebJobConstants.SqlEodContainerProcess,
					ProcessedDate = targetDate.Date,
					JobName = "Assign End Of Day Container Data By Site",
					Message = "EoD container process started"
				});
				var isSuccessful = true;
				var numberOfRecords = 0;
				var maxIterations = 10;
				while (isSuccessful)
				{
					var response = await eodProcessor.ProcessEndOfDayContainersAsync(site, webJobRun.Id, targetDate, mostRecentRun.CreateDate);
					isSuccessful = response.IsSuccessful;
					numberOfRecords += response.NumberOfRecords;
					if ((!force) && response.NumberOfRecords < 500) // This must match "TOP 500" in GetContainersForEndOfDayProcess
						break;
					if (force && response.NumberOfRecords == 0)
						break;
					if ((! force) && --maxIterations <= 0)
                    {
						isSuccessful = false;
						break;	// will pickup where it left off, later.
                    }
				}
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = isSuccessful,
					NumberOfRecords = numberOfRecords,
					Message = "EoD container process complete"
				});

				if (isSuccessful)
				{
					logger.Log(LogLevel.Information, $"Eod Container Process successful for Site: {site.SiteName}. Total Records Processed: {numberOfRecords}");
				}
				else if (maxIterations > 0)
				{
					logger.Log(LogLevel.Error, $"Failed to complete Eod Container Process for Site: {site.SiteName}.");
					//emailService.SendServiceErrorNotifications("Eod Container Process", $"Failed to complete Eod Container Process for Site: {site.SiteName} Total Records Processed: {numberOfRecords}");
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Container Process. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Eod Container Process", ex.ToString());
			}
		}

		private async Task CheckForDuplicateEndOfDayPackagesBySite(Site site, DateTime dateToCheck)
		{
			try
			{
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					WebJobTypeConstant = WebJobConstants.EodPackageDuplicateCheck,
					ProcessedDate = dateToCheck.Date,
					JobName = "Check for EoD duplicate packages",
					Message = "EoD package process started"
				});

				var response = await eodProcessor.CheckForDuplicateEndOfDayPackagesAsync(site, dateToCheck, webJobRun.Id);

				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfRecords,
					Message = "EoD package process complete"
				});

				if (response.IsSuccessful)
				{
					logger.Log(LogLevel.Information, $"Eod Package Duplicate Check successful for Site: {site.SiteName}. Total Duplicates found: {response.NumberOfRecords}");
				}
				else
				{
					logger.Log(LogLevel.Error, $"Failed to complete Eod Package Duplicate Check for Site: {site.SiteName} Failed Records: { response.NumberOfRecords}");
					emailService.SendServiceErrorNotifications("Eod Package Duplicate Check", $"Failed to complete Eod Package Process for Site: {site.SiteName}");
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Package Duplicate Check. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Eod Package Process", ex.ToString());
			}
		}

		private async Task CheckForDuplicateEndOfDayContainersBySite(Site site, DateTime dateToCheck)
		{
			try
			{
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					WebJobTypeConstant = WebJobConstants.EodContainerDuplicateCheck,
					ProcessedDate = dateToCheck.Date,
					JobName = "Check for EoD duplicate containers",
					Message = "EoD container process started"
				});

				var response = await eodProcessor.CheckForDuplicateEndOfDayContainersAsync(site, dateToCheck, webJobRun.Id);

				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfRecords,
					Message = "EoD container process complete"
				});

				if (response.IsSuccessful)
				{
					logger.Log(LogLevel.Information, $"Eod Container Duplicate Check successful for Site: {site.SiteName}. Total Duplicates found: {response.NumberOfRecords}");
				}
				else
				{
					logger.Log(LogLevel.Error, $"Failed to complete Eod Container Duplicate Check for Site: {site.SiteName}");
					emailService.SendServiceErrorNotifications("Eod Container Duplicate Check", $"Failed to complete Eod Container Process for Site: {site.SiteName}");
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Container Duplicate Check. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Eod Container Process", ex.ToString());
			}
		}

		public async Task<bool> ShouldRunJobBeforeFileGeneration(Site site, DateTime processedDate, string webJobConstant)
		{
			var mostRecentRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, processedDate, webJobConstant);
			if (! mostRecentRun.IsSuccessful)
            {
				logger.LogInformation($"EoD File generation: Job Type {webJobConstant} failed on last run, repeat process");
				return true;
            }			
			else if (mostRecentRun.NumberOfRecords > 0)
			{
				logger.LogInformation($"EoD File generation: Job Type {webJobConstant} had positive records on last run, repeat process");
				return true;
			}
			return false;
		}

		public async Task ResetPackageEod(string message)
		{
			try
			{
				var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
				var site = await siteProcessor.GetSiteBySiteNameAsync(endOfDayQueueMessage.SiteName);
				await eodProcessor.ResetPackageEod(site, endOfDayQueueMessage.TargetDate);
				await CheckForDuplicateEndOfDayPackagesBySite(site, endOfDayQueueMessage.TargetDate);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Reset Process. Exception: { ex }");
			}
		}

		private async Task<bool> IsEodStarted(Site site, DateTime processedDate)
		{
			var result = false;
			foreach (var jobType in eodJobTypes.Keys)
			{
				var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, processedDate, jobType);
				if (mostRecentJobRun.IsSuccessful || mostRecentJobRun.InProgress)
				{
					result = true;
					break;
				}
			}
			logger.LogInformation($"EOD started for: {site.SiteName}: {result}");
			return result;
		}

		private async Task<(bool, IDictionary<string, IList<WebJobRunResponse>>)> IsEodCompleted(Site site, DateTime processedDate)
		{
			var result = true;
			var webJobRuns = new Dictionary<string, IList<WebJobRunResponse>>();
			var subClients = await subClientProcessor.GetSubClientsAsync();
			foreach (var jobType in eodJobTypes.Keys)
			{
				webJobRuns[jobType] = new List<WebJobRunResponse>();
				if (eodJobTypes[jobType].BySubClient)
                {
					foreach (var subClient in subClients.Where(s => s.SiteName == site.SiteName))
					{
						var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, subClient.Name, processedDate.Date, jobType, true);
						if (! mostRecentJobRun.IsSuccessful)
							result = false;
						webJobRuns[jobType].Add(mostRecentJobRun);
					}
                }
                else
                {
					var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, processedDate.Date, jobType, true);
					if (! mostRecentJobRun.IsSuccessful)
						result = false;
					webJobRuns[jobType].Add(mostRecentJobRun);
				}
			}
			logger.LogInformation($"EOD completed for: {site.SiteName}: {result}");
			foreach (var response in webJobRuns)
			{
				foreach (var webJobRun in response.Value)
				{
					if (StringHelper.Exists(webJobRun.SubClientName))
						logger.LogInformation($"EOD job: {response.Key} for: {webJobRun.SubClientName}: {webJobRun.IsSuccessful}");
					else
						logger.LogInformation($"EOD job: {response.Key} for: {site.SiteName}: {webJobRun.IsSuccessful}");
				}
			}
			return (result, webJobRuns);
		}

		public async Task MonitorEod(WebJobSettings webJobSettings)
        {
            foreach (var site in await siteProcessor.GetAllSitesAsync())
            {
				if (webJobSettings.IsDuringScheduledHours(site))
					await MonitorEod(site);
			}
		}

		private async Task MonitorEod(Site site)
        {
 			var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
			logger.Log(LogLevel.Information, $"EOD: MonitorEod for site: {site.SiteName} for date: {siteLocalTime.Date:yyyyMMdd}");

			// First check if there has been any activity at the site.
			var eodPackages = await eodPackageRepository.GetEodOverview(site.SiteName, siteLocalTime);
			var eodContainers = await eodContainerRepository.GetEodOverview(site.SiteName, siteLocalTime);
			if (eodPackages.Count() == 0 && eodContainers.Count() == 0)
            {
				logger.Log(LogLevel.Information, $"EOD: MonitorEod: No activity for site: {site.SiteName}, date: {siteLocalTime.Date}");
				return;
			}
						
			logger.Log(LogLevel.Information, $"EOD: MonitorEod: {eodPackages.Count()} packages for : {site.SiteName}, date: {siteLocalTime.Date}");
			await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
			{
				SiteName = site.SiteName,
				JobName = "Monitor EOD",
				JobType = WebJobConstants.MonitorEodJobType,
				ProcessedDate = siteLocalTime.Date,
				Username = "System",
				Message = string.Empty,
				IsSuccessful = true,
				CreateDate = DateTime.Now,
				LocalCreateDate = siteLocalTime
			});

			var isEodStarted = await IsEodStarted(site, siteLocalTime);
			if (! isEodStarted)
			{
				logger.Log(LogLevel.Error, $"EOD not started for site: {site.SiteName}");
				var email = new EmailMessage();
				site.EodSummaryEmailList.Where(x => StringHelper.Exists(x)).ToList()
					.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
				if (email.ToAddresses.Any())
				{
					email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
					email.Subject = $"*** Alert ***: EOD not started for site: {site.SiteName}";
					email.Content = $"Please start the End of Day Process for site: {site.SiteName}";
					await emailService.SendAsync(email);
				}
			}
            else
			{
				(var isEodCompleted, var webJobRuns) = await CheckEodComplete(site, siteLocalTime);
				if (! isEodCompleted)
                {
					logger.Log(LogLevel.Error, $"EOD not completed for site: {site.SiteName}");
					var email = new EmailMessage();
					site.EodSummaryEmailList.Where(x => StringHelper.Exists(x)).ToList()
						.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
					if (email.ToAddresses.Any())
					{
						var content = new StringBuilder();
						content.Append("<h2>Job Status</h2>");
						foreach (var jobType in webJobRuns.Keys)
						{
							foreach (var webJobRun in webJobRuns[jobType])
							{
								var status = "Not started";
								if (webJobRun.InProgress)
									status = "In progress";
								else if (webJobRun.IsSuccessful)
									status = "Completed";
								else if (StringHelper.Exists(webJobRun.Message))
									status = $"Failed: {webJobRun.Message}";
								if (StringHelper.Exists(webJobRun.SubClientName))
									content.Append($"<li>{eodJobTypes[jobType].Description}\t{webJobRun.SubClientName}\t{status}</li>\n");
								else
									content.Append($"<li>{eodJobTypes[jobType].Description}\t{webJobRun.SiteName}\t{status}</li>\n");
							}
						}
						email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
						email.Subject = $"*** Alert ***: EOD not completed for site: {site.SiteName} {siteLocalTime.ToString("g")}";
						email.Content = content.ToString();
						await emailService.SendAsync(email, true);
					}
				}
			}
		}

		public async Task<(bool, IDictionary<string, IList<WebJobRunResponse>>)> CheckEodComplete(Site site, DateTime dateToProcess, string jobType = null)
		{
			var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
			if (siteLocalTime.AddHours(-8).Date != dateToProcess.Date) // Skip for historical runs
				return (false, null);

			(var isEodCompleted, var webJobRuns) = await IsEodCompleted(site, dateToProcess);
			if (isEodCompleted)
			{
				var checkEodJobType = WebJobConstants.CheckEodJobType;
				var lastRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, dateToProcess, checkEodJobType);
				if (!lastRun.IsSuccessful)
				{
					var email = new EmailMessage();
					site.EodSummaryEmailList.Where(x => StringHelper.Exists(x)).ToList()
						.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
					if (email.ToAddresses.Any())
					{
						email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
						email.Subject = $"*** Alert ***: EOD completed for site: {site.SiteName}";
						lastRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, dateToProcess, WebJobConstants.EodPackageMonitorJobType);
						if (lastRun.InProgress)
						{
							email.Content = $"End of Day Monitor Job in progress for site: {site.SiteName}";
						}
						else if (lastRun.IsSuccessful)
						{
							email.Content = $"End of Day Process completed  for site: {site.SiteName}";
						}
						else
						{
							email.Content = $"End of Day Monitor Job failed for site: {site.SiteName}";
						}
						await emailService.SendAsync(email);
					}
					await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
					{
						Id = Guid.NewGuid().ToString(),
						SiteName = site.SiteName,
						ClientName = string.Empty,
						SubClientName = string.Empty,
						JobName = "EOD Completed.",
						JobType = checkEodJobType,
						ProcessedDate = dateToProcess.Date,
						Username = "System",
						Message = "",
						IsSuccessful = true,
						CreateDate = DateTime.Now,
						LocalCreateDate = siteLocalTime
					});

				}
			}
			return (isEodCompleted, webJobRuns);
		}

		private async Task<bool> HasJobAlreadyRun(Site site, DateTime processedDate, string jobType)
		{
			var result = true;
			var subClients = await subClientProcessor.GetSubClientsAsync();
			if (eodJobTypes[jobType].BySubClient)
			{
				foreach (var subClient in subClients.Where(s => s.SiteName == site.SiteName))
				{
					var mostRecentJobRun = 
						await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, subClient.Name, processedDate.Date, jobType, true);
					if (!mostRecentJobRun.IsSuccessful)
						result = false;
				}
			}
			else
			{
				var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, processedDate.Date, jobType, true);
				if (!mostRecentJobRun.IsSuccessful)
					result = false;
			}
			return result;
		}

		// Note: This function will only get called serially for a particular site.
		public async Task<bool> IsEodBlocked(Site site, DateTime targetDate, string jobType, string userName = null, bool sendEmails = false)
        {
			if (jobType != WebJobConstants.RunEodJobType && await HasJobAlreadyRun(site, targetDate, jobType))
			{
				logger.LogInformation($"{eodJobTypes[jobType].Description} blocked because it has already completed successfully for site: {site.SiteName}, manifest date: {targetDate:MM/dd/yyyy}");
				return true;
			}
			var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
			if (siteLocalTime.AddHours(-8).Date != targetDate.Date) // Skip for historical runs
				return false;

			var lastPackageRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodPackageProcess);
			if (!lastPackageRun.IsSuccessful || jobType == WebJobConstants.RunEodJobType)
			{
				await ProcessEndOfDayPackagesBySite(site, targetDate, true);
				lastPackageRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodPackageProcess);
			}

			var lastPackageDuplicateCheck = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.EodPackageDuplicateCheck);
			if (!lastPackageDuplicateCheck.IsSuccessful || jobType == WebJobConstants.RunEodJobType)
			{
				await CheckForDuplicateEndOfDayPackagesBySite(site, targetDate);
				lastPackageDuplicateCheck = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.EodPackageDuplicateCheck);
			}

			var lastContainerRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodContainerProcess);
			if (!lastContainerRun.IsSuccessful || jobType == WebJobConstants.RunEodJobType)
			{
				await ProcessEndOfDayContainersBySite(site, targetDate, true);
				lastContainerRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.SqlEodContainerProcess);
			}


			var lastContainerDuplicateCheck = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.EodContainerDuplicateCheck);
			if (!lastContainerDuplicateCheck.IsSuccessful || jobType == WebJobConstants.RunEodJobType)
			{
				await CheckForDuplicateEndOfDayContainersBySite(site, targetDate);
				lastContainerDuplicateCheck = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, targetDate, WebJobConstants.EodContainerDuplicateCheck);
			}

			var failed = !lastPackageRun.IsSuccessful || !lastPackageDuplicateCheck.IsSuccessful 
				|| !lastContainerRun.IsSuccessful || !lastContainerDuplicateCheck.IsSuccessful;
			var blocked = lastPackageDuplicateCheck.NumberOfRecords > 0 || lastContainerDuplicateCheck.NumberOfRecords > 0;
			if (failed)
            {
				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					Id = Guid.NewGuid().ToString(),
					SiteName = site.SiteName,
					ClientName = string.Empty,
					SubClientName = string.Empty,
					JobName = eodJobTypes[jobType].Description,
					JobType = jobType,
					ProcessedDate = targetDate.Date,
					Username = "System",
					Message = "EOD processing blocked by failed EoD jobs",
					IsSuccessful = false,
					CreateDate = DateTime.Now,
					LocalCreateDate = siteLocalTime
				});
				logger.LogError($"{eodJobTypes[jobType].Description} for site: {site.SiteName} blocked by failed EoD jobs");
			}
			else if (blocked)
            {
				// Set sendEmails = true for only one job type, because only want to send one email per run.
				// Note: Currently, we can't control the order of the EOD jobs.
				if (sendEmails) 
                {
					// Retrieve duplicate barcodes.
					var eodPackages = await eodPackageRepository.GetEodOverview(site.SiteName, targetDate);
					var areDuplicatePackages = eodPackages.GroupBy(p => p.Barcode).Select(g => g.Count()).FirstOrDefault(s => s > 1) > 1;
					var eodContainers = await eodContainerRepository.GetEodOverview(site.SiteName, targetDate);
					var areDuplicateContainers = eodContainers.GroupBy(p => p.ContainerId).Select(g => g.Count()).FirstOrDefault(s => s > 1) > 1;

					var email = new EmailMessage();
					emailConfiguration.ExceptionsEmailContactList.Where(x => StringHelper.Exists(x)).ToList()
						.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
					if (email.ToAddresses.Any())
					{
						var content = new StringBuilder();
						if (areDuplicatePackages)
                        {
							content.Append("<h2>Duplicate Package Barcodes: PackageId, ...</h2>");
							foreach (var group in eodPackages.GroupBy(p => p.Barcode))
							{
								if (group.Count() > 1)
								{
									var ids = group.Select(p => p.PackageId).ToArray();
									content.Append($"<li>{group.Key}:\t{string.Join(", ", ids)}</li>\n");
								}
							}
                        }
						if (areDuplicateContainers)
						{
							content.Append("<h2>Duplicate Container Barcodes</h2>");
							foreach (var group in eodContainers.GroupBy(p => p.ContainerId))
							{
								if (group.Count() > 1)
								{
									content.Append($"<li>{group.Key}</li>\n");
								}
							}
						}
						email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
						email.Subject = $"*** Alert ***: EOD processing blocked: Duplicate barcodes for site: {site.SiteName}";
						email.Content = content.ToString();
						await emailService.SendAsync(email, true);
					}
				}
				await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					Id = Guid.NewGuid().ToString(),
					SiteName = site.SiteName,
					ClientName = string.Empty,
					SubClientName = string.Empty,
					JobName = eodJobTypes[jobType].Description,
					JobType = jobType,
					ProcessedDate = targetDate.Date,
					Username = "System",
					Message = "EOD processing blocked by duplicate barcodes",
					IsSuccessful = false,
					CreateDate = DateTime.Now,
					LocalCreateDate = siteLocalTime
				});
				logger.LogError($"{eodJobTypes[jobType].Description} for site: {site.SiteName} blocked by duplicate barcodes");
			}
			if (sendEmails && userName != null)
				await NotifyUser(site, targetDate, blocked || failed, userName);
			return blocked || failed;
		}

		public async Task ResetContainerEod(string message)
		{
			try
			{
				var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
				var site = await siteProcessor.GetSiteBySiteNameAsync(endOfDayQueueMessage.SiteName);

				await eodProcessor.ResetContainerEod(site, endOfDayQueueMessage.TargetDate);
				await CheckForDuplicateEndOfDayContainersBySite(site, endOfDayQueueMessage.TargetDate);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception during Eod Reset Process. Exception: { ex }");
			}

		}

		private async Task NotifyUser(Site site, DateTime dateToProcess, bool blocked, string userName)
		{
			try
            {
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				if (siteLocalTime.Date == dateToProcess.Date)
				{
					var user = (await identityDbContext.Users
						.AsNoTracking()
						.ToListAsync())
						.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.InvariantCultureIgnoreCase));
					var email = new EmailMessage();
					site.EodSummaryEmailList.Where(x => StringHelper.Exists(x)).ToList()
						.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
					if (StringHelper.Exists(user?.Email) && email.ToAddresses.FirstOrDefault(a => a.Address.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase)) == null)
					{
						email.ToAddresses.Add(new EmailAddress { Name = user.UserName, Address = user.Email });
					}
					if (email.ToAddresses.Any())
					{
						email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
						if (blocked)
							email.Subject = $"*** Alert ***: EOD processing blocked for site: {site.SiteName}";
						else
							email.Subject = $"*** Alert ***: EOD processing started for site: {site.SiteName}";
						email.Content = string.Empty;
						await emailService.SendAsync(email);
					}
				}
            } catch (Exception ex)
            {
				logger.Log(LogLevel.Error, $"Failed to send email to user: {userName} that started EOD process. Exception: { ex }");
			}
		}
		
		public async Task<bool> StartEod(string message)
        {
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Run all EOD jobs incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			try
			{					
				logger.LogInformation($"EOD: Start EOD for site: {siteName}");
				var webJob = await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
				{
					Id = Guid.NewGuid().ToString(),
					SiteName = site.SiteName,
					ClientName = string.Empty,
					SubClientName = string.Empty,
					JobName = "EOD started",
					JobType = WebJobConstants.RunEodJobType,
					ProcessedDate = dateToProcess.Date,
					Username = username,
					Message = string.Empty,
					IsSuccessful = true,
					CreateDate = DateTime.Now,
					LocalCreateDate = siteLocalTime
				});

				if (force)
                {
					await CheckForDuplicateEndOfDayPackagesBySite(site, dateToProcess);
					await CheckForDuplicateEndOfDayContainersBySite(site, dateToProcess);
				}				
				else if (await IsEodBlocked(site, dateToProcess, WebJobConstants.RunEodJobType, username, true))
				{
					return false;
				}
				if (siteLocalTime.Date == dateToProcess.Date) // Skip for historical runs
				{
					await eodProcessor.DeleteObsoleteContainersAsync(site, dateToProcess, webJob);
					//await eodProcessor.DeleteEmptyContainersAsync(site, dateToProcess, webJob);
				}

				var queueClient = queueFactory.GetClient();
				var queue = queueClient.GetQueueReference(config["MonitorEodPackagesQueue"]);
				await queue.AddMessageAsync(new CloudQueueMessage(message));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to start EOD. Exception: { ex }");
				emailService.SendServiceErrorNotifications("EodService:StartEod", ex.ToString());
				return false;
			}
			finally
			{
				semaphore.Release();
			}
			return true;
		}
	}
}
