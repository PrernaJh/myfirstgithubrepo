using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Text;
using static PackageTracker.Data.Constants.BarcodeConstants;
using static PackageTracker.Data.Constants.ServiceLevelConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace PackageTracker.Domain
{
	public class UspsShippingProcessor : IUspsShippingProcessor
	{
		private readonly ILogger<UspsShippingProcessor> logger;

		public UspsShippingProcessor(ILogger<UspsShippingProcessor> logger)
		{
			this.logger = logger;
		}

		public string GenerateUspsBarcode(Package package)
		{
			try
			{
				var serviceTypeCode = GenerateUspsServiceTypeCode(package);

				var channelApplicationIdentifier = package.MailerId.Length == 6 ? ChannelApplicationIdentifierSixDigitMid : ChannelApplicationIdentifierNineDigitMid;
				var serialNumber = BarcodeUtility.GenerateUspsSerialNumberByMidLength(package.Sequence, package.MailerId.Length);
				var zip = AddressUtility.TrimZipToFirstFive(package.Zip);
				var barcodeWithoutCheckDigit = $"{RoutingApplicationIdentifier}{zip}{channelApplicationIdentifier}{serviceTypeCode}{package.MailerId}{serialNumber}";
				var barcodeInputForCheckDigit = barcodeWithoutCheckDigit.Substring(8, 25);  // input is package identification code, includes rest of barcode starting after zip
				var checkDigit = BarcodeUtility.GenerateUspsCheckDigit(barcodeInputForCheckDigit, BarcodeConstants.UspsCheckDigitMultiplier);

				return $"{barcodeWithoutCheckDigit}{checkDigit}";
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Error generating USPS Barcode for Package ID: {package.Id} Exception: {ex}");
				return string.Empty;
			}
		}

		public string GenerateUspsFormattedBarcode(string barcode)
		{
			try
			{
				if (StringHelper.Exists(barcode))
				{
					var formattedBarcode = new StringBuilder(barcode);

					formattedBarcode.Insert(0, "(")
									.Insert(4, ")")
									.Insert(10, ">8")
									.Insert(12, "(")
									.Insert(15, ")");

					return formattedBarcode.ToString();
				}
				else
				{
					return string.Empty;
				}
			}
			catch (Exception ex)
			{

				logger.Log(LogLevel.Error, $"Error generating USPS Barcode for: {barcode} Exception: {ex}");
				return string.Empty;
			}
		}

		public string GenerateUspsHumanReadableBarcode(string barcode)
		{
            try
            {
                return barcode[8..];
            }
            catch (Exception ex)
            {
				logger.Log(LogLevel.Error, $"Error generating USPS Barcode: {barcode} Exception: {ex}");
				return string.Empty;
			}
		}

		private static string GenerateUspsServiceTypeCode(Package package)
		{
			var serviceTypeCode = package.ServiceLevel switch
			{
				Signature => package.ShippingMethod switch
				{
					UspsParcelSelect => ParcelSelectSignatureServiceTypeCode,
					UspsParcelSelectLightWeight => ParcelSelectLightWeightSignatureServiceTypeCode,
					UspsPriority => PrioritySignatureServiceTypeCode,
					UspsFirstClass => FirstClassSignatureServiceTypeCode,
					UspsFcz => FirstClassSignatureServiceTypeCode,
					UspsPriorityExpress => PriorityExpressServiceTypeCode,
					_ => string.Empty
				},
				_ => package.ShippingMethod switch // default to delcon
				{
					UspsParcelSelect => ParcelSelectDelConServiceTypeCode,
					UspsParcelSelectLightWeight => ParcelSelectLightWeightDelConServiceTypeCode,
					UspsPriority => PriorityDelConServiceTypeCode,
					UspsFirstClass => FirstClassDelConServiceTypeCode,
					UspsFcz => FirstClassDelConServiceTypeCode,
					UspsPriorityExpress => PriorityExpressServiceTypeCode,
					_ => string.Empty
				}
			};

			if (StringHelper.DoesNotExist(serviceTypeCode))
			{
				throw new Exception("Failed to generate USPS Service Type Code during barcode generation ");
			}

			return serviceTypeCode;
		}
	}
}
