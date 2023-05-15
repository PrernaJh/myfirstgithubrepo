using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Service.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Service
{
	public class CreatedPackagePostProcessService : ICreatedPackagePostProcessService
	{
		private readonly ILogger<CreatedPackagePostProcessService> logger;

		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IEmailService emailService;
		private readonly IPackagePostProcessor packagePostProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;
		private readonly IZipOverrideProcessor zipOverrideProcessor;

		public CreatedPackagePostProcessService(ILogger<CreatedPackagePostProcessService> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IEmailService emailService,
			IPackagePostProcessor packagePostProcessor,
			IPackageRepository packageRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor,
			IZipOverrideProcessor zipOverrideProcessor
			)
		{
			this.logger = logger;

			this.activeGroupProcessor = activeGroupProcessor;
			this.emailService = emailService;
			this.packagePostProcessor = packagePostProcessor;
			this.packageRepository = packageRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.zipOverrideProcessor = zipOverrideProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task PostProcessCreatedPackages()
		{
			var subClients = await subClientProcessor.GetSubClientsAsync();
			foreach (var subClient in subClients)
			{
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var mostRecentRun = await webJobRunProcessor.GetMostRecentJobRunBySubClientAndJobType(
					site.SiteName, subClient.Name, WebJobConstants.PostProcessCreatedPackagesJobType, true);
				if (!await packageRepository.HavePackagesChangedForSiteAsync(site.SiteName, mostRecentRun.CreateDate))
					continue;
				var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
				{
					Site = site,
					SubClientName = subClient.Name,
					WebJobTypeConstant = WebJobConstants.PostProcessCreatedPackagesJobType,
					JobName = "Post Process Created Packages",
					Message = "Post Process Created Packages started"
				});
				try
				{
					var zipOverrideGroupIds = await activeGroupProcessor.GetZipOverrideActiveGroupIds(subClient.Name);
					var packages = (await packagePostProcessor.GetPackagesForCreatePackagePostProcessing(
						subClient.Name, mostRecentRun.CreateDate, webJobRun.CreateDate)).ToList();
					await zipOverrideProcessor.AssignZipOverridesForListOfPackages(packages, zipOverrideGroupIds);
					packages.ForEach(p => p.WebJobIds.Add(webJobRun.Id));
					await packageRepository.UpdatePackagesForCreatePackagePostProcess(packages);
					webJobRun.NumberOfRecords = packages.Count();
					webJobRun.IsSuccessful = true;
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, $"Post Process Created Packages failed for subClient: {subClient.Name}. Exception: { ex }");
					emailService.SendServiceErrorNotifications($"Post Process Created Packages failed for subClient: {subClient.Name}", ex.ToString());
				}
				await webJobRunProcessor.EndWebJob(new EndWebJobRequest
				{
					WebJobRun = webJobRun,
					IsSuccessful = webJobRun.IsSuccessful,
					NumberOfRecords = webJobRun.NumberOfRecords,
					Message = webJobRun.IsSuccessful ? "Post Process Created Packages complete" : "Post Process Created Packages failed",
				});
			}
		}
	}
}

