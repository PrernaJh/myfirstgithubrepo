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
using System.Threading.Tasks;
using static PackageTracker.Domain.Utilities.AddressUtility;
using MMS.API.Domain.Interfaces;
using PackageTracker.Domain.Models;

namespace MMS.API.Domain.Processors
{
	public class ContainerShippingProcessor : IContainerShippingProcessor
	{
		private readonly IConfiguration config;
		private readonly IFedExShipClient fedExShipClient;
		private readonly ILogger<ContainerShippingProcessor> logger;
		private readonly IPackagePostProcessor packagePostProcessor;
		private readonly ISiteProcessor siteProcessor;

		public ContainerShippingProcessor(
			IConfiguration config,
			IFedExShipClient fedExShipClient,
			ILogger<ContainerShippingProcessor> logger,
			IPackagePostProcessor packagePostProcessor,
			ISiteProcessor siteProcessor)
		{
			this.config = config;
			this.fedExShipClient = fedExShipClient;
			this.logger = logger;
			this.packagePostProcessor = packagePostProcessor;
			this.siteProcessor = siteProcessor;
		}

		public async Task<FedExShippingDataResponse> GetFedexContainerShippingData(ShippingContainer container)
		{
			var fedExShippingData = new FedExShippingDataResponse();
			try
			{
				var site = await siteProcessor.GetSiteBySiteNameAsync(container.SiteName);
				fedExShippingData = await GetFedExShippingDataAsync(container, site);
				container.CarrierBarcode = fedExShippingData.Barcode;
				container.Base64Label = fedExShippingData.Base64Label;
			}
			catch (Exception ex)
			{				
				throw ex;
			}

			return fedExShippingData;
		}

		private bool AssignFedexCustomsDataToContainer(ShippingContainer container, Site site)
		{
			var response = false;
			var parseCsz = ParseCityStateZip(container.DropShipSiteCsz);
			if (parseCsz.State == "PR")
			{
				var additionalData = container.AdditionalShippingData;
				additionalData.FedexShipperAccountNumber = site.FedExCredentials.AccountNumber;
				additionalData.FedexShipperCountryCode = "US";
				additionalData.FedexShipmentDescription = site.FedexShipmentDescription;
				additionalData.FedexShipmentCurrencyCode = site.FedexShipmentCurrencyCode;
				additionalData.FedexShipmentMonetaryValue = site.FedexShipmentMonetaryValue;
				response = true;
			}
			return response;
		}
		
		private async Task<FedExShippingDataResponse> GetFedExShippingDataAsync(ShippingContainer container, Site site)
		{
			var fedExShippingData = new FedExShippingDataResponse();
			var request = await GenerateFedExShipRequestAsync(container, site);
			var response = new processShipmentResponse();
			try
			{				
				response = await fedExShipClient.processShipmentAsync(request);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception returned from FedEx API processing shipment for containerId ID: {container.ContainerId} Exception: {ex}");
				throw ex;
			}
			// Don't log plaintext ApiKey and ApiPassword:
			request.WebAuthenticationDetail.UserCredential.Key = site.FedExCredentials.ApiKey;           // Encrypted value
			request.WebAuthenticationDetail.UserCredential.Password = site.FedExCredentials.ApiPassword; // Encrypted value
			logger.LogInformation($"Fedex API Request: Container: {container.ContainerId}\n{XmlUtility<ProcessShipmentRequest>.Serialize(request)}");
			logger.LogInformation($"Fedex API Response: Container: {container.ContainerId}\n{XmlUtility<processShipmentResponse>.Serialize(response)}");

			if (response.ProcessShipmentReply?.Notifications != null)
			{
				var firstError = string.Empty;
				foreach (var notification in response.ProcessShipmentReply.Notifications)
				{
                    if (notification.Severity == NotificationSeverityType.FAILURE || notification.Severity == NotificationSeverityType.ERROR)
                    {
                        container.CarrierApiErrors.Add(new CarrierApiError
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
							logger.LogError($"Container: {container.ContainerId}: {message}");
							break;
						case NotificationSeverityType.WARNING:
							logger.LogWarning($"Container: {container.ContainerId}: {message}");
							break;
						default:
							logger.LogInformation($"Container: {container.ContainerId}: {message}");
							break;
					}

                    if (firstError != string.Empty)
                    {
						fedExShippingData.Message = firstError;
						fedExShippingData.Successful = false;
						return fedExShippingData;						
                    }
                }
			}

			var barcode = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 7)?.Content ?? string.Empty;
			var base64Label = Convert.ToBase64String(response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault()?.Label?.Parts?.FirstOrDefault()?.Image) ?? string.Empty;
			

			var descriptors = MapShippingMethodToLabelServiceDescriptors(container.ShippingMethod);

			var additionalData = container.AdditionalShippingData;
			additionalData.TrackingNumber = response.ProcessShipmentReply?.CompletedShipmentDetail?.MasterTrackingId?.TrackingNumber ?? string.Empty;
			additionalData.OriginId = response.ProcessShipmentReply?.CompletedShipmentDetail?.OperationalDetail?.OriginLocationId ?? string.Empty;
			additionalData.Cad = $"{site.FedExCredentials.MeterNumber}{@"/"}{config.GetSection("FedExCadSuffixV17").Value}";
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
			additionalData.HumanReadableSecondaryBarcode = response.ProcessShipmentReply?.CompletedShipmentDetail?.CompletedPackageDetails?.FirstOrDefault().OperationalDetail?.OperationalInstructions?.FirstOrDefault(x => x.Number == 18)?.Content ?? string.Empty;

			fedExShippingData.Barcode = barcode;
			fedExShippingData.Base64Label = base64Label;
			fedExShippingData.Successful = true;

			return fedExShippingData;
		}

		private async Task<ProcessShipmentRequest> GenerateFedExShipRequestAsync(ShippingContainer container, Site site)
		{
			var cryptoKey = config.GetSection("FedExApis:Crypto").Get<AESKey>();
			var key = CryptoUtility.Decrypt(cryptoKey, site.FedExCredentials.ApiKey);
			var password = CryptoUtility.Decrypt(cryptoKey, site.FedExCredentials.ApiPassword);
			decimal.TryParse(container.Weight, out var containerWeight);
			var parseCsz = ParseCityStateZip(container.DropShipSiteCsz);
			var assignFedexCustomsDataToContainer = AssignFedexCustomsDataToContainer(container, site);

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
					AccountNumber = site.FedExCredentials.AccountNumber,
					MeterNumber = site.FedExCredentials.MeterNumber
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
					ShipTimestamp = TimeZoneUtility.GetLocalTime(site.TimeZone).ToString("yyyy-MM-ddTHH:mm:sszzz"),
					DropoffType = DropoffType.REGULAR_PICKUP,
					ServiceType = MapContainerToFedExServiceType(container, assignFedexCustomsDataToContainer),
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
							StreetLines = GenerateAddressArray(site.AddressLineOne, site.AddressLineTwo), // site
							City = site.City,
							StateOrProvinceCode = site.State,
							PostalCode = site.Zip,
							CountryCode = "US"
						}
					},
					Recipient = new Party
					{
						Contact = new Contact
						{
							PersonName = container.DropShipSiteDescription,
							PhoneNumber = "6082222222"
						},
						Address = new Address
						{
							StreetLines = GenerateAddressArray(container.DropShipSiteAddress), // dropship
							City = parseCsz.City,
							StateOrProvinceCode = parseCsz.State,
							PostalCode = parseCsz.FullZip,
							CountryCode = parseCsz.State == "PR" ? "PR" : "US"
						}
					},
					ShippingChargesPayment = new Payment
					{
						PaymentType = FedExShipApi.PaymentType.SENDER,
						Payor = new Payor
						{
							ResponsibleParty = new Party
							{
								AccountNumber = site.FedExCredentials.AccountNumber
							}
						}
					},
					LabelSpecification = new LabelSpecification
					{						
						LabelFormatType = LabelFormatType.COMMON2D,
						ImageType = ShippingDocumentImageType.ZPLII,
						ImageTypeSpecified = true,
						LabelStockType = LabelStockType.STOCK_4X6,
						LabelStockTypeSpecified = true,
						CustomerSpecifiedDetail = new CustomerSpecifiedLabelDetail 
						{
							SecondaryBarcodeSpecified = true,
							SecondaryBarcode = SecondaryBarcodeType.NONE
						}
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
								Value = containerWeight
							}
                            //, // TODO: how to pass in half inches?
                            //Dimensions = new Dimensions
                            //{
                            //    Length = package.Length,
                            //    Width = package.Width,
                            //    Height = package.Depth,
                            //    Units = "IN"
                            //}
                        }
					}
				}
			};

			if (container.IsSaturdayDelivery && container.ShippingMethod == ContainerConstants.FedExExpressPriority) //  container.ShippingMethod must always be PRIORITY_OVERNIGHT if IsSaturdayDelivery = true
			{
				shipmentRequest.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested();
				shipmentRequest.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes =
					new ShipmentSpecialServiceType[1]
					{
							ShipmentSpecialServiceType.SATURDAY_DELIVERY
					};
			}

			if (assignFedexCustomsDataToContainer)
            {
				int packageCount = await packagePostProcessor.CountPackagesForContainerAsync(container.ContainerId, container.SiteName);
				AssignFedexCustomsData(container, Math.Max(1, packageCount), shipmentRequest);
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

		private static void AssignFedexCustomsData(ShippingContainer container, int packageCount, ProcessShipmentRequest shipmentRequest)
		{
			if (container.AdditionalShippingData != null)
			{
				var customsClearanceDetail = new CustomsClearanceDetail();
				shipmentRequest.RequestedShipment.CustomsClearanceDetail = customsClearanceDetail;
				customsClearanceDetail.DutiesPayment = new Payment();
				customsClearanceDetail.DutiesPayment.PaymentType = FedExShipApi.PaymentType.SENDER;
				customsClearanceDetail.DutiesPayment.Payor = new Payor();
				customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty = new Party();
				customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.AccountNumber = container.AdditionalShippingData.FedexShipperAccountNumber;
				customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address = new Address();
				customsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address.CountryCode = container.AdditionalShippingData.FedexShipperCountryCode;

				customsClearanceDetail.DocumentContent = InternationalDocumentContentType.NON_DOCUMENTS;

				customsClearanceDetail.CustomsValue = new Money();
				customsClearanceDetail.CustomsValue.Currency = container.AdditionalShippingData.FedexShipmentCurrencyCode;
				decimal.TryParse(container.AdditionalShippingData.FedexShipmentMonetaryValue, out var amount);
				customsClearanceDetail.CustomsValue.Amount = amount * packageCount;

				customsClearanceDetail.Commodities = new Commodity[1] { new Commodity() };
				customsClearanceDetail.Commodities[0].Name = container.AdditionalShippingData.FedexShipmentDescription;
				customsClearanceDetail.Commodities[0].Description = container.AdditionalShippingData.FedexShipmentDescription;
				customsClearanceDetail.Commodities[0].NumberOfPieces = packageCount.ToString();
				customsClearanceDetail.Commodities[0].CountryOfManufacture = container.AdditionalShippingData.FedexShipperCountryCode;
				customsClearanceDetail.Commodities[0].Weight = new Weight();
				customsClearanceDetail.Commodities[0].Weight.Units = WeightUnits.LB;
				decimal.TryParse(container.Weight, out var weight);
				customsClearanceDetail.Commodities[0].Weight.Value = weight;
				customsClearanceDetail.Commodities[0].Quantity = packageCount;
				customsClearanceDetail.Commodities[0].QuantitySpecified = true;
				customsClearanceDetail.Commodities[0].QuantityUnits = "EA";
				customsClearanceDetail.Commodities[0].UnitPrice = new Money();
				customsClearanceDetail.Commodities[0].UnitPrice.Currency = container.AdditionalShippingData.FedexShipmentCurrencyCode;
				customsClearanceDetail.Commodities[0].UnitPrice.Amount = amount;
			}
		}

		private static FedExShipApi.ServiceType MapContainerToFedExServiceType(ShippingContainer container, bool assignFedexCustomsDataToContainer)
		{
			decimal.TryParse(container.Weight, out var weight);
			if (container.ClosedLabelTypeId == LabelTypeIdConstants.FedexExpressContainer)
			{
				if (container.ShippingMethod == ContainerConstants.FedExExpressPriority)
				{
					return assignFedexCustomsDataToContainer
						? (weight >= 151 // lbs
								? FedExShipApi.ServiceType.INTERNATIONAL_PRIORITY_FREIGHT
								: FedExShipApi.ServiceType.INTERNATIONAL_PRIORITY)
						: FedExShipApi.ServiceType.PRIORITY_OVERNIGHT;
				}
				else
				{
					return assignFedexCustomsDataToContainer
						? (weight >= 151 // lbs
								? FedExShipApi.ServiceType.INTERNATIONAL_ECONOMY_FREIGHT
								: FedExShipApi.ServiceType.INTERNATIONAL_ECONOMY)
						: FedExShipApi.ServiceType.STANDARD_OVERNIGHT;
				}
			}
			else if (container.ClosedLabelTypeId == LabelTypeIdConstants.FedexGroundContainer)
			{
				return assignFedexCustomsDataToContainer
					? (weight >= 151 // lbs
								? FedExShipApi.ServiceType.INTERNATIONAL_ECONOMY_FREIGHT
								: FedExShipApi.ServiceType.INTERNATIONAL_ECONOMY)
					: FedExShipApi.ServiceType.FEDEX_GROUND;
			}
			else
			{
				throw new Exception();
			}
		}

		private (string Descriptor, string DescriptorLetter) MapShippingMethodToLabelServiceDescriptors(string shippingMethod)
		{
			var response = (string.Empty, string.Empty);

			if (shippingMethod == ContainerConstants.FedExExpress)
			{
				response = ("Express", "E");
			}
			else if (shippingMethod == ContainerConstants.FedExGround)
			{
				response = ("Ground", "G");
			}

			return response;
		}
	}
}
