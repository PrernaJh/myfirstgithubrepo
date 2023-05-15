using Microsoft.Extensions.Logging;
using MMS.API.Domain.Models.AutoScan;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.ProcessScanAndAuto;
using MMS.API.Domain.Models.Returns;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.Linq;

namespace MMS.API.Domain.Processors
{
    public class PackageErrorProcessor : IPackageErrorProcessor
    {
        private readonly IPackageLabelProcessor labelProcessor;
        private readonly ILogger<PackageErrorProcessor> logger;
        private readonly ICreatePackageZplProcessor singlePackageZplProcessor;
        private readonly IAutoScanZplProcessor zplProcessor;

        public PackageErrorProcessor(
            IPackageLabelProcessor labelProcessor,
            ILogger<PackageErrorProcessor> logger,
            ICreatePackageZplProcessor singlePackageZplProcessor,
            IAutoScanZplProcessor zplProcessor)
        {
            this.labelProcessor = labelProcessor;
            this.logger = logger;
            this.singlePackageZplProcessor = singlePackageZplProcessor;
            this.zplProcessor = zplProcessor;
        }

        public string EvaluateScannedPackageStatus(Package package, ProcessScanPackage processScan)
        {
            var eventDescription = string.Empty;

            if (processScan.IsServiced && processScan.IsShipped && processScan.IsLabeled)
            {
                package.ProcessedDate = DateTime.Now;
                package.LocalProcessedDate = TimeZoneUtility.GetLocalTime(package.TimeZone);
                package.PackageStatus = EventConstants.Processed;
                eventDescription = "Scanned by user";
            }
            else if (processScan.IsServiced && processScan.IsReturned && processScan.IsLabeled)
            {
                package.ProcessedDate = DateTime.Now;
                package.LocalProcessedDate = TimeZoneUtility.GetLocalTime(package.TimeZone);
                package.PackageStatus = EventConstants.Exception;
                eventDescription = "Scanned by user and serviced as RETURN";
            }
            else if (!processScan.IsJobAssigned)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidJob;
                package.PackageStatus = EventConstants.Exception;
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage}";
            }
            else if (!processScan.IsServiced)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.ServiceRuleNotFound;
                package.PackageStatus = EventConstants.Exception;
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage}";
            }
            else if(package.CarrierApiErrors.Any())
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.CarrierDataError;
                package.PackageStatus = EventConstants.Exception;
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage} " +
                    $"{package.CarrierApiErrors.FirstOrDefault().Description ?? string.Empty}";
            }
            else
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.CarrierDataError;
                package.PackageStatus = EventConstants.Exception;
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage}";
            }

            return eventDescription;
        }

        public string EvaluateCreatedPackageStatus(Package package)
        {
            var errorMessage = string.Empty;

            if (StringHelper.DoesNotExist(package.SubClientName))
            {
                errorMessage = ErrorLabelConstants.SubClientError;
                package.PackageStatus = EventConstants.Exception;
            }
            else if (StringHelper.DoesNotExist(package.BinCode))
            {
                errorMessage = ErrorLabelConstants.BinError;
                package.PackageStatus = EventConstants.Exception;
            }
            else if (StringHelper.DoesNotExist(package.ShippingCarrier) || StringHelper.DoesNotExist(package.ShippingMethod))
            {
                errorMessage = ErrorLabelConstants.ServiceError;
                package.PackageStatus = EventConstants.Exception;
            }
            else if (StringHelper.DoesNotExist(package.Barcode))
            {
                errorMessage = ErrorLabelConstants.TrackingNumberError;
                package.PackageStatus = EventConstants.Exception;
            }
            else if (StringHelper.DoesNotExist(package.Base64Label) && package.LabelTypeId == 0)
            {
                errorMessage = ErrorLabelConstants.LabelError;
                package.PackageStatus = EventConstants.Exception;
            }

            return errorMessage;
        }

        public void GenerateCreatedPackageError(Package package, ProcessScanPackage processScan)
        {
            if (StringHelper.Exists(processScan.ErrorLabelMessage))
            {
                package.Base64Label = singlePackageZplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.Id, package);

                var eventDescription = $"Error processing created package: {processScan.ErrorLabelMessage}";
                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.ManualScan,
                    EventStatus = package.PackageStatus,
                    Description = eventDescription,
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
        }

        public void GenerateBinValidationZpl(Package package, string message)
        {
            if (StringHelper.Exists(package.Base64Label))
            {
                package.HistoricalBase64Labels.Add(package.Base64Label);
            }
            package.Base64Label = singlePackageZplProcessor.GenerateThreeLineLabel(message, package.BinCode, package.PackageId);
        }

        public void GenerateBinValidationLabel(Package package)
        {
            var labelFieldValues = labelProcessor.GetLabelDataForSortCodeChange(package.BinCode, package.PackageId);
            package.LabelFieldValues = labelFieldValues;
        }

        public void GenerateScanPackageError(Package package, ProcessScanPackage processScan)
        {
            var eventDescription = string.Empty;

            if (StringHelper.DoesNotExist(package.Id))
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidPackageId;
            }
            else if (processScan.Weight <= 0)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidWeight;
            }
            else if (package.PackageStatus == EventConstants.Recalled)
            {
                var request = new ReturnLabelRequest
                {
                    SiteName = package.SiteName,
                    PackageId = package.PackageId,
                    ReturnReason = ErrorLabelConstants.Recall
                };
                package.RecallStatus = EventConstants.RecallScanned;
                package.LabelTypeId = LabelTypeIdConstants.ReturnToSender;
                package.LabelFieldValues = labelProcessor.GetLabelFieldsForReturnToCustomer(request);
            }
            else if (package.IsLocked)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.EndOfDayProcessed;
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage}";
            }
            else
            {
                processScan.IsInvalidStatus = true;
                processScan.ErrorLabelMessage = $"{ErrorLabelConstants.InvalidStatus}: {package.PackageStatus}";
                eventDescription = $"Error during processing: type of {processScan.ErrorLabelMessage}";
            }

            if (processScan.ShouldUpdate)
            {
                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.ManualScan,
                    EventStatus = package.PackageStatus,
                    Description = eventDescription,
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
        }

        public void GenerateAutoScanCreatedPackageError(Package package, ProcessAutoScanPackage processScan, ParcelDataResponse response)
        {
            if (StringHelper.Exists(processScan.ErrorLabelMessage))
            {
                package.Base64Label = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.Id, package.TimeZone);
                response.Zpl = package.Base64Label ?? string.Empty;

                var eventDescription = $"Error processing created package: {processScan.ErrorLabelMessage}";

                logger.LogError($"AutoScan: Error Processing PackageId {package.PackageId} Error Message: {processScan.ErrorLabelMessage}");

                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.AutoScan,
                    EventStatus = package.PackageStatus,
                    Description = eventDescription,
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
        }

        public void GenerateAutoScanPackageError(Package package, ProcessAutoScanPackage processScan, ParcelDataResponse response)
        {
            var eventDescription = string.Empty;

            if (StringHelper.DoesNotExist(package.Id))
            {
                // a package in a scannable status was not found, or the duplicate check triggered a stop
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidPackageId;
                logger.LogError($"PackageId {processScan.PackageIdRequest} not found or failed duplicate check. Username: {processScan.Username} Site: {processScan.SiteNameRequest}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, processScan.SiteNameRequest, processScan.PackageIdRequest) ?? string.Empty;
            }
            else if (processScan.Weight <= 0)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidWeight;
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, processScan.SiteNameRequest, processScan.PackageIdRequest, package.TimeZone) ?? string.Empty;
            }
            else if (package.PackageStatus == EventConstants.Recalled)
            {
                // return to sender
                package.RecallStatus = EventConstants.RecallScanned;
                response.LogicalName = string.Empty;
                response.Verify = package.Barcode;
                package.Base64Label = zplProcessor.GenerateErrorLabel(ErrorLabelConstants.Recall, package.SiteName, package.Id, package.TimeZone) ?? string.Empty;
                response.Zpl = package.Base64Label;
                eventDescription = "Attempt to process RECALLED package";
            }
            else if (package.IsLocked)
            {
                // already manifested
                processScan.ErrorLabelMessage = ErrorLabelConstants.EndOfDayProcessed;
                logger.LogError($"PackageId {package.PackageId} has already been manifested. Username: {processScan.Username} Site: {processScan.SiteNameRequest}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.PackageId, package.TimeZone) ?? string.Empty;
                eventDescription = processScan.ErrorLabelMessage;
            }
            else
            {
                processScan.ErrorLabelMessage = $"{ErrorLabelConstants.InvalidStatus}: {package.PackageStatus}";
                logger.LogError($"PackageId {package.PackageId} is in an invalid status. Username: {processScan.Username} Site: {processScan.SiteNameRequest}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, processScan.SiteNameRequest, processScan.PackageIdRequest) ?? string.Empty;
                eventDescription = processScan.ErrorLabelMessage;
            }

            if (processScan.ShouldUpdate)
            {
                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.AutoScan,
                    EventStatus = package.PackageStatus,
                    Description = eventDescription,
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
        }
    }
}
