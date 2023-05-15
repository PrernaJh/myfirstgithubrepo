using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Returns;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace MMS.API.Domain.Processors
{
    public class PackageLabelProcessor : IPackageLabelProcessor
    {
        private readonly IConfiguration config;
        private readonly ILogger<PackageLabelProcessor> logger;

        public PackageLabelProcessor(IConfiguration config, ILogger<PackageLabelProcessor> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public (Package, bool) GetPackageLabelData(Package package)
        {
            try
            {
                var labelFieldValues = new List<LabelFieldValue>();
                var isLabeled = false;
                var returnToSender = package.ShippingMethod == ReturnToCustomer;
                var carrier = package.ShippingCarrier;

                if (returnToSender)
                {
                    var returnLabelRequest = new ReturnLabelRequest
                    {
                        SiteName = package.SiteName,
                        PackageId = package.PackageId
                    };
                    package.LabelTypeId = LabelTypeIdConstants.ReturnToSender;
                    labelFieldValues = GetLabelFieldsForReturnToCustomer(returnLabelRequest);
                }
                else if (carrier == Usps)
                {
                    AddUspsLabelFieldValues(package, labelFieldValues);
                }
                else if (carrier == Ups)
                {
                    package.LabelTypeId = LabelTypeIdConstants.UpsShipping;
                    labelFieldValues.AddRange(new List<LabelFieldValue>
                    {
                        new LabelFieldValue
                        {
                            Position = 0,
                            FieldValue = package.ShippingMethod == UpsGround ? string.Empty : package.UpsGeoDescriptor
                        }
                    });
                }
                else if (carrier == FedEx)
                {
                    package.LabelTypeId = LabelTypeIdConstants.FedexShipping;

                    var formattedDateWeightAndDimensions = FormatFedExFields(package);

                    labelFieldValues.AddRange(new List<LabelFieldValue>
                    {
                        new LabelFieldValue
                        {
                            Position = 0,
                            FieldValue = package.AdditionalShippingData.FormattedDeliveryDate // Formatted Date
						},
                        new LabelFieldValue
                        {
                            Position = 1,
                            FieldValue = package.AdditionalShippingData.FormattedShippingMethod // Formatted shipping method
						},
                        new LabelFieldValue
                        {
                            Position = 2,
                            FieldValue = package.AdditionalShippingData.OriginId
                        },
                        new LabelFieldValue
                        {
                            Position = 3,
                            FieldValue = formattedDateWeightAndDimensions.Date
                        },
                        new LabelFieldValue
                        {
                            Position = 4,
                            FieldValue = formattedDateWeightAndDimensions.Weight
                        },
                        new LabelFieldValue
                        {
                            Position = 5,
                            FieldValue = package.AdditionalShippingData.Cad
                        },
                        new LabelFieldValue
                        {
                            Position = 6,
                            FieldValue = formattedDateWeightAndDimensions.Dimensions
                        },
                        new LabelFieldValue
                        {
                            Position = 7,
                            FieldValue = package.AdditionalShippingData.HumanReadableTrackingNumber
                        },
                        new LabelFieldValue
                        {
                            Position = 8,
                            FieldValue = package.AdditionalShippingData.FormId
                        },
                        new LabelFieldValue
                        {
                            Position = 9,
                            FieldValue = package.AdditionalShippingData.Ursa
                        },
                        new LabelFieldValue
                        {
                            Position = 10,
                            FieldValue = package.Zip
                        },
                        new LabelFieldValue
                        {
                            Position = 11,
                            FieldValue = package.AdditionalShippingData.StateAndCountryCode
                        },
                        new LabelFieldValue
                        {
                            Position = 12,
                            FieldValue = package.AdditionalShippingData.AirportId
                        },
                        new LabelFieldValue
                        {
                            Position = 13,
                            FieldValue = package.AdditionalShippingData.FormattedServiceDescriptor
                        },
                        new LabelFieldValue
                        {
                            Position = 14,
                            FieldValue = package.AdditionalShippingData.FormattedServiceDescriptorLetter
                        },
                        new LabelFieldValue
                        {
                            Position = 15,
                            FieldValue = package.AddressLine1
                        },
                        new LabelFieldValue
                        {
                            Position = 16,
                            FieldValue = package.AdditionalShippingData.OperationalSystemId
                        }
                    });
                }

                package.LabelTypeId = package.LabelTypeId;
                package.LabelFieldValues = labelFieldValues;
                isLabeled = true;

                return (package, isLabeled);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error while labeling for Package ID: {package.Id} Exception: {ex}");
                return (package, false);
            }
        }

        public void AddUspsLabelFieldValues(Package package, List<LabelFieldValue> labelFieldValues)
        {
            package.LabelTypeId = LabelTypeIdConstants.UspsPackage;
            var isNested = StringHelper.Exists(package.ContainerId);
            bool shouldPrintAddressService = ShouldPrintAddressService(package);
            labelFieldValues.AddRange(new List<LabelFieldValue>
            {
                new LabelFieldValue
                {
                    Position = 0,
                    FieldValue = FormatUspsServiceType(package.ShippingMethod)
                },
                new LabelFieldValue
                {
                    Position = 1,
                    FieldValue = package.Zip
                },
                new LabelFieldValue
                {
                    Position = 2,
                    FieldValue = isNested ? config.GetSection("NestedPackageLabelCharacter").Value : string.Empty
                },
                new LabelFieldValue
                {
                    Position = 3,
                    FieldValue = shouldPrintAddressService ? "ADDRESS SERVICE REQUESTED" : string.Empty
                },
                new LabelFieldValue
                {
                    Position = 4,
                    FieldValue = package.UspsPermitNumber
                }
            });
        }

        public List<LabelFieldValue> GetLabelFieldsForReturnToCustomer(ReturnLabelRequest request)
        {
            var response = new List<LabelFieldValue>();
            var reason = StringHelper.Exists(request.ReturnReason) ? request.ReturnReason.ToUpper() : string.Empty;
            var description = StringHelper.Exists(request.ReturnDescription) ? request.ReturnDescription.ToUpper() : string.Empty;

            response.AddRange(new List<LabelFieldValue>
                {
                    new LabelFieldValue
                    {
                        Position = 0,
                        FieldValue = "EXCEPTION TICKET"
                    },
                    new LabelFieldValue
                    {
                        Position = 1,
                        FieldValue = request.SiteName
                    },
                    new LabelFieldValue
                    {
                        Position = 2,
                        FieldValue = reason
                    },
                    new LabelFieldValue
                    {
                        Position = 3,
                        FieldValue = description
                    },
                    new LabelFieldValue
                    {
                        Position = 4,
                        FieldValue = ErrorLabelConstants.ReturnToCustomer,
                    },
                    new LabelFieldValue
                    {
                        Position = 5,
                        FieldValue = request.PackageId
                    }
                });

            return response;
        }

        public List<LabelFieldValue> GetLabelFieldsForReturnEodProcessed(string siteName, string packageId)
        {
            var response = new List<LabelFieldValue>();

            response.AddRange(new List<LabelFieldValue>
                {
                    new LabelFieldValue
                    {
                        Position = 0,
                        FieldValue = "EXCEPTION"
                    },
                    new LabelFieldValue
                    {
                        Position = 1,
                        FieldValue = siteName
                    },
                    new LabelFieldValue
                    {
                        Position = 2,
                        FieldValue = "END OF DAY PROCESSED"
                    },
                    new LabelFieldValue
                    {
                        Position = 3,
                        FieldValue = string.Empty
                    },
                    new LabelFieldValue
                    {
                        Position = 4,
                        FieldValue = string.Empty
                    },
                    new LabelFieldValue
                    {
                        Position = 5,
                        FieldValue = packageId
                    }
                });

            return response;

        }

        public void GetLabelDataForAutoScanReprint(Package package)
        {
            var isNested = StringHelper.Exists(package.ContainerId);
            var shouldPrintAddressService = ShouldPrintAddressService(package);

            package.ForceExceptionOverrideLabelTypeId = package.LabelTypeId;
            package.LabelTypeId = LabelTypeIdConstants.UspsPackage;
            package.LabelFieldValues.AddRange(new List<LabelFieldValue>
            {
                new LabelFieldValue
                {
                    Position = 0,
                    FieldValue = FormatUspsServiceType(package.ShippingMethod)
                },
                new LabelFieldValue
                {
                    Position = 1,
                    FieldValue = package.Zip
                },
                new LabelFieldValue
                {
                    Position = 2,
                    FieldValue = isNested ? string.Empty : config.GetSection("NestedPackageLabelCharacter").Value
                },
                new LabelFieldValue
                {
                    Position = 3,
                    FieldValue = shouldPrintAddressService ? "ADDRESS SERVICE REQUESTED" : string.Empty
                }
            });
        }

        public List<LabelFieldValue> GetLabelDataForSortCodeChange(string binCode, string packageId)
        {
            var response = new List<LabelFieldValue>();

            response.AddRange(new List<LabelFieldValue>
                {
                    new LabelFieldValue
                    {
                        Position = 0,
                        FieldValue = ErrorLabelConstants.SortCodeChange
                    },
                    new LabelFieldValue
                    {
                        Position = 1,
                        FieldValue = binCode
                    },
                    new LabelFieldValue
                    {
                        Position = 2,
                        FieldValue = packageId
                    }
                });

            return response;
        }

        private static bool ShouldPrintAddressService(Package package)
        {
            return package.ShippingMethod == UspsFirstClass
            || package.ShippingMethod == UspsFcz
            || package.ShippingMethod == UspsPriority
            || package.ShippingMethod == UspsParcelSelectLightWeight
            || package.ShippingMethod == UspsParcelSelect;
        }

        private string FormatUspsServiceType(string serviceTypeName)
        {
            return serviceTypeName switch
            {
                UspsParcelSelect => "PARCEL SELECT", 
                UspsParcelSelectLightWeight => "PS LIGHTWEIGHT",
                UspsFirstClass => "FIRST CLASS",
                UspsPriority => "PRIORITY",
                UspsFcz => "FIRST CLASS",
                _ => string.Empty,
            };
        }

        private (string Date, string Weight, string Dimensions) FormatFedExFields(Package package)
        {
            var localTime = TimeZoneUtility.GetLocalTime(package.TimeZone);
            var formattedDate = localTime.ToString("ddMMMyy").ToUpper();
            var formattedWeight = $"{package.Weight.ToString(".00")}LB";
            var formattedDimensions = $"{package.Length.ToString(".")}x{package.Width.ToString(".")}x{package.Depth.ToString(".")} IN";

            return (formattedDate, formattedWeight, formattedDimensions);
        }
    }
}
