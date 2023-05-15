using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Containers;
using MMS.API.Domain.Models.OperationalContainers;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class ContainerProcessor : IContainerProcessor
    {
        private readonly ILogger<ContainerProcessor> logger;
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IBinProcessor binProcessor;
        private readonly IContainerRepository containerRepository;
        private readonly IContainerLabelProcessor labelProcessor;
        private readonly IOperationalContainerProcessor operationalContainerProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly IZipOverrideRepository zipOverrideRepository;
        private readonly IZoneMapRepository zoneMapRepository;

        public ContainerProcessor(ILogger<ContainerProcessor> logger,
            IActiveGroupProcessor activeGroupProcessor,
            IBinProcessor binProcessor,
            IContainerRepository containerRepository,
            IContainerLabelProcessor labelProcessor,
            IOperationalContainerProcessor operationalContainerProcessor,
            ISiteProcessor siteProcessor,
            IZipOverrideRepository zipOverrideRepository,
            IZoneMapRepository zoneMapRepository)
        {
            this.logger = logger;
            this.activeGroupProcessor = activeGroupProcessor;
            this.binProcessor = binProcessor;
            this.containerRepository = containerRepository;
            this.labelProcessor = labelProcessor;
            this.operationalContainerProcessor = operationalContainerProcessor;
            this.siteProcessor = siteProcessor;
            this.zipOverrideRepository = zipOverrideRepository;
            this.zoneMapRepository = zoneMapRepository;
        }

        public async Task<CreateContainersResponse> CreateContainersAsync(CreateContainersRequest request)
        {
            var response = new CreateContainersResponse();
            var binCodes = new List<string>();
            var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);
            var binActiveGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(request.SiteName);
            var zoneMapGroupId = await activeGroupProcessor.GetZoneMapActiveGroupIdAsync();
            var zoneMap = await zoneMapRepository.GetZoneMapAsync(AddressUtility.TrimZipToFirstThree(site.Zip), zoneMapGroupId);

            request.BinCodes.ForEach(x => binCodes.Add(x));
            binCodes.Sort();
            var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);

            foreach (var binCode in binCodes)
            {
                try
                {
                    var bin = await binProcessor.GetBinByBinCodeAsync(binCode, binActiveGroupId);

                    if (StringHelper.Exists(bin.Id))
                    {
                        var operationalContainer = await operationalContainerProcessor.GetMostRecentOperationalContainerAsync(site.SiteName, binCode);

                        if (StringHelper.Exists(operationalContainer.Id) && operationalContainer.Status == ContainerEventConstants.Active)
                        {
                            response.DuplicateBinCodes.Add(binCode);
                            logger.Log(LogLevel.Information, $"Active Container already exists for: {binCode}. ContainerId: {operationalContainer.Id}");
                        }
                        else
                        {
                            var createContainerRequest = new CreateContainerRequest
                            {
                                Site = site,
                                Bin = bin,
                                ZoneMap = zoneMap,
                                IsSecondaryCarrier = request.IsSecondaryCarrier,
                                IsSaturdayDelivery = request.IsSaturdayDelivery,
                                OperationalContainerId = Guid.NewGuid().ToString(),
                                Username = request.Username,
                                MachineId = request.MachineId
                            };

                            var createContainerResponse = await CreateContainerAsync(createContainerRequest);

                            if (createContainerResponse.IsSuccessful)
                            {
                                var container = await containerRepository.AddItemAsync(createContainerResponse.ShippingContainer,
                                    createContainerResponse.ShippingContainer.ContainerId);

                                if (StringHelper.Exists(container.Id)) // if database transaction was successful
                                {
                                    await operationalContainerProcessor.AddOperationalContainerAsync(new AddOperationalContainerRequest
                                    {
                                        Id = container.OperationalContainerId,
                                        SiteName = container.SiteName,
                                        BinCode = container.BinCode,
                                        ContainerId = container.ContainerId,
                                        Status = container.Status,
                                        IsSecondaryCarrier = container.IsSecondaryCarrier
                                    });
                                }

                                response.Containers.Add(new CreateContainer
                                {
                                    ContainerId = container.ContainerId ?? string.Empty,
                                    ContainerStatus = container.Status,
                                    ContainerType = container.ContainerType,
                                    HumanReadableBarcode = container.HumanReadableBarcode ?? string.Empty,
                                    LabelTypeId = container.LabelTypeId,
                                    LabelFieldValues = container.LabelFieldValues
                                });
                            }
                            else
                            {
                                response.FailedBinCodes.Add(binCode);
                                logger.Log(LogLevel.Warning, $"Failed to create container for {binCode}.");
                            }
                        }
                    }
                    else
                    {
                        response.FailedBinCodes.Add(binCode);
                        logger.Log(LogLevel.Warning, $"{binCode} Not Found.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, $"Failed to process bin code: {binCode} Site: {site.SiteName} Exception: {ex}");
                    response.FailedBinCodes.Add(binCode);
                }
            }

            return response;
        }

        public async Task<CreateContainerResponse> CreateContainerAsync(CreateContainerRequest request)
        {
            if (request.IsSecondaryCarrier && StringHelper.DoesNotExist(request.Bin.BinCodeSecondary)) // fail if a bin does not have a secondary carrier
            {
                return new CreateContainerResponse();
            }

            var isSuccessful = false;
            var site = request.Site;
            var bin = request.Bin;
            var zoneMap = request.ZoneMap;
            var createDate = DateTime.Now;
            var siteCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
            var dropShipSiteDescription = request.IsSecondaryCarrier ? bin.DropShipSiteDescriptionSecondary : bin.DropShipSiteDescriptionPrimary;
            var dropShipSiteAddress = request.IsSecondaryCarrier ? bin.DropShipSiteAddressSecondary : bin.DropShipSiteAddressPrimary;
            var dropShipSiteCsz = request.IsSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary;
            var dropShipSiteNote = request.IsSecondaryCarrier ? bin.DropShipSiteNoteSecondary : bin.DropShipSiteNotePrimary;
            var regionalCarrierHub = request.IsSecondaryCarrier ? bin.RegionalCarrierHubSecondary : bin.RegionalCarrierHubPrimary;
            var parseCsz = AddressUtility.ParseCityStateZip(dropShipSiteCsz);

            var container = new ShippingContainer
            {
                Status = ContainerEventConstants.Active,
                SiteId = site.Id,
                SiteName = site.SiteName,
                OperationalContainerId = request.OperationalContainerId,
                BinActiveGroupId = bin.ActiveGroupId,
                ZoneMapActiveGroupId = zoneMap.ActiveGroupId,
                BinCode = bin.BinCode,
                BinCodeSecondary = bin.BinCodeSecondary,
                BinLabelType = request.IsSecondaryCarrier ? bin.LabelTypeSecondary : bin.LabelTypePrimary,
                ShippingCarrier = request.IsSecondaryCarrier ? bin.ShippingCarrierSecondary : bin.ShippingCarrierPrimary,
                ShippingMethod = request.IsSecondaryCarrier ? bin.ShippingMethodSecondary : bin.ShippingMethodPrimary,
                ContainerType = request.IsSecondaryCarrier ? bin.ContainerTypeSecondary : bin.ContainerTypePrimary,
                DropShipSiteDescription = dropShipSiteDescription,
                DropShipSiteAddress = dropShipSiteAddress,
                DropShipSiteCsz = dropShipSiteCsz,
                DropShipSiteNote = dropShipSiteNote,
                RegionalCarrierHub = regionalCarrierHub,
                IsSecondaryCarrier = request.IsSecondaryCarrier,
                IsSaturdayDelivery = request.IsSaturdayDelivery,
                IsRural = await IsRural(parseCsz.FullZip),
                IsOutside48States = AddressUtility.IsNotInLower48States(parseCsz.State),
                IsRateAssigned = false,
                Weight = string.Empty,
                Username = request.Username,
                MachineId = request.MachineId,
                SiteCreateDate = siteCreateDate,                
                CreateDate = createDate,
                Cost = 0,
                Charge = 0
            };

            AssignZone(container, bin, request.IsSecondaryCarrier, zoneMap);

            container = await labelProcessor.GetCreateContainerLabelData(container, site, bin, request);

            if (StringHelper.Exists(container.ContainerId)) // if label creation and barcode generation was successful
            {
                var eventType = request.IsReplacement ? ContainerEventConstants.UpdateScan : ContainerEventConstants.Created;
                var description = request.IsReplacement ? $"Created by UpdateContainers" : "Created by CreateContainers";

                if (request.IsReplacement)
                {
                    container.Events.AddRange(request.EventsToReplace);
                }

                container.Events.Add(new Event
                {
                    EventId = container.Events.Count + 1,
                    EventType = eventType,
                    EventStatus = container.Status,
                    Description = description,
                    Username = request.Username,
                    MachineId = request.MachineId,
                    EventDate = container.CreateDate,
                    LocalEventDate = container.SiteCreateDate
                });

                isSuccessful = true;
            }

            return new CreateContainerResponse
            {
                ShippingContainer = container,
                IsSuccessful = isSuccessful
            };
        }

        private async Task<bool> IsRural(string zipCode)
        {
            var zipOverrideGroupId = await activeGroupProcessor.GetZipMapActiveGroup(ActiveGroupTypeConstants.ZipsUspsRural);
            var zipOverride = await zipOverrideRepository.GetZipOverrideByZipCodeAsync(
                AddressUtility.TrimZipToFirstFive(zipCode), zipOverrideGroupId);
            return StringHelper.Exists(zipOverride.Id);
        }

        public async Task<ReprintClosedContainerResponse> ReprintClosedContainerAsync(ReprintClosedContainerRequest request)
        {
            try
            {
                var response = new ReprintClosedContainerResponse();
                var container = await containerRepository.GetClosedContainerByContainerIdAsync(request.ContainerId, request.SiteName);
                var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);
                if (StringHelper.Exists(container.Id))
                {
                    response = new ReprintClosedContainerResponse
                    {
                        ContainerId = container.ContainerId,
                        HumanReadableBarcode = container.HumanReadableBarcode,
                        ContainerType = container.ContainerType,
                        Weight = container.Weight,
                        LabelTypeId = container.ClosedLabelTypeId,
                        LabelFieldValues = container.ClosedLabelFieldValues
                    };
                    container.Events.Add(new Event
                    {
                        EventId = container.Events.Count + 1,
                        EventType = ContainerEventConstants.Reprint,
                        EventStatus = container.Status,
                        Description = "Label Reprinted By User",
                        Username = request.Username,
                        MachineId = request.MachineId,
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                    });
                    await containerRepository.UpdateItemAsync(container);
                }
                else
                {
                    logger.LogError($"Unable to find container ID: {request.ContainerId} Site: {request.SiteName} Username: {request.Username}");
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while reprinting closed container ID: {request.ContainerId} Site: {request.SiteName} Username: {request.Username} Exception: {ex}");
                return new ReprintClosedContainerResponse();
            }
        }

        public async Task<ReprintActiveContainersResponse> ReprintActiveContainersAsync(ReprintActiveContainersRequest request)
        {
            try
            {
                var response = new ReprintActiveContainersResponse();

                foreach (var binCode in request.BinCodes)
                {
                    var site = await siteProcessor.GetSiteBySiteNameAsync(request.SiteName);
                    var localTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                    var container = await containerRepository.GetActiveContainerByBinCodeAsync(site.SiteName, binCode, localTime);

                    if (StringHelper.Exists(container.Id))
                    {
                        response.Containers.Add(new CreateContainer
                        {
                            ContainerId = container.ContainerId,
                            ContainerStatus = container.Status,
                            ContainerType = container.ContainerType,
                            HumanReadableBarcode = container.HumanReadableBarcode,
                            LabelTypeId = container.LabelTypeId,
                            LabelFieldValues = container.LabelFieldValues
                        });
                        container.Events.Add(new Event
                        {
                            EventId = container.Events.Count + 1,
                            EventType = ContainerEventConstants.Reprint,
                            EventStatus = container.Status,
                            Description = "Label Reprinted By User",
                            Username = request.Username,
                            MachineId = request.MachineId,
                            EventDate = DateTime.Now,
                            LocalEventDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
                        });
                        await containerRepository.UpdateItemAsync(container);
                    }
                    else
                    {
                        response.FailedBinCodes.Add(binCode);
                        logger.LogError($"No active container found to reprint for binCode: {binCode} Site: {request.SiteName} Username: {request.Username}");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                var binCodes = string.Empty;
                foreach (var binCode in request.BinCodes)
                {
                    binCodes += $"{binCode} ";
                }
                logger.LogError($"Error while reprinting active containers ID: {binCodes} Site: {request.SiteName} Username: {request.Username} Exception: {ex}");
                return new ReprintActiveContainersResponse();
            }
        }

        public async Task<GetBinCodesResponse> GetBinCodesAsync(string siteName)
        {
            var response = new GetBinCodesResponse();
            var activeBinGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(siteName);
            var bins = await binProcessor.GetBinCodesAsync(activeBinGroupId);
            var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
            var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
            var activeContainers = await containerRepository.GetActiveContainersForSiteAsync(siteName, siteLocalTime);
            foreach (var bin in bins.OrderBy(x => x.BinCode))
            {
                if (StringHelper.Exists(bin.BinCode))
                {
                    var activeContainer = activeContainers.FirstOrDefault(c => c.BinCode == bin.BinCode);
                    response.BinCodes.Add(new BinCodeResponse
                    {
                        BinCode = bin.BinCode,
                        Group = GenerateGrouping(bin.BinCode),
                        HasActiveContainer = activeContainer != null
                    });
                }
            }

            response.Groups.AddRange(response.BinCodes.GroupBy(x => x.Group).OrderBy(x => x.Key).Select(x => x.Key).ToList());
            return response;
        }

        private void AssignZone(ShippingContainer container, Bin bin, bool isSecondaryCarrier, ZoneMap zoneMap)
        {
            var cityStateZip = AddressUtility.ParseCityStateZip(isSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary);
            var zones = StringHelper.SplitInParts(zoneMap.ZoneMatrix, 2).ToList();
            var positionParsed = int.TryParse(AddressUtility.TrimZipToFirstThree(cityStateZip.FullZip), out var matrixPosition);
            var trueMatrixPosition = matrixPosition != 0 ? matrixPosition - 1 : matrixPosition; // minus 1 because the zone matrix is not zero based, but the input list is

            var zoneParsed = int.TryParse(zones[trueMatrixPosition].ToString().Substring(0, 1), out var zone);

            if (positionParsed && zoneParsed)
            {
                container.Zone = zone;
            }
            else
            {
                logger.Log(LogLevel.Error, $"Failed to assign Zone on container creation. Container unique ID: {container.Id} ZoneMap Id: {zoneMap.Id}");
            }
        }

        private string GenerateGrouping(string binCode)
        {
            var response = string.Empty;
            var firstChar = binCode.IndexOf('-') + 1;
            var grouping = binCode.Substring(firstChar, 2);

            if (grouping.Contains('-'))
            {
                response = binCode.Substring(firstChar, 1);
            }
            else
            {
                response = binCode.Substring(firstChar, 2);
            }
            return response;
        }
    }
}
