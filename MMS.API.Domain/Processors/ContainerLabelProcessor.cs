using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MMS.API.Domain.Interfaces;
using PackageTracker.Data.Utilities;
using MMS.API.Domain.Models.Containers;
using PackageTracker.Domain.Models;
using System.Linq;

namespace MMS.API.Domain.Processors
{
	public class ContainerLabelProcessor : IContainerLabelProcessor
	{
		private readonly IContainerBarcodeProcessor barcodeProcessor;
		private readonly IContainerShippingProcessor shippingProcessor;
		private readonly ILogger<ContainerLabelProcessor> logger;
		private readonly IZipMapProcessor zipMapProcessor;

		public ContainerLabelProcessor(IContainerBarcodeProcessor barcodeProcessor, IContainerShippingProcessor shippingProcessor, ILogger<ContainerLabelProcessor> logger, IZipMapProcessor zipMapProcessor)
		{
			this.barcodeProcessor = barcodeProcessor;
			this.shippingProcessor = shippingProcessor;
			this.logger = logger;
			this.zipMapProcessor = zipMapProcessor;
		}

		public async Task<ShippingContainer> GetCreateContainerLabelData(ShippingContainer container, Site site, Bin bin, CreateContainerRequest createContainerRequest)
		{
			container.LabelTypeId = LabelTypeIdConstants.CreateContainer;

			if (container.ContainerType == ContainerConstants.ContainerTypePallet)
			{
                if (!createContainerRequest.IsReplacement) // initial create
                {
					var palletBarcodes = await barcodeProcessor.GeneratePalletContainerId(site, container);
					container.ContainerId = palletBarcodes.Barcode;
					container.HumanReadableBarcode = palletBarcodes.HumanReadableBarcode;
				}
				else // update only
				{
					container.ContainerId = createContainerRequest.ContainerIdToReplace;
					container.HumanReadableBarcode = createContainerRequest.HumanReadableBarcodeToReplace;
					container.SerialNumber = createContainerRequest.SerialNumberToReplace;
				}
			}
			else if (container.ContainerType == ContainerConstants.ContainerTypeBag)
			{
                if (!createContainerRequest.IsReplacement)
				{
					container.ContainerId = await barcodeProcessor.GenerateBagContainerId(site, container, bin);
					container.HumanReadableBarcode = container.ContainerId;
				}
				else
				{
					container.ContainerId = createContainerRequest.ContainerIdToReplace;
					container.HumanReadableBarcode = createContainerRequest.HumanReadableBarcodeToReplace;
					container.SerialNumber = createContainerRequest.SerialNumberToReplace;
				}
			}

			if (StringHelper.Exists(container.ContainerId))
			{
				AssignCreateContainerLabelFieldValues(container, site, bin);
			}

			return container;
		}

		public async Task<FedExShippingDataResponse> GetClosedContainerLabelData(ShippingContainer container, Site site, Bin bin)
		{
			var response = new FedExShippingDataResponse();
			container.AdditionalShippingData = new AdditionalShippingData();
			container.ClosedLabelTypeId = GetClosedContainerLabelTypeId(container);

			var isPrimary = !container.IsSecondaryCarrier;
			var binCode = isPrimary ? bin.BinCode : bin.BinCodeSecondary;			

			if (container.ClosedLabelTypeId == LabelTypeIdConstants.PmodContainer)
			{
				var isPallet = container.ContainerType == ContainerConstants.ContainerTypePallet;
				container.CarrierBarcode = await barcodeProcessor.GeneratePmodBarcode(site, container, bin);
				container.HumanReadableCarrierBarcode = Regex.Replace(container.CarrierBarcode.Substring(8), ".{4}", "$0 ");
				GeneratePmodLabel(container, site, isPallet);
			}
			else if (container.ClosedLabelTypeId == LabelTypeIdConstants.ThirdPartyShippingGS1)
			{
				var (barcode, humanReadable) = await barcodeProcessor.GenerateRegionalCarrierBarcode(site, container, bin);
				container.CarrierBarcode = barcode;
				container.HumanReadableCarrierBarcode = humanReadable;

				GenerateThirdPartyShippingLabelGS1(container, site);			
			}
			else if(container.ClosedLabelTypeId == LabelTypeIdConstants.ThirdPartyShipping)
            {
				GenerateThirdPartyShippingLabel(container, site);
				container.CarrierBarcode = container.ContainerId; 
			}
			else if (container.ClosedLabelTypeId == LabelTypeIdConstants.FedexExpressContainer)
			{
				if (container.IsSaturdayDelivery && container.ShippingMethod != ContainerConstants.FedExExpressPriority)
				{
					container.ShippingMethodOverride = container.ShippingMethod;
					container.ShippingMethod = ContainerConstants.FedExExpressPriority;
				}

				response = await shippingProcessor.GetFedexContainerShippingData(container);
				container.HumanReadableCarrierBarcode = container.AdditionalShippingData.HumanReadableTrackingNumber;
				GenerateFedexExpressLabel(container, site);
			}
			else if (container.ClosedLabelTypeId == LabelTypeIdConstants.FedexGroundContainer)
			{
				response = await shippingProcessor.GetFedexContainerShippingData(container);
				GenerateFedexGroundLabel(container, site);
			}
			else if (container.ClosedLabelTypeId == LabelTypeIdConstants.OnTrac)
			{
				container.CarrierBarcode = await barcodeProcessor.GenerateOnTracBarcode(site, container);
				await GenerateOnTracLabel(container, site, binCode);
			}

			return response;
		}
		private static void GenerateThirdPartyShippingLabel(ShippingContainer container, Site site)
		{
			var localTimeFormatted = TimeZoneUtility.GetLocalTime(site.TimeZone).ToString("MM/dd/yyyy hh:mm:ss tt");
			container.ClosedLabelFieldValues = container.LabelFieldValues;

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = container.ShippingCarrier
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = container.RegionalCarrierHub
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 14,
				FieldValue = localTimeFormatted
			});
		}
		private static void GenerateThirdPartyShippingLabelGS1(ShippingContainer container, Site site)
		{
			var localTimeFormatted = TimeZoneUtility.GetLocalTime(site.TimeZone).ToString("MM/dd/yyyy hh:mm:ss tt");
			container.ClosedLabelFieldValues = container.LabelFieldValues;

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = container.ShippingCarrier
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = container.RegionalCarrierHub
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 14,
				FieldValue = localTimeFormatted
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 15,
				FieldValue = container.CarrierBarcode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 16,
				FieldValue = container.HumanReadableCarrierBarcode
			});
		}
		private static void AssignCreateContainerLabelFieldValues(ShippingContainer container, Site site, Bin bin)
		{
			var isPrimary = !container.IsSecondaryCarrier;
			var binCode = isPrimary ? bin.BinCode : bin.BinCodeSecondary;
			var isDduOrScf = IsDduOrScf(binCode);

			container.LabelFieldValues = new List<LabelFieldValue>()
			{
				new LabelFieldValue
				{
				Position = 0,
				FieldValue = site.Description
				},
				new LabelFieldValue
				{
				Position = 1,
				FieldValue = site.AddressLineOne
				},
				new LabelFieldValue
				{
				Position = 2,
				FieldValue = $"{site.City} {site.State} {site.Zip}"
				},
				new LabelFieldValue
				{
				Position = 3,
				FieldValue = container.DropShipSiteDescription
				},
				new LabelFieldValue
				{
				Position = 4,
				FieldValue = container.DropShipSiteAddress
				},
				new LabelFieldValue
				{
				Position = 5,
				FieldValue = container.DropShipSiteCsz
				},
				new LabelFieldValue
				{
				Position = 6,
				FieldValue = binCode
				},
				new LabelFieldValue
				{
				Position = 7,
				FieldValue = bin.LabelListDescription
				},
				new LabelFieldValue
				{
				Position = 8,
				FieldValue = StringHelper.Exists(isDduOrScf.UspsPalletResponse) ? $"PSLW IRREG {isDduOrScf.UspsPalletResponse}" : $"PSLW IRREG"
				},
				new LabelFieldValue
				{
				Position = 9,
				FieldValue = $"{site.Description} {site.City} {site.State} {site.Zip}"
				},
				new LabelFieldValue
				{
				Position = 10,
				FieldValue = container.HumanReadableBarcode
				},
				new LabelFieldValue
				{
				Position = 11,
				FieldValue = bin.LabelListZip
				},
				new LabelFieldValue
				{
				Position = 99, // any additional fields on label type id 5 need to start at from 99 in order to not overlap with the Closed Label Field Values which start at 12
				FieldValue = container.DropShipSiteNote
				},
				new LabelFieldValue
				{
				Position = 100,
				FieldValue = $"{container.ShippingCarrier}-{container.ShippingMethod}"
				}
			};
		}

		private static void GenerateFedexExpressLabel(ShippingContainer container, Site site)
		{
			container.ClosedLabelFieldValues = container.LabelFieldValues;
			var parseCsz = AddressUtility.ParseCityStateZip(container.DropShipSiteCsz);
			var formattedFields = FormatFedexFields(container, site);

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = container.AdditionalShippingData.OriginId
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = formattedFields.ShipDate
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 14,
				FieldValue = formattedFields.Weight
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 15,
				FieldValue = container.AdditionalShippingData.Cad
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 16,
				FieldValue = formattedFields.Dimensions
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 17,
				FieldValue = container.AdditionalShippingData.FormattedDeliveryDate
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 18,
				FieldValue = container.AdditionalShippingData.FormattedShippingMethod
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 19,
				FieldValue = container.AdditionalShippingData.HumanReadableTrackingNumber
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 20,
				FieldValue = container.AdditionalShippingData.FormId
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 21,
				FieldValue = container.AdditionalShippingData.Ursa
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 22,
				FieldValue = AddressUtility.TrimZipToFirstFive(parseCsz.FullZip)
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 23,
				FieldValue = container.AdditionalShippingData.StateAndCountryCode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 24,
				FieldValue = container.AdditionalShippingData.AirportId
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 25,
				FieldValue = container.AdditionalShippingData.OperationalSystemId
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 26,
				FieldValue = container.CarrierBarcode
			});
		}

		private static void GenerateFedexGroundLabel(ShippingContainer container, Site site)
		{
			container.ClosedLabelFieldValues = container.LabelFieldValues;
			var parseCsz = AddressUtility.ParseCityStateZip(container.DropShipSiteCsz);
			var formattedFields = FormatFedexFields(container, site);

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = container.AdditionalShippingData.OriginId
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = formattedFields.ShipDate
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 14,
				FieldValue = formattedFields.Weight
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 15,
				FieldValue = container.AdditionalShippingData.Cad
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 16,
				FieldValue = formattedFields.Dimensions
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 17,
				FieldValue = container.AdditionalShippingData.HumanReadableTrackingNumber
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 18,
				FieldValue = parseCsz.FullZip
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 19,
				FieldValue = container.AdditionalShippingData.HumanReadableSecondaryBarcode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 20,
				FieldValue = container.CarrierBarcode
			});
		}

		private async Task GenerateOnTracLabel(ShippingContainer container, Site site, string binCode)
		{			
			var recipientContactName = $"USPS - {container.DropShipSiteDescription}";
			var cityStateZip = AddressUtility.ParseCityStateZip(container.DropShipSiteCsz);
			var fiveDigitZip = AddressUtility.TrimZipToFirstFive(cityStateZip.FullZip);
			decimal.TryParse(container.Weight, out var containerWeight);
			if (containerWeight < 1)
			{
				containerWeight = 1;
				container.Weight = "1";
			}
			var parsedContainerWeight = decimal.Round(containerWeight).ToString(); // onTrac label weight is a rounded decimal, no trailing zeroes
			var isTomorrowSaturday = false;

			if (container.SiteCreateDate.DayOfWeek == DayOfWeek.Friday)
			{
				isTomorrowSaturday = true;
			}

			var onTracTrackingNumber = container.CarrierBarcode;
			var onTracPdfBarcode = barcodeProcessor.GenerateOnTracPdfBarcode(site, container, onTracTrackingNumber, recipientContactName, cityStateZip, isTomorrowSaturday);

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 0,
				FieldValue = $"{site.Description}"
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 1,
				FieldValue = $"{site.AddressLineOne}"
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 2,
				FieldValue = $"{site.City} {site.State} {site.Zip}"
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 3,
				FieldValue = recipientContactName,
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 4,
				FieldValue = container.DropShipSiteAddress
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 5,
				FieldValue = $"{cityStateZip.City} {cityStateZip.State} {fiveDigitZip}"
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 6,
				FieldValue = binCode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 7,
				FieldValue = container.CarrierBarcode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 8,
				FieldValue = isTomorrowSaturday ? "SATURDAY" : ""
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 9,
				FieldValue = parsedContainerWeight
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 10,
				FieldValue = container.RegionalCarrierHub // onTrac sort code
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 11,
				FieldValue = $"01{fiveDigitZip}" // GROUND service indicator plus dest zip
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = onTracPdfBarcode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = container.SiteCreateDate.ToString("MM/dd/yyyy hh:mm:ss tt")
			});
		}

		private static void GeneratePmodLabel(ShippingContainer container, Site site, bool isPallet)
		{
			container.ClosedLabelFieldValues = container.LabelFieldValues;

			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = isPallet ? site.PmodPalletPermitNumber : site.PermitNumber
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = container.HumanReadableCarrierBarcode
			});
			container.ClosedLabelFieldValues.Add(new LabelFieldValue
			{
				Position = 14,
				FieldValue = container.CarrierBarcode
			});
		}

		

		private static int GetClosedContainerLabelTypeId(ShippingContainer container)
		{
			if (container.BinLabelType == ContainerConstants.PmodPallet || container.BinLabelType == ContainerConstants.PmodBag)
			{
				return LabelTypeIdConstants.PmodContainer;
			}
			else if (container.BinLabelType == ContainerConstants.ThirdPartyPallet
				|| container.BinLabelType == ContainerConstants.ThirdPartyBag
				|| container.BinLabelType == ContainerConstants.UspsOriginPallet)
			{
				return LabelTypeIdConstants.ThirdPartyShipping;
			}
			else if (container.BinLabelType == ContainerConstants.ThirdPartyPalletGS1
				|| container.BinLabelType == ContainerConstants.ThirdPartyBagGS1)
			{
				return LabelTypeIdConstants.ThirdPartyShippingGS1;
			}
			else if (container.BinLabelType == ContainerConstants.FedExExpress)
			{
				return LabelTypeIdConstants.FedexExpressContainer;
			}
			else if (container.BinLabelType == ContainerConstants.FedExGround)
			{
				return LabelTypeIdConstants.FedexGroundContainer;

			}
			else if (container.BinLabelType == ContainerConstants.OnTracPallet || container.BinLabelType == ContainerConstants.OnTracBag)
			{
				return LabelTypeIdConstants.OnTrac;
			}
			else
			{
				return LabelTypeIdConstants.Error;
			}
		}

		private static (string PmodResponse, string UspsPalletResponse) IsDduOrScf(string binCode)
		{
			var pmodResponse = string.Empty;
			var uspsPalletResponse = string.Empty;
			if (StringHelper.Exists(binCode))
            {
				if (binCode.Substring(0, 1) == "D")
				{
					pmodResponse = "DDU";
					uspsPalletResponse = "5D";
				}
				else if (binCode.Substring(0, 1) == "S")
				{
					pmodResponse = "SCF";
					uspsPalletResponse = "SCF";
				}
            }
			return (pmodResponse, uspsPalletResponse);
		}

		private static (string Weight, string Dimensions, string ShipDate) FormatFedexFields(ShippingContainer container, Site site)
		{
			var localTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
			var shipDate = localTime.ToString("ddMMMyy").ToUpper();

			decimal.TryParse(container.Weight, out var decimalWeight);

			var roundedWeight = decimal.Round(decimalWeight, 0);

			if (roundedWeight < 1)
			{
				roundedWeight = 1;
			}

			var formattedWeight = $"{roundedWeight}.00LB";
			var formattedDimensions = string.Empty;
			//var formattedDimensions = $"{length.ToString(".")}x{width.ToString(".")}x{depth.ToString(".")} IN"; // TODO: waiting on container dimensions
			return (formattedWeight, formattedDimensions, shipDate);
		}

		// we are not currently using Lso as a container carrier, however we may use them in the future
		private async Task GenerateLsoLabel(ShippingContainer container, Site site, string binCode, string dropShipSiteDescription, string dropShipSiteAddress, string dropShipSiteCsz)
		{
			var cityStateZip = AddressUtility.ParseCityStateZip(dropShipSiteCsz);
			var fiveDigitZip = AddressUtility.TrimZipToFirstFive(cityStateZip.FullZip);
			decimal.TryParse(container.Weight, out var containerWeight);
			var parsedContainerWeight = ParseLsoContainerWeight(containerWeight); // lso weight ex: "110.50LBS"
			var isTomorrowSaturday = false;

			if (container.SiteCreateDate.DayOfWeek == DayOfWeek.Friday)
			{
				isTomorrowSaturday = true;
			}

			var serviceId = isTomorrowSaturday ? "S" : "E";
			var marketingName = isTomorrowSaturday ? "LSO Saturday" : "LSO Early Overnight";
			var addlDescription = isTomorrowSaturday ? string.Empty : "8:30 in select cities";
			var lsoZipMap = await zipMapProcessor.GetZipMapAsync(ActiveGroupTypeConstants.ZipsLso, fiveDigitZip);
			var lsoSort = lsoZipMap.Value;

			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 0,
				FieldValue = site.AlternateCarrierBarcodePrefix // Airbill Number
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 1,
				FieldValue = dropShipSiteDescription
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 2,
				FieldValue = dropShipSiteAddress
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 3,
				FieldValue = dropShipSiteCsz
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 4,
				FieldValue = site.Description
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 5,
				FieldValue = site.AddressLineOne
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 6,
				FieldValue = $"{site.City} {site.State} {site.Zip}"
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 7,
				FieldValue = serviceId
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 8,
				FieldValue = lsoSort
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 9,
				FieldValue = marketingName
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 10,
				FieldValue = addlDescription
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 11,
				FieldValue = binCode // Quickcode
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 12,
				FieldValue = parsedContainerWeight
			});
			container.LabelFieldValues.Add(new LabelFieldValue
			{
				Position = 13,
				FieldValue = container.ContainerId // ref
			});
		}

		private static string ParseLsoContainerWeight(decimal containerWeight)
		{
			var response = "0.00";

			if (containerWeight > 0)
			{
				response = $"{containerWeight:.00}LBS";
			}

			return response;
		}
	}
}
