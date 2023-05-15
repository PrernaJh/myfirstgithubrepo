using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Data
{
    public class TrackPackageDatasetProcessor : ITrackPackageDatasetProcessor
    {
        private readonly ILogger<TrackPackageDatasetProcessor> logger;
        private readonly IPackageDatasetRepository packageDatasetRepository;
        private readonly IShippingContainerDatasetRepository shippingContainerDatasetRepository;
        private readonly ITrackPackageDatasetRepository trackPackageDatasetRepository;
        private readonly ICarrierEventCodeRepository carrierEventCodeRepository;
        private readonly IEvsCodeRepository evsCodeRepository;
        private readonly IPostalDaysRepository postalDaysRepository;

        private readonly IDictionary<string, int> carrierIsStopTheClock = new Dictionary<string, int>(); // "ShippingCarrier:Code" => IsStopTheClock
        private readonly IDictionary<string, int> carrierIsUndeliverable = new Dictionary<string, int>(); // "ShippingCarrier:Code" => IsUndeliverable
        private readonly IDictionary<string, string> carrierEventDescriptions = new Dictionary<string, string>(); // "ShippingCarrier:Code" => Description

        public bool ReloadEventCodes { get; set; } = true;
        public void ForceReloadEventCodes()
        {
            ReloadEventCodes = true;
        }
        private void LoadEventCodes()
        {
            if (ReloadEventCodes)
            {
                ReloadEventCodes = false;

                var evsCodes = evsCodeRepository.GetEvsCodesAsync().GetAwaiter().GetResult().ToList();
                evsCodes.ForEach(c => carrierIsStopTheClock[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.IsStopTheClock);
                evsCodes.ForEach(c => carrierIsUndeliverable[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.IsUndeliverable);
                evsCodes.ForEach(c => carrierEventDescriptions[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.Description);

                var carrierCodes = carrierEventCodeRepository.GetCarrierEventCodesAsync().GetAwaiter().GetResult().ToList();
                carrierCodes.ForEach(c => carrierIsStopTheClock[$"{c.ShippingCarrier}:{c.Code}"] = c.IsStopTheClock);
                carrierCodes.ForEach(c => carrierIsUndeliverable[$"{c.ShippingCarrier}:{c.Code}"] = c.IsUndeliverable);
                carrierCodes.ForEach(c => carrierEventDescriptions[$"{c.ShippingCarrier}:{c.Code}"] = c.Description);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="carrierEventCodeRepository"></param>
        /// <param name="evsCodeRepository"></param>
        /// <param name="packageDatasetRepository"></param>
        /// <param name="shippingContainerDatasetRepository"></param>
        /// <param name="trackPackageDatasetRepository"></param>
        public TrackPackageDatasetProcessor(ILogger<TrackPackageDatasetProcessor> logger,
            ICarrierEventCodeRepository carrierEventCodeRepository,
            IEvsCodeRepository evsCodeRepository,
            IPackageDatasetRepository packageDatasetRepository,
            IShippingContainerDatasetRepository shippingContainerDatasetRepository,
            ITrackPackageDatasetRepository trackPackageDatasetRepository,
            IPostalDaysRepository postalDaysRepository
            )
        {
            this.logger = logger;
            this.carrierEventCodeRepository = carrierEventCodeRepository;
            this.evsCodeRepository = evsCodeRepository;
            this.packageDatasetRepository = packageDatasetRepository;
            this.shippingContainerDatasetRepository = shippingContainerDatasetRepository;
            this.trackPackageDatasetRepository = trackPackageDatasetRepository;
            this.postalDaysRepository = postalDaysRepository;
        }

        // Note: This method was only used for Historical data and is therefore it shouldn't be used for anything else.
        public async Task<ReportResponse> InsertTrackPackageDatasets(List<PackageDataset> packageDatasets, List<TrackPackageDataset> items)
        {
            var response = new ReportResponse() { IsSuccessful = true };
            try
            {
                int chunk = 1000;
                for (int offset = 0; offset < items.Count; offset += chunk)
                {
                    var trackPackageDatasets = items.Skip(offset).Take(chunk);
                    foreach (var trackPackage in trackPackageDatasets)
                    {
                        var package = packageDatasets.FirstOrDefault(p => p.PackageId == trackPackage.PackageId);
                        trackPackage.PackageDatasetId = package?.Id;
                    }
                    await BulkInsertOrUpdateAsync(response, trackPackageDatasets.ToList(), new List<PackageDataset>(), new List<ShippingContainerDataset>());
                }
                if (response.IsSuccessful)
                    logger.LogInformation($"Number of track package datasets inserted: {response.NumberOfDocuments}");
                else
                    logger.LogError($"Failed to bulk insert track package datasets. Total Failures: {response.NumberOfFailedDocuments}");
            }
            catch (Exception ex)
            {
                response.Message = ex.ToString();
                response.IsSuccessful = false;
                logger.LogError($"Failed to bulk insert track package datasets. Exception: { ex }");
            }
            return response;
        }

        public async Task<ReportResponse> UpdateTrackPackageDatasets(string shippingCarrier, List<TrackPackage> items)
        {
            var response = new ReportResponse() { IsSuccessful = true };
            try
            {
                if (items.Any())
                {
                    var chunk = 1000;
                    for (var offset = 0; offset < items.Count; offset += chunk)
                    {
                        var trackPackages = items.Skip(offset).Take(chunk).ToList();
                        var packageDatasets = await packageDatasetRepository.GetDatasetsByTrackingNumberAsync(trackPackages);
                        var shippingContainerDatasets = new List<ShippingContainerDataset>();
                        var unmatched = trackPackages
                            .Where(t => packageDatasets.FirstOrDefault(p => p.ShippingBarcode == t.TrackingNumber) == null);
                        // Find all containers which match by tracking number.
                        var containersMatchedTrackingNumber = await shippingContainerDatasetRepository
                            .GetDatasetsByTrackingNumberAsync(unmatched.ToList());
                        shippingContainerDatasets.AddRange(containersMatchedTrackingNumber);
                        // Exclude all tracking data records which already matched a package or a container.
                        unmatched = unmatched
                            .Where(t => containersMatchedTrackingNumber.FirstOrDefault(c => c.UpdatedBarcode == t.TrackingNumber) == null);
                        // Find all containers which match by container id.
                        var containersMatchedContainerId = await shippingContainerDatasetRepository
                            .GetDatasetsByContainerIdAsync(unmatched.ToList());
                        shippingContainerDatasets.AddRange(containersMatchedContainerId);
                        unmatched = unmatched
                            .Where(t => containersMatchedContainerId.FirstOrDefault(c => c.ContainerId == t.TrackingNumber) == null);
                        logger.LogInformation($"Total records: {trackPackages.Count}"
                            + $", matched packages: {trackPackages.Where(t => packageDatasets.FirstOrDefault(p => p.ShippingBarcode == t.TrackingNumber) != null).Count()}"
                            + $", matched containers: {trackPackages.Where(t => containersMatchedTrackingNumber.FirstOrDefault(c => c.UpdatedBarcode == t.TrackingNumber) != null).Count()}"
                            + $", matched containers by Id: {trackPackages.Where(t => containersMatchedContainerId.FirstOrDefault(c => c.ContainerId == t.TrackingNumber) != null).Count()}"
                            + $", unmatched: {unmatched.Count()}");

                        var trackPackageDatasets = new List<TrackPackageDataset>();
                        var packageDataSetsToUpdate = new List<PackageDataset>();
                        var shippingContainerDataSetsToUpdate = new List<ShippingContainerDataset>();
                        foreach (var trackPackage in trackPackages)
                        {
                            var packageDataset = packageDatasets.FirstOrDefault(p => p.ShippingBarcode == trackPackage.TrackingNumber);
                            ShippingContainerDataset shippingContainerDataset = null;
                            // always use the first shippingContainerDataset where barcode equals current trackingPackages trackingNumber
                            if (packageDataset == null)
                                shippingContainerDataset = shippingContainerDatasets.FirstOrDefault(c => c.UpdatedBarcode == trackPackage.TrackingNumber);
                            if (packageDataset == null && shippingContainerDataset == null)
                                shippingContainerDataset = shippingContainerDatasets.FirstOrDefault(c => c.ContainerId == trackPackage.TrackingNumber);
                            if (packageDataset != null || shippingContainerDataset != null)
                            {
                                var trackPackageDataset = CreateDataset(trackPackage, packageDataset, shippingContainerDataset);

                                // add to list
                                trackPackageDatasets.Add(trackPackageDataset);

                                // bind dto to poco, then add poco to list objects
                                SaveLastKnownEventData(shippingCarrier, packageDataSetsToUpdate, shippingContainerDataSetsToUpdate,
                                    packageDataset, shippingContainerDataset, trackPackageDataset);
                            }
                        }

                        // data access code
                        if (trackPackageDatasets.Any() || packageDataSetsToUpdate.Any() || shippingContainerDataSetsToUpdate.Any())
                            await BulkInsertOrUpdateAsync(response, trackPackageDatasets, packageDataSetsToUpdate, shippingContainerDataSetsToUpdate);
                    }
                }
                if (response.IsSuccessful)
                    logger.LogInformation($"Number of track package datasets inserted: {response.NumberOfDocuments}");
                else
                    logger.LogError($"Failed to bulk insert track package datasets. Total Failures: {response.NumberOfFailedDocuments}");
            }
            catch (Exception ex)
            {
                response.Message = ex.ToString();
                response.IsSuccessful = false;
                logger.LogError($"Failed to bulk update track package datasets. Exception: { ex }");
            }
            return response;
        }

        /// <summary>
        ///     Save last know event data in package or shipping container
        /// </summary>
        /// <param name="trackPackageDatasets"></param>
        /// <param name="packageDataSetsToUpdate"></param>
        /// <param name="shippingContainerDataSetsToUpdate"></param>
        /// <param name="packageDataset"></param>
        /// <param name="shippingContainerDataset"></param>
        /// <param name="trackPackageDataset"></param>
        private void SaveLastKnownEventData(string shippingCarrier,
            IList<PackageDataset> packageDataSetsToUpdate, IList<ShippingContainerDataset> shippingContainerDataSetsToUpdate,
            PackageDataset packageDataset, ShippingContainerDataset shippingContainerDataset, TrackPackageDataset trackPackageDataset)
        {
            int isStopTheClock = IsStopTheClock(trackPackageDataset.EventCode, shippingCarrier);
            int isUndeliverable = IsUndeliverable(trackPackageDataset.EventCode, shippingCarrier);
            if (packageDataset != null)
            {
                if (packageDataset.IsStopTheClock == null)
                    packageDataset.IsStopTheClock = 0;
                if (packageDataset.IsUndeliverable == null)
                    packageDataset.IsUndeliverable = 0;
                // Add undeliverable events to package
                if (isUndeliverable != 0)
                {
                    // Can have multiple undeliverable events with the same event date and event code.
                    var matchedItem = packageDataset.UndeliverableEvents.FirstOrDefault(
                        e => e.EventDate == trackPackageDataset.EventDate && e.EventCode == trackPackageDataset.EventCode);
                    if (matchedItem != null)
                        packageDataset.UndeliverableEvents.Remove(matchedItem);
                    packageDataset.UndeliverableEvents.Add(new UndeliverableEventDataset
                    {
                        PackageId = packageDataset.PackageId,
                        CosmosId = packageDataset.CosmosId,
                        SiteName = packageDataset.SiteName,
                        EventDate = trackPackageDataset.EventDate,
                        EventCode = trackPackageDataset.EventCode,
                        EventDescription =
                            EventDescription(trackPackageDataset.EventCode, shippingCarrier, trackPackageDataset.EventDescription),
                        EventLocation = trackPackageDataset.EventLocation,
                        EventZip = trackPackageDataset.EventZip
                    });
                    // add to list
                    if (packageDataSetsToUpdate.FirstOrDefault(p => p.CosmosId == packageDataset.CosmosId) == null)
                        packageDataSetsToUpdate.Add(packageDataset);
                }
                // Update stop the clock event date, etc. only when receiving first stc event, which might also be an undeliverable event.
                if (isStopTheClock != 0 && packageDataset.StopTheClockEventDate == null)
                {
                    packageDataset.StopTheClockEventDate = trackPackageDataset.EventDate;
                    packageDataset.PostalDays =
                        postalDaysRepository.CalculatePostalDays(packageDataset.StopTheClockEventDate.Value, packageDataset.LocalProcessedDate, packageDataset.ShippingMethod);
                    packageDataset.CalendarDays =
                        postalDaysRepository.CalculateCalendarDays(packageDataset.StopTheClockEventDate.Value, packageDataset.LocalProcessedDate);
                    // add to list
                    if (packageDataSetsToUpdate.FirstOrDefault(p => p.CosmosId == packageDataset.CosmosId) == null)
                        packageDataSetsToUpdate.Add(packageDataset);
                }
                // Update last known event date if date is newer and either stop the clock is not set or undeliverable is set.
                if ((packageDataset.LastKnownEventDate == null ||
                    trackPackageDataset.EventDate >= packageDataset.LastKnownEventDate)
                    && (packageDataset.IsStopTheClock == 0 || packageDataset.IsUndeliverable == 1))
                {
                    packageDataset.IsStopTheClock = isStopTheClock;
                    packageDataset.IsUndeliverable = isUndeliverable;
                    packageDataset.LastKnownEventDate = trackPackageDataset.EventDate;
                    packageDataset.LastKnownEventDescription =
                        EventDescription(trackPackageDataset.EventCode, shippingCarrier, trackPackageDataset.EventDescription);
                    packageDataset.LastKnownEventLocation = trackPackageDataset.EventLocation;
                    packageDataset.LastKnownEventZip = trackPackageDataset.EventZip;
                    // add to list
                    if (packageDataSetsToUpdate.FirstOrDefault(p => p.CosmosId == packageDataset.CosmosId) == null)
                        packageDataSetsToUpdate.Add(packageDataset);
                }
            }
            else if (shippingContainerDataset != null && shippingContainerDataset.StopTheClockEventDate == null
                && (shippingContainerDataset.LastKnownEventDate == null ||
                    trackPackageDataset.EventDate >= shippingContainerDataset.LastKnownEventDate || isStopTheClock == 1))
            {
                if (isStopTheClock == 1 && shippingContainerDataset.StopTheClockEventDate == null)
                {
                    shippingContainerDataset.StopTheClockEventDate = trackPackageDataset.EventDate;
                }
                shippingContainerDataset.LastKnownEventDate = trackPackageDataset.EventDate;
                shippingContainerDataset.LastKnownEventDescription =
                    EventDescription(trackPackageDataset.EventCode, shippingCarrier, trackPackageDataset.EventDescription);
                shippingContainerDataset.LastKnownEventLocation = trackPackageDataset.EventLocation;
                shippingContainerDataset.LastKnownEventZip = trackPackageDataset.EventZip;
                // add to list
                if (shippingContainerDataSetsToUpdate.FirstOrDefault(p => p.CosmosId == shippingContainerDataset.CosmosId) == null)
                    shippingContainerDataSetsToUpdate.Add(shippingContainerDataset);
            }
        }

        public int IsStopTheClock(string eventCode, string shippingCarrier)
        {
            LoadEventCodes();
            carrierIsStopTheClock.TryGetValue($"{shippingCarrier}:{eventCode}", out var isSTC);
            return isSTC;
        }

        public int IsUndeliverable(string eventCode, string shippingCarrier)
        {
            LoadEventCodes();
            carrierIsUndeliverable.TryGetValue($"{shippingCarrier}:{eventCode}", out var isUndeliverable);
            return isUndeliverable;
        }
        public string EventDescription(string eventCode, string shippingCarrier, string eventDescription)
        {
            LoadEventCodes();
            if (carrierEventDescriptions.TryGetValue($"{shippingCarrier}:{eventCode}", out var description))
                eventDescription = description;
            return eventDescription;
        }

        private async Task BulkInsertOrUpdateAsync(ReportResponse response,
            List<TrackPackageDataset> trackPackageDatasets,
            List<PackageDataset> packageDataSets,
            List<ShippingContainerDataset> shippingContainerDatasets
            )
        {
            // Need to remove duplicates to avoid bulk insert exception.
            var items = new List<TrackPackageDataset>();
            foreach (var group in trackPackageDatasets
                .Where(p => p.PackageDatasetId != null)
                .GroupBy(p => new { p.PackageDatasetId, p.EventCode, p.EventDate, p.EventDescription }))
            {
                items.Add(group.FirstOrDefault());
            }
            foreach (var group in trackPackageDatasets
                .Where(p => p.ShippingContainerDatasetId != null)
                .GroupBy(p => new { p.ShippingContainerDatasetId, p.EventCode, p.EventDate, p.EventDescription }))
            {
                items.Add(group.FirstOrDefault());
            }
            trackPackageDatasets = items;

            var bulkInsert = false;
            for (var retries = 3; --retries >= 0;)
            {
                bulkInsert = await trackPackageDatasetRepository.ExecuteBulkInsertOrUpdateAsync(trackPackageDatasets);
                if (bulkInsert)
                    break;
                Thread.Sleep(1000);
            }
            response.NumberOfDocuments += trackPackageDatasets.Count();
            if (!bulkInsert)
            {
                response.NumberOfFailedDocuments += trackPackageDatasets.Count();
                response.IsSuccessful = false;
            }
            else
            {
                if (packageDataSets.Any())
                {
                    // ExecuteBulkInsertOrUpdateAsync takes longer so only do it if we have undeliverable events to add.
                    bulkInsert = await packageDatasetRepository.ExecuteBulkInsertOrUpdateAsync(packageDataSets.
                        Where(p => p.UndeliverableEvents.Count > 0).ToList());
                    if (!bulkInsert)
                    {
                        response.NumberOfFailedDocuments += packageDataSets.Where(p => p.UndeliverableEvents.Count > 0).Count();
                        response.IsSuccessful = false;
                    }
                    bulkInsert = await packageDatasetRepository.ExecuteBulkUpdateAsync(packageDataSets.
                        Where(p => p.UndeliverableEvents.Count == 0).ToList());
                    if (!bulkInsert)
                    {
                        response.NumberOfFailedDocuments += packageDataSets.Where(p => p.UndeliverableEvents.Count == 0).Count();
                        response.IsSuccessful = false;
                    }
                }
                if (shippingContainerDatasets.Any())
                {
                    bulkInsert = await shippingContainerDatasetRepository.ExecuteBulkUpdateAsync(shippingContainerDatasets);
                    if (!bulkInsert)
                    {
                        response.NumberOfFailedDocuments += shippingContainerDatasets.Count();
                        response.IsSuccessful = false;
                    }
                }
            }
        }

        /// <summary>
        /// enhancing based on story 1269
        /// calculating postal days instead of using the user defined function [PostalDateDiff]
        /// </summary>
        /// <param name="trackPackage"></param>
        /// <param name="packageDataset"></param>
        /// <param name="shippingContainerDataset"></param>
        /// <returns></returns>
        private TrackPackageDataset CreateDataset(TrackPackage trackPackage,
            PackageDataset packageDataset, ShippingContainerDataset shippingContainerDataset)
        {
            var eventDate = DateTime.Now;
            var eventCode = string.Empty;
            var eventDescription = string.Empty;
            var eventLocation = string.Empty;
            var eventZip = string.Empty;

            if (trackPackage.UspsTrackingData != null)
            {
                eventDate = trackPackage.UspsTrackingData.EventDateTime;
                eventCode = trackPackage.UspsTrackingData.EventCode;
                eventDescription = trackPackage.UspsTrackingData.EventName;
                eventLocation = trackPackage.UspsTrackingData.ScanningFacilityName;
                eventZip = trackPackage.UspsTrackingData.ScanningFacilityZip;
            }
            else if (trackPackage.UpsTrackingData != null)
            {
                eventDate = trackPackage.UpsTrackingData.DeliveryDateTime;
                eventDescription = trackPackage.UpsTrackingData.DeliveryLocationDescription;
                // Activity location <City> <State>
                eventLocation = $"{trackPackage.UpsTrackingData.ActivityLocationPoliticalDivision2} {trackPackage.UpsTrackingData.ActivityLocationPoliticalDivision1}";
                eventZip = trackPackage.UpsTrackingData.ActivityLocationPostcodePrimaryLow;
            }
            else if (trackPackage.FedExTrackingData != null) // TODO change this (i am unsure here)
            {
                eventDate = trackPackage.FedExTrackingData.LastStatusDateTime;
                eventCode = trackPackage.FedExTrackingData.LastStatusCode;
                eventDescription = StringHelper.Exists(trackPackage.FedExTrackingData.LastStatusDescription)
                    ? trackPackage.FedExTrackingData.LastStatusDescription
                    : EventDescription(trackPackage.FedExTrackingData.LastStatusCode, ShippingCarrierConstants.FedEx, string.Empty);
                eventLocation = trackPackage.FedExTrackingData.EventAddress;
            }
            var result = new TrackPackageDataset
            {
                SiteName = packageDataset?.SiteName ?? shippingContainerDataset?.SiteName,
                TrackingNumber = packageDataset?.ShippingBarcode ?? shippingContainerDataset?.UpdatedBarcode,
                PackageId = packageDataset?.PackageId,
                PackageDatasetId = packageDataset?.Id,
                ShippingContainerId = shippingContainerDataset?.ContainerId,
                ShippingContainerDatasetId = shippingContainerDataset?.Id,
                ShippingCarrier = packageDataset?.ShippingCarrier ?? shippingContainerDataset?.ShippingCarrier,

                EventDate = eventDate,
                EventCode = eventCode,
                EventDescription = eventDescription,
                EventLocation = eventLocation,
                EventZip = eventZip
            };
            return result;
        }
    }
}

