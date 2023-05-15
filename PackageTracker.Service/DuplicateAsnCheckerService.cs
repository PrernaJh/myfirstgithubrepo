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
    public class DuplicateAsnCheckerService : IDuplicateAsnCheckerService
    {
        private readonly ILogger<DuplicateAsnCheckerService> logger;
        private readonly IEmailService emailService;
        private readonly IPackageDuplicateProcessor packageDuplicateProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public DuplicateAsnCheckerService(ILogger<DuplicateAsnCheckerService> logger,
            IEmailService emailService, 
            IPackageDuplicateProcessor packageDuplicateProcessor, 
            ISiteProcessor siteProcessor,
            IWebJobRunProcessor webJobRunProcessor)
        {
            this.logger = logger;
            this.emailService = emailService;
            this.packageDuplicateProcessor = packageDuplicateProcessor;
            this.siteProcessor = siteProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        public async Task CheckForDuplicateAsns(WebJobSettings webJobSettings)
        {
            var sites = await siteProcessor.GetAllSitesAsync();
            foreach (var site in sites)
            {
                if (webJobSettings.IsDuringScheduledHours(site))
                {
                    var result = await packageDuplicateProcessor.GetPackagesForDuplicateAsnChecker(site.SiteName);
                    var duplicatePackages = result.ToList();

                    await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                    {
                        SiteName = site.SiteName,
                        JobName = "Monitor Recent ASN Imports for duplicates",
                        JobType = WebJobConstants.CheckForDuplicateAsnsJobType,
                        Username = "System",
                        Message = string.Empty,
                        IsSuccessful = true
                    });

                    if (duplicatePackages.Any())
                    {
                        var ids = new List<string>();
                        duplicatePackages.ForEach(p => ids.Add(p.PackageId));
                        var text = String.Join(", ", ids);
                        logger.Log(LogLevel.Information,
                            $"ASN File Import Alert sent for site: {site.SiteName}, Duplicate ASNs: {text}");

                        emailService.SendServiceErrorNotifications("ASN File Import: Duplicate ASNs",
                            $"{site.SiteName}: {text}.");
                    }
                }
            }
        }
    }
}
