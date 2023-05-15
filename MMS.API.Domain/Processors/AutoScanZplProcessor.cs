using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.ZplUtilities;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace MMS.API.Domain.Processors
{
    public class AutoScanZplProcessor : IAutoScanZplProcessor
    {
        private readonly IConfiguration config;
        ILogger<AutoScanZplProcessor> logger;
        private ZPLConfiguration zplConfiguration;

        public AutoScanZplProcessor(
            IConfiguration config,
            ILogger<AutoScanZplProcessor> logger,
            IOptionsMonitor<ZPLConfiguration> options)
        {
            this.config = config;
            this.logger = logger;
            zplConfiguration = options.CurrentValue;

            options.OnChange(zplConfig =>
            {
                zplConfiguration = zplConfig;
                logger.Log(LogLevel.Information, "The ZPL configuration has been updated.");
            });
        }

        public string GenerateUspsLabel(Package package)
        {
            try
            {
                var nestingCharacter = StringHelper.Exists(package.ContainerId) ? config.GetSection("NestedPackageLabelCharacter").Value : string.Empty;
                var zplLabelTemplate = zplConfiguration.AutoScanUspsLabelTemplate;
                var humanBarcodeWithSpaces = Regex.Replace(package.HumanReadableBarcode, ".{4}", "$0 ");

                var zplLabel = zplLabelTemplate
                   .Replace("{BinCode}", package.BinCode)
                   .Replace("{ShippingMethod}", FormatUspsServiceType(package.ShippingMethod))
                   .Replace("{PermitNumber}", package.UspsPermitNumber)
                   .Replace("{Zip}", package.Zip)
                   .Replace("{Barcode}", package.FormattedBarcode)
                   .Replace("{HumanReadableBarcode}", humanBarcodeWithSpaces)
                   .Replace("{Nesting}", nestingCharacter);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create USPS ZPL Label. Exception: {ex}");
                return string.Empty;
            }
        }

        public string GenerateUpsGroundLabel(Package package, string upsBase64)
        {
            try
            {
                var zplLabel = string.Empty;
                var zplLabelTemplate = zplConfiguration.AutoScanUpsGroundLabelTemplate;
                var data = Convert.FromBase64String(upsBase64);
                var decodedString = Encoding.UTF8.GetString(data);
                var lineArray = decodedString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // get lines from UPS zpl based on pixel position (^FO line prefix)
                var zipWithPrefixFullLine = Array.Find(lineArray, x => x.StartsWith("^FO284,524"));
                var customBarcodeDataFullLine = Array.Find(lineArray, x => x.StartsWith("^FO20,431"));
                var stateZipThreeFullLine = Array.Find(lineArray, x => x.StartsWith("^FO269,436"));
                var formattedServiceTypeFullLine = Array.Find(lineArray, x => x.StartsWith("^FO9,670"));
                var readableTrackingNumberFullLine = Array.Find(lineArray, x => x.StartsWith("^FO9,731"));

                // parse matched zpl lines into data for our template custom fields
                var zipWithPrefix = GetZplLineContents(zipWithPrefixFullLine, "^FV");
                var customBarcodeData = GetZplLineContents(customBarcodeDataFullLine, "^BD");
                var stateZipThree = GetZplLineContents(stateZipThreeFullLine, "^FV");
                var formattedServiceType = GetZplLineContents(formattedServiceTypeFullLine, "^FV");
                var readableTrackingNumber = GetZplLineContents(readableTrackingNumberFullLine, "^FV");

                // replace template custom fields with data from UPS zpl
                zplLabel = zplLabelTemplate
                    .Replace("{CustomBarcodeData}", customBarcodeData)
                    .Replace("{StateZipThreeAnd1To10}", stateZipThree)
                    .Replace("{ZipWithPrefix}", zipWithPrefix)
                    .Replace("{FormattedShippingMethod}", formattedServiceType)
                    .Replace("{ReadableTrackingNumber}", readableTrackingNumber)
                    .Replace("{TrackingNumber}", package.Barcode);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create UPS ZPL Label. Exception: {ex}");
                return string.Empty;
            }
        }

        public string GenerateUpsAirLabel(Package package, string upsBase64)
        {
            try
            {
                var zplLabel = string.Empty;
                var zplLabelTemplate = zplConfiguration.AutoScanUpsAirLabelTemplate;
                var data = Convert.FromBase64String(upsBase64);
                var decodedString = Encoding.UTF8.GetString(data);
                var lineArray = decodedString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var zipWithPrefixFullLine = Array.Find(lineArray, x => x.StartsWith("^FO284,524"));
                var customBarcodeDataFullLine = Array.Find(lineArray, x => x.StartsWith("^FO20,431"));
                var stateZipThreeFullLine = Array.Find(lineArray, x => x.StartsWith("^FO269,436"));
                var formattedServiceTypeFullLine = Array.Find(lineArray, x => x.StartsWith("^FO9,670"));
                var readableTrackingNumberFullLine = Array.Find(lineArray, x => x.StartsWith("^FO9,731"));
                var codeFullLine = Array.Find(lineArray, x => x.StartsWith("^FO676,674"));

                var zipWithPrefix = GetZplLineContents(zipWithPrefixFullLine, "^FV");
                var customBarcodeData = GetZplLineContents(customBarcodeDataFullLine, "^BD");
                var stateZipThree = GetZplLineContents(stateZipThreeFullLine, "^FV");
                var formattedServiceType = GetZplLineContents(formattedServiceTypeFullLine, "^FV");
                var readableTrackingNumber = GetZplLineContents(readableTrackingNumberFullLine, "^FV");
                var code = GetZplLineContents(codeFullLine, "^FV");

                zplLabel = zplLabelTemplate
                    .Replace("{CustomBarcodeData}", customBarcodeData)
                    .Replace("{StateZipThreeAnd1To10}", stateZipThree)
                    .Replace("{ZipWithPrefix}", zipWithPrefix)
                    .Replace("{GeoDescriptor}", package.UpsGeoDescriptor)
                    .Replace("{FormattedShippingMethod}", formattedServiceType)
                    .Replace("{UnknownNumericCode}", code)
                    .Replace("{ReadableTrackingNumber}", readableTrackingNumber)
                    .Replace("{TrackingNumber}", package.Barcode);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create UPS ZPL Label. Exception: {ex}");
                return string.Empty;
            }
        }

        public string GenerateErrorLabel(string errorLabelMessage, string siteName, string packageId, string timeZone = null)
        {
            try
            {
                var zplErrorLabelTemplate = zplConfiguration.AutoScanErrorLabelTemplate;
                var formattedDate = string.Empty;

                if(timeZone != null)
                {
                    formattedDate = TimeZoneUtility.GetLocalTime(timeZone).ToString("yyyy-MM-dd HH:mm.ss");
                }

                var zplLabel = zplErrorLabelTemplate
                        .Replace("{PackageId}", packageId)
                        .Replace("{ErrorLabelMessage}", errorLabelMessage)
                        .Replace("{SiteName}", siteName)
                        .Replace("{FormattedLocalDateTime}", formattedDate);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create ZPL Error Label. Exception: {ex}");
                return string.Empty;
            }
        }

        private static string GetZplLineContents(string line, string startAtZplCommand)
        {
            var startIndex = line.IndexOf(startAtZplCommand) + 3;
            var lineAfterStartIndex = line.Substring(startIndex);
            var removeFsCommandAtEnd = lineAfterStartIndex.Substring(0, lineAfterStartIndex.Length - 3);

            return removeFsCommandAtEnd;
        }

        private static string FormatUspsServiceType(string serviceTypeName)
        {
            return serviceTypeName switch // the autoscan label requires 3 spaces before FIRST CLASS
            {
                UspsParcelSelect => " PARCEL SELECT", 
                UspsParcelSelectLightWeight => "PS LIGHTWEIGHT",
                UspsFirstClass => "   FIRST CLASS",
                UspsPriority => "PRIORITY",
                UspsFcz => "   FIRST CLASS",
                _ => string.Empty,
            };
        }
    }
}