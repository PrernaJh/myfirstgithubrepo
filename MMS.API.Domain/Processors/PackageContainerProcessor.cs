using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Containers;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class PackageContainerProcessor : IPackageContainerProcessor
    {
        private readonly IContainerRepository containerRepository;
        private readonly IEodPostProcessor eodPostProcessor;
        private readonly ILogger<PackageContainerProcessor> logger;
        private readonly IOperationalContainerProcessor operationalContainerProcessor;
        private readonly IPackageRepository packageRepository;

        public PackageContainerProcessor(
            IContainerRepository containerRepository,
            IEodPostProcessor eodPostProcessor,
            ILogger<PackageContainerProcessor> logger,
            IOperationalContainerProcessor operationalContainerProcessor,
            IPackageRepository packageRepository)
        {
            this.containerRepository = containerRepository;
            this.eodPostProcessor = eodPostProcessor;
            this.logger = logger;
            this.operationalContainerProcessor = operationalContainerProcessor;
            this.packageRepository = packageRepository;
        }

        public async Task AssignPackageContainerData(Package package, bool isAutoScan = false)
        {
            var shouldAssignToActiveContainer = (isAutoScan && (package.IsAptbBin || package.IsScscBin)) || package.IsScscBin; // if its an autoscan and the bin is either aptb or scsc, 
                                                                                                                               // otherwise if its not an autoscan check if the bin is scsc
            if (shouldAssignToActiveContainer)
            {
                var operationalContainer = await operationalContainerProcessor.GetMostRecentOperationalContainerAsync(package.SiteName, package.BinCode);

                if (StringHelper.Exists(operationalContainer.Id) && operationalContainer.Status == ContainerEventConstants.Active)
                {
                    package.ContainerId = operationalContainer.ContainerId;
                    package.IsSecondaryContainerCarrier = operationalContainer.IsSecondaryCarrier;
                    package.EodUpdateCounter += 1;

                    package.PackageEvents.Add(new Event
                    {
                        EventId = package.PackageEvents.Count + 1,
                        EventType = EventConstants.ContainerAssigned,
                        EventStatus = package.PackageStatus,
                        Description = $"Assigned to ContainerId {package.ContainerId}",
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                    });
                }
                else
                {
                    logger.LogInformation($"Operational container not found for package {package.Id}");
                }
            }
        }

        public async Task<AssignContainerResponse> AssignPackageNewContainerAsync(AssignContainerRequest request)
        {
            var response = new AssignContainerResponse();
            try
            {
                var oldContainer = new ShippingContainer();
                var package = await packageRepository.GetProcessedPackageByPackageId(request.PackageId, request.SiteName);
                package.IsLocked = await eodPostProcessor.ShouldLockPackage(package);
                if (!package.IsLocked && StringHelper.Exists(package.BinCode))
                {
                    if (StringHelper.Exists(package.ContainerId))
                    {
                        oldContainer = await containerRepository.GetContainerByContainerId(package.SiteName, package.ContainerId);
                    }
                    var newContainer = await containerRepository.GetActiveOrClosedContainerByContainerIdAsync(request.NewContainerId, package.SiteName);
                    var newContainerIsLocked = await eodPostProcessor.ShouldLockContainer(newContainer);
                    if ((! newContainerIsLocked) && StringHelper.Exists(newContainer.Id) && oldContainer.Id != newContainer.Id)
                    {
                        UpdatePackageNewContainer(request.Username, request.MachineId, package, newContainer, oldContainer);
                        var bulkResponse = await packageRepository.UpdatePackagesSetContainer(new List<Package>() { package });
                        response.IsSuccessful = bulkResponse.IsSuccessful;
                        response.Message = bulkResponse.Message;
                        if (bulkResponse.IsSuccessful)
                        {
                            response.PackageIdUpdated = package.PackageId;
                        }
                    }
                    else
                    {
                        if (StringHelper.DoesNotExist(newContainer.Id))
                        {
                            newContainer = await containerRepository.GetContainerByContainerId(request.SiteName, request.NewContainerId);
                            if (StringHelper.DoesNotExist(newContainer.Id))
                            {
                                response.ErrorCode = ContainerErrorConstants.ContainerNotFound;
                            }
                            else
                            {
                                response.ErrorCode = ContainerErrorConstants.ContainerIncorrectState;
                            }
                        }
                        else if (oldContainer.Id == newContainer.Id)
                        {
                            response.ErrorCode = ContainerErrorConstants.ContainerAlreadyAssigned;
                        }
                        else if (newContainerIsLocked)
                        {
                            response.ErrorCode = ContainerErrorConstants.ContainerIncorrectState;
                        }
                        var msg = $"AssignNewContainer: Failed to assign container {request.NewContainerId}, Error Code: {response.ErrorCode}";
                        response.Message = msg;
                        logger.Log(LogLevel.Error, msg);
                    }
                }
                else
                {
                    response.ErrorCode = ContainerErrorConstants.PackageNotFound;
                    var msg = $"AssignNewContainer: Package not found {request.PackageId}, Error Code: {response.ErrorCode}";
                    response.Message = msg;
                    logger.Log(LogLevel.Error, msg);
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = ContainerErrorConstants.Exception;
                response.IsSuccessful = false;
                response.Message = $"AssignNewContainer:Failed to assign container {request.NewContainerId}, Error Code: {response.ErrorCode}";
                logger.Log(LogLevel.Error, $"AssignNewContainer: Failed to assign container {request.NewContainerId}. Exception: {ex.Message}");
            }
            return response;
        }

        public async Task<AssignContainerResponse> AssignPackageActiveContainerAsync(AssignContainerRequest request)
        {
            try
            {
                var response = new AssignContainerResponse();
                var oldContainer = new ShippingContainer();
                 var package = await packageRepository.GetProcessedPackageByPackageId(request.PackageId, request.SiteName);

                if (StringHelper.Exists(package.Id))
                {
                    package.IsLocked = await eodPostProcessor.ShouldLockPackage(package);

                    if (!package.IsLocked)
                    {
                        var localTime = TimeZoneUtility.GetLocalTime(package.TimeZone);

                        if (StringHelper.Exists(package.ContainerId))
                        {
                            oldContainer = await containerRepository.GetContainerByContainerId(package.SiteName, package.ContainerId);
                        }

                        var operationalContainer = await operationalContainerProcessor.GetActiveOperationalContainerAsync(request.SiteName, package.BinCode);
                        
                        var activeContainerExists = StringHelper.Exists(operationalContainer.Id);
                        var packageAlreadyAssigned = oldContainer.ContainerId == operationalContainer.ContainerId;

                        if (activeContainerExists && !packageAlreadyAssigned)
                        {
                            var activeContainer = await containerRepository.GetActiveContainerByContainerIdAsync(operationalContainer.ContainerId, package.SiteName);
                            UpdatePackageNewContainer(request.Username, request.MachineId, package, activeContainer, oldContainer);
                            var bulkResponse = await packageRepository.UpdatePackagesSetContainer(new List<Package>() { package });
                            response.Message = bulkResponse.Message;

                            if (bulkResponse.IsSuccessful)
                            {
                                response.IsSuccessful = true;
                                response.PackageIdUpdated = package.PackageId;
                                response.ActiveBinCode = activeContainer.BinCode;
                            }
                        }
                        else
                        {
                            if (!activeContainerExists)
                            {
                                response.ActiveBinCode = package.BinCode;
                                response.ErrorCode = ContainerErrorConstants.ActiveContainerNotFound;
                            }
                            else if (packageAlreadyAssigned)
                            {
                                response.ActiveBinCode = operationalContainer.BinCode;
                                response.ErrorCode = ContainerErrorConstants.ContainerAlreadyAssigned;
                            }

                            var msg = $"Failed to assign package to active container {request.PackageId}";
                            response.Message = msg;
                            logger.Log(LogLevel.Warning, msg);
                        }
                    }
                    else
                    {
                        response.ErrorCode = ContainerErrorConstants.PackageNotFound;
                        var msg = $"Package is locked: {request.PackageId}";
                        response.Message = msg;
                        logger.Log(LogLevel.Warning, msg);
                    }
                }
                else
                {
                    response.ErrorCode = ContainerErrorConstants.PackageNotFound;
                    var msg = $"Package not found {request.PackageId}, Error Code: {response.ErrorCode}";
                    response.Message = msg;
                    logger.Log(LogLevel.Warning, msg);
                }

                return response;
            }
            catch (Exception ex)
            {
                var errorCode = ContainerErrorConstants.Exception;
                logger.Log(LogLevel.Error, $"Failed to assign package to active container {request.PackageId}. Exception: {ex.Message}");
                return new AssignContainerResponse
                {
                    ErrorCode = errorCode,
                    Message = $"Failed to assign package to active container {request.PackageId}, Error Code: {errorCode}"
                };
            }
        }

        public void UpdatePackageNewContainer(string username, string machineId, Package package, ShippingContainer newContainer, ShippingContainer oldContainer)
        {
            package.ContainerId = newContainer.ContainerId;
            package.IsSecondaryContainerCarrier = newContainer.IsSecondaryCarrier;
            package.IsRateAssigned = false; // Cause AssignRatesJob to re-assign.
            package.EodUpdateCounter += 1;

            if (package.BinCode != newContainer.BinCode)
            {
                package.HistoricalBinCodes.Add(package.BinCode);
                package.BinCode = newContainer.BinCode;
            }

            var replaced = StringHelper.Exists(oldContainer.Id);

            if (replaced)
            {
                package.HistoricalContainerIds.Add(oldContainer.Id);
            }

            package.PackageEvents.Add(new Event
            {
                EventId = package.PackageEvents.Count + 1,
                EventType = EventConstants.ContainerAssigned,
                EventStatus = package.PackageStatus,
                Description = replaced ?
                    $"Replaced ContainerId {oldContainer.ContainerId} with {newContainer.ContainerId}" :
                    $"Assigned to ContainerId {newContainer.ContainerId}",
                Username = username,
                MachineId = machineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
            });
        }
    }
}
