using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class PackageRepeatProcessor : IPackageRepeatProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IBinProcessor binProcessor;
        private readonly ILogger<PackageRepeatProcessor> logger;
        private readonly IPackageRepository packageRepository;
        private readonly IShippingProcessor shippingProcessor;
        private readonly ISubClientProcessor subClientProcessor;

        public PackageRepeatProcessor(
            IActiveGroupProcessor activeGroupProcessor,
            IBinProcessor binProcessor,
            ILogger<PackageRepeatProcessor> logger,
            IPackageRepository packageRepository,
            IShippingProcessor shippingProcessor,
            ISubClientProcessor subClientProcessor)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.binProcessor = binProcessor;
            this.logger = logger;
            this.packageRepository = packageRepository;
            this.shippingProcessor = shippingProcessor;
            this.subClientProcessor = subClientProcessor;
        }

        public async Task<Package> ProcessRepeatScan(Package packageToReplace, string username, string machineId)
        {
            logger.LogInformation($"Initializing Repeat Scan for PackageId {packageToReplace.PackageId}. Username: {username} Site: {packageToReplace.SiteName}");

            if (StringHelper.Exists(packageToReplace.Id) && !packageToReplace.IsLocked)
            {
                return await UpdatePackagesForRepeatScan(packageToReplace, username, machineId);
            }
            else
            {
                return packageToReplace;
            }
        }

        private async Task<Package> UpdatePackagesForRepeatScan(Package packageToReplace, string username, string machineId)
        {
            var packageToScan = GeneratePackageForRepeatScan(packageToReplace);
            packageToScan.ServiceRuleGroupId = await activeGroupProcessor.GetServiceRuleActiveGroupIdAsync(packageToScan.SubClientName);
            await binProcessor.AssignPackageToCurrentBinAsync(packageToScan);
            packageToScan.HistoricalRepeatScanPackageIds.Add(packageToReplace.Id);
            packageToScan.PackageEvents.Add(new Event
            {
                EventId = packageToScan.PackageEvents.Count + 1,
                EventType = EventConstants.RepeatScan,
                EventStatus = packageToScan.PackageStatus,
                Description = $"Repeat Scan of package Id: {packageToReplace.Id}",
                Username = username,
                MachineId = machineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(packageToScan.TimeZone)
            });

            packageToReplace.PackageStatus = EventConstants.Replaced;
            packageToReplace.EodUpdateCounter += 1;
            packageToReplace.PackageEvents.Add(new Event
            {
                EventId = packageToReplace.PackageEvents.Count + 1,
                EventType = EventConstants.RepeatScan,
                EventStatus = packageToReplace.PackageStatus,
                Description = $"Replaced by repeat scan",
                Username = username,
                MachineId = machineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(packageToScan.TimeZone)
            });

            if (packageToReplace.ShippingCarrier == ShippingCarrierConstants.Ups)
            {
                var subClient = await subClientProcessor.GetSubClientByNameAsync(packageToReplace.SubClientName);
                await shippingProcessor.VoidUpsShipmentAsync(packageToReplace, subClient);
            }

            await packageRepository.UpdateItemAsync(packageToReplace); // set old package to REPLACED status and reset EOD processing
            return await packageRepository.AddItemAsync(packageToScan, packageToScan.PackageId); // add new package so that it can be processed
        }

        private static Package GeneratePackageForRepeatScan(Package packageToReplace)
        {
            var historicalContainerIds = packageToReplace.HistoricalContainerIds;

            if (StringHelper.Exists(packageToReplace.ContainerId))
            {
                historicalContainerIds.Add(packageToReplace.ContainerId);
            }

            var response = new Package
            {
                PartitionKey = packageToReplace.PartitionKey,
                PackageId = packageToReplace.PackageId,
                ContainerId = string.Empty,
                MailCode = PackageIdUtility.GetRepeatScanMailCode(packageToReplace),
                JobBarcode = string.Empty,
                PackageStatus = EventConstants.Repeat,
                RecallStatus = packageToReplace.RecallStatus,
                RecallDate = packageToReplace.RecallDate,
                SiteName = packageToReplace.SiteName,
                SubClientName = packageToReplace.SubClientName,
                SubClientKey = packageToReplace.SubClientKey,
                BinCode = string.Empty,
                PackageEvents = new List<Event>(packageToReplace.PackageEvents),
                ClientName = packageToReplace.ClientName,
                SiteId = packageToReplace.SiteId,
                SiteZip = packageToReplace.SiteZip,
                SiteAddressLineOne = packageToReplace.SiteAddressLineOne,
                SiteCity = packageToReplace.SiteCity,
                SiteState = packageToReplace.SiteState,
                Sequence = packageToReplace.Sequence,
                UpsGeoDescriptor = packageToReplace.UpsGeoDescriptor,
                Zone = packageToReplace.Zone,
                TimeZone = packageToReplace.TimeZone,
                MailerId = packageToReplace.MailerId,
                UspsPermitNumber = packageToReplace.UspsPermitNumber,
                MarkUpType = packageToReplace.MarkUpType,
                IsMarkUpTypeCompany = packageToReplace.IsMarkUpTypeCompany,
                IsPoBox = packageToReplace.IsPoBox,
                IsRural = packageToReplace.IsRural,
                IsUpsDas = packageToReplace.IsUpsDas,
                IsOutside48States = packageToReplace.IsOutside48States,
                IsOrmd = packageToReplace.IsOrmd,
                IsDduScfBin = packageToReplace.IsDduScfBin,
                ZipOverrides = packageToReplace.ZipOverrides,
                AsnImportWebJobId = packageToReplace.AsnImportWebJobId,
                BinGroupId = packageToReplace.BinGroupId,
                BinMapGroupId = packageToReplace.BinMapGroupId,
                ZoneMapGroupId = packageToReplace.ZoneMapGroupId,
                UpsGeoDescriptorGroupId = packageToReplace.UpsGeoDescriptorGroupId,
                ZipOverrideIds = packageToReplace.ZipOverrideIds,
                ZipOverrideGroupIds = packageToReplace.ZipOverrideGroupIds,
                DuplicatePackageIds = packageToReplace.DuplicatePackageIds,
                HistoricalContainerIds = historicalContainerIds,
                HistoricalBinCodes = packageToReplace.HistoricalBinCodes,
                HistoricalBinGroupIds = packageToReplace.HistoricalBinGroupIds,
                HistoricalRepeatScanPackageIds = packageToReplace.HistoricalRepeatScanPackageIds,
                RecipientName = packageToReplace.RecipientName,
                AddressLine1 = packageToReplace.AddressLine1,
                AddressLine2 = packageToReplace.AddressLine2,
                AddressLine3 = packageToReplace.AddressLine3,
                City = packageToReplace.City,
                State = packageToReplace.State,
                Zip = packageToReplace.Zip,
                FullZip = packageToReplace.FullZip,
                Phone = packageToReplace.Phone,
                ReturnName = packageToReplace.ReturnName,
                ReturnAddressLine1 = packageToReplace.ReturnAddressLine1,
                ReturnAddressLine2 = packageToReplace.ReturnAddressLine2,
                ReturnCity = packageToReplace.ReturnCity,
                ReturnState = packageToReplace.ReturnState,
                ReturnZip = packageToReplace.ReturnZip,
                ReturnPhone = packageToReplace.ReturnPhone
            };

            return response;
        }
    }
}
