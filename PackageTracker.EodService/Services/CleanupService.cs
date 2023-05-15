using AutoMapper.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Interfaces;
using System;
using System.Threading.Tasks;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Services
{
    public class CleanupService : ICleanupService
    {
        private readonly ILogger<CleanupService> logger;
        private readonly IEmailService emailService;
        private readonly IEodContainerRepository eodContainerRepository;
        private readonly IEodPackageRepository eodPackageRepository;
        private readonly ISiteProcessor siteProcessor;

        public CleanupService(ILogger<CleanupService> logger,
            IEmailService emailService,
            IEodContainerRepository eodContainerRepository,
            IEodPackageRepository eodPackageRepository,
            ISiteProcessor siteProcessor
            )
        {
            this.logger = logger;
            this.emailService = emailService;
            this.eodContainerRepository = eodContainerRepository;
            this.eodPackageRepository = eodPackageRepository;
            this.siteProcessor = siteProcessor;
        }

        public async Task CleanupEodCollectionsAsync(WebJobSettings webJobSettings)
        {
            var monthsBeforeDelete = webJobSettings.GetParameterIntValue("MonthsBeforeDelete", 6);
            var chunkSize =  webJobSettings.GetParameterIntValue("ChunkSize", 10000);
            var sites = await siteProcessor.GetAllSitesAsync();
            foreach (var site in sites)
            {
                var cutoff = TimeZoneUtility.GetLocalTime(site.TimeZone).Date.AddMonths(-monthsBeforeDelete);

                // Delete Eod containers which are older than, for example, 6 months ...
                while ((await eodContainerRepository.CountOldEodContainersAsync(site.SiteName, cutoff)) > 0)
                {
                    var bulkContainerResponse = await eodContainerRepository.DeleteOldEodContainersAsync(site.SiteName, cutoff, chunkSize);
                    if (bulkContainerResponse.IsSuccessful)
                    {
                        logger.LogInformation($"Time to delete Eod containers for site: {site.SiteName}, Cutoff: {cutoff}: {bulkContainerResponse.ElapsedTime}");
                    }
                    else
                    {
                        logger.LogError($"Failed to Delete Eod containers for site: {site.SiteName}, Cutoff: {cutoff}: {bulkContainerResponse.Message}");
                        emailService.SendServiceErrorNotifications($"Eod Cleanup Service: Failed to Delete Eod containers for site: {site.SiteName}, Cutoff: {cutoff}.", bulkContainerResponse.Message);
                        break;
                    }
                }

                // Delete Eod packages which are older than, for example, 6 months ...
                while ((await eodPackageRepository.CountOldEodPackagesAsync(site.SiteName, cutoff)) > 0)
                {
                    var bulkPackageResponse = await eodPackageRepository.DeleteOldEodPackagesAsync(site.SiteName, cutoff, chunkSize);
                    if (bulkPackageResponse.IsSuccessful)
                    {
                        logger.LogInformation($"Time to delete Eod packages for site: {site.SiteName}, Cutoff: {cutoff}: {bulkPackageResponse.ElapsedTime}");
                    }
                    else
                    {
                        logger.LogError($"Failed to Delete Eod packages for site: {site.SiteName}, Cutoff: {cutoff}: {bulkPackageResponse.Message}");
                        emailService.SendServiceErrorNotifications($"Eod Cleanup Service: Failed to Delete Eod packages for site: {site.SiteName}, Cutoff: {cutoff}.", bulkPackageResponse.Message);
                        break;
                    }
                }
            }
        }
    }
}
