using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class PackageScanProcessor : IPackageScanProcessor
    {
        private readonly IConfiguration config;
        private readonly ICreatePackageScanProcessor createPackageScanProcessor;
        private readonly IEodPostProcessor eodPostProcessor;
        private readonly IJobScanProcessor jobProcessor;
        private readonly ILogger<PackageScanProcessor> logger;
        private readonly IPackageContainerProcessor packageContainerProcessor;
        private readonly IPackageDuplicateProcessor packageDuplicateProcessor;
        private readonly IPackageErrorProcessor packageErrorProcessor;
        private readonly IPackageLabelProcessor labelProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly IPackageRepeatProcessor repeatProcessor;
        private readonly IPackageServiceProcessor serviceProcessor;
        private readonly IShippingProcessor shippingProcessor;

        public PackageScanProcessor(
            IConfiguration config,
            ICreatePackageScanProcessor createPackageScanProcessor,
            IEodPostProcessor eodPostProcessor,
            IJobScanProcessor jobProcessor,
            ILogger<PackageScanProcessor> logger,
            IPackageContainerProcessor packageContainerProcessor,
            IPackageDuplicateProcessor packageDuplicateProcessor,
            IPackageErrorProcessor packageErrorProcessor,
            IPackageLabelProcessor labelProcessor,
            IPackageRepository packageRepository,
            IPackageRepeatProcessor repeatProcessor,
            IPackageServiceProcessor serviceProcessor,
            IShippingProcessor shippingProcessor)
        {
            this.config = config;
            this.createPackageScanProcessor = createPackageScanProcessor;
            this.eodPostProcessor = eodPostProcessor;
            this.jobProcessor = jobProcessor;
            this.logger = logger;
            this.labelProcessor = labelProcessor;
            this.packageContainerProcessor = packageContainerProcessor;
            this.packageDuplicateProcessor = packageDuplicateProcessor;
            this.packageErrorProcessor = packageErrorProcessor;
            this.packageRepository = packageRepository;
            this.repeatProcessor = repeatProcessor;
            this.serviceProcessor = serviceProcessor;
            this.shippingProcessor = shippingProcessor;
        }

        public async Task<(ScanPackageResponse ScanPackageResponse, PackageTimer Timer)> ProcessScanPackageRequest(ScanPackageRequest request, bool isRepeatScan = false)
        {
            var response = new ScanPackageResponse();
            var processScan = new ProcessScanPackage
            {
                Username = request.Username,
                MachineId = request.MachineId,
                JobId = request.JobId,
                SiteNameRequest = request.SiteName,
                PackageIdRequest = request.PackageId,
                Weight = GetWeightFromRequest(request),
                IsRepeatScan = isRepeatScan
            };
            processScan.Timer.TotalWatch.Start();

            processScan.Timer.SelectQueryWatch.Start();
            var packages = await packageRepository.GetPackagesByPackageId(request.PackageId, request.SiteName);
            var package = await packageDuplicateProcessor.EvaluateDuplicatePackagesOnScan(packages.ToList(), processScan.IsRepeatScan);
            processScan.Timer.SelectQueryWatch.Stop();

            if (StringHelper.Exists(package.Id))
            {
                if (package.IsCreated)
                {
                    await createPackageScanProcessor.ProcessCreatedPackage(package, processScan);
                }
                else
                {
                    var isReprocess = ShouldReplacePackage(package, processScan.IsRepeatScan);
                    var isPackageValidToProcess = false;
                    var isPackageValidToReprocess = false;

                    if (isReprocess)
                    {
                        isPackageValidToReprocess = await IsPackageValidToReprocess(package, processScan.Weight);
                    }
                    else
                    {
                        isPackageValidToProcess = await IsPackageValidToProcess(package, processScan.Weight);
                    }

                    if (isPackageValidToProcess || isPackageValidToReprocess)
                    {
                        processScan.ShouldUpdate = true;

                        var isReplaced = false;

                        if (isReprocess && isPackageValidToReprocess)
                        {
                            package = await repeatProcessor.ProcessRepeatScan(package, request.Username, request.MachineId);
                            isReplaced = true;
                        }

                        if (isPackageValidToProcess || isReplaced)
                        {
                            package.Weight = processScan.Weight;
                            package.JobBarcode = request.JobId;
                            package.SiteName = request.SiteName;
                            package.EodUpdateCounter += 1;

                            await ProcessPackage(package, processScan.Timer, processScan);

                            package.PrintLabel = true;
                            processScan.ProcessingCompleted = true;
                        }
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
                    logger.LogError($"PackageId {request.PackageId} not found or failed duplicate check. Username: {request.Username} Site: {request.SiteName}");
                }

                if (processScan.IsCreatedPackage)
                {
                    packageErrorProcessor.GenerateCreatedPackageError(package, processScan);
                }
                else
                {
                    packageErrorProcessor.GenerateScanPackageError(package, processScan);
                }
            }

            if (processScan.ShouldUpdate)
            {
                processScan.Timer.UpdateQueryWatch.Start();
                await packageRepository.UpdateItemAsync(package);
                processScan.Timer.UpdateQueryWatch.Stop();
            }

            return (GenerateScanPackageResponse(package, processScan), processScan.Timer);
        }

        public async Task<ReprintPackageResponse> ReprintPackageAsync(ReprintPackageRequest request)
        {
            var disableReprint = config.GetValue<bool>("IsReprintDisabled");

            if (disableReprint)
            {
                return new ReprintPackageResponse
                {
                    IsReprintDisabled = true
                };
            }

            var package = await packageRepository.GetProcessedPackageByPackageId(request.PackageId, request.SiteName);

            if (package.PackageStatus == EventConstants.Processed && !package.IsCreated)
            {
                var description = string.Empty;

                if (package.LabelTypeId == LabelTypeIdConstants.AutoScan)
                {
                    description = "AutoScan Package Label Reprinted By User";
                    labelProcessor.GetLabelDataForAutoScanReprint(package);
                }
                else
                {
                    description = "Package Label Reprinted By User";
                }

                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventStatus = EventConstants.Processed,
                    EventType = EventConstants.Reprint,
                    Description = description,
                    Username = request.Username,
                    MachineId = request.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });

                var updatedPackage = await packageRepository.UpdateItemAsync(package);

                return new ReprintPackageResponse
                {
                    Carrier = updatedPackage.ShippingCarrier,
                    ServiceType = updatedPackage.ShippingMethod,
                    Bin = updatedPackage.BinCode,
                    Barcode = updatedPackage.Barcode,
                    HumanReadableBarcode = updatedPackage.HumanReadableBarcode,
                    LabelTypeId = updatedPackage.LabelTypeId,
                    LabelFieldValues = updatedPackage.LabelFieldValues,
                    Base64Label = StringHelper.Exists(package.Base64Label) ? package.Base64Label : string.Empty,
                    RecipientName = updatedPackage.RecipientName,
                    FullAddress = GenerateDisplayAddress(updatedPackage)
                };
            }
            else
            {
                return new ReprintPackageResponse { LabelFieldValues = { new LabelFieldValue { Position = 0, FieldValue = "PACKAGE NOT PROCESSED" } } };
            }
        }

        public async Task<ValidatePackageResponse> ProcessValidatePackageRequest(ValidatePackageRequest request)
        {
            var package = await packageRepository.GetProcessedPackageByPackageId(request.PackageId, request.SiteName);

            if (StringHelper.DoesNotExist(package.Id))
            {
                package = await packageRepository.GetPackageByTrackingNumberAndSiteName(request.ShippingBarcode, request.SiteName);
            }

            if (StringHelper.Exists(package.Id) && package.PackageId == request.PackageId && package.Barcode == request.ShippingBarcode)
            {
                return new ValidatePackageResponse
                {
                    PackageId = package.PackageId,
                    FullAddress = GenerateDisplayAddress(package),
                    ShippingBarcode = package.Barcode,
                    BinCode = package.BinCode,
                    RecipientName = package.RecipientName,
                    IsValid = true
                };
            }
            else
            {
                return new ValidatePackageResponse();
            }
        }

        public async Task<GetPackageHistoryResponse> GetPackageEvents(GetPackageHistoryRequest request)
        {
            var response = new GetPackageHistoryResponse();
            var packages = await packageRepository.GetPackagesByPackageId(request.PackageId, request.SiteName);

            foreach (var packagesByPackageId in packages.GroupBy(x => x.PackageId))
            {
                foreach (var package in packagesByPackageId.OrderBy(y => y.CreateDate))
                {
                    foreach (var packageEvent in package.PackageEvents.OrderBy(z => z.EventId))
                    {
                        response.PackageHistoryViewItems.Add(new PackageHistoryViewItem
                        {
                            EventSource = packageEvent.MachineId,
                            EventDate = packageEvent.LocalEventDate.ToString("yyyy-MM-dd-HH:mm.ss"),
                            Description = packageEvent.Description,
                            Weight = package.Weight.ToString(),
                            PackageStatus = packageEvent.EventStatus,
                            Username = packageEvent.Username
                        });
                    }
                }
            }

            return response;
        }

        public async Task<ForceExceptionResponse> ProcessForceExceptionRequest(ForceExceptionRequest request)
        {
            var package = await packageRepository.GetImportedOrProcessedPackage(request.PackageId, request.SiteName);

            if (StringHelper.Exists(package.Id) && !package.IsCreated)
            {
                var isLocked = await eodPostProcessor.ShouldLockPackage(package);
                var errorLabelMessage = string.Empty;
                if (!isLocked)
                {
                    package.PackageStatus = EventConstants.Exception;
                    package.EodUpdateCounter += 1;
                    package.ForceExceptionOverrideLabelTypeId = package.LabelTypeId;
                    package.LabelTypeId = LabelTypeIdConstants.Error;
                    errorLabelMessage = ErrorLabelConstants.OperatorGenerated;

                    package.PackageEvents.Add(new Event
                    {
                        EventId = package.PackageEvents.Count + 1,
                        EventType = EventConstants.ForcedException,
                        EventStatus = EventConstants.Exception,
                        Description = "Package Exception Status Forced By User",
                        Username = request.Username,
                        MachineId = request.MachineId,
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                    });
                }
                else
                {
                    errorLabelMessage = ErrorLabelConstants.EndOfDayProcessed;
                    package.PackageEvents.Add(new Event
                    {
                        EventId = package.PackageEvents.Count + 1,
                        EventType = EventConstants.ForcedException,
                        EventStatus = package.PackageStatus,
                        Description = "Package locked by end of day file process",
                        Username = request.Username,
                        MachineId = request.MachineId,
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                    });
                }

                var updatedPackage = await packageRepository.UpdateItemAsync(package);

                return GenerateForceExceptionResponse(updatedPackage, errorLabelMessage);
            }
            else
            {
                logger.LogError($"Failed to force exception for packageId: {request.PackageId} Site: {request.SiteName} Username: {request.Username}");
                return new ForceExceptionResponse();
            }
        }

        private async Task ProcessPackage(Package package, PackageTimer timer, ProcessScanPackage processScan)
        {
            processScan.IsJobAssigned = await jobProcessor.GetJobDataForPackageScan(package);

            if (processScan.IsJobAssigned)
            {
                timer.ServiceWatch.Start();
                processScan.IsServiced = await serviceProcessor.GetServiceDataAsync(package);
                timer.ServiceWatch.Stop();
            }

            if (processScan.IsServiced)
            {
                if (package.ShippingMethod == ShippingMethodConstants.ReturnToCustomer)
                {
                    processScan.IsReturned = true;
                }
                else
                {
                    timer.ShippingWatch.Start();
                    processScan.IsShipped = await shippingProcessor.GetShippingDataAsync(package);
                    timer.ShippingWatch.Stop();
                }

                if (processScan.IsShipped || processScan.IsReturned)
                {
                    if (processScan.IsShipped && package.ShippingCarrier == ShippingCarrierConstants.Usps)
                    {
                        timer.ContainerWatch.Start();
                        await packageContainerProcessor.AssignPackageContainerData(package);
                        timer.ContainerWatch.Stop();
                    }

                    (package, processScan.IsLabeled) = labelProcessor.GetPackageLabelData(package);
                }
            }

            var eventDescription = packageErrorProcessor.EvaluateScannedPackageStatus(package, processScan);

            package.PackageEvents.Add(new Event
            {
                EventId = package.PackageEvents.Count + 1,
                EventType = EventConstants.ManualScan,
                EventStatus = package.PackageStatus,
                Description = eventDescription,
                TrackingNumber = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
                Username = processScan.Username,
                MachineId = processScan.MachineId,
                EventDate = DateTime.Now,
                LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
            });
        }

        private async Task<bool> IsPackageValidToProcess(Package package, decimal weight)
        {
            package.IsLocked = await eodPostProcessor.ShouldLockPackage(package);
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

        private async Task<bool> IsPackageValidToReprocess(Package package, decimal weight)
        {
            package.IsLocked = await eodPostProcessor.ShouldLockPackage(package);
            var validStatusesForRepeatScan = new List<string>
            {
                EventConstants.Exception,
                EventConstants.Processed,
                EventConstants.Released
            };

            var validToRepeat = StringHelper.Exists(package.Id)
                    && validStatusesForRepeatScan.Contains(package.PackageStatus)
                    && weight > 0
                    && !package.IsLocked;

            return validToRepeat;
        }

        private bool ShouldReplacePackage(Package package, bool isRepeatScan)
        {
            var response = false;

            if (isRepeatScan && package.PackageStatus != EventConstants.Imported)
            {
                response = true;
            }
            else if (package.PackageStatus == EventConstants.Released && package.PackageEvents.Any(x => x.EventStatus == EventConstants.Processed))
            {
                response = true;
            }
            return response;
        }

        private ScanPackageResponse GenerateScanPackageResponse(Package package, ProcessScanPackage processScan)
        {
            var base64LabelToReturn = string.Empty;

            if (package.PrintLabel && StringHelper.Exists(package.Base64Label))
            {
                base64LabelToReturn = package.Base64Label;
            }

            return new ScanPackageResponse
            {
                PackageId = package.PackageId,
                Succeeded = package.PackageStatus == EventConstants.Processed,
                PrintLabel = package.PrintLabel,
                IsQCRequired = package.IsQCRequired,
                ResponseDate = DateTime.Now,
                Weight = package.Weight.ToString(),
                Carrier = package.ShippingCarrier,
                ServiceType = package.ShippingMethod,
                Bin = package.BinCode,
                Barcode = package.Barcode,
                HumanReadableBarcode = StringHelper.Exists(package.HumanReadableBarcode) ? package.HumanReadableBarcode : string.Empty,
                RecipientName = package.RecipientName,
                FullAddress = GenerateDisplayAddress(package),
                LabelTypeId = (package.IsLocked || processScan.IsInvalidStatus) ? LabelTypeIdConstants.Error : package.LabelTypeId,
                LabelFieldValues = package.LabelFieldValues,
                Base64Label = base64LabelToReturn,
                ErrorLabelMessage = processScan.ErrorLabelMessage
            };
        }

        private ForceExceptionResponse GenerateForceExceptionResponse(Package package, string errorLabelMessage)
        {
            return new ForceExceptionResponse
            {
                PackageId = package.PackageId,
                Succeeded = package.PackageStatus == EventConstants.Exception,
                ResponseDate = DateTime.Now,
                LabelTypeId = LabelTypeIdConstants.Error,
                ErrorLabelMessage = errorLabelMessage
            };
        }

        private static decimal GetWeightFromRequest(ScanPackageRequest request)
        {
            var numericRequestWeight = Regex.Replace(request.Weight, "[^0-9.]", "");
            decimal.TryParse(numericRequestWeight, out var weight);
            return weight;
        }

        private static string GenerateDisplayAddress(Package package)
        {
            var res = $"{package.AddressLine1}";

            if (StringHelper.Exists(package.AddressLine2))
            {
                if (StringHelper.Exists(package.AddressLine3))
                {
                    res += $" {package.AddressLine2} {package.AddressLine3}";
                }

                res += $" {package.AddressLine2}";
            }

            return res += $" {package.City} {package.State} {package.Zip}";
        }
    }
}