using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.AutoScan;
using MMS.API.Domain.Models.Containers;
using MMS.API.Domain.Models.CreatePackage;
using MMS.API.Domain.Models.ProcessScanAndAuto;
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
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace MMS.API.Domain.Processors
{
    public class AutoScanProcessor : IAutoScanProcessor
    {
        private readonly IAutoScanZplProcessor zplProcessor;
        private readonly IBinProcessor binProcessor;
        private readonly IClientFacilityProcessor clientFacilityProcessor;
        private readonly ILogger<AutoScanProcessor> logger;
        private readonly IJobScanProcessor jobProcessor;
        private readonly IPackageContainerProcessor packageContainerProcessor;
        private readonly IPackageDuplicateProcessor packageDuplicateProcessor;
        private readonly IPackageErrorProcessor packageErrorProcessor;
        private readonly IPackageRepeatProcessor repeatProcessor;
        private readonly IPackagePostProcessor packagePostProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly IPackageServiceProcessor serviceProcessor;
        private readonly IShippingProcessor shippingProcessor;
        private readonly ICreatePackageProcessor singlePackageProcessor;
        private readonly ICreatePackageZplProcessor singlePackageZplProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IUspsShippingProcessor uspsShippingProcessor;

        public AutoScanProcessor(
            IAutoScanZplProcessor zplProcessor,
            IBinProcessor binProcessor,
            IClientFacilityProcessor clientFacilityProcessor,
            ILogger<AutoScanProcessor> logger,
            IJobScanProcessor jobProcessor,
            IPackageContainerProcessor packageContainerProcessor,
            IPackageDuplicateProcessor packageDuplicateProcessor,
            IPackageErrorProcessor packageErrorProcessor,
            IPackageRepeatProcessor repeatProcessor,
            IPackagePostProcessor packagePostProcessor,
            IPackageRepository packageRepository,
            IPackageServiceProcessor serviceProcessor,
            IShippingProcessor shippingProcessor,
            ICreatePackageProcessor singlePackageProcessor,
            ICreatePackageZplProcessor singlePackageZplProcessor,
            ISubClientProcessor subClientProcessor,
            IUspsShippingProcessor uspsShippingProcessor)
        {
            this.zplProcessor = zplProcessor;
            this.binProcessor = binProcessor;
            this.clientFacilityProcessor = clientFacilityProcessor;
            this.logger = logger;
            this.jobProcessor = jobProcessor;
            this.packageContainerProcessor = packageContainerProcessor;
            this.packageDuplicateProcessor = packageDuplicateProcessor;
            this.packageErrorProcessor = packageErrorProcessor;
            this.repeatProcessor = repeatProcessor;
            this.packagePostProcessor = packagePostProcessor;
            this.packageRepository = packageRepository;
            this.serviceProcessor = serviceProcessor;
            this.shippingProcessor = shippingProcessor;
            this.singlePackageProcessor = singlePackageProcessor;
            this.singlePackageZplProcessor = singlePackageZplProcessor;
            this.subClientProcessor = subClientProcessor;
            this.uspsShippingProcessor = uspsShippingProcessor;
        }

        public async Task<(ParcelDataResponse, PackageTimer)> ProcessAutoScanPackageRequest(ParcelDataRequest request)
        {
            var timer = new PackageTimer();
            timer.TotalWatch.Start();
            var response = new ParcelDataResponse();
            var processScan = new ProcessAutoScanPackage
            {
                Username = request.Username,
                MachineId = request.MachineId,
                SiteNameRequest = request.Site,
                PackageIdRequest = request.Barcode,
                Weight = request.Weight,
            };

            timer.SelectQueryWatch.Start();
            var packages = await packageRepository.GetPackagesByPackageId(request.Barcode, request.Site);
            var package = await packageDuplicateProcessor.EvaluateDuplicatePackagesOnScan(packages.ToList());
            timer.SelectQueryWatch.Stop();

            if (StringHelper.Exists(package.Id))
            {
                package.JobBarcode = request.JobId;

                if (package.IsCreated)
                {
                    await ProcessCreatedPackage(package, response, processScan);
                }
                else
                {
                    package.PrintLabel = true;
                    var isPackageValidToProcess = IsAutoScanPackageValidToProcess(package, request.Weight);

                    if (isPackageValidToProcess)
                    {
                        processScan.ShouldUpdate = true;
                        var isReleasedReprocess = package.PackageStatus == EventConstants.Released &&
                            package.PackageEvents.Any(x => x.EventStatus == EventConstants.Processed);

                        if (isReleasedReprocess)
                        {
                            package = await repeatProcessor.ProcessRepeatScan(package, request.Username, request.MachineId);
                        }

                        package.Weight = request.Weight;
                        package.SiteName = request.Site;
                        package.EodUpdateCounter += 1;

                        await ProcessAutoScanPackage(package, timer, response, processScan);
                        processScan.ProcessingCompleted = true;
                    }
                }
            }

            if (!processScan.ProcessingCompleted)
            {
                response.PrintLabel = true;
                if (StringHelper.Exists(package.Id))
                {
                    processScan.ShouldUpdate = true;
                }
                else
                {
                    logger.LogError($"PackageId {request.Barcode} not found. Username: {request.Username} Site: {request.Site}");
                }

                if (processScan.IsCreatedPackage)
                {
                    packageErrorProcessor.GenerateAutoScanCreatedPackageError(package, processScan, response);
                }
                else
                {
                    packageErrorProcessor.GenerateAutoScanPackageError(package, processScan, response);
                }
            }

            if (processScan.ShouldUpdate)
            {
                timer.UpdateQueryWatch.Start();
                await packageRepository.UpdateItemAsync(package);
                timer.UpdateQueryWatch.Stop();
            }

            return (response, timer);
        }

        public async Task<ParcelConfirmDataResponse> ConfirmParcelData(ParcelConfirmDataRequest request)
        {
            // if we have 2 identical ASNs imported at 2 sites simultaneously, this API call can fail due to a race condition
            // In order to resolve this, we need to add a siteName property to the request.  This requires Numina code
            // However this call is effectively useless as it has no business requirement, only Numina asked for it
            // We don't even know what happens if it fails 

            var package = await packageRepository.GetPackageForConfirmParcelData(request.Barcode);
            var confirmed = package.BinCode == request.LogicalLane;

            return new ParcelConfirmDataResponse()
            {
                Confirmed = confirmed
            };
        }

        public async Task<(NestParcelResponse Response, PackageTimer Timer, string Message)> ProcessNestPackageRequest(NestParcelRequest request)
        {
            var timer = new PackageTimer();
            timer.TotalWatch.Start();
            var message = string.Empty;
           
            var assignContainerRequest = new AssignContainerRequest
            {
                SiteName = request.Site,
                PackageId = request.Barcode,
                Username = request.Username,
                MachineId = request.MachineId,
            };

            var assignActiveContainerResponse = await packageContainerProcessor.AssignPackageActiveContainerAsync(assignContainerRequest);
            var response = new NestParcelResponse()
            {
                BinCode = assignActiveContainerResponse.ActiveBinCode
            };
            if (assignActiveContainerResponse.ErrorCode == ContainerErrorConstants.ContainerAlreadyAssigned)
            {
                message = "Active container already assigned.";
            }
            else if (assignActiveContainerResponse.IsSuccessful)
            {
                message = "Active container assigned to package.";
                
            }
            else
            {
                message = $"Failed to nest package. {assignActiveContainerResponse.Message} Error Code: {assignActiveContainerResponse.ErrorCode}";
            }
            timer.TotalWatch.Stop();

            return (response, timer, message);
        }

        private async Task ProcessAutoScanPackage(Package package, PackageTimer timer, ParcelDataResponse response, ProcessAutoScanPackage processScan)
        {
            var eventDescription = string.Empty;
            var isServiced = false;
            processScan.IsJobAssigned = await jobProcessor.GetJobDataForPackageScan(package);

            if (processScan.IsJobAssigned)
            {
                timer.ServiceWatch.Start();
                isServiced = await serviceProcessor.GetServiceDataAsync(package);
                timer.ServiceWatch.Stop();
            }
            if (isServiced)
            {
                timer.ShippingWatch.Start();

                if (package.ShippingCarrier == Usps)
                {
                    package.Barcode = uspsShippingProcessor.GenerateUspsBarcode(package);

                    if (StringHelper.Exists(package.Barcode))
                    {
                        var isAutoScan = true;
                        timer.ContainerWatch.Start();
                        await packageContainerProcessor.AssignPackageContainerData(package, isAutoScan);
                        timer.ContainerWatch.Stop();

                        package.HumanReadableBarcode = package.Barcode[8..];
                        package.FormattedBarcode = uspsShippingProcessor.GenerateUspsFormattedBarcode(package.Barcode);
                        package.Base64Label = zplProcessor.GenerateUspsLabel(package);
                        package.PackageStatus = EventConstants.Processed;
                    }
                }
                else if (package.ShippingCarrier == Ups)
                {
                    logger.LogInformation($"Shipping Method: {package.ShippingMethod} for ASN {package.PackageId}");
                    var isZpl = true;
                    var shouldAddCustomsData = false;
                    var subClient = await subClientProcessor.GetSubClientByNameAsync(package.SubClientName);

                    shouldAddCustomsData = await shippingProcessor.AssignUpsCustomsDataToPackage(package, subClient);

                    logger.LogInformation($"UPS ACCOUNT NUMBER: {subClient.UpsAccountNumber}");
                    var upsResponse = await shippingProcessor.GetUpsShippingDataAsync(package,
                        subClient.UpsAccountNumber, shouldAddCustomsData, isZpl, subClient.UpsDirectDeliveryOnly);
                    package.Barcode = upsResponse.Barcode;
                    package.Base64Label = package.ShippingMethod == UpsGround ? zplProcessor.GenerateUpsGroundLabel(package, upsResponse.Base64Label)
                                                                              : zplProcessor.GenerateUpsAirLabel(package, upsResponse.Base64Label);
                    package.PackageStatus = EventConstants.Processed;
                }
                else if (package.ShippingMethod == ReturnToCustomer)
                {
                    package.Base64Label = zplProcessor.GenerateErrorLabel(ErrorLabelConstants.ReturnToCustomer, package.SiteName, package.PackageId, package.TimeZone);
                    package.PackageStatus = EventConstants.Exception;
                }

                package.LabelTypeId = LabelTypeIdConstants.AutoScan;
                timer.ShippingWatch.Stop();
            }

            if (StringHelper.Exists(package.Base64Label))
            {
                package.ProcessedDate = DateTime.Now;
                package.LocalProcessedDate = TimeZoneUtility.GetLocalTime(package.TimeZone);
                response.LogicalName = package.BinCode ?? "ERROR";
                response.Verify = package.Barcode ?? "000000000";
                response.Zpl = package.Base64Label;
                eventDescription = "Scanned by AutoScan system";
            }
            else if (!processScan.IsJobAssigned)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidJob;
                package.PackageStatus = EventConstants.Exception;
                logger.LogError($"PackageId {package.PackageId} invalid job during autoscan. Username: {processScan.Username} Site: {package.SiteName}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.PackageId) ?? string.Empty;
                eventDescription = processScan.ErrorLabelMessage;
            }
            else if (!isServiced)
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.ServiceRuleNotFound;
                package.PackageStatus = EventConstants.Exception;
                logger.LogError($"PackageId {package.PackageId} No service rule found during autoscan. Username: {processScan.Username} Site: {package.SiteName}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.PackageId) ?? string.Empty;
                eventDescription = processScan.ErrorLabelMessage;
            }
            else
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.CarrierDataError;
                package.PackageStatus = EventConstants.Exception;
                logger.LogError($"PackageId {package.PackageId} failed to get carrier data during autoscan. Username: {processScan.Username} Site: {package.SiteName}");
                response.Zpl = zplProcessor.GenerateErrorLabel(processScan.ErrorLabelMessage, package.SiteName, package.PackageId) ?? string.Empty;
                eventDescription = processScan.ErrorLabelMessage;
            }
            response.PrintLabel = package.PrintLabel;
            package.PackageEvents.Add(new Event
            {
                EventId = package.PackageEvents.Count + 1,
                EventType = EventConstants.AutoScan,
                EventStatus = package.PackageStatus,
                Description = eventDescription,
                TrackingNumber = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
                Username = processScan.Username,
                MachineId = processScan.MachineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
            });
        }

        private async Task ProcessCreatedPackage(Package package, ParcelDataResponse response, ProcessAutoScanPackage processScan)
        {
            processScan.IsJobAssigned = await jobProcessor.GetJobDataForCreatePackageScan(package);

            if (processScan.IsJobAssigned)
            {
                if (package.PrintLabel)
                {
                    var clientFacility = await clientFacilityProcessor.GetClientFacility(package.ClientFacilityName);
                    var generateCreatePackageRequest = new GenerateCreatePackageRequest
                    {
                        ClientFacility = clientFacility,
                        IsAutoScan = true
                    };

                    await singlePackageProcessor.ProcessGenerateCreatePackage(package, generateCreatePackageRequest);
                    processScan.ErrorLabelMessage = packageErrorProcessor.EvaluateCreatedPackageStatus(package);

                    if (StringHelper.DoesNotExist(processScan.ErrorLabelMessage))
                    {
                        processScan.ProcessingCompleted = true;
                    }
                }
                else
                {
                    package.HistoricalBase64Labels.Add(package.Base64Label);
                    package.Base64Label = string.Empty;
                    var isBinVerified = await binProcessor.VerifyCreatedPackageBinOnScan(package); // only verify bin if we are not printing a shipping label

                    if (!isBinVerified) // package is still processed on bin verify fail, print special label
                    {
                        package.PrintLabel = true;
                        packageErrorProcessor.GenerateBinValidationZpl(package, ErrorLabelConstants.SortCodeChange);
                    }
                    await packageContainerProcessor.AssignPackageContainerData(package, true);
                    package.PackageStatus = EventConstants.Processed;
                    processScan.ProcessingCompleted = true;
                }

                package.ProcessedDate = DateTime.Now;
                package.LocalProcessedDate = TimeZoneUtility.GetLocalTime(package.TimeZone);
                response.PrintLabel = package.PrintLabel;
                response.LogicalName = package.BinCode ?? "ERROR";
                response.Verify = package.Barcode ?? "000000000";
                response.Zpl = package.PrintLabel ? package.Base64Label : string.Empty;

                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.AutoScan,
                    EventStatus = package.PackageStatus,
                    Description = "Scanned by AutoScan system",
                    TrackingNumber = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
            else
            {
                logger.LogError($"PackageId {package.PackageId} invalid job during single package autoscan. Username: {processScan.Username} Site: {package.SiteName}");
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidJob;
                package.PackageStatus = EventConstants.Exception;
            }

            processScan.ShouldUpdate = true;
            processScan.IsCreatedPackage = true;
        }

        private bool IsAutoScanPackageValidToProcess(Package package, decimal weight)
        {
            var validStatusesForProcessing = new List<string>
            {
                EventConstants.Imported,
                EventConstants.Released
            };

            var validToProcess = StringHelper.Exists(package.Id)
                    && validStatusesForProcessing.Contains(package.PackageStatus)
                    && weight > 0
                    && !package.IsLocked;

            return validToProcess;
        }
    }
}