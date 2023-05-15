using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.ArchiveService.Interfaces;
using ParcelPrepGov.Reports.Interfaces;
using System;
using System.Threading.Tasks;
using PackageTracker.Data.Utilities;
using System.Linq;

namespace PackageTracker.ArchiveService
{
    public class ArchiveDataService : IArchiveDataService
    {
        private readonly ILogger<ArchiveDataService> logger;
        private readonly IEmailService emailService;
        private readonly IArchiveDataProcessor archiveDataProcessor;
        private readonly IPackageDatasetRepository packageDatasetRepository;
        private readonly IShippingContainerDatasetRepository shippingContainerDatasetRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public ArchiveDataService(ILogger<ArchiveDataService> logger,
            IEmailService emailService,
            IArchiveDataProcessor archiveDataProcessor,
            IPackageDatasetRepository packageDatasetRepository,
            IShippingContainerDatasetRepository shippingContainerDatasetRepository,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            IWebJobRunProcessor webJobRunProcessor
            )
        {
            this.logger = logger;
            this.emailService = emailService;
            this.archiveDataProcessor = archiveDataProcessor;
            this.packageDatasetRepository = packageDatasetRepository;
            this.shippingContainerDatasetRepository = shippingContainerDatasetRepository;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        public async Task ArchivePackagesAsync(WebJobSettings webJobSettings)
        {
            DateTime.TryParse(webJobSettings.GetParameterStringValue("StartDate", "2021-01-01"), out var startDate);
            var sites = await siteProcessor.GetAllSitesAsync();
            var subClients = await subClientProcessor.GetSubClientsAsync();
            foreach(var site in sites)
            {
                var lookback = TimeZoneUtility.GetLocalTime(site.TimeZone).Date
                    .AddMonths(-webJobSettings.GetParameterIntValue("MonthsBeforeArchive", 6));
                var allSubClientsSuccessfulForSite = true;
                foreach (var subClient in subClients.Where(sc => sc.SiteName == site.SiteName))
                {
                    var isSuccessful = true;
                    var message = string.Empty;
                    var numberOfRecords = 0;
                    var subClientStartDate = subClient.StartDate.Year != 1 ? subClient.StartDate : startDate;
                    try
                    {
                        // Delete unprocessed packages which are older than, for example, 6 months ...
                        await packageDatasetRepository.DeleteOlderPackagesAsync(subClient.Name, false, lookback.AddDays(-1));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to Delete Unarchived Packages for subClient: {subClient.Name}, Lookback: {lookback}. Exception: { ex }");
                        emailService.SendServiceErrorNotifications("Archive Data Service: Failed to Delete Unarchived Packages for subClient", ex.ToString());
                    }
                    if (subClientStartDate >= lookback)
                        continue;

                    // While there are more dates to process:
                    while (isSuccessful)
                    {
                        var package = await packageDatasetRepository.FindOldestPackageForArchiveAsync(subClient.Name, subClientStartDate);
                        if (package == null || package.LocalProcessedDate.Date >= lookback)
                            break;
                        var processedDate = package.LocalProcessedDate.Date;
                        var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
                        {
                            Site = site,
                            SubClientName = subClient.Name,
                            ProcessedDate = processedDate,
                            WebJobTypeConstant = WebJobConstants.PackageArchiveJobType,
                            JobName = "Archive Data Export",
                            Message = "Archive Data Export started"
                        });
                        try
                        {
                            var packages = await packageDatasetRepository.GetPackagesForArchiveAsync(subClient.Name, processedDate);
                            logger.LogInformation($"Archive {packages.Count} packages for subClient: {subClient.Name} for manifest date: {processedDate}.");
                            numberOfRecords = packages.Count;
                            await archiveDataProcessor.ArchivePackagesAsync(subClient, processedDate, packages);

                            await packageDatasetRepository.DeleteArchivedPackagesAsync(subClient.Name, processedDate);
                            message = "Archive Data Export completed";
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Failed to Export Archive Packages for subClient: {subClient.Name}, Manifest Date: {processedDate}. Exception: { ex }");
                            emailService.SendServiceErrorNotifications($"Archive Data Service: Failed to Export Archive Packages for subClient: {subClient.Name}", ex.ToString());
                            message = ex.Message;
                            isSuccessful = false;
                        }
                        await webJobRunProcessor.EndWebJob(new EndWebJobRequest
                        {
                            WebJobRun = webJobRun,
                            IsSuccessful = isSuccessful,
                            NumberOfRecords = numberOfRecords,
                            Message = message,
                        });
                    }
                    try
                    {
                        // Delete processed packages which are older than the start date ...
                        if (isSuccessful)
                            await packageDatasetRepository.DeleteOlderPackagesAsync(subClient.Name, true, subClientStartDate.AddDays(-1));
                        else
                            allSubClientsSuccessfulForSite = false;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to Delete Unarchived Packages for subClient: {subClient.Name}, Start Date: {subClientStartDate}. Exception: { ex }");
                        emailService.SendServiceErrorNotifications($"Archive Data Service: Failed to Delete Unarchived Packages for subClient: {subClient.Name}", ex.ToString());
                    }
                }
                if (allSubClientsSuccessfulForSite)
                {
                    try
                    {
                        // Delete containers which are older than, for example, 6 months ...
                        await shippingContainerDatasetRepository.DeleteOlderContainersAsync(site.SiteName, lookback.AddDays(-1));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to Delete Closed Containers for site: {site.SiteName}, Lookback: {lookback}. Exception: { ex }");
                        emailService.SendServiceErrorNotifications($"Archive Data Service: Failed to Delete Closed Containers for site: {site.SiteName}", ex.ToString());
                    }
                }
            }
        }
   }
}
