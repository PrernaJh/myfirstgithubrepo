using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class PackageDuplicateProcessor : IPackageDuplicateProcessor
    {
        private readonly ILogger<PackageDuplicateProcessor> logger;
        private readonly IPackageRepository packageRepository;

        public PackageDuplicateProcessor(ILogger<PackageDuplicateProcessor> logger,
            IPackageRepository packageRepository)
        {
            this.logger = logger;
            this.packageRepository = packageRepository;
        }

        public async Task<IEnumerable<Package>> GetDuplicatePackages(Package package)
        {
            var duplicatePackages = await packageRepository.GetPackagesByPackageId(package.PackageId, package.SiteName);
            return CheckDuplicatePackages(package, duplicatePackages);
        }

        public async Task<IEnumerable<Package>> GetPackagesForDuplicateAsnChecker(string siteName)
        {
            var packages = await packageRepository.GetPackagesForDuplicateAsnChecker(siteName);
            var result = new List<Package>();
            var now = DateTime.Now;
            // Find duplicates
            foreach (var group in packages.GroupBy(p => p.PackageId))
            {
                if (group.Count() > 1 && group.FirstOrDefault().CreateDate == now)
                    result.Add(group.FirstOrDefault());
            }
            return result;
        }

        public async Task<Package> EvaluateDuplicatePackagesOnScan(List<Package> packages, bool isRepeatScan = false)
        {
            var isCreated = false;
            var packageToAttemptToProcess = new Package();
            var packageStatusList = new Dictionary<string, string>();

            if (packages.Any(x => x.IsCreated))
            {
                isCreated = true;
            }
            else
            {
                foreach (var package in packages)
                {
                    packageStatusList.Add(package.Id, package.PackageStatus);
                    CheckDuplicatePackages(package, packages);
                }
            }

            if (isCreated)
            {
                if (!isRepeatScan && packages.Count() == 1 && packages.Single().PackageStatus == EventConstants.Created)
                {
                    packageToAttemptToProcess = packages.Single();
                }
            }
            else
            {
                foreach (var package in packages)
                {
                    var oldStatus = packageStatusList.Single(x => x.Key == package.Id).Value;
                    if (package.PackageStatus != oldStatus)
                    {
                        await packageRepository.UpdateItemAsync(package); // Update changed packages so these changes end up in reports
                    }
                }

                packageToAttemptToProcess = packages.FirstOrDefault(p => p.PackageStatus != EventConstants.Blocked && p.PackageStatus != EventConstants.Replaced) ?? new Package();
            }

            return packageToAttemptToProcess;
        }

        public IEnumerable<Package> CheckDuplicatePackages(Package package, IEnumerable<Package> duplicatePackages)
        {
            var duplicateImports = new List<Package>();
            var recalledReleasedPackages = duplicatePackages.Where(x => x.Id != package.Id &&
                                                        (x.PackageStatus == EventConstants.Recalled ||
                                                        x.PackageStatus == EventConstants.Released)
                                                        ).ToList();
            var duplicatesWhichBlockImport = duplicatePackages.Where(x => x.Id != package.Id && (
                                                        x.PackageStatus == EventConstants.Processed
                                                        )).ToList();
            var recalledReleasedPackage = recalledReleasedPackages.FirstOrDefault();
            if (recalledReleasedPackage != null && package.PackageStatus == EventConstants.Imported)
            {
                if (recalledReleasedPackage.RecallStatus == EventConstants.RecallCreated ||
                    recalledReleasedPackage.RecallStatus == EventConstants.Imported ||
                    recalledReleasedPackage.RecallStatus == EventConstants.Released)
                {
                    if (recalledReleasedPackage.PackageStatus == EventConstants.Recalled)
                    {
                        package.PackageStatus = EventConstants.Recalled;
                        package.RecallDate = recalledReleasedPackage.RecallDate;
                        package.RecallStatus = EventConstants.Imported;
                    }
                    else
                    {
                        package.PackageStatus = EventConstants.Released;
                        package.RecallDate = recalledReleasedPackage.RecallDate;
                        package.ReleaseDate = recalledReleasedPackage.ReleaseDate;
                        package.RecallStatus = EventConstants.Released;
                    }
                    recalledReleasedPackage.PackageStatus = EventConstants.Replaced;
                    package.PackageEvents = new List<Event>(recalledReleasedPackage.PackageEvents);
                    package.DuplicatePackageIds.Add(recalledReleasedPackage.Id);
                    duplicateImports.Add(recalledReleasedPackage);
                    logger.LogInformation($"Imported package set to {package.PackageStatus} by previous package, packageId: {package.PackageId}");
                }
                else
                {
                    duplicatesWhichBlockImport.Add(recalledReleasedPackage);
                }
            }
            if (duplicatesWhichBlockImport.Any())
            {
                package.PackageStatus = EventConstants.Blocked;
                foreach (var otherDuplicate in duplicatesWhichBlockImport)
                {
                    package.DuplicatePackageIds.Add(otherDuplicate.Id);
                    logger.LogInformation($"Imported package blocked by previous package with status: {otherDuplicate.PackageStatus}, packageId: {package.PackageId}");
                }
            }
            return duplicateImports;
        }
    }
}
