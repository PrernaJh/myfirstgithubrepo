using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.TrackingService.Interfaces;
using ParcelPrepGov.Reports.Interfaces;
using System;
using System.Threading.Tasks;

namespace PackageTracker.TrackingService
{
	public class ReportService : IReportService
	{
		private readonly ILogger<ReportService> logger;
		private readonly IActiveGroupRepository activeGroupRepository;
		private readonly IBinDatasetProcessor binDatasetProcessor;
		private readonly IContainerRepository containerRepository;
		private readonly IEmailService emailService;
		private readonly IJobDatasetProcessor jobDatasetProcessor;
		private readonly IJobRepository jobRepository;
		private readonly IPackageDatasetProcessor packageDatasetProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly IShippingContainerDatasetProcessor shippingContainerDatasetProcessor;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientDatasetProcessor subClientDatasetProcessor;
		private readonly ISubClientRepository subClientRepository;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public ReportService(ILogger<ReportService> logger,
			IActiveGroupRepository activeGroupRepository,
			IBinDatasetProcessor binDatasetProcessor,
			IContainerRepository containerRepository,
			IEmailService emailService,
			IJobDatasetProcessor jobDatasetProcessor,
			IJobRepository jobRepository,
			IPackageDatasetProcessor packageDatasetProcessor,
			IPackageRepository packageRepository,
			IShippingContainerDatasetProcessor shippingContainerDatasetProcessor,
			ISiteProcessor siteProcessor,
			ISubClientDatasetProcessor subClientDatasetProcessor,
			ISubClientRepository subClientRepository,
			IWebJobRunProcessor webJobRunProcessor
			)
		{
			this.logger = logger;
			this.activeGroupRepository = activeGroupRepository;
			this.binDatasetProcessor = binDatasetProcessor;
			this.containerRepository = containerRepository;
			this.emailService = emailService;
			this.jobDatasetProcessor = jobDatasetProcessor;
			this.jobRepository = jobRepository;
			this.packageDatasetProcessor = packageDatasetProcessor;
			this.packageRepository = packageRepository;
			this.shippingContainerDatasetProcessor = shippingContainerDatasetProcessor;
			this.siteProcessor = siteProcessor;
			this.subClientDatasetProcessor = subClientDatasetProcessor;
			this.subClientRepository = subClientRepository;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task UpdateBinDatasets()
		{
			try
			{
				var sites = await siteProcessor.GetAllSitesAsync();
				foreach (var site in sites)
				{
					var previousRun = await webJobRunProcessor.GetMostRecentJobRunBySiteAndJobType(site.SiteName, WebJobConstants.BinDatasetJobType, true);
					if (!await activeGroupRepository.HaveActiveGroupsChangedAsync(ActiveGroupTypeConstants.Bins, site.SiteName, previousRun.CreateDate))
					{
						continue;
					}

					var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
					{
						Site = site,
						WebJobTypeConstant = WebJobConstants.BinDatasetJobType,
						JobName = "Update Bin Datasets",
						Message = "Update Bin Datasets started"
					});
					var response = await binDatasetProcessor.UpdateBinDatasets(site);
					await webJobRunProcessor.EndWebJob(new EndWebJobRequest
					{
						WebJobRun = webJobRun,
						IsSuccessful = response.IsSuccessful,
						NumberOfRecords = response.NumberOfDocuments,
						Message = "Update Bin Datasets complete"
					});
					if (!response.IsSuccessful)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to import bin datasets for Site: {site.SiteName}", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to import bin datasets: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to import bin datasets", $"Exception: {ex}");
			}
		}


		public async Task UpdateJobDatasets()
		{
			try
			{
				var sites = await siteProcessor.GetAllSitesAsync();
				foreach (var site in sites)
				{
					var previousRun = await webJobRunProcessor.GetMostRecentJobRunBySiteAndJobType(site.SiteName, WebJobConstants.JobDatasetJobType, true);
					if (!await jobRepository.HaveJobsChangedForSiteAsync(site.SiteName, previousRun.CreateDate))
					{
						continue;
					}

					var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
					{
						Site = site,
						WebJobTypeConstant = WebJobConstants.JobDatasetJobType,
						JobName = "Update Job Datasets",
						Message = "Update Job Datasets started"
					});
					var response = await jobDatasetProcessor.UpdateJobDatasets(site);
					await webJobRunProcessor.EndWebJob(new EndWebJobRequest
					{
						WebJobRun = webJobRun,
						IsSuccessful = response.IsSuccessful,
						NumberOfRecords = response.NumberOfDocuments,
						Message = "Update Job Datasets complete"
					});
					if (!response.IsSuccessful)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to import job datasets for Site: {site.SiteName}", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to import job datasets: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to import job datasets", $"Exception: {ex}");
			}
		}

		public async Task UpdatePackageDatasets()
		{
			try
			{
				var sites = await siteProcessor.GetAllSitesAsync();
				foreach (var site in sites)
				{
					var previousRun = await webJobRunProcessor.GetMostRecentJobRunBySiteAndJobType(site.SiteName, WebJobConstants.PackageDatasetJobType, true);
					if (previousRun.CreateDate.Year == 1)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to find previous {WebJobConstants.PackageDatasetJobType} job for Site: {site.SiteName}", "");
					}
					else if (!await packageRepository.HavePackagesChangedForSiteAsync(site.SiteName, previousRun.CreateDate))
					{
						continue;
					}

					var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
					{
						Site = site,
						WebJobTypeConstant = WebJobConstants.PackageDatasetJobType,
						JobName = "Update Package Datasets",
						Message = "Update Package Datasets started"
					});
					var response = await packageDatasetProcessor.UpdatePackageDatasets(site, previousRun.CreateDate, webJobRun.CreateDate);
					await webJobRunProcessor.EndWebJob(new EndWebJobRequest
					{
						WebJobRun = webJobRun,
						IsSuccessful = response.IsSuccessful,
						NumberOfRecords = response.NumberOfDocuments,
						Message = "Update Package Datasets complete"
					});
					if (!response.IsSuccessful)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to import package datasets for Site: {site.SiteName}", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to import package datasets: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to import package datasets", $"Exception: {ex}");
			}
		}

		public async Task MonitorEodPackages(string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Monitor EoD packages incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var userName = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;

			try
			{
				var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					WebJobTypeConstant = WebJobConstants.EodPackageMonitorJobType,
					ProcessedDate = dateToProcess,
					JobName = "Monitor EOD Packages",
					Message = "Monitor EOD Packages started"
				});

				var response = await packageDatasetProcessor.MonitorEodPackages(site, userName, dateToProcess);
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfDocuments,
					Message = "Monitor EOD Packages complete"
				}); 
				if (!response.IsSuccessful)
				{
					emailService.SendServiceErrorNotifications($"Error: Failed to monitor EOD packages for site: {siteName} for processedDate: {dateToProcess}", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to monitor EOD packages for site: {siteName} for processedDate: {dateToProcess}: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to monitor EOD packages for site: {siteName} for processedDate: {dateToProcess}", $"Exception: {ex}");
			}
		}

		public async Task UpdateShippingContainerDatasets()
		{
			try
			{
				var sites = await siteProcessor.GetAllSitesAsync();
				foreach (var site in sites)
				{
					var previousRun = await webJobRunProcessor.GetMostRecentJobRunBySiteAndJobType(site.SiteName, WebJobConstants.ShippingContainerDatasetJobType, true);
					if (previousRun.CreateDate.Year == 1)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to find previous {WebJobConstants.ShippingContainerDatasetJobType} job for Site: {site.SiteName}", "");
					}
					else if (!await containerRepository.HaveContainersChangedForSiteAsync(site.SiteName, previousRun.CreateDate))
					{
						continue;
					}

					var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
					{
						Site = site,
						WebJobTypeConstant = WebJobConstants.ShippingContainerDatasetJobType,
						JobName = "Update Shipping Container Datasets",
						Message = "Update Shipping Container Datasets started"
					});
					var response = await shippingContainerDatasetProcessor.UpdateShippingContainerDatasets(site, previousRun.CreateDate);
					await webJobRunProcessor.EndWebJob(new EndWebJobRequest
					{
						WebJobRun = webJobRun,
						IsSuccessful = response.IsSuccessful,
						NumberOfRecords = response.NumberOfDocuments,
						Message = "Update Shipping Container Datasets complete"
					});
					if (!response.IsSuccessful)
					{
						emailService.SendServiceErrorNotifications($"Error: Failed to import shipping container datasets for Site: {site.SiteName}", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to import shipping container datasets: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to import shipping container datasets", $"Exception: {ex}");
			}
		}

		public async Task UpdateSubClientDatasets()
		{
			try
			{
				var previousRun = await webJobRunProcessor.GetMostRecentJobRunBySiteAndJobType(SiteConstants.AllSites, WebJobConstants.SubClientDatasetJobType, true);
				if (!await subClientRepository.HaveSubClientsChangedAsync(previousRun.CreateDate))
				{
					return;
				}

				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = new Site { SiteName = SiteConstants.AllSites },
					WebJobTypeConstant = WebJobConstants.SubClientDatasetJobType,
					JobName = "Update SubClient Datasets",
					Message = "Update Job Datasets started"
				});
				var response = await subClientDatasetProcessor.UpdateSubClientDatasets();
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = response.IsSuccessful,
					NumberOfRecords = response.NumberOfDocuments,
					Message = "Update SubClient Datasets complete"
				});
				if (!response.IsSuccessful)
				{
					emailService.SendServiceErrorNotifications($"Error: Failed to import SubClient datasets", $"Failed items: {response.NumberOfFailedDocuments} out of {response.NumberOfDocuments}. Exception: {response.Message}");
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to import SubClient datasets: {ex}");
				emailService.SendServiceErrorNotifications($"Error: Failed to import SubClient datasets", $"Exception: {ex}");
			}
		}
	}
}
