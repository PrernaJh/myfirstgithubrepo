using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.CreatePackage;
using MMS.API.Domain.Models.ProcessScanAndAuto;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class CreatePackageScanProcessor : ICreatePackageScanProcessor
    {
        private readonly IBinProcessor binProcessor;
        private readonly IClientFacilityProcessor clientFacilityProcessor;
        private readonly IJobScanProcessor jobProcessor;
        private readonly IPackageContainerProcessor packageContainerProcessor;
        private readonly IPackageErrorProcessor packageErrorProcessor;
        private readonly ICreatePackageProcessor createPackageProcessor;

        public CreatePackageScanProcessor(
            IBinProcessor binProcessor,
            IClientFacilityProcessor clientFacilityProcessor,
            IJobScanProcessor jobProcessor,
            IPackageContainerProcessor packageContainerProcessor,
            IPackageErrorProcessor packageErrorProcessor,
            ICreatePackageProcessor createPackageProcessor)
        {
            this.binProcessor = binProcessor;
            this.clientFacilityProcessor = clientFacilityProcessor;
            this.jobProcessor = jobProcessor;
            this.packageContainerProcessor = packageContainerProcessor;
            this.packageErrorProcessor = packageErrorProcessor;
            this.createPackageProcessor = createPackageProcessor;
        }

        public async Task ProcessCreatedPackage(Package package, ProcessScanPackage processScan)
        {
            package.JobBarcode = processScan.JobId;
            processScan.IsJobAssigned = await jobProcessor.GetJobDataForCreatePackageScan(package);

            if (processScan.IsJobAssigned)
            {
                if (package.PrintLabel)
                {
                    var clientFacility = await clientFacilityProcessor.GetClientFacility(package.ClientFacilityName);
                    var generateCreatePackageRequest = new GenerateCreatePackageRequest
                    {
                        ClientFacility = clientFacility,
                        IsScanPackage = true,
                        Timer = processScan.Timer
                    };

                    await createPackageProcessor.ProcessGenerateCreatePackage(package, generateCreatePackageRequest);
                    processScan.Timer = generateCreatePackageRequest.Timer;
                    processScan.ErrorLabelMessage = packageErrorProcessor.EvaluateCreatedPackageStatus(package);

                    if (StringHelper.DoesNotExist(processScan.ErrorLabelMessage))
                    {
                        processScan.ProcessingCompleted = true;
                    }
                }
                else // shipping label was generated in advance
                {
                    package.HistoricalBase64Labels.Add(package.Base64Label);
                    package.Base64Label = string.Empty;
                    var isBinVerified = await binProcessor.VerifyCreatedPackageBinOnScan(package); // only verify bin if we are not printing a shipping label

                    if (!isBinVerified) // package is still processed on bin verify fail, print special label
                    {
                        package.PrintLabel = true;
                        package.LabelTypeId = LabelTypeIdConstants.SortCodeChange;
                        processScan.Timer.BinValidationTimer.Start();
                        packageErrorProcessor.GenerateBinValidationLabel(package);
                        processScan.Timer.BinValidationTimer.Stop();
                    }

                    processScan.Timer.ContainerWatch.Start();
                    await packageContainerProcessor.AssignPackageContainerData(package);
                    processScan.Timer.ContainerWatch.Stop();

                    package.PackageStatus = EventConstants.Processed;
                    processScan.ProcessingCompleted = true;
                }

                package.ProcessedDate = DateTime.Now;
                package.LocalProcessedDate = TimeZoneUtility.GetLocalTime(package.TimeZone);
                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.ManualScan,
                    EventStatus = package.PackageStatus,
                    TrackingNumber = TrackingNumberUtility.GetTrackingNumber(package),
                    Description = "Scanned by user",
                    Username = processScan.Username,
                    MachineId = processScan.MachineId,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });
            }
            else
            {
                processScan.ErrorLabelMessage = ErrorLabelConstants.InvalidJob;
                package.PackageStatus = EventConstants.Exception;
            }

            processScan.IsCreatedPackage = true;
            processScan.ShouldUpdate = true;
        }
    }
}
