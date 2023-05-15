using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.CreatePackage;
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
    public class CreatePackageProcessor : ICreatePackageProcessor
    {
        private readonly IAutoScanZplProcessor autoScanZplProcessor;
        private readonly IBinProcessor binProcessor;
        private readonly IClientFacilityProcessor clientFacilityProcessor;
        private readonly ICreatePackageServiceProcessor createPackageServiceProcessor;
        private readonly ILogger<CreatePackageProcessor> logger;
        private readonly IPackageContainerProcessor packageContainerProcessor;
        private readonly IPackageErrorProcessor packageErrorProcessor;
        private readonly IPackageLabelProcessor packageLabelProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly ISequenceProcessor sequenceProcessor;
        private readonly IUspsShippingProcessor uspsShippingProcessor;
        private readonly ICreatePackageZplProcessor singlePackageZplProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IZoneProcessor zoneProcessor;

        public CreatePackageProcessor(
            IAutoScanZplProcessor autoScanZplProcessor,
            IBinProcessor binProcessor,
            IClientFacilityProcessor clientFacilityProcessor,
            ICreatePackageServiceProcessor createPackageServiceProcessor,
            ILogger<CreatePackageProcessor> logger,
            IPackageContainerProcessor packageContainerProcessor,
            IPackageErrorProcessor packageErrorProcessor,
            IPackageLabelProcessor packageLabelProcessor,
            IPackageRepository packageRepository,
            ISequenceProcessor sequenceProcessor,
            IUspsShippingProcessor uspsShippingProcessor,
            ICreatePackageZplProcessor singlePackageZplProcessor,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            IZoneProcessor zoneProcessor)
        {
            this.autoScanZplProcessor = autoScanZplProcessor;
            this.binProcessor = binProcessor;
            this.clientFacilityProcessor = clientFacilityProcessor;
            this.createPackageServiceProcessor = createPackageServiceProcessor;
            this.logger = logger;
            this.packageContainerProcessor = packageContainerProcessor;
            this.packageErrorProcessor = packageErrorProcessor;
            this.packageLabelProcessor = packageLabelProcessor;
            this.packageRepository = packageRepository;
            this.sequenceProcessor = sequenceProcessor;
            this.uspsShippingProcessor = uspsShippingProcessor;
            this.singlePackageZplProcessor = singlePackageZplProcessor;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.zoneProcessor = zoneProcessor;
        }

        public async Task<(CreatePackageResponse CreatePackageResponse, PackageTimer Timer)> ProcessCreatePackageRequestAsync(CreatePackageRequest request)
        {
            try
            {
                var timer = new PackageTimer();
                timer.TotalWatch.Start();
                var errorMessage = string.Empty;
                var isSuccessful = true;
                var response = new CreatePackageResponse();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(request.SubClient);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var clientFacility = await clientFacilityProcessor.GetClientFacility(request.ClientFacility);
                var validateResponse = ValidateCreatePackageRequest(request, clientFacility, subClient);

                if (validateResponse.IsInvalid)
                {
                    logger.LogError($"CreatePackageRequest {validateResponse.ErrorMessage}");
                    return (GenerateCreatePackageResponse(new Package(), false, validateResponse.ErrorMessage), timer);
                }

                var package = GeneratePackageToCreate(request, site, subClient);

                if (validateResponse.AssignDimensions)
                {
                    AssignDimensions(package, request);
                }

                if (!package.PrintLabel)
                {
                    var generateRequest = new GenerateCreatePackageRequest
                    {
                        ClientFacility = clientFacility,
                        Timer = timer,
                        IsInitialCreate = true
                    };
                    await ProcessGenerateCreatePackage(package, generateRequest);
                    errorMessage = packageErrorProcessor.EvaluateCreatedPackageStatus(package);
                }

                if (StringHelper.Exists(package.SiteName) && StringHelper.DoesNotExist(errorMessage))
                {
                    package.PackageStatus = EventConstants.Created;
                }
                else
                {
                    package.Base64Label = singlePackageZplProcessor.GenerateErrorLabel(errorMessage, package.SiteName, package.Id, package);
                }

                package.PackageEvents.Add(new Event
                {
                    EventId = package.PackageEvents.Count + 1,
                    EventType = EventConstants.CreateSinglePackage,
                    EventStatus = package.PackageStatus,
                    TrackingNumber = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
                    Description = "Created through Single Package API",
                    Username = request.Username,
                    EventDate = DateTime.Now,
                    LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                });

                response = GenerateCreatePackageResponse(package, isSuccessful, errorMessage);
                timer.AddQueryWatch.Start();
                var createdPackage = await packageRepository.AddItemAsync(package, package.PackageId);
                timer.AddQueryWatch.Stop();
                timer.TotalWatch.Stop();
                return (response, timer);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create package. Username: {request.Username} Site: {request.SubClient} Exception: {ex}");
                return (new CreatePackageResponse(), new PackageTimer());
            }
        }

        public async Task<DeletePackageResponse> DeletePackageAsync(DeletePackageRequest request)
        {
            try
            {
                var response = new DeletePackageResponse();
                var errorMessage = string.Empty;
                var subClient = await subClientProcessor.GetSubClientByNameAsync(request.SubClient);
                var package = await packageRepository.GetCreatedPackage(request.PackageId, subClient.SiteName);

                if (StringHelper.Exists(package.Id))
                {
                    package.PackageStatus = EventConstants.Deleted;
                    package.PackageEvents.Add(new Event
                    {
                        EventId = package.PackageEvents.Count + 1,
                        EventType = EventConstants.DeleteSinglePackage,
                        EventStatus = package.PackageStatus,
                        Description = "Deleted through Single Package API",
                        Username = request.Username,
                        EventDate = DateTime.Now,
                        LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
                    });

                    var updatedPackage = await packageRepository.UpdateItemAsync(package);
                    response.IsSuccessful = true;
                }
                else
                {
                    response.Message = $"PackageId {request.PackageId} not found";
                }
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to Delete PackageId: {request.PackageId} Exception: {ex}");
                return new DeletePackageResponse();
            }
        }

        public async Task ProcessGenerateCreatePackage(Package package, GenerateCreatePackageRequest request)
        {
            var isServiced = false;
            var daysPlus = request.ClientFacility.ClientFacilityRules.Single(x => x.Name == package.SiteName).DaysPlus;
            request.Timer.SequenceWatch.Start();
            var sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(package.SiteName, SequenceTypeConstants.SinglePackage, SequenceTypeConstants.SinglePackageMaxSequence, SequenceTypeConstants.SinglePackageStartSequence);
            package.Sequence = sequence.Number;
            request.Timer.SequenceWatch.Stop();
            request.Timer.ServiceWatch.Start();
            await zoneProcessor.AssignZone(package);

            await binProcessor.AssignCreatePackageBinAsync(package, request.IsInitialCreate, daysPlus);

            if (request.IsScanPackage || request.IsAutoScan)
            {
                await packageContainerProcessor.AssignPackageContainerData(package);
            }
            var isBinned = StringHelper.Exists(package.BinCode);
            request.Timer.ServiceWatch.Stop();

            if (isBinned)
            {
                request.Timer.ServiceWatch.Start();
                isServiced = await createPackageServiceProcessor.GetCreatePackageServiceDataAsync(package);
                request.Timer.ServiceWatch.Stop();

                if (isServiced && StringHelper.Exists(sequence.SiteName))
                {
                    request.Timer.ShippingWatch.Start();

                    if (package.TrackingRuleType == CreatePackageConstants.SystemRuleTypeConstant)
                    {
                        package.Barcode = uspsShippingProcessor.GenerateUspsBarcode(package);
                    }

                    if (StringHelper.Exists(package.Barcode))
                    {
                        package.HumanReadableBarcode = uspsShippingProcessor.GenerateUspsHumanReadableBarcode(package.Barcode);
                        package.FormattedBarcode = uspsShippingProcessor.GenerateUspsFormattedBarcode(package.Barcode);

                        if (request.IsScanPackage) // get label field values for desktop
                        {
                            var labelFieldValues = new List<LabelFieldValue>();
                            packageLabelProcessor.AddUspsLabelFieldValues(package, labelFieldValues);
                            package.LabelFieldValues = labelFieldValues;
                            package.PackageStatus = EventConstants.Processed;
                        }
                        else if (request.IsAutoScan)
                        {
                            package.Base64Label = autoScanZplProcessor.GenerateUspsLabel(package);
                            package.PackageStatus = EventConstants.Processed;
                        }
                        else // initial Create
                        {
                            package.Base64Label = singlePackageZplProcessor.GenerateUspsLabel(package, package.ShippingMethod);
                            package.PackageStatus = EventConstants.Created;
                        }
                    }
                }
            }
            request.Timer.ShippingWatch.Stop();
        }

        private Package GeneratePackageToCreate(CreatePackageRequest request, Site site, SubClient subClient)
        {
            try
            {
                var localCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone);
                DateTime.TryParse(request.ShipDate, out var shipDate);
                decimal.TryParse(request.WeightInOunces, out var weightInOunces);
                var weight = WeightUtility.ConvertOuncesToPounds(weightInOunces);

                var package = new Package
                {
                    PackageId = request.PackageId,
                    MailCode = CreatePackageConstants.DefaultMailCode,
                    Weight = weight,
                    WeightInOunces = weightInOunces,
                    SiteName = site.SiteName,
                    SiteId = site.Id,
                    MailerId = subClient.UspsImpbMid,
                    UspsPermitNumber = subClient.UspsPermitNo,
                    ShippingCarrier = request.Carrier,
                    ShippingMethod = request.ShippingMethod,
                    Barcode = request.TrackingNumber,
                    BinCode = request.BinCode,
                    SiteAddressLineOne = site.AddressLineOne,
                    SiteCity = site.City,
                    SiteState = site.State,
                    SiteZip = site.Zip,
                    TimeZone = site.TimeZone,
                    SubClientName = subClient.Name,
                    SubClientKey = subClient.Key,
                    ClientName = subClient.ClientName,
                    ClientFacilityName = request.ClientFacility,
                    RecipientName = request.ToName,
                    AddressLine1 = request.ToAddressLine1,
                    AddressLine2 = request.ToAddressLine2,
                    City = request.ToCity,
                    State = request.ToState,
                    Zip = request.ToZip5,
                    FullZip = AddressUtility.GenerateFormattedFullZip(request.ToZip5, request.ToZip4),
                    ReturnName = request.FromName,
                    ReturnAddressLine1 = request.FromAddressLine1,
                    ReturnAddressLine2 = request.FromAddressLine2,
                    ReturnCity = request.FromCity,
                    ReturnState = request.FromState,
                    ReturnZip = AddressUtility.GenerateFormattedFullZip(request.FromZip5, request.FromZip4),
                    BusinessRuleType = request.BusinessRuleType,
                    TrackingRuleType = request.TrackingRuleType,
                    BinRuleType = request.BinRuleType,
                    PrintLabel = !request.GenerateShippingLabel,
                    EodUpdateCounter = 1,
                    EodProcessCounter = 0,
                    SqlEodProcessCounter = 0,
                    ToFirm = request.ToFirm,
                    FromFirm = request.FromFirm,
                    IsPoBox = request.IsPoBox,
                    IsOrmd = request.IsOrmd,
                    IsOutside48States = AddressUtility.IsNotInLower48States(request.ToState),
                    IsCreated = true,
                    CustomerReferenceNumber = request.CustomerReferenceNumber,
                    CustomerReferenceNumber2 = request.CustomerReferenceNumber2,
                    CustomerReferenceNumber3 = request.CustomerReferenceNumber3,
                    CustomerReferenceNumber4 = request.CustomerReferenceNumber4,
                    ShipDate = shipDate,
                    CreateDate = DateTime.UtcNow,
                    LocalCreateDate = localCreateDate
                };

                return package;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception on package validation: {ex}");
                return new Package();
            }
        }

        private static void AssignDimensions(Package package, CreatePackageRequest request)
        {
            decimal.TryParse(request.Length, out var length);
            decimal.TryParse(request.Width, out var width);
            decimal.TryParse(request.Height, out var height);

            package.Length = length;
            package.Width = width;
            package.Depth = height;

            package.TotalDimensions = package.Width * package.Length * package.Depth;
        }

        private static (bool IsInvalid, bool AssignDimensions, string ErrorMessage) ValidateCreatePackageRequest(CreatePackageRequest request, ClientFacility clientFacility, SubClient subClient)
        {
            var errorMessages = new List<string>();
            var invalidClientFacility = StringHelper.DoesNotExist(clientFacility.Id);
            var invalidSubClient = StringHelper.DoesNotExist(subClient.Id) || clientFacility.ClientName != subClient.ClientName;
            var invalidPackageId = StringHelper.DoesNotExist(request.PackageId) || request.PackageId.Length < 6;

            decimal.TryParse(request.WeightInOunces, out var weight);
            var invalidWeight = weight <= 0 || weight > 1120; // 1120 oz/70 lb max

            var invalidDate = false;
            if (StringHelper.Exists(clientFacility.Id))
            {
                DateTime.TryParse(request.ShipDate, out var shipDate);
                invalidDate = shipDate != null && shipDate < TimeZoneUtility.GetLocalTime(clientFacility.TimeZone).AddDays(-1);
            }

            var invalidAddress = StringHelper.DoesNotExist(request.ToName)
                || StringHelper.DoesNotExist(request.ToAddressLine1)
                || StringHelper.DoesNotExist(request.ToCity)
                || StringHelper.DoesNotExist(request.ToState)
                || StringHelper.DoesNotExist(request.ToZip5)
                || StringHelper.DoesNotExist(request.FromName)
                || StringHelper.DoesNotExist(request.FromAddressLine1)
                || StringHelper.DoesNotExist(request.FromCity)
                || StringHelper.DoesNotExist(request.FromState)
                || StringHelper.DoesNotExist(request.FromZip5)
                || request.ToZip5.Length < 5
                || request.FromZip5.Length < 5;


            var invalidBusinessRules = ValidateBusinessRuleType(request);
            var invalidTrackingRules = ValidateTrackingRuleType(request);
            var invalidBinRules = ValidateBinRuleType(request);
            var validateDimensionsResponse = ValidateDimensionRuleType(request);            

            if (invalidClientFacility)
            {
                errorMessages.Add("INVALID CLIENT FACILITY");
            }
            if (invalidSubClient)
            {
                errorMessages.Add("INVALID SUBCLIENT");
            }
            if (invalidPackageId)
            {
                errorMessages.Add("INVALID PACKAGE ID");
            }
            if (invalidAddress)
            {
                errorMessages.Add("INVALID ADDRESS");
            }
            if (invalidWeight)
            {
                errorMessages.Add("INVALID WEIGHT");
            }
            if (invalidDate)
            {
                errorMessages.Add("INVALID SHIP DATE");
            }
            if (invalidBusinessRules)
            {
                errorMessages.Add("INVALID BUSINESS RULE DATA");
            }
            if (invalidTrackingRules)
            {
                errorMessages.Add("INVALID TRACKING DATA");
            }
            if (invalidBinRules)
            {
                errorMessages.Add("INVALID BIN DATA");
            }
            if (validateDimensionsResponse.InvalidDimensionRules)
            {
                errorMessages.Add("INVALID DIMENSION DATA");
            }

            if (errorMessages.Any())
            {
                var message = StringHelper.ToStringList(errorMessages, ", ");

                return (true, validateDimensionsResponse.AssignDimensions, message);
            }
            else
            {
                return (false, validateDimensionsResponse.AssignDimensions, string.Empty); // valid request
            }
        }

        private static bool ValidateBinRuleType(CreatePackageRequest request)
        {
            var invalidBinRules = false;

            if (request.BinRuleType == CreatePackageConstants.ClientRuleTypeConstant)
            {
                invalidBinRules = StringHelper.DoesNotExist(request.BinCode);
            }
            else if (request.BinRuleType == CreatePackageConstants.SystemRuleTypeConstant)
            {
                request.BinCode = string.Empty;
            }
            else
            {
                invalidBinRules = true;
            }

            return invalidBinRules;
        }

        private static (bool InvalidDimensionRules, bool AssignDimensions) ValidateDimensionRuleType(CreatePackageRequest request)
        {
            var invalidDimensionRules = false;
            var assignDimensions = false;

            if (request.DimensionRuleType == CreatePackageConstants.ClientRuleTypeConstant)
            {
                decimal.TryParse(request.Length, out var length);
                decimal.TryParse(request.Width, out var width);
                decimal.TryParse(request.Height, out var height);

                invalidDimensionRules = length == 0 || width == 0 || height == 0;
                assignDimensions = true;
            }

            return (invalidDimensionRules, assignDimensions);
        }

        private static bool ValidateTrackingRuleType(CreatePackageRequest request)
        {
            var invalidTrackingRules = false;

            if (request.TrackingRuleType == CreatePackageConstants.ClientRuleTypeConstant)
            {
                invalidTrackingRules = StringHelper.DoesNotExist(request.TrackingNumber);
            }
            else if (request.TrackingRuleType == CreatePackageConstants.SystemRuleTypeConstant)
            {
                request.TrackingNumber = string.Empty;
            }
            else
            {
                invalidTrackingRules = true;
            }

            return invalidTrackingRules;
        }

        private static bool ValidateBusinessRuleType(CreatePackageRequest request)
        {
            var invalidBusinessRules = false;

            if (request.BusinessRuleType == CreatePackageConstants.ClientRuleTypeConstant)
            {
                if (!ShippingCarrierConstants.ValidSinglePackageCarriers.Contains(request.Carrier))
                {
                    invalidBusinessRules = true;
                }
                else if (!ShippingMethodConstants.ValidSinglePackageServiceTypes.Contains(request.ShippingMethod))
                {
                    invalidBusinessRules = true;
                }
            }
            else if (request.BusinessRuleType == CreatePackageConstants.SystemRuleTypeConstant)
            {
                request.Carrier = string.Empty;
                request.ShippingMethod = string.Empty;
            }
            else
            {
                invalidBusinessRules = true;
            }

            return invalidBusinessRules;
        }

        private CreatePackageResponse GenerateCreatePackageResponse(Package package, bool isSuccessful, string message)
        {
            return new CreatePackageResponse
            {
                PackageId = package.PackageId,
                Barcode = package.Barcode,
                Carrier = package.ShippingCarrier,
                ShippingMethod = package.ShippingMethod,
                CustomerReferenceNumber = package.CustomerReferenceNumber,
                CustomerReferenceNumber2 = package.CustomerReferenceNumber2,
                CustomerReferenceNumber3 = package.CustomerReferenceNumber3,
                CustomerReferenceNumber4 = package.CustomerReferenceNumber4,
                Base64 = package.Base64Label,
                IsSuccessful = isSuccessful,
                Message = message
            };
        }
    }
}
