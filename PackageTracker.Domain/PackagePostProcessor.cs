using Microsoft.Extensions.Logging;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class PackagePostProcessor : IPackagePostProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<PackagePostProcessor> logger;
        private readonly IPackageDuplicateProcessor packageDuplicateProcessor;
        private readonly IPackageRepository packageRepository;

        public PackagePostProcessor(IActiveGroupProcessor activeGroupProcessor,
                                    ILogger<PackagePostProcessor> logger,
                                    IPackageDuplicateProcessor packageDuplicateProcessor,
                                    IPackageRepository packageRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.packageDuplicateProcessor = packageDuplicateProcessor;
            this.packageRepository = packageRepository;
        }

        public async Task<bool> UpdatePackageServiceRuleGroupIds(string subClientName, int daysToLookback, string webJobId)
        {
            var isSuccessful = false;
            var currentServiceRuleGroupId = await activeGroupProcessor.GetServiceRuleActiveGroupIdAsync(subClientName);
            var packagesToUpdate = new List<Package>();
            var packages = await packageRepository.GetImportedOrReleasedPackagesBySubClient(subClientName, daysToLookback);

            foreach (var package in packages)
            {
                if (package.ServiceRuleGroupId != currentServiceRuleGroupId)
                {
                    package.WebJobIds.Add(webJobId);
                    package.HistoricalServiceRuleGroupIds.Add(package.ServiceRuleGroupId);
                    package.ServiceRuleGroupId = currentServiceRuleGroupId;
                    packagesToUpdate.Add(package);
                }
            }

            if (packagesToUpdate.Any())
            {
                var bulkResponse = await packageRepository.UpdatePackagesSetServiceRuleGroupIds(packagesToUpdate);
                isSuccessful = bulkResponse.IsSuccessful;
            }
            else
            {
                isSuccessful = true;
            }
            logger.LogInformation($"Updated current service rule group ID to {currentServiceRuleGroupId} for {packagesToUpdate.Count()} packages");

            return isSuccessful;
        }

        public async Task<IEnumerable<Package>> GetPackagesWithOutdatedBinData(int daysToLookback, int maxCount, string subClientName, string binGroupId, string binMapGroupId)
        {
            return await packageRepository.GetPackagesWithOutdatedBinData(daysToLookback, maxCount, subClientName, binGroupId, binMapGroupId);
        }

        public async Task<IEnumerable<Package>> GetPackagesByShippingContainerAsync(string containerId, string siteName)
        {
            return await packageRepository.GetPackagesByContainerAsync(containerId, siteName);
        }

        public async Task<bool> IsContainerAssignedToPackages(string containerId, string siteName)
        {
            var packages = await packageRepository.GetPackageIdsByContainerAsync(containerId, siteName);

            if (packages.Any())
            {
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Package>> GetPackagesForConsumerDetailFile(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime)
        {
            return await packageRepository.GetPackagesForConsumerDetailFile(subClientName, lastScanDateTime, nextScanDateTime);
        }

        public async Task<IEnumerable<Package>> GetPackagesForCreatePackagePostProcessing(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime)
        {
            return await packageRepository.GetPackagesForCreatePackagePostProcessing(subClientName, lastScanDateTime, nextScanDateTime);
        }
        public async Task<Package> GetPackageByPackageId(string packageId, string siteName)
        {
            var packages = (await packageRepository.GetPackagesByPackageId(packageId, siteName)).ToList();
            var package = await packageDuplicateProcessor.EvaluateDuplicatePackagesOnScan(packages);
            return package;

        }

        public async Task<int> CountPackagesForContainerAsync(string containerId, string siteName)
        {
            return await packageRepository.CountPackagesForContainerAsync(containerId, siteName);
        }
    }
}
