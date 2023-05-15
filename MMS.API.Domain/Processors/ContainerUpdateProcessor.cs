using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Containers;
using MMS.API.Domain.Models.OperationalContainers;
using MMS.API.Domain.Utilities;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MMS.API.Domain.Processors
{
    public class ContainerUpdateProcessor : IContainerUpdateProcessor
    {
        private readonly ILogger<ContainerUpdateProcessor> logger;
        private readonly IBinProcessor binProcessor;
        private readonly IContainerProcessor containerProcessor;
        private readonly IContainerRepository containerRepository;
        private readonly IEodPostProcessor eodPostProcessor;
        private readonly IContainerLabelProcessor labelProcessor;
        private readonly IOperationalContainerProcessor operationalContainerProcessor;
        private readonly IPackageContainerProcessor packageContainerProcessor;
        private readonly IPackagePostProcessor packagePostProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly IZoneMapRepository zoneMapRepository;

        public ContainerUpdateProcessor(ILogger<ContainerUpdateProcessor> logger,
            IBinProcessor binProcessor,
            IContainerProcessor containerProcessor,
            IContainerRepository containerRepository,
            IEodPostProcessor eodPostProcessor,
            IContainerLabelProcessor labelProcessor,
            IOperationalContainerProcessor operationalContainerProcessor,
            IPackageContainerProcessor packageContainerProcessor,
            IPackagePostProcessor packagePostProcessor,
            IPackageRepository packageRepository,
            ISiteProcessor siteProcessor,
            IZoneMapRepository zoneMapRepository)
        {
            this.logger = logger;
            this.binProcessor = binProcessor;
            this.containerProcessor = containerProcessor;
            this.containerRepository = containerRepository;
            this.eodPostProcessor = eodPostProcessor;
            this.labelProcessor = labelProcessor;
            this.operationalContainerProcessor = operationalContainerProcessor;
            this.packageContainerProcessor = packageContainerProcessor;
            this.packagePostProcessor = packagePostProcessor;
            this.packageRepository = packageRepository;
            this.siteProcessor = siteProcessor;
            this.zoneMapRepository = zoneMapRepository;
        }

        public async Task<CloseContainerResponse> CloseContainerAsync(CloseContainerRequest request)
        {
            var response = new CloseContainerResponse();
            var container = await containerRepository.GetActiveContainerByContainerIdAsync(request.ContainerId, request.SiteName);
            var shouldUpdateOperationalContainer = false;
            var shouldUpdateContainer = false;
            var shouldReturnValidReponse = true;

            if (StringHelper.Exists(container.Id))
            {
                var validateContainerWeight = ValidateContainerWeight(request.Weight, container.ContainerType);

                if (validateContainerWeight.IsValid)
                {
                    response.IsSecondaryCarrier = container.IsSecondaryCarrier;
                    response.IsSaturdayDelivery = container.IsSaturdayDelivery;
                    response.BinCode = container.BinCode;
                    container.Weight = request.Weight;
                    var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);

                    if (container.Status == ContainerEventConstants.Active)
                    {
                        var bin = await binProcessor.GetBinByBinCodeAsync(container.BinCode, container.BinActiveGroupId);
                        if (StringHelper.Exists(bin.Id))
                        {
                            var closedResponse = await labelProcessor.GetClosedContainerLabelData(container, site, bin);
                            if (closedResponse.Successful)
                            {
                                container.Status = ContainerEventConstants.Closed;
                                container.EodUpdateCounter += 1;
                                container.ProcessedDate = DateTime.Now;
                                container.LocalProcessedDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
                                container.Events.Add(new Event
                                {
                                    EventId = container.Events.Count + 1,
                                    EventType = ContainerEventConstants.CloseScan,
                                    EventStatus = container.Status,
                                    Description = "Container closed",
                                    Username = request.Username,
                                    MachineId = request.MachineId,
                                    EventDate = container.ProcessedDate,
                                    LocalEventDate = container.LocalProcessedDate
                                });
                                shouldUpdateOperationalContainer = true;
                                shouldUpdateContainer = true;
                            }
                            else
                            {
                                container.Events.Add(new Event
                                {
                                    EventId = container.Events.Count + 1,
                                    EventType = ContainerEventConstants.CloseScan,
                                    EventStatus = container.Status,
                                    Description = $"Error during processing: type of {ErrorLabelConstants.CarrierDataError} {closedResponse.Message}",
                                    Username = request.Username,
                                    MachineId = request.MachineId,
                                    EventDate = container.ProcessedDate,
                                    LocalEventDate = container.LocalProcessedDate
                                });
                                response.Message = closedResponse.Message;
                                shouldUpdateOperationalContainer = true;
                                shouldUpdateContainer = true;
                                shouldReturnValidReponse = false;
                            }
                        }
                        else
                        {
                            var message = $"Bin not found: {container.BinCode}";
                            logger.LogError(message);
                            response.Message = message;
                        }
                    }
                    else
                    {
                        container.Events.Add(new Event
                        {
                            EventId = container.Events.Count + 1,
                            EventType = ContainerEventConstants.CloseScan,
                            EventStatus = container.Status,
                            Description = "Weight updated by close scan",
                            Username = request.Username,
                            MachineId = request.MachineId,
                            EventDate = DateTime.Now,
                            LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                        });
                        shouldUpdateContainer = true;
                    }

                    if (shouldUpdateContainer)
                    {
                        container = await containerRepository.UpdateItemAsync(container);
                    }

                    if (shouldUpdateOperationalContainer)
                    {
                        await operationalContainerProcessor.UpdateOperationalContainerStatus(new UpdateOperationalContainerRequest
                        {
                            Id = container.OperationalContainerId,
                            SiteName = container.SiteName,
                            BinCode = container.BinCode,
                            Status = container.Status
                        });
                    }

                    if (shouldReturnValidReponse)
                    {
                        response.ContainerId = container.ContainerId;
                        response.ContainerType = container.ContainerType;
                        response.Weight = container.Weight;
                        response.LabelTypeId = container.ClosedLabelTypeId;
                        response.LabelFieldValues = container.ClosedLabelFieldValues;
                        response.IsSuccessful = shouldReturnValidReponse;
                    }
                    else
                    {
                        response.IsSuccessful = shouldReturnValidReponse;
                        response.ContainerId = container.ContainerId;
                        response.ContainerType = container.ContainerType;
                    }
                }
                else
                {
                    var message = StringHelper.Exists(response.Message)
                        ? response.Message
                        : $"{validateContainerWeight.Message} for Container ID: {container.ContainerId}";
                    logger.LogError(message);
                    response.Message = message;
                }
            }
            else
            {
                var message = $"Container ID not found: {request.ContainerId}";
                logger.LogError(message);
                response.Message = message;
            }

            return response;
        }

        public async Task<DeleteContainerResponse> DeleteContainerAsync(DeleteContainerRequest request)
        {
            var response = new DeleteContainerResponse();
            var container = await containerRepository.GetActiveContainerByContainerIdAsync(request.ContainerId, request.SiteName);

            if (StringHelper.Exists(container.Id))
            {
                var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);
                var isContainerAssignedToPackages = await packagePostProcessor.IsContainerAssignedToPackages(container.ContainerId, container.SiteName);

                if (!isContainerAssignedToPackages) // do not delete a container if it has packages assigned
                {
                    container.Status = ContainerEventConstants.Deleted;
                    container.EodUpdateCounter += 1;
                    container.Events.Add(new Event
                    {
                        EventId = container.Events.Count + 1,
                        EventType = ContainerEventConstants.DeleteScan,
                        EventStatus = container.Status,
                        Description = "Container deleted",
                        Username = request.Username,
                        MachineId = request.MachineId,
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                    });

                    await containerRepository.UpdateItemAsync(container);
                    await operationalContainerProcessor.UpdateOperationalContainerStatus(new UpdateOperationalContainerRequest
                    {
                        Id = container.OperationalContainerId,
                        SiteName = container.SiteName,
                        BinCode = container.BinCode,
                        Status = container.Status
                    });
                    response.IsSuccessful = true;
                    response.Message = $"Container Deleted: {container.BinCode} {request.ContainerId}";
                    logger.LogInformation($"Container Deleted: {container.BinCode} {request.ContainerId} Site: {request.SiteName}");
                }
                else
                {
                    logger.LogInformation($"Unable to delete container with nested packages: {container.BinCode} {request.ContainerId} for Site: {request.SiteName}");
                    response.Message = $"Unable to delete container with nested packages: {container.BinCode} {request.ContainerId}";
                }
            }
            else
            {
                logger.LogInformation($"Active Container not found: {request.ContainerId} for Site: {request.SiteName}");
                response.Message = $"Active Container not found: {request.ContainerId}";
            }

            return response;
        }

        public async Task<UpdateContainerResponse> UpdateContainerAsync(UpdateContainerRequest request)
        {
            var response = new UpdateContainerResponse();
            var oldContainer = await containerRepository.GetActiveOrClosedContainerByContainerIdAsync(request.ContainerId, request.SiteName);
            var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);

            if (!StringHelper.Exists(oldContainer.Id))
            {
                logger.LogWarning($"Active or Closed Container not found: {request.ContainerId} for Site: {request.SiteName}");
                response.Message = $"Active or Closed Container not found: {request.ContainerId}";
            }
            else
            {
                var isSecondaryCarrierChanged = request.IsSecondaryCarrier != oldContainer.IsSecondaryCarrier;
                var isSaturdayChanged = request.IsSaturdayDelivery.HasValue && request.IsSaturdayDelivery.Value != oldContainer.IsSaturdayDelivery;
                var shouldBeReplaced = isSaturdayChanged || isSecondaryCarrierChanged;
                decimal.TryParse(oldContainer.Weight, out var oldWeight);
                decimal.TryParse(request.Weight, out var newWeight);
                var shouldWeightBeUpdated = newWeight != 0;

                if (shouldBeReplaced)
                {
                    if (oldContainer.Status != ContainerEventConstants.Active)
                    {
                        if (isSecondaryCarrierChanged)
                        {
                            logger.LogWarning($"Cannot update IsSecondaryCarrier for a CLOSED container: {request.ContainerId}");
                            response.Message = $"Cannot update IsSecondaryCarrier for a CLOSED container: {request.ContainerId}";
                        }
                        else if (isSaturdayChanged)
                        {
                            logger.LogWarning($"Cannot update IsSaturdayChanged for a CLOSED container: {request.ContainerId}");
                            response.Message = $"Cannot update IsSaturdayChanged for a CLOSED container: {request.ContainerId}";
                        }
                    }
                    else
                    {
                        var bin = await binProcessor.GetBinByBinCodeAsync(oldContainer.BinCode, oldContainer.BinActiveGroupId);
                        var zoneMap = await zoneMapRepository.GetZoneMapAsync(AddressUtility.TrimZipToFirstThree(site.Zip), oldContainer.ZoneMapActiveGroupId);
                        var createContainerRequest = new CreateContainerRequest
                        {
                            Site = site,
                            Bin = bin,
                            ZoneMap = zoneMap,
                            IsReplacement = true,
                            IsSecondaryCarrier = request.IsSecondaryCarrier,
                            IsSaturdayDelivery = request.IsSaturdayDelivery.HasValue ? request.IsSaturdayDelivery.Value : oldContainer.IsSaturdayDelivery,
                            ContainerIdToReplace = oldContainer.ContainerId,
                            HumanReadableBarcodeToReplace = oldContainer.HumanReadableBarcode,
                            SerialNumberToReplace = oldContainer.SerialNumber,
                            EventsToReplace = oldContainer.Events,
                            OperationalContainerId = oldContainer.OperationalContainerId,
                            Username = request.Username,
                            MachineId = request.MachineId
                        };

                        var createContainerResponse = await containerProcessor.CreateContainerAsync(createContainerRequest);
                        if (!createContainerResponse.IsSuccessful)
                        {
                            logger.LogError($"Failed to update container: {request.ContainerId} for Site: {request.SiteName}");
                            response.Message = $"Failed to update container: {request.ContainerId}";
                        }
                        else
                        {
                            var newContainer = createContainerResponse.ShippingContainer;

                            oldContainer.Status = ContainerEventConstants.Replaced;
                            oldContainer.EodUpdateCounter += 1;

                            string updateText = string.Empty;
                            if (isSaturdayChanged && isSecondaryCarrierChanged)
                            {
                                updateText = $"isSaturday = {request.IsSaturdayDelivery.Value.ToString()} and isSecondary = {request.IsSecondaryCarrier.ToString()}";
                            }
                            else
                            {
                                if (isSecondaryCarrierChanged)
                                {
                                    updateText = $"isSecondary = {request.IsSecondaryCarrier.ToString()}";
                                }
                                if (isSaturdayChanged)
                                {
                                    updateText = $"isSaturday = {request.IsSaturdayDelivery.Value.ToString()}";
                                }
                            }

                            oldContainer.Events.Add(new Event
                            {
                                EventId = oldContainer.Events.Count + 1,
                                EventType = ContainerEventConstants.UpdateScan,
                                EventStatus = oldContainer.Status,
                                Description = $"Replaced by Update, {updateText}.",
                                Username = request.Username,
                                MachineId = request.MachineId,
                                EventDate = DateTime.Now,
                                LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                            });

                            await containerRepository.UpdateItemAsync(oldContainer);
                            await containerRepository.AddItemAsync(newContainer, newContainer.ContainerId);
                            await operationalContainerProcessor.UpdateIsSecondaryCarrierAsync(new UpdateOperationalContainerRequest
                            {
                                Id = newContainer.OperationalContainerId,
                                SiteName = newContainer.SiteName,
                                BinCode = newContainer.BinCode,
                                IsSecondaryCarrier = newContainer.IsSecondaryCarrier

                            });

                            var packages = await packagePostProcessor.GetPackagesByShippingContainerAsync(newContainer.ContainerId, site.SiteName);

                            if (packages.Any())
                            {
                                foreach (var package in packages)
                                {
                                    UpdatePackageIsSecondaryContainer(request.Username, request.MachineId, package, newContainer);
                                }

                                await packageRepository.UpdatePackagesSetIsSecondaryContainer(packages.ToList());
                            }

                            AssignUpdateContainerResponseData(request, response, newContainer);
                            logger.LogInformation($"Container Updated: ContainerId: {request.ContainerId}: {newContainer.ContainerId} BinCode: {newContainer.BinCode} Site: {newContainer.SiteName}");
                        }
                    }
                }
                else if (shouldWeightBeUpdated)
                {
                    if (oldWeight == newWeight)
                    {
                        logger.LogInformation($"Container weight update request was the same as current weight. ContainerId: {oldContainer.ContainerId} BinCode: {oldContainer.BinCode} Site: {oldContainer.SiteName}");
                        response.Message = $"Container weight update request was the same as current weight. ContainerId: {request.ContainerId}";
                    }
                    else
                    {
                        var validateContainerWeight = ValidateContainerWeight(request.Weight, oldContainer.ContainerType);
                        if (!validateContainerWeight.IsValid)
                        {
                            logger.LogWarning($"{validateContainerWeight.Message}: {request.ContainerId} for Site: {request.SiteName}");
                            response.Message = validateContainerWeight.Message;
                        }
                        else if (await eodPostProcessor.ShouldLockContainer(oldContainer))
                        {
                            logger.LogWarning($"Cannot update weight for a CLOSED container after EOD has been started: { request.ContainerId}");
                            response.Message = $"Cannot update weight for a CLOSED container after EOD has been started: { request.ContainerId}";
                        }
                        else
                        {
                            oldContainer.Weight = request.Weight;
                            oldContainer.IsRateAssigned = false;
                            oldContainer.EodUpdateCounter += 1;
                            oldContainer.Events.Add(new Event
                            {
                                EventId = oldContainer.Events.Count + 1,
                                EventType = ContainerEventConstants.UpdateScan,
                                EventStatus = oldContainer.Status,
                                Description = $"Weight updated by UpdateContainer. Old weight {oldWeight}: New weight: {newWeight}",
                                Username = request.Username,
                                MachineId = request.MachineId,
                                EventDate = DateTime.Now,
                                LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                            });

                            await containerRepository.UpdateItemAsync(oldContainer);
                            AssignUpdateContainerResponseData(request, response, oldContainer);
                            logger.LogInformation($"Container weight Updated: ContainerId: {request.ContainerId}: {oldContainer.ContainerId}: BinCode: {oldContainer.BinCode} Site: {oldContainer.SiteName} Old weight {oldContainer.Weight}: New weight: {request.Weight}");
                        }
                    }
                }
                else
                {
                    logger.LogError($"Failed to update container: {request.ContainerId} for Site: {request.SiteName}");
                    response.Message = $"Failed to update container: {request.ContainerId}";
                }
            }
            return response;
        }

        public async Task<ReplaceContainerResponse> ReplaceContainerAsync(ReplaceContainerRequest request)
        {
            var response = new ReplaceContainerResponse();

            try
            {
                var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);

                var oldContainer = await containerRepository.GetActiveOrClosedContainerByContainerIdAsync(request.OldContainerId, site.SiteName);
                var newContainer = await containerRepository.GetActiveOrClosedContainerByContainerIdAsync(request.NewContainerId, site.SiteName);
                var canReplaceContainer = await CanReplaceContainer(oldContainer, newContainer, request);

                if (canReplaceContainer.IsSuccessful)
                {
                    var packages = await packagePostProcessor.GetPackagesByShippingContainerAsync(oldContainer.ContainerId, site.SiteName);

                    if (packages.Any())
                    {
                        foreach (var package in packages)
                        {
                            packageContainerProcessor.UpdatePackageNewContainer(request.Username, request.MachineId, package, newContainer, oldContainer);
                            response.PackageIdsUpdated.Add(package.PackageId);
                        }

                        oldContainer.Status = ContainerEventConstants.Replaced;
                        oldContainer.EodUpdateCounter += 1;
                        oldContainer.Events.Add(new Event
                        {
                            EventId = oldContainer.Events.Count + 1,
                            EventType = ContainerEventConstants.ReplaceScan,
                            EventStatus = oldContainer.Status,
                            Description = "Container replaced on scan",
                            Username = request.Username,
                            MachineId = request.MachineId,
                            EventDate = DateTime.Now,
                            LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                        });

                        newContainer.Events.Add(new Event
                        {
                            EventId = newContainer.Events.Count + 1,
                            EventType = ContainerEventConstants.PackagesAdded,
                            EventStatus = newContainer.Status,
                            Description = "Packages added",
                            Username = request.Username,
                            MachineId = request.MachineId,
                            EventDate = DateTime.Now,
                            LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                        });

                        await containerRepository.UpdateItemAsync(oldContainer);
                        await containerRepository.UpdateItemAsync(newContainer);
                        await operationalContainerProcessor.UpdateOperationalContainerStatus(new UpdateOperationalContainerRequest
                        {
                            Id = oldContainer.OperationalContainerId,
                            SiteName = oldContainer.SiteName,
                            BinCode = oldContainer.BinCode,
                            Status = oldContainer.Status
                        });

                        var bulkResponse = await packageRepository.UpdatePackagesSetContainer(packages.ToList());
                        response.IsSuccessful = bulkResponse.IsSuccessful;
                    }
                    else
                    {
                        var msg = $"No packages associated with container: {request.OldContainerId} Could not reassign to new containerID: {request.NewContainerId}, " +
                            $"Error Code: {response.ErrorCode}";
                        response.ErrorCode = ContainerErrorConstants.ContainerEmpty;
                        response.Message = msg;
                        logger.Log(LogLevel.Error, msg);
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, canReplaceContainer.Message);
                    response = canReplaceContainer;
                }
            }
            catch (Exception ex)
            {
                response.ErrorCode = ContainerErrorConstants.Exception;
                response.IsSuccessful = false;
                response.Message = $"Failed to replace Container ID: {request.OldContainerId}, Error Code: {response.ErrorCode}";
                logger.Log(LogLevel.Error, $"Failed to replace Container ID: {request.OldContainerId}. Exception: {ex}");
            }
            return response;
        }      

        private async Task<ReplaceContainerResponse> CanReplaceContainer(ShippingContainer oldContainer, ShippingContainer newContainer, ReplaceContainerRequest request)
        {
            var response = new ReplaceContainerResponse();
            var isNewContainerLocked = await eodPostProcessor.ShouldLockContainer(newContainer);
            var isOldContainerLocked = await eodPostProcessor.ShouldLockContainer(oldContainer);

            if (isNewContainerLocked || isOldContainerLocked)
            {
                var msg = $"Could not reassign to new containerID: {request.NewContainerId}, " +
                    $"Error Code: {response.ErrorCode}";

                response.Message = msg;
                response.IsSuccessful = false;
                response.ErrorCode = ContainerErrorConstants.NewContainerIncorrectState;
            }
            else if (oldContainer.BinCode.IsDDU() && newContainer.BinCode.IsDDU())
            {
                if (oldContainer.BinCode != newContainer.BinCode)
                {
                    response.ErrorCode = ContainerErrorConstants.ContainerIncorrectOperation;
                    response.Message = $"Could not replace container because the sort codes do not match, Error Code: {response.ErrorCode}";
                }

                response.IsSuccessful = oldContainer.BinCode == newContainer.BinCode;
            }
            else if (oldContainer.BinCode.IsSCF() && newContainer.BinCode.IsSCF())
            {
                if (oldContainer.BinCode != newContainer.BinCode)
                {
                    response.ErrorCode = ContainerErrorConstants.ContainerIncorrectOperation;
                    response.Message = $"Could not replace container because the sort codes do not match, Error Code: {response.ErrorCode}";
                }

                response.IsSuccessful = oldContainer.BinCode == newContainer.BinCode;
            }
            else if (StringHelper.Exists(oldContainer.Id) && StringHelper.Exists(newContainer.Id))
            {
                response.IsSuccessful = true;
            }
            else
            {
                if (StringHelper.DoesNotExist(oldContainer.Id))
                {
                    response.IsSuccessful = false;
                    oldContainer = await containerRepository.GetContainerByContainerId(request.SiteName, request.OldContainerId);
                    if (StringHelper.DoesNotExist(oldContainer.Id))
                    {
                        response.ErrorCode = ContainerErrorConstants.OldContainerNotFound;
                    }
                    else
                    {
                        response.ErrorCode = ContainerErrorConstants.OldContainerIncorrectState;
                    }
                }
                else if (StringHelper.DoesNotExist(newContainer.Id))
                {
                    response.IsSuccessful = false;
                    newContainer = await containerRepository.GetContainerByContainerId(request.SiteName, request.NewContainerId);
                    if (StringHelper.DoesNotExist(newContainer.Id))
                    {
                        response.ErrorCode = ContainerErrorConstants.NewContainerNotFound;
                    }
                    else
                    {
                        response.ErrorCode = ContainerErrorConstants.NewContainerIncorrectState;
                    }
                }

                var msg = $"One or both containers not found. Old: {request.OldContainerId} New: {request.NewContainerId}, Error Code: {response.ErrorCode}";
                response.Message = msg;
            }

            return response;
        }

        private static (bool IsValid, string Message) ValidateContainerWeight(string requestWeight, string containerType)
        {
            var isValid = false;
            var message = string.Empty;

            if (!decimal.TryParse(requestWeight, out var containerWeight))
            {
                message = "Weight must be a numeric value";
            }
            else if (containerType == ContainerConstants.ContainerTypePallet && containerWeight > 2000m)
            {
                message = "Weight of a pallet cannot exceed 2000 lbs";
            }
            else if (containerType == ContainerConstants.ContainerTypeBag && containerWeight > 70m)
            {
                message = "Weight of a bag cannot exceed 70 lbs";
            }
            else if (containerWeight <= 0m)
            {
                message = "Weight of a container must be over 0 lbs";
            }
            else
            {
                isValid = true;
            }
            return (isValid, message);
        }

        private static void AssignUpdateContainerResponseData(UpdateContainerRequest request, UpdateContainerResponse response, ShippingContainer newContainer)
        {
            response.ContainerId = newContainer.ContainerId;
            response.ContainerStatus = newContainer.Status;
            response.ContainerType = newContainer.ContainerType;
            response.HumanReadableBarcode = newContainer.HumanReadableBarcode;
            response.LabelTypeId = newContainer.LabelTypeId;
            response.LabelFieldValues = newContainer.LabelFieldValues;
            response.Weight = newContainer.Weight;
            response.IsSuccessful = true;
            response.Message = $"Container Updated: {request.ContainerId} BinCode: {newContainer.BinCode}";
        }

        private static void UpdatePackageIsSecondaryContainer(string username, string machineId, Package package, ShippingContainer container)
        {
            package.IsSecondaryContainerCarrier = container.IsSecondaryCarrier;
            package.IsRateAssigned = false;
            package.EodUpdateCounter += 1;

            package.PackageEvents.Add(new Event
            {
                EventId = package.PackageEvents.Count + 1,
                EventType = EventConstants.ContainerAssigned,
                EventStatus = package.PackageStatus,
                Description = "Updated IsSecondaryContainerCarrier",
                Username = username,
                MachineId = machineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
            });
        }
    }
}
