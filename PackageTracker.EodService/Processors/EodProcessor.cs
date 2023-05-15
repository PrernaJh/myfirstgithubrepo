using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IContainerDetailProcessor = PackageTracker.EodService.Interfaces.IContainerDetailProcessor;
using IEodProcessor = PackageTracker.EodService.Interfaces.IEodProcessor;
using IEvsFileProcessor = PackageTracker.EodService.Interfaces.IEvsFileProcessor;
using IExpenseProcessor = PackageTracker.EodService.Interfaces.IExpenseProcessor;
using IInvoiceProcessor = PackageTracker.EodService.Interfaces.IInvoiceProcessor;
using IPackageDetailProcessor = PackageTracker.EodService.Interfaces.IPackageDetailProcessor;
using IReturnAsnProcessor = PackageTracker.EodService.Interfaces.IReturnAsnProcessor;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Processors
{
    public class EodProcessor : IEodProcessor
    {
        private readonly IBinRepository binRepository;
        private readonly IContainerDetailProcessor containerDetailProcessor;
        private readonly IContainerRepository containerRepository;
        private readonly IEodContainerRepository eodContainerRepository;
        private readonly IEodPackageRepository eodPackageRepository;
        private readonly IExpenseProcessor expenseProcessor;
        private readonly IEvsFileProcessor evsFileProcessor;
        private readonly IInvoiceProcessor invoiceProcessor;
        private readonly ILogger<EodProcessor> logger;
        private readonly IOperationalContainerRepository operationalContainerRepository;
        private readonly IPackageDetailProcessor packageDetailProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly IRateProcessor rateProcessor;
        private readonly IReturnAsnProcessor returnAsnProcessor;
        private readonly ISemaphoreManager semaphoreManager;
        private readonly ISubClientProcessor subClientProcessor;

        public EodProcessor(
            IBinRepository binRepository,
            IContainerDetailProcessor containerDetailProcessor,
            IContainerRepository containerRepository,
            IEodContainerRepository eodContainerRepository,
            IEodPackageRepository eodPackageRepository,
            IEvsFileProcessor evsFileProcessor,
            IExpenseProcessor expenseProcessor,
            IInvoiceProcessor invoiceProcessor,
            ILogger<EodProcessor> logger,
            IOperationalContainerRepository operationalContainerRepository,
            IPackageDetailProcessor packageDetailProcessor,
            IPackageRepository packageRepository,
            IRateProcessor rateProcessor,
            IReturnAsnProcessor returnAsnProcessor,
            ISemaphoreManager semaphoreManager,
            ISubClientProcessor subClientProcessor)
        {
            this.binRepository = binRepository;
            this.containerDetailProcessor = containerDetailProcessor;
            this.containerRepository = containerRepository;
            this.eodContainerRepository = eodContainerRepository;
            this.eodPackageRepository = eodPackageRepository;
            this.evsFileProcessor = evsFileProcessor;
            this.expenseProcessor = expenseProcessor;
            this.invoiceProcessor = invoiceProcessor;
            this.logger = logger;
            this.operationalContainerRepository = operationalContainerRepository;
            this.packageDetailProcessor = packageDetailProcessor;
            this.packageRepository = packageRepository;
            this.rateProcessor = rateProcessor;
            this.returnAsnProcessor = returnAsnProcessor;
            this.semaphoreManager = semaphoreManager;
            this.subClientProcessor = subClientProcessor;
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> ProcessEndOfDayPackagesAsync(Site site, string webJobId, DateTime targetDate, DateTime lastScanDateTime)
        {
            try
            {
                var (isSuccessful, numberOfRecords) = (false, 0);
                var eodPackagesToImport = new List<EodPackage>();
                var eodPackagesToUpdate = new List<EodPackage>();
                var packagesToEvaluate = await packageRepository.GetPackagesForSqlEndOfDayProcess(site.SiteName, targetDate, lastScanDateTime);

                if (packagesToEvaluate.Any())
                {
                    logger.LogInformation($"{packagesToEvaluate.Count()} packages found for eod processing at {site.SiteName}");

                    foreach (var packageSubClientGroup in packagesToEvaluate.GroupBy(x => x.SubClientName))
                    {
                        var subClient = await subClientProcessor.GetSubClientByNameAsync(packageSubClientGroup.Key);
                        var packagesForEodImport = new List<Package>();
                        var packagesForEodUpdate = new List<Package>();

                        var ratedPackages = await rateProcessor.AssignPackageRatesForEod(subClient, packageSubClientGroup.ToList(), webJobId);
                        foreach (var package in ratedPackages)
                        {
                            if (package.PackageStatus == EventConstants.Processed)
                            {
                                packagesForEodImport.Add(package);
                            }                            
                            else
                            {
                                packagesForEodUpdate.Add(package);
                            }
                        }

                        var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, ratedPackages, false);
                        var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, ratedPackages, true);
                        foreach (var packageToUpdate in packagesForEodUpdate)
                        {
                            var eodPackage = GenerateEodPackage(site, webJobId, subClient, packageToUpdate);
                            eodPackage.IsPackageProcessed = false;
                            eodPackagesToUpdate.Add(eodPackage);
                        }

                        foreach (var packageToImport in packagesForEodImport)
                        {
                            var eodPackage = GenerateEodPackage(site, webJobId, subClient, packageToImport);
                            GenerateEodPackageFileRecords(site, subClient, eodPackage, packageToImport,
                                primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
                            eodPackagesToImport.Add(eodPackage);
                        }
                    }

                    var totalPackagesToImportOrUpdate = eodPackagesToUpdate.Count() + eodPackagesToImport.Count();
                    logger.LogInformation($"Completed Eod record generation for {totalPackagesToImportOrUpdate} packages");

                    var eodPackages = new List<EodPackage>();
                    eodPackages.AddRange(eodPackagesToImport);
                    eodPackages.AddRange(eodPackagesToUpdate);
                    var bulkResponse = await eodPackageRepository.UpsertEodPackagesAsync(eodPackages);
                    logger.LogInformation($"Total time for Eod packages bulk insert or update: {bulkResponse.ElapsedTime}");
                    if (bulkResponse.IsSuccessful)
                    {
                        if (totalPackagesToImportOrUpdate == 0)
                        {
                            logger.LogInformation($"No packages in correct state for Eod import or update for site {site.SiteName}");
                        }

                        packagesToEvaluate.ToList().ForEach(p => p.SqlEodProcessCounter = p.EodUpdateCounter);
                        var bulkPackageUpdateResponse = await packageRepository.UpdatePackagesSqlEodProcessed(packagesToEvaluate.ToList());
                        if (bulkPackageUpdateResponse.IsSuccessful)
                        {
                            logger.LogInformation($"Request Units Consumed by package update in Eod package process: {bulkPackageUpdateResponse.RequestCharge}, Total time: {bulkPackageUpdateResponse.ElapsedTime}");
                            isSuccessful = true;
                            numberOfRecords = packagesToEvaluate.Count();
                        }
                        else
                        {
                            logger.LogError($"Failure on package bulk update in Eod package process. WebJobId: {webJobId} Sitename: {site.SiteName}: {bulkPackageUpdateResponse.Message}");
                        }
                    }
                    else
                    {
                        logger.LogError($"Failure on eod bulk import/update in Eod package process. WebJobId: {webJobId} Sitename: {site.SiteName}");
                    }
                }
                else
                {
                    isSuccessful = true;
                    logger.LogInformation($"No packages found for eod packages process for site {site.SiteName}");
                }

                return (isSuccessful, numberOfRecords);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJobId} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> ProcessEndOfDayContainersAsync(Site site, string webJobId, DateTime targetDate, DateTime lastScanDateTime)
        {
            try
            {
                var (isSuccessful, numberOfRecords) = (false, 0);
                var containersForEodImport = new List<ShippingContainer>();
                var containersForEodUpdate = new List<ShippingContainer>();
                var eodContainersToImport = new List<EodContainer>();
                var eodContainersToUpdate = new List<EodContainer>();
                var containersToEvaluate = await containerRepository.GetContainersForSqlEndOfDayProcess(site.SiteName, targetDate, lastScanDateTime);

                if (containersToEvaluate.Any())
                {
                    logger.LogInformation($"{containersToEvaluate.Count()} containers found for eod processing at {site.SiteName}");

                    var activeBins = await GetUniqueBinsByActiveGroups(containersToEvaluate);

                    var ratedContainers = await rateProcessor.AssignContainerRatesForEod(site, containersToEvaluate.ToList(), webJobId);
                    foreach (var container in ratedContainers)
                    {
                        if (container.Status == ContainerEventConstants.Closed)
                        {
                            containersForEodImport.Add(container);
                        }
                        else
                        {
                            containersForEodUpdate.Add(container);
                        }                      
                    }

                    var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, ratedContainers, false);
                    var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, ratedContainers, true);
                    foreach (var containerToUpdate in containersForEodUpdate)
                    {
                        var eodContainerToUpdate = GenerateEodContainer(site, webJobId, containerToUpdate);
                        eodContainerToUpdate.IsContainerClosed = false;
                        eodContainersToUpdate.Add(eodContainerToUpdate);
                    }

                    foreach (var containerToImport in containersForEodImport)
                    {
                        var eodContainerToImport = GenerateEodContainer(site, webJobId, containerToImport);
                        GenerateEodContainerFileRecords(site, activeBins, eodContainerToImport, containerToImport,
                            primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
                        eodContainersToImport.Add(eodContainerToImport);
                    }

                    var totalContainersToImportOrUpdate = eodContainersToImport.Count() + eodContainersToUpdate.Count();
                    logger.LogInformation($"Completed Eod record generation for {totalContainersToImportOrUpdate} containers");

                    var eodContainers = new List<EodContainer>();
                    eodContainers.AddRange(eodContainersToImport);
                    eodContainers.AddRange(eodContainersToUpdate);
                    var bulkResponse = await eodContainerRepository.UpsertEodContainersAsync(eodContainers);
                    logger.LogInformation($"Total time for Eod containers bulk insert or update: {bulkResponse.ElapsedTime}");

                    if (bulkResponse.IsSuccessful)
                    {
                        if (totalContainersToImportOrUpdate == 0)
                        {
                            logger.LogInformation($"No containers in correct state for eod import or update for site {site.SiteName}");
                        }

                        containersToEvaluate.ToList().ForEach(c => c.SqlEodProcessCounter = c.EodUpdateCounter);

                        var bulkContainerUpdateResponse = await containerRepository.UpdateContainersSqlEodProcessed(containersToEvaluate.ToList());
                        if (bulkContainerUpdateResponse.IsSuccessful)
                        {
                            logger.LogInformation($"Request Units Consumed by container update in Eod container process: {bulkContainerUpdateResponse.RequestCharge}, Total time: {bulkContainerUpdateResponse.ElapsedTime}");
                            isSuccessful = true;
                            numberOfRecords = containersToEvaluate.Count();
                        }
                        else
                        {
                            logger.LogError($"Failure on container bulk update in eod container process. WebJobId: {webJobId} Sitename: {site.SiteName}: {bulkContainerUpdateResponse.Message}");
                        }
                    }
                    else
                    {
                        logger.LogError($"Failure on eod bulk import/update in eod container process. WebJobId: {webJobId} Sitename: {site.SiteName}");
                    }
                }
                else
                {
                    isSuccessful = true;
                    logger.LogInformation($"No containers found for eod containers process for site {site.SiteName}");
                }

                return (isSuccessful, numberOfRecords);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJobId} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }

        public async Task ResetPackageEod(Site site, DateTime targetDate, bool updateDatabase = true, bool cleanupFirst = true)
        {
            var lookbacks = EndOfDayUtility.GetLookbacksFromTargetDate(targetDate, site.TimeZone);
            var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{targetDate.Date}");
            await semaphore.WaitAsync();
            logger.LogInformation($"Begin Eod package reset for {targetDate} at {site.SiteName}");
            try
            {
                var siteTimeNow = TimeZoneUtility.GetLocalTime(site.TimeZone);
                if (cleanupFirst)
                {
                    var deleteResponse = await eodPackageRepository.DeleteEodPackages(site.SiteName, targetDate);
                    logger.LogInformation($"Total time for delete eod packages: {deleteResponse.ElapsedTime} for site: {site.SiteName}, date: {targetDate}");
                    if (!deleteResponse.IsSuccessful)
                    {
                        logger.LogError($"Eod package reset failed for {targetDate} at {site.SiteName}");
                        return;
                    }
                }

                var packages = await packageRepository.GetProcessedPackagesByDate(targetDate, siteName: site.SiteName);
                logger.LogInformation($"Retrieved {packages.Count()} packages for site: {site.SiteName}, date: {targetDate}");

                var subClients = await subClientProcessor.GetSubClientsAsync();
                var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), false);
                var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), true);

                int chunk = 500;
                var bulkResponse = new BatchDbResponse { IsSuccessful = true };
                for (int offset = 0; offset < packages.Count(); offset += chunk)
                {
                    var packagesChunk = packages.Skip(offset).Take(chunk);
                    var eodPackagesToAdd = new List<EodPackage>();
                    foreach (var package in packagesChunk)
                    {
                        var subClient = subClients.FirstOrDefault(s => s.Name == package.SubClientName);
                        var eodPackage = GenerateEodPackage(site, null, subClient, package);
                        eodPackagesToAdd.Add(eodPackage);
                        GenerateEodPackageFileRecords(site, subClient, eodPackage, package,
                            primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
                        package.SqlEodProcessCounter = package.EodUpdateCounter;
                    }

                    logger.LogInformation($"Add {eodPackagesToAdd.Count()} Eod packages for site: {site.SiteName}, date: {targetDate}");
                    var response = await eodPackageRepository.UpsertEodPackagesAsync(eodPackagesToAdd);
                    if (response.IsSuccessful)
                    {
                        logger.LogInformation($"Total time for Eod packages bulk insert or update: {response.ElapsedTime}");
                        bulkResponse.Count += response.Count;
                    }
                    else
                    {
                        bulkResponse.IsSuccessful = false;
                        break;
                    }

                    if (updateDatabase)
                    {
                        logger.LogInformation($"Update {eodPackagesToAdd.Count()}  packages for site: {site.SiteName}, date: {targetDate}");
                        var updateResponse = await packageRepository.UpdatePackagesEodProcessed(packagesChunk.ToList());
                        if (updateResponse.IsSuccessful)
                        {
                            logger.LogInformation($"Total time for packages update: {updateResponse.ElapsedTime}");
                        }
                        else
                        {
                            bulkResponse.IsSuccessful = false;
                            break;
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task ResetContainerEod(Site site, DateTime targetDate, bool updateDatabase = true, bool cleanupFirst = true)
        {
            var lookbacks = EndOfDayUtility.GetLookbacksFromTargetDate(targetDate, site.TimeZone);
            var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{targetDate.Date}");
            await semaphore.WaitAsync();
            logger.LogInformation($"Begin Eod container reset for {targetDate} at {site.SiteName}");
            try
            {
                var siteTimeNow = TimeZoneUtility.GetLocalTime(site.TimeZone);
                if (cleanupFirst)
                {
                    var deleteResponse = await eodContainerRepository.DeleteEodContainers(site.SiteName, targetDate);
                    logger.LogInformation($"Total time for delete Eod containers: { deleteResponse.ElapsedTime} for site: {site.SiteName}, date: {targetDate}");
                    if (!deleteResponse.IsSuccessful)
                    {
                        logger.LogError($"Eod container reset failed for {targetDate} at {site.SiteName}");
                        return;
                    }
                }

                var containers = await containerRepository.GetClosedContainersByDate(targetDate, siteName: site.SiteName);
                logger.LogInformation($"Retrieved {containers.Count()} containers for site: {site.SiteName}, date: {targetDate}.");

                var activeBins = await GetUniqueBinsByActiveGroups(containers);
                var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, containers.ToList(), false);
                var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, containers.ToList(), true);

                int chunk = 500;
                var bulkResponse = new BatchDbResponse { IsSuccessful = true };
                for (int offset = 0; offset < containers.Count(); offset += chunk)
                {
                    var containersChunk = containers.Skip(offset).Take(chunk);
                    var eodContainersToAdd = new List<EodContainer>();
                    foreach (var container in containersChunk)
                    {
                        var eodContainer = GenerateEodContainer(site, null, container);
                        eodContainersToAdd.Add(eodContainer);
                        GenerateEodContainerFileRecords(site, activeBins, eodContainer, container,
                            primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
                        container.SqlEodProcessCounter = container.EodUpdateCounter;
                    }

                    logger.LogInformation($"Add {eodContainersToAdd.Count()} Eod containers for site: {site.SiteName}, date: {targetDate}");
                    var response = await eodContainerRepository.UpsertEodContainersAsync(eodContainersToAdd);
                    if (response.IsSuccessful)
                    {
                        logger.LogInformation($"Total time for Eod containers bulk insert or update: {response.ElapsedTime}");
                        bulkResponse.Count += response.Count;
                    }
                    else
                    {
                        bulkResponse.IsSuccessful = false;
                        break;
                    }

                    if (updateDatabase)
                    {
                        logger.LogInformation($"Update {containersChunk.Count()} containers for site: {site.SiteName}, date: {targetDate}");
                        var updateResponse = await containerRepository.UpdateContainersEodProcessed(containersChunk.ToList());
                        if (updateResponse.IsSuccessful)
                        {
                            logger.LogInformation($"Total time for containers update: {updateResponse.ElapsedTime}");
                        }
                        else
                        {
                            bulkResponse.IsSuccessful = false;
                            break;
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public EodPackage GenerateEodPackage(Site site, string webJobId, SubClient subClient, Package packageToImport)
        {
            var eodPackageToImport = new EodPackage
            {
                CosmosId = packageToImport.Id,
                CosmosCreateDate = packageToImport.CreateDate,
                PackageId = packageToImport.PackageId,
                ContainerId = packageToImport.ContainerId,
                Barcode = packageToImport.Barcode,
                LocalProcessedDate = packageToImport.LocalProcessedDate.Date,
                SiteName = site.SiteName,
                SubClientName = subClient.Name,
                IsPackageProcessed = true,
            };
            return eodPackageToImport;
        }

        public void GenerateEodPackageFileRecords(Site site, SubClient subClient, EodPackage eodPackage, Package package,
            Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps)
        {

            eodPackage.PackageDetailRecord = packageDetailProcessor.CreatePackageDetailRecord(package);
            eodPackage.ReturnAsnRecord = package.ClientName == ClientSubClientConstants.DalcClientName
                                 ? returnAsnProcessor.CreateReturnAsnShortRecord(package)
                                 : returnAsnProcessor.CreateReturnAsnRecord(package);
            eodPackage.InvoiceRecord = invoiceProcessor.CreateInvoiceRecord(package);
            eodPackage.ExpenseRecord = expenseProcessor.CreatePackageExpenseRecord(package);
            eodPackage.EvsPackage = evsFileProcessor.GenerateManifestPackage(site, subClient, package,
                primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
        }

        public EodContainer GenerateEodContainer(Site site, string webJobId, ShippingContainer containerToImport)
        {
            var eodContainerToImport = new EodContainer
            {
                CosmosId = containerToImport.Id,
                CosmosCreateDate = containerToImport.CreateDate,
                ContainerId = containerToImport.ContainerId,
                IsContainerClosed = true,
                CreateDate = DateTime.Now,
                LocalProcessedDate = containerToImport.LocalProcessedDate.Date,
                SiteName = site.SiteName
            };

            return eodContainerToImport;
        }

        public void GenerateEodContainerFileRecords(Site site, List<Bin> activeBins, EodContainer eodContainer, ShippingContainer container,
            Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps)
        {
            if (ShouldProduceContainerDetailRecord(container))
            {
                var bin = activeBins.FirstOrDefault(x => x.BinCode == container.BinCode && x.ActiveGroupId == container.BinActiveGroupId);
                eodContainer.ContainerDetailRecord = containerDetailProcessor.CreateContainerDetailRecord(site, bin, container);
            }
            if (ShouldProducePmodRecords(container))
            {
                eodContainer.PmodContainerDetailRecord = containerDetailProcessor.CreatePmodContainerDetailRecord(container);
                eodContainer.ExpenseRecord = expenseProcessor.CreateContainerExpenseRecord(container);
                eodContainer.EvsPackageRecord = evsFileProcessor.GenerateManifestPackageForContainer(site, container,
                    primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
            }
            eodContainer.EvsContainerRecord = evsFileProcessor.GenerateManifestContainer(site, container,
                primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
        }

        private bool ShouldProduceContainerDetailRecord(ShippingContainer container)
        {
            var ineligibleCarriers = new List<string>()
            {
                ContainerConstants.FedExCarrier,
                ContainerConstants.UpsCarrier,
                ContainerConstants.UspsCarrier,
                ContainerConstants.LocalUsps
            };

            if (ineligibleCarriers.Contains(container.ShippingCarrier))
            {
                return false;
            }
            return true;
        }

        private bool ShouldProducePmodRecords(ShippingContainer container)
        {
            if (container.ShippingMethod == ContainerConstants.UspsPmodBag || container.ShippingMethod == ContainerConstants.UspsPmodPallet)
            {
                return true;
            }
            return false;
        }

        public async Task<List<Bin>> GetUniqueBinsByActiveGroups(IEnumerable<ShippingContainer> containers)
        {
            var activeBins = new List<Bin>();
            var containersByActiveGroup = containers.GroupBy(x => x.BinActiveGroupId);

            foreach (var containerActiveGroup in containersByActiveGroup)
            {
                var activeBinsByGroupId = await binRepository.GetBinsByActiveGroupIdAsync(containerActiveGroup.Key);

                foreach (var containerBinCodeGroup in containerActiveGroup.GroupBy(x => x.BinCode))
                {
                    activeBins.Add(activeBinsByGroupId.FirstOrDefault(x => x.BinCode == containerBinCodeGroup.Key));
                }
            }

            return activeBins;
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> CheckForDuplicateEndOfDayPackagesAsync(Site site, DateTime dateToCheck, string webJobId)
        {
            try
            {
                var eodPackages = await eodPackageRepository.GetEodOverview(site.SiteName, dateToCheck);
                var count = 0;
                var eodPackagesToUpdate = new List<EodPackage>();
                foreach (var group in eodPackages.OrderByDescending(p => p.CreateDate).GroupBy(p => p.PackageId))
                {
                    if (group.Count() > 1)
                    {
                        // Remove old packages in group that have the same package Id.
                        foreach (var package in group.Skip(1).Where(p => p.PackageId == group.First().PackageId))
                        {
                            package.IsPackageProcessed = false;
                            eodPackagesToUpdate.Add(package);
                            logger.LogInformation($"Removed duplicate packages for {group.Key}");
                        }
                    }
                }
                eodPackages = eodPackages.Where(p => p.IsPackageProcessed).ToList();
                foreach (var group in eodPackages.OrderByDescending(p => p.CreateDate).GroupBy(p => p.Barcode))
                {
                    if (group.Count() > 1)
                    {
                        logger.LogInformation($"Found duplicate packages for {group.Key}");
                        count++;
                    }
                }
                var response = await eodPackageRepository.UpsertEodPackagesAsync(eodPackagesToUpdate);
                return (response.IsSuccessful, count);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJobId} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> CheckForDuplicateEndOfDayContainersAsync(Site site, DateTime dateToCheck, string webJobId)
        {
            try
            {
                var eodContainers = await eodContainerRepository.GetEodOverview(site.SiteName, dateToCheck);
                var count = 0;
                var eodContainersToUpdate = new List<EodContainer>();
                foreach (var group in eodContainers.OrderByDescending(p => p.CreateDate).GroupBy(p => p.ContainerId))
                {
                    if (group.Count() > 1)
                    {
                        // Remove old containers in group that have the same container Id.
                        foreach (var container in group.Skip(1))
                        {
                            container.IsContainerClosed = false;
                            eodContainersToUpdate.Add(container);
                            logger.LogInformation($"Removed duplicate containers for {group.Key}");
                        }
                        if (group.Count(p => p.IsContainerClosed) > 1)
                        {
                            logger.LogInformation($"Found duplicate containers for {group.Key}");
                            count++;
                        }
                    }
                }
                var response = await eodContainerRepository.UpsertEodContainersAsync(eodContainersToUpdate);
                return (response.IsSuccessful, count);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJobId} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> DeleteObsoleteContainersAsync(Site site, DateTime dateToProcess, WebJobRun webJob)
        {
            try
            {
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var now = DateTime.UtcNow;
                var localTimeOffset = (int)(now - siteLocalTime).TotalMinutes;
                var operationalContainers =
                    await operationalContainerRepository.GetOutOfDateOperationalContainersAsync(site.SiteName, dateToProcess.AddDays(1), localTimeOffset);
                logger.LogInformation($"{operationalContainers.Count()} outdated operational containers found for for site {site.SiteName}.");

                var operationalContainersToUpdate = new List<OperationalContainer>();
                foreach (var container in operationalContainers)
                {
                    container.Status = ContainerEventConstants.Deleted;
                    operationalContainersToUpdate.Add(container);
                }
                var containers = await containerRepository.GetOutOfDateContainersAsync(site.SiteName, dateToProcess.AddDays(1));
                logger.LogInformation($"{containers.Count()} outdated containers found for for site {site.SiteName}.");
                var containersToUpdate = new List<ShippingContainer>();
                foreach (var container in containers)
                {
                    logger.LogInformation($"Removed outdated container: {container.ContainerId}");
                    container.Status = ContainerEventConstants.Deleted;
                    container.Events.Add(new Event
                    {
                        EventId = container.Events.Count + 1,
                        EventType = ContainerEventConstants.Deleted,
                        EventStatus = container.Status,
                        Description = "Container deleted",
                        Username = webJob.Username,
                        MachineId = "System",
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                    });
                    containersToUpdate.Add(container);
                }
                var operationalResponse = await operationalContainerRepository.UpdateItemsAsync(operationalContainersToUpdate);
                var containerResponse = await containerRepository.UpdateItemsAsync(containersToUpdate);
                return (operationalResponse.IsSuccessful && containerResponse.IsSuccessful, operationalContainers.Count());
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJob.Id} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }

        public async Task<(bool IsSuccessful, int NumberOfRecords)> DeleteEmptyContainersAsync(Site site, DateTime dateToCheck, WebJobRun webJob)
        {
            try
            {
                var eodOverview = await eodContainerRepository.GetEodOverview(site.SiteName, dateToCheck);
                var eodContainers = await eodContainerRepository.GetReferencedContainers(site.SiteName, dateToCheck);
                var containers = await containerRepository.GetClosedContainersByDate(dateToCheck, site.SiteName);
                var count = 0;
                var eodContainersToUpdate = new List<EodContainer>();
                var containersToUpdate = new List<ShippingContainer>();
                foreach (var eodContainer in eodOverview)
                {
                   if (eodContainers.FirstOrDefault(c => c.CosmosId == eodContainer.CosmosId) == null)
                   {
                        logger.LogInformation($"Removed empty container: {eodContainer.ContainerId}");
                        count++;
                        eodContainer.IsContainerClosed = false;
                        eodContainersToUpdate.Add(eodContainer);

                        var container = containers.FirstOrDefault(c => c.Id == eodContainer.CosmosId);
                        if (container != null)
                        {
                            container.Status = ContainerEventConstants.Deleted;
                            container.Events.Add(new Event
                            {
                                EventId = container.Events.Count + 1,
                                EventType = ContainerEventConstants.Deleted,
                                EventStatus = container.Status,
                                Description = "Container deleted",
                                Username = webJob.Username,
                                MachineId = "System",
                                EventDate = DateTime.Now,
                                LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                            });
                            containersToUpdate.Add(container);
                        }
                   }
                }
                var eodResponse = await eodContainerRepository.UpsertEodContainersAsync(eodContainersToUpdate);
                var containerResponse = await containerRepository.UpdateItemsAsync(containersToUpdate);
                return (eodResponse.IsSuccessful && containerResponse.IsSuccessful, count);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure in eod process. WebJobId: {webJob.Id} Sitename: {site.SiteName} Exception: {ex}");
                return (false, 0);
            }
        }
    }
}
