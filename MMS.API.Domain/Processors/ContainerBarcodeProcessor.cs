using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Processors;
using MMS.API.Domain.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class ContainerBarcodeProcessor : IContainerBarcodeProcessor
    {
        private readonly IConfiguration config;
        private readonly ILogger<ContainerLabelProcessor> logger;
        private readonly ISequenceProcessor sequenceProcessor;

        public ContainerBarcodeProcessor(IConfiguration config, ILogger<ContainerLabelProcessor> logger, ISequenceProcessor sequenceProcessor)
        {
            this.config = config;
            this.logger = logger;
            this.sequenceProcessor = sequenceProcessor;
        }

        public async Task<string> GenerateBagContainerId(Site site, ShippingContainer container, Bin bin)
        {
            var response = string.Empty;
            var mailerId = site.SackMailerId;
            var dropShipSiteCsz = container.IsSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary;
            var parsedCsz = AddressUtility.ParseCityStateZip(dropShipSiteCsz);
            var fiveDigitZip = AddressUtility.TrimZipToFirstFive(parsedCsz.FullZip);
            var contentIdentifierNumber = GetContentIdentifierNumber(bin);

            if (StringHelper.Exists(contentIdentifierNumber))
            {
                var labelSource = "1";
                var labelType = "8";
                var sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.Container, SequenceTypeConstants.FiveDigitMaxSequence);
                container.SerialNumber = sequence.Number.ToString().PadLeft(5, '0');
                response = $"{fiveDigitZip}{contentIdentifierNumber}{labelSource}{mailerId}{container.SerialNumber}{labelType}";
            }
            else
            {
                // if we don't have a contentIdentifierNumber, the container is in an error state
                logger.Log(LogLevel.Error, $"Invalid Content Identifier Number for containerId: {container.ContainerId}. SiteName: {site.SiteName}");
                container.LabelTypeId = 0;
            }

            return response;
        }

        public async Task<(string Barcode, string HumanReadableBarcode)> GeneratePalletContainerId(Site site, ShippingContainer container)
        {
            // Check if we have created an new sequence for site for pallets.
            var sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.Pallet, SequenceTypeConstants.SevenDigitMaxSequence);
            container.SerialNumber = BarcodeUtility.GeneratePmodContainerSerialNumberByMidLength(sequence.Number, site.PalletMailerId.Length);
            var barcode = $"99M{site.PalletMailerId}{container.SerialNumber}";
            container.HumanReadableBarcode = $"99 M {site.PalletMailerId} {container.SerialNumber}";

            return (barcode, container.HumanReadableBarcode);
        }

        public async Task<(string Barcode, string HumanReadableBarcode)> GenerateRegionalCarrierBarcode(Site site, ShippingContainer container, Bin bin)
        {            
            var dropShipSiteCsz = container.IsSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary;
            var parsedCsz = AddressUtility.ParseCityStateZip(dropShipSiteCsz);
            var fiveDigitZip = AddressUtility.TrimZipToFirstFive(parsedCsz.FullZip);
            var state = site.State;
            var letter = container.ContainerType == ContainerConstants.ContainerTypeBag ? "B" : "G"; 
            var regionalSequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.Regional, SequenceTypeConstants.SixDigitMaxSequence);
            var serialNumber = BarcodeConstants.GS1SerialNumberApplicationIdentifier;            

            container.ClosedSerialNumber = regionalSequence.Number.ToString().PadLeft(6, '0');            
            
            var barcode = $"{BarcodeConstants.GS1RoutingCodeApplicationIdentifier}{letter}{state}{fiveDigitZip}{serialNumber}{container.ClosedSerialNumber}";
            var humanReadable = $"{BarcodeConstants.GS1RoutingCodeApplicationIdentifier}{letter}{state}{fiveDigitZip}{serialNumber}{container.ClosedSerialNumber}";                        
            
            return (barcode, humanReadable);
        }

        public async Task<string> GeneratePmodBarcode(Site site, ShippingContainer container, Bin bin)
        {
            var barcode = string.Empty;
            var dropShipSiteCsz = container.IsSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary;
            var parsedCsz = AddressUtility.ParseCityStateZip(dropShipSiteCsz);
            var fiveDigitZip = AddressUtility.TrimZipToFirstFive(parsedCsz.FullZip);
            var pmodSequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.Pmod, SequenceTypeConstants.SevenDigitMaxSequence);
            var mailerId = container.BinLabelType == ContainerConstants.PmodBag ? site.SackMailerId : site.PalletMailerId;
            var channelApplicationIdentifier = mailerId.Length == 6 ? BarcodeConstants.ChannelApplicationIdentifierSixDigitMid : BarcodeConstants.ChannelApplicationIdentifierNineDigitMid;
            var serviceTypeCode = "123";
            container.ClosedSerialNumber = BarcodeUtility.GenerateUspsSerialNumberByMidLength(pmodSequence.Number, mailerId.Length);
            var barcodeWithoutCheckDigit = $"{BarcodeConstants.RoutingApplicationIdentifier}{fiveDigitZip}{channelApplicationIdentifier}{serviceTypeCode}{mailerId}{container.ClosedSerialNumber}";
            var barcodeInputForCheckDigit = barcodeWithoutCheckDigit.Substring(8, 25);  // input includes remainder of barcode starting after zip
            var checkDigit = BarcodeUtility.GenerateUspsCheckDigit(barcodeInputForCheckDigit, BarcodeConstants.UspsCheckDigitMultiplier);
            barcode = $"{barcodeWithoutCheckDigit}{checkDigit}";

            return barcode;
        }

        public async Task<string> GenerateOnTracBarcode(Site site, ShippingContainer container)
        {
            var barcode = string.Empty;
            var onTracSequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.OnTrac, SequenceTypeConstants.SevenDigitMaxSequence);
            container.ClosedSerialNumber = onTracSequence.Number.ToString().PadLeft(7, '0');
            var trackingSeed = $"{config.GetSection("OnTracTrackingSeed").Value}";
            var leadingLetter = "C"; // leading char is always C
            var leadingNumber = "4"; // this is the leading char converted to its numeric equivalent according to ontrac docs, purpose is check digit generation
            var checkDigit = BarcodeUtility.GenerateOnTracCheckDigit($"{leadingNumber}{trackingSeed}{container.ClosedSerialNumber}");

            barcode = $"{leadingLetter}{trackingSeed}{container.ClosedSerialNumber}{checkDigit}";
            return barcode;
        }

        public string GenerateOnTracPdfBarcode(Site site, ShippingContainer container, string trackingNumber, string recipientContactName, (string City, string State, string FullZip) cityStateZip, bool isSaturday)
        {
            var city = $"{cityStateZip.City}";

            var destinationZip = AddressUtility.TrimZipToFirstFive(cityStateZip.FullZip);
            var shipperAccountNumber = site.AlternateCarrierBarcodePrefix; // TODO: Discover this value
            var julianPickupDateForYear = container.SiteCreateDate.DayOfYear;
            var parsedWeight = GetOnTracPdfBarcodeParsedWeight(container.Weight); // Formatted as nnnnn.nnLB, leading zeroes not required
            var destinationAddressLine1 = container.DropShipSiteAddress.Length > 30 ? container.DropShipSiteAddress.Substring(0, 30) : container.DropShipSiteAddress;
            var destinationCity = city.Length > 30 ? city.Substring(0, 30) : city;
            var destinationStateCode = $"{cityStateZip.State}";
            var recipientName = recipientContactName;
            var onTracVersionNumber = "01";
            var companyName = recipientContactName;
            var phoneNumber = string.Empty;
            var destinationAddressLine2 = string.Empty;
            var shipFromZipCode = site.Zip;
            var isSaturdayZeroOrOne = isSaturday ? "1" : "0";
            var customerReferenceNumber = container.CarrierBarcode;

            var fs28 = (char)28; // fs 28 U001C
            var gs29 = (char)29; // gs 29 U001D
            var rs30 = (char)30; // rs 30 U001E
            var eot04 = (char)04; // eot 04 U0004

            // template: [)<RS>01<GS>02{destinationZip}<GS>840<GS>01<GS>{trackingNumber}<GS>EMSY<GS>{shipperAccountNumber}<GS>{julianPickupDateForYear}<GS><GS>1/1<GS>{parsedWeight}<GS>N<GS>{destinationAddressLine1}<GS>{destinationCity}<GS>{destinationStateCode}<GS>{recipientName}<RS>06<GS>3Z{onTracVersionNumber}<GS>11Z{companyName}<GS>12Z{phoneNumber}<GS>14Z{destinationAddressLine2}<GS>15Z{shipFromZipCode}<GS>20Z0.00<FS>U<FS>0.00<GS>21Z0<GS>22Z0<GS>24Z{isSaturdayZeroOrOne}<GS>9K{customerReferenceNumber}<GS><RS><EOT>

            var pdfBarcode = @$"[)>{rs30}01{gs29}02{destinationZip}{gs29}840{gs29}01{gs29}{trackingNumber}{gs29}EMSY{gs29}{shipperAccountNumber}{gs29}{julianPickupDateForYear}{gs29}{gs29}1/1{gs29}{parsedWeight}{gs29}N{gs29}{destinationAddressLine1}{gs29}{destinationCity}{gs29}{destinationStateCode}{gs29}{recipientName}{rs30}06{gs29}3Z{onTracVersionNumber}{gs29}11Z{companyName}{gs29}12Z{phoneNumber}{gs29}14Z{destinationAddressLine2}{gs29}15Z{shipFromZipCode}{gs29}20Z0.00{fs28}U{fs28}0.00{gs29}21Z1{gs29}22Z0{gs29}24Z{isSaturdayZeroOrOne}{gs29}9K{customerReferenceNumber}{gs29}{rs30}{eot04}";

            return pdfBarcode;
        }

        private static string GetContentIdentifierNumber(Bin bin)
        {
            var response = string.Empty;

            if (bin.BinCode.Substring(0, 1) == "S")
            {
                response = "596";
            }
            else if (bin.BinCode.Substring(0, 1) == "D")
            {
                response = "590";
            }
            else if (bin.BinCode.Substring(0, 1) == "F")
            {
                response = "000";
            }
            return response;
        }

        private static string GetOnTracPdfBarcodeParsedWeight(string weight)
        {
            decimal.TryParse(weight, out var decimalWeight);
            return $"{decimal.Round(decimalWeight, 2)}LB";
        }
    }
}
