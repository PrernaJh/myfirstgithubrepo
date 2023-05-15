using FedExShipApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.WebServices;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UpsShipApi;
using UpsVoidApi;
using static PackageTracker.Data.Constants.ServiceConstants;
using static PackageTracker.Data.Constants.ServiceLevelConstants;
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;
using static PackageTracker.Domain.Utilities.AddressUtility;

namespace PackageTracker.Domain
{
    public class ShippingProcessor : IShippingProcessor
    {
        private readonly ILogger<ShippingProcessor> logger;
        private readonly IConfiguration config;
        private readonly IFedExShipClient fedExShipClient;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IUspsShippingProcessor uspsShippingProcessor;
        private readonly IUpsShipClient upsShippingClient;
        private readonly IUpsVoidClient upsVoidClient;

        public ShippingProcessor(
            ILogger<ShippingProcessor> logger,
            IConfiguration config,
            IFedExShipClient fedExShipClient,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            IUspsShippingProcessor uspsShippingProcessor,
            IUpsShipClient upsShippingClient,
            IUpsVoidClient upsVoidClient
            )
        {
            this.logger = logger;
            this.config = config;
            this.fedExShipClient = fedExShipClient;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.uspsShippingProcessor = uspsShippingProcessor;
            this.upsShippingClient = upsShippingClient;
            this.upsVoidClient = upsVoidClient;
        }

        public async Task<bool> GetShippingDataAsync(Package package)
        {
            try
            {
                var subClient = await subClientProcessor.GetSubClientByNameAsync(package.SubClientName);
                var isShipped = false;
                package.AdditionalShippingData = new AdditionalShippingData();
                if (package.ShippingCarrier == Usps)
                {
                    package.Barcode = uspsShippingProcessor.GenerateUspsBarcode(package);
                    package.HumanReadableBarcode = package.Barcode.Substring(8);

                    if (StringHelper.Exists(package.Barcode))
                    {
                        isShipped = true;
                    }
                }
                else if (package.ShippingCarrier == Ups)
                {
                    var shouldAddCustomsData = await AssignUpsCustomsDataToPackage(package, subClient);
                    var upsShippingData = await GetUpsShippingDataAsync(package,
                        subClient.UpsAccountNumber, shouldAddCustomsData, false, subClient.UpsDirectDeliveryOnly);

                    package.Barcode = upsShippingData.Barcode;
                    package.Base64Label = upsShippingData.Base64Label;

                    if (StringHelper.Exists(package.Barcode) && StringHelper.Exists(package.Base64Label))
                    {
                        isShipped = true;
                    }
                }
                else if (package.ShippingCarrier == FedEx)
                {
                    var fedExShippingData = await GetFedExShippingDataAsync(package, subClient);
                    package.Barcode = fedExShippingData.Barcode;
                    package.Base64Label = fedExShippingData.Base64Label;

                    if (StringHelper.Exists(package.Barcode) && package.AdditionalShippingData != null)
                    {
                        isShipped = true;
                    }
                }

                return isShipped;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error getting shipping data for Package ID: {package.Id} Exception: {ex}");
                return false;
            }
        }

        public async Task<bool> AssignUpsCustomsDataToPackage(Package package, SubClient subClient)
        {
            var response = false;
            if (package.State == "PR")
            {
                var site = await siteProcessor.GetSiteBySiteNameAsync(package.SiteName);
                var puertoRicoCountryCode = "PR";
                var additionalData = package.AdditionalShippingData;
                additionalData.UpsShipmentCountryCode = puertoRicoCountryCode;
                additionalData.UpsShipperAttentionName = site.UpsShipperAttentionName;
                additionalData.UpsShipperPhone = site.UpsShipperPhone;
                additionalData.UpsShipmentDescription = subClient.UpsShipmentDescription;
                additionalData.UpsShipmentCurrencyCode = subClient.UpsShipmentCurrencyCode;
                additionalData.UpsShipmentMonetaryValue = subClient.UpsShipmentMonetaryValue;
                response = true;
            }
            return response;
        }

        public async Task<(string Barcode, string Base64Label)> GetUpsShippingDataAsync(Package package, string upsAccountNumber, bool addCustomsData, bool isZpl, bool isDirectDeliveryOnly)
        {
            var accessLicenseNumber = config.GetSection("UpsApis").GetSection("AccessLicenseNumber").Value;
            var username = config.GetSection("UpsApis").GetSection("Username").Value;
            var password = config.GetSection("UpsApis").GetSection("Password").Value;

            var upsSecurity = new UpsShipApi.UPSSecurity
            {
                UsernameToken = new UpsShipApi.UPSSecurityUsernameToken
                {
                    Username = username,
                    Password = password,
                },
                ServiceAccessToken = new UpsShipApi.UPSSecurityServiceAccessToken
                {
                    AccessLicenseNumber = accessLicenseNumber
                }
            };

            var shipRequest = GenerateUpsShipRequest(package, upsAccountNumber, addCustomsData, isZpl, isDirectDeliveryOnly);
            if (addCustomsData)
            {
                AssignUpsCustomsData(package, shipRequest);
            }
            logger.LogInformation(XmlUtility<ShipmentRequest>.Serialize(shipRequest));

            var upsShipResponse = new ShipmentResponse1();
            try
            {
                upsShipResponse = await upsShippingClient.ProcessShipmentAsync(upsSecurity, shipRequest);
                logger.LogInformation(XmlUtility<ShipmentResponse1>.Serialize(upsShipResponse));
            }
            catch (FaultException<UpsShipApi.ErrorDetailType[]> fe)
            {
                logger.Log(LogLevel.Error, $"Error returned from UPS Ship API processing shipment for package ID: {package.PackageId}");
                foreach (var detail in fe.Detail)
                {
                    var severity = detail.Severity ?? string.Empty;
                    var errorCode = detail.PrimaryErrorCode?.Code ?? string.Empty;
                    var errorDescription = detail.PrimaryErrorCode?.Description ?? string.Empty;

                    logger.Log(LogLevel.Error, $"Error Severity: {severity}, Code: {errorCode}, Description: {errorDescription}");
                    
                    package.CarrierApiErrors.Add(new CarrierApiError
                    {
                        Severity = severity,
                        Code = errorCode,
                        Description = errorDescription
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Exception returned from UPS Ship API processing shipment for package ID: {package.PackageId} Exception: {ex}");
            }

            var barcode = upsShipResponse?.ShipmentResponse?.ShipmentResults?.ShipmentIdentificationNumber ?? string.Empty;
            var base64Label = upsShipResponse?.ShipmentResponse?.ShipmentResults?.PackageResults?.FirstOrDefault().ShippingLabel?.GraphicImage ?? string.Empty;

            return (barcode, base64Label);
        }

        public async Task<bool> VoidUpsShipmentAsync(Package package, SubClient subClient)
        {
            var accessLicenseNumber = config.GetSection("UpsApis").GetSection("AccessLicenseNumber").Value;
            var username = config.GetSection("UpsApis").GetSection("Username").Value;
            var password = config.GetSection("UpsApis").GetSection("Password").Value;
            var upsSecurity = new UpsVoidApi.UPSSecurity
            {
                UsernameToken = new UpsVoidApi.UPSSecurityUsernameToken
                {
                    Username = username,
                    Password = password,
                },
                ServiceAccessToken = new UpsVoidApi.UPSSecurityServiceAccessToken
                {
                    AccessLicenseNumber = accessLicenseNumber
                }
            };

            var voidRequest = GenerateUpsVoidRequest(package, subClient.UpsAccountNumber);
            logger.LogInformation(XmlUtility<VoidShipmentRequest>.Serialize(voidRequest));

            var test = XmlUtility<VoidShipmentRequest>.Serialize(voidRequest);

            try
            {
                var upsVoidResponse = await upsVoidClient.ProcessVoidAsync(upsSecurity, voidRequest);
                logger.LogInformation(XmlUtility<VoidShipmentResponse1>.Serialize(upsVoidResponse));
                return true;
            }
            catch (FaultException<UpsVoidApi.ErrorDetailType[]> fe)
            {
                logger.LogError($"Error returned from UPS Void API processing void shipment request for package ID: {package.PackageId}");
                foreach (var detail in fe.Detail)
                {
                    logger.LogError($"Error Severity: {detail.Severity}, Code: {detail.PrimaryErrorCode.Code}, Description: {detail.PrimaryErrorCode.Description}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception returned from UPS Void API processing void shipment request for package ID: {package.PackageId} Exception: {ex}");
            }
            return false;
        }

        private (string Descriptor, string DescriptorLetter) MapShippingMethodToLabelServiceDescriptors(string shippingMethod)
        {
            var response = (string.Empty, string.Empty);

            if (shippingMethod == ShippingMethodConstants.FedExPriorityOvernight)
            {
                response = ("Express", "E");
            }
            else if (shippingMethod == ShippingMethodConstants.FedExGround)
            {
                response = ("Ground", "G");
            }

            return response;
        }

        private static VoidShipmentRequest GenerateUpsVoidRequest(Package package, string upsAccountNumber)
        {
            var voidRequest = new VoidShipmentRequest
            {
                Request = new UpsVoidApi.RequestType
                {
                },
                VoidShipment = new VoidShipmentRequestVoidShipment
                {
                    ShipmentIdentificationNumber = package.Barcode
                }
            };
            return voidRequest;
        }

        private ShipmentRequest GenerateUpsShipRequest(Package package, string upsAccountNumber, bool addCustomsData, bool isZpl, bool isDirectDeliveryOnly)
        {
            var serviceTypeCode = GenerateUpsServiceTypeCode(package.ShippingMethod);
            var referenceNumber = PackageIdUtility.GenerateReferenceCode(package);

            var shipmentRequest = new ShipmentRequest
            {
                Request = new UpsShipApi.RequestType
                {
                    RequestOption = new string[1] { "nonvalidate" }
                },
                Shipment = new ShipmentType
                {
                    Shipper = new ShipperType
                    {
                        Name = "FSC",
                        ShipperNumber = upsAccountNumber,
                        Address = new ShipAddressType
                        {
                            AddressLine = GenerateAddressArray(package.SiteAddressLineOne),
                            City = package.SiteCity,
                            StateProvinceCode = package.SiteState,
                            PostalCode = package.SiteZip,
                            CountryCode = "US"
                        }
                    },
                    ShipTo = new ShipToType
                    {
                        Name = StringHelper.Exists(package.RecipientName) ? package.RecipientName : "VA Recipient",
                        Phone = new ShipPhoneType { Number = RegexUtility.SanitizePhoneNumber(package.Phone) },
                        Address = new ShipToAddressType
                        {
                            AddressLine = GenerateAddressArray(package.AddressLine1, package.AddressLine2, package.AddressLine3),
                            City = package.City,
                            StateProvinceCode = package.State,
                            PostalCode = package.Zip,
                            CountryCode = "US"
                        }
                    },
                    ShipFrom = new ShipFromType
                    {
                        Name = "FSC",
                        Address = new ShipAddressType
                        {
                            AddressLine = GenerateAddressArray(package.SiteAddressLineOne),
                            City = package.SiteCity,
                            StateProvinceCode = package.SiteState,
                            PostalCode = package.SiteZip,
                            CountryCode = "US"
                        }
                    },
                    PaymentInformation = new PaymentInfoType
                    {
                        ShipmentCharge = new ShipmentChargeType[1] { new ShipmentChargeType
                        {
                            Type = "01", // Transportation
                            BillShipper = new BillShipperType
                            {
                                AccountNumber = upsAccountNumber
                            }
                        } }
                    },
                    Service = new UpsShipApi.ServiceType
                    {
                        Code = serviceTypeCode,
                        Description = "Description"
                    },
                    ShipmentServiceOptions = new ShipmentTypeShipmentServiceOptions
                    {
                    },
                    SubClassification = !string.IsNullOrWhiteSpace(package.Shape) ? (package.Shape == ServiceConstants.Irregular ? ServiceConstants.UpsIrregularRequestCode : ServiceConstants.UpsMachinableRequestCode) : string.Empty,
                    Package = new PackageType[1] { new PackageType
                    {
                        Description = "Description",
                        ReferenceNumber = new ReferenceNumberType [1]
                        {
                            new ReferenceNumberType
                            {
                                Value = referenceNumber,
                            }
                        },
                        Packaging = new UpsShipApi.PackagingType
                        {
                            Code = "02" // Customer Supplied Package
                        },
                        PackageWeight = new PackageWeightType
                        {
                            UnitOfMeasurement = new ShipUnitOfMeasurementType
                            {
                                Code = "LBS"
                            },
                            Weight = package.Weight.ToString()
                        },
                        Dimensions = new DimensionsType
                        {
                            UnitOfMeasurement = new ShipUnitOfMeasurementType
                            {
                                Code = "IN"
                            },
                            Length = package.Length != 0 ? package.Length.ToString() : "1",
                            Width = package.Width != 0 ? package.Width.ToString() : "1",
                            Height = package.Depth != 0 ? package.Depth.ToString() : "1"
                        },
                        PackageServiceOptions = new PackageServiceOptionsType
                        {
                        }
                    }
                }
                },
                LabelSpecification = new LabelSpecificationType
                {
                    LabelImageFormat = new LabelImageFormatType
                    {
                        Code = isZpl ? "ZPL" : "GIF",
                        Description = isZpl ? "ZPL" : "GIF"
                    },
                    LabelStockSize = new LabelStockSizeType
                    {
                        Height = isZpl ? "6" : "4",
                        Width = isZpl ? "4" : "6"
                    }
                }
            };

            if (package.ShippingMethod == UpsNextDayAir)
            {
                //shipmentRequest.Shipment.Package[0].PackageServiceOptions.ProactiveIndicator = "true"; // no longer needed.
                if (package.IsSaturday)
                {
                    shipmentRequest.Shipment.ShipmentServiceOptions.SaturdayDeliveryIndicator = "true";
                }
            }

            if (package.ServiceLevel != Signature)
            {
                shipmentRequest.Shipment.Package[0].PackageServiceOptions.ShipperReleaseIndicator = "true";
            }
            else
            {
                shipmentRequest.Shipment.Package[0].PackageServiceOptions.DeliveryConfirmation = new DeliveryConfirmationType
                {
                    DCISType = UpsSignatureRequestCode
                };
            }

            if (isDirectDeliveryOnly)
            {
                shipmentRequest.Shipment.ShipmentServiceOptions.DirectDeliveryOnlyIndicator = "true";
            }
            return shipmentRequest;
        }

        private static void AssignUpsCustomsData(Package package, ShipmentRequest shipmentRequest)
        {
            if (package.AdditionalShippingData != null)
            {
                shipmentRequest.Shipment.Shipper.AttentionName = package.AdditionalShippingData.UpsShipperAttentionName;
                shipmentRequest.Shipment.Shipper.Phone = new ShipPhoneType
                {
                    Number = package.AdditionalShippingData.UpsShipperPhone
                };
                shipmentRequest.Shipment.ShipTo.Address.CountryCode = package.AdditionalShippingData.UpsShipmentCountryCode;
                shipmentRequest.Shipment.ShipTo.AttentionName = package.RecipientName;
                shipmentRequest.Shipment.ShipFrom.AttentionName = package.ReturnName;
                shipmentRequest.Shipment.ShipFrom.Phone = new ShipPhoneType
                {
                    Number = RegexUtility.SanitizePhoneNumber(package.ReturnPhone)
                };
                shipmentRequest.Shipment.Description = package.AdditionalShippingData.UpsShipmentDescription;
                shipmentRequest.Shipment.InvoiceLineTotal = new CurrencyMonetaryType
                {
                    CurrencyCode = package.AdditionalShippingData.UpsShipmentCurrencyCode,
                    MonetaryValue = package.AdditionalShippingData.UpsShipmentMonetaryValue
                };
            }
        }

        private bool AssignFedexCustomsDataToPackage(Package package, SubClient subClient)
        {
            var response = false;

            if (package.State == "PR")
            {
                var additionalData = package.AdditionalShippingData;
                additionalData.FedexShipperAccountNumber = subClient.FedExCredentials.AccountNumber;
                additionalData.FedexShipperCountryCode = "US";
                additionalData.FedexShipmentDescription = subClient.FedexShipmentDescription;
                additionalData.FedexShipmentCurrencyCode = subClient.FedexShipmentCurrencyCode;
                additionalData.FedexShipmentMonetaryValue = subClient.FedexShipmentMonetaryValue;
                response = true;
            }
            return response;
        }

        private async Task<(string Barcode, string Base64Label)> GetFedExShippingDataAsync(Package package, SubClient subClient)
        {
            var request = GenerateFedExShipRequest(package, subClient);
            var response = new processShipmentResponse();
            try
            {
                response = await fedExShipClient.processShipmentAsync(request);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Exception returned from FedEx API processing shipment for package ID: {package.PackageId} Exception: {ex}");
                throw;
            }
            // Don't log plaintext ApiKey and ApiPassword:
            request.WebAuthenticationDetail.UserCredential.Key = subClient.FedExCredentials.ApiKey;           // Encrypted value
            request.WebAuthenticationDetail.UserCredential.Password = subClient.FedExCredentials.ApiPassword; // Encrypted value

            logger.LogInformation($"Fedex API Request: Package: {package.PackageId}\n{XmlUtility<ProcessShipmentRequest>.Serialize(request)}");
            logger.LogInformation($"Fedex API Request: Package: {package.PackageId}\n{XmlUtility<processShipmentResponse>.Serialize(response)}");

            if (response.ProcessShipmentReply?.Notifications != null)
            {
                var firstError = string.Empty;
                foreach (var notification in response.ProcessShipmentReply.Notifications)
                {
                    if (notification.Severity == NotificationSeverityType.FAILURE || notification.Severity == NotificationSeverityType.ERROR)
                    {                               
                        package.CarrierApiErrors.Add(new CarrierApiError
                        {
                            
                            Severity = Enum.GetName(typeof(NotificationSeverityType), notification.Severity) ?? string.Empty,
                            Code = notification.Code ?? string.Empty,
                            Description = notification.Message ?? string.Empty
                        });
                    }
                    
                    string message = $"FedEx API: Notification: Severity: {notification.Severity}: Message: {notification.Message}";
                    switch (notification.Severity)
                    {
                        case NotificationSeverityType.ERROR:
                        case NotificationSeverityType.FAILURE:
                            if (firstError == string.Empty)
                                firstError = message;
                            logger.LogError($"Package: {package.PackageId}: {message}");
                            break;
                        case NotificationSeverityType.WARNING:
                            logger.LogWarning($"Package: {package.PackageId}: {message}");
                            break;
                        default:
                            logger.LogInformation($"Package: {package.PackageId}: {message}");
                            break;
                    }
                    if (firstError != string.Empty)
                    {
                        throw new ArgumentException(firstError);
                    }
                }
            }

            var barcode = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault()?.OperationalDetail?.Barcodes?.StringBarcodes?.FirstOrDefault()?.Value ?? string.Empty;
            var base64Label = Convert.ToBase64String(response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault()?.Label?.Parts?.FirstOrDefault()?.Image) ?? string.Empty;
            var descriptors = MapShippingMethodToLabelServiceDescriptors(package.ShippingMethod);

            var additionalData = package.AdditionalShippingData;
            additionalData.TrackingNumber = response.ProcessShipmentReply?.CompletedShipmentDetail?.MasterTrackingId?.TrackingNumber ?? string.Empty;
            additionalData.OriginId = response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.OriginLocationId ?? string.Empty;
            additionalData.Cad = $"{subClient.FedExCredentials.MeterNumber}{@"/"}{config.GetSection("FedExCadSuffixV17").Value}";
            additionalData.Ursa = $"{response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.UrsaPrefixCode ?? string.Empty} {response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.UrsaSuffixCode ?? string.Empty}";
            additionalData.FormId = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails.FirstOrDefault().TrackingIds?.FirstOrDefault().FormId ?? string.Empty;
            additionalData.AirportId = response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.AirportId ?? string.Empty;
            additionalData.StateAndCountryCode = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 16)?.Content ?? string.Empty;
            additionalData.FormattedDeliveryDate = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 12)?.Content ?? string.Empty;
            additionalData.FormattedShippingMethod = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 13)?.Content ?? string.Empty;
            additionalData.FormattedServiceDescriptor = descriptors.Descriptor;
            additionalData.FormattedServiceDescriptorLetter = descriptors.DescriptorLetter;
            additionalData.AstraPlannedServiceLevel = response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.AstraPlannedServiceLevel ?? string.Empty;
            additionalData.HumanReadableTrackingNumber = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 10)?.Content ?? string.Empty;
            additionalData.OperationalSystemId = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 8)?.Content ?? string.Empty;
            return (barcode, base64Label);
        }

        private ProcessShipmentRequest GenerateFedExShipRequest(Package package, SubClient subClient)
        {
            var cryptoKey = config.GetSection("FedExApis:Crypto").Get<AESKey>();
            var key = CryptoUtility.Decrypt(cryptoKey, subClient.FedExCredentials.ApiKey);
            var password = CryptoUtility.Decrypt(cryptoKey, subClient.FedExCredentials.ApiPassword);
            var assignFedexCustomsDataToPackage = AssignFedexCustomsDataToPackage(package, subClient);
            var referenceNumber = PackageIdUtility.GenerateReferenceCode(package);

            var shipmentRequest = new ProcessShipmentRequest
            {
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential
                    {
                        Key = key,
                        Password = password
                    }
                },
                ClientDetail = new ClientDetail
                {
                    AccountNumber = subClient.FedExCredentials.AccountNumber,
                    MeterNumber = subClient.FedExCredentials.MeterNumber
                },
                Version = new VersionId
                {
                    ServiceId = "ship",
                    Major = 17,
                    Intermediate = 0,
                    Minor = 0
                },
                RequestedShipment = new RequestedShipment
                {
                    ShipTimestamp = TimeZoneUtility.GetLocalTime(package.TimeZone).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    DropoffType = DropoffType.REGULAR_PICKUP,
                    ServiceType = MapShippingMethodToFedExServiceType(package.ShippingMethod, assignFedexCustomsDataToPackage),
                    PackagingType = FedExShipApi.PackagingType.YOUR_PACKAGING,
                    Shipper = new Party
                    {
                        Contact = new Contact
                        {
                            CompanyName = "FSC",
                            PhoneNumber = "9999999999"
                        },
                        Address = new Address
                        {
                            StreetLines = GenerateAddressArray(package.SiteAddressLineOne),
                            City = package.SiteCity,
                            StateOrProvinceCode = package.SiteState,
                            PostalCode = package.SiteZip,
                            CountryCode = "US"
                        }
                    },
                    Recipient = new Party
                    {
                        Contact = new Contact
                        {
                            PersonName = package.RecipientName,
                            PhoneNumber = RegexUtility.SanitizePhoneNumber(package.Phone)
                        },
                        Address = new Address
                        {
                            StreetLines = StringHelper.Exists(package.AddressLine3)
                                ? GenerateAddressArray(package.AddressLine1, package.AddressLine2 + ", " + package.AddressLine3)
                                : GenerateAddressArray(package.AddressLine1, package.AddressLine2),
                            City = package.City,
                            StateOrProvinceCode = package.State,
                            PostalCode = package.Zip,
                            CountryCode = package.State == "PR" ? "PR" : "US"
                        }
                    },
                    ShippingChargesPayment = new Payment
                    {
                        PaymentType = FedExShipApi.PaymentType.SENDER,
                        Payor = new Payor
                        {
                            ResponsibleParty = new Party
                            {
                                AccountNumber = subClient.FedExCredentials.AccountNumber
                            }
                        }
                    },
                    LabelSpecification = new LabelSpecification
                    {
                        LabelFormatType = LabelFormatType.COMMON2D,
                        ImageType = ShippingDocumentImageType.ZPLII,
                        ImageTypeSpecified = true,
                        LabelStockType = LabelStockType.STOCK_4X6,
                        LabelStockTypeSpecified = true
                    },
                    PackageCount = "1",
                    RequestedPackageLineItems = new RequestedPackageLineItem[1]
                    {
                        new RequestedPackageLineItem
                        {
                            SequenceNumber = "1",
                            Weight = new Weight
                            {
                                Units = WeightUnits.LB,
                                Value = package.Weight
                            },
                            CustomerReferences = new CustomerReference[1]
                            {
                                new CustomerReference
                                {
                                    CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE,
                                    Value = referenceNumber
                                }
                            }                            
                            /*
                            ,
                            Dimensions = new Dimensions // TODO: how to pass in half inches?
                            {
                                Length = package.Length,
                                Width = package.Width,
                                Height = package.Depth,
                                Units = "IN"
                            }
							*/
                        }
                    }
                }
            };

            if (assignFedexCustomsDataToPackage)
            {
                AssignFedexCustomsData(package, shipmentRequest);
            }

            var parentKey = config.GetSection("FedExApis:ApiKey").Value;
            var parentPassword = config.GetSection("FedExApis:ApiPassword").Value;
            if (StringHelper.Exists(parentKey) && StringHelper.Exists(parentPassword))
            {
                shipmentRequest.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential
                {
                    Key = parentKey,
                    Password = parentPassword
                };
            }
            return shipmentRequest;
        }

        private static void AssignFedexCustomsData(Package package, ProcessShipmentRequest shipmentRequest)
        {
            if (package.AdditionalShippingData != null)
            {
                var customsClearanceDetail = new CustomsClearanceDetail();
                shipmentRequest.RequestedShipment.CustomsClearanceDetail = customsClearanceDetail;
                customsClearanceDetail.DutiesPayment = new Payment();
                customsClearanceDetail.DutiesPayment.PaymentType = FedExShipApi.PaymentType.SENDER;
                customsClearanceDetail.DutiesPayment.Payor = new Payor();
                customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty = new Party();
                customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.AccountNumber = package.AdditionalShippingData.FedexShipperAccountNumber;
                customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address = new Address();
                customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address.CountryCode = package.AdditionalShippingData.FedexShipperCountryCode;

                customsClearanceDetail.DocumentContent = InternationalDocumentContentType.NON_DOCUMENTS;

                customsClearanceDetail.CustomsValue = new Money();
                customsClearanceDetail.CustomsValue.Currency = package.AdditionalShippingData.FedexShipmentCurrencyCode;
                decimal.TryParse(package.AdditionalShippingData.FedexShipmentMonetaryValue, out var amount);
                customsClearanceDetail.CustomsValue.Amount = amount;

                customsClearanceDetail.Commodities = new Commodity[1] { new Commodity() };
                customsClearanceDetail.Commodities[0].Name = package.AdditionalShippingData.FedexShipmentDescription;
                customsClearanceDetail.Commodities[0].Description = package.AdditionalShippingData.FedexShipmentDescription;
                customsClearanceDetail.Commodities[0].NumberOfPieces = "1";
                customsClearanceDetail.Commodities[0].CountryOfManufacture = package.AdditionalShippingData.FedexShipperCountryCode;
                customsClearanceDetail.Commodities[0].Weight = new Weight();
                customsClearanceDetail.Commodities[0].Weight.Units = WeightUnits.LB;
                customsClearanceDetail.Commodities[0].Weight.Value = package.Weight;
                customsClearanceDetail.Commodities[0].Quantity = 1;
                customsClearanceDetail.Commodities[0].QuantitySpecified = true;
                customsClearanceDetail.Commodities[0].QuantityUnits = "EA";
                customsClearanceDetail.Commodities[0].UnitPrice = new Money();
                customsClearanceDetail.Commodities[0].UnitPrice.Currency = package.AdditionalShippingData.FedexShipmentCurrencyCode;
                customsClearanceDetail.Commodities[0].UnitPrice.Amount = amount;
            }
        }

        private static FedExShipApi.ServiceType MapShippingMethodToFedExServiceType(string shippingMethod, bool assignFedexCustomsDataToPackage)
        {
            if (shippingMethod == ShippingMethodConstants.FedExPriorityOvernight)
            {
                return assignFedexCustomsDataToPackage
                                ? FedExShipApi.ServiceType.INTERNATIONAL_PRIORITY
                                : FedExShipApi.ServiceType.PRIORITY_OVERNIGHT;
            }
            else if (shippingMethod == ShippingMethodConstants.FedExGround)
            {
                return assignFedexCustomsDataToPackage
                                ? FedExShipApi.ServiceType.INTERNATIONAL_ECONOMY
                                : FedExShipApi.ServiceType.FEDEX_GROUND;
            }
            else
            {
                throw new Exception();
            }
        }

        private string GenerateUpsServiceTypeCode(string shippingMethod)
        {
            var response = string.Empty;

            if (shippingMethod == UpsGround || shippingMethod == "UPSGROUND")
            {
                response = UpsGroundRequestCode;
            }
            else if (shippingMethod == UpsNextDayAir)
            {
                response = UpsNextDayAirRequestCode;
            }
            else if (shippingMethod == UpsNextDayAirSaver)
            {
                response = UpsNextDayAirSaverRequestCode;
            }
            else if (shippingMethod == UpsSecondDayAir)
            {
                response = UpsSecondDayAirRequestCode;
            }
            logger.LogInformation($"UPS SERVICE TYPE CODE: {response}");
            return response;
        }
    }
}