using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.ZplUtilities;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace MMS.API.Domain.Processors
{
    public class CreatePackageZplProcessor : ICreatePackageZplProcessor
    {
        private readonly IConfiguration config;
        ILogger<CreatePackageZplProcessor> logger;
        private ZPLConfiguration zplConfiguration;

        public CreatePackageZplProcessor(
            IConfiguration config,
            ILogger<CreatePackageZplProcessor> logger,
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

        public string GenerateUspsLabel(Package package, string shippingMethod)
        {
            try
            {
                var nestingCharacter = StringHelper.Exists(package.ContainerId) ? config.GetSection("NestedPackageLabelCharacter").Value : string.Empty;
                var zplLabelTemplate = GetZplLabelTemplate(package.ClientName, package.ShippingMethod);
                var shippingMethodValues = GenerateFormattedShippingMethodValues(package.ShippingMethod);
                var shipDateForLabel = package.ShipDate != null ? package.ShipDate.Value.ToString("MM/dd/yyyy") : string.Empty;
                var humanBarcodeWithSpaces = Regex.Replace(package.HumanReadableBarcode, ".{4}", "$0 ");
                var zplLabel = new StringBuilder(zplLabelTemplate);

                zplLabel.Replace("{0}", shippingMethodValues.FormattedShippingMethod)
                       .Replace("{1}", shippingMethodValues.MarkLetter)
                       .Replace("{2}", shippingMethod)
                       .Replace("{3}", package.ReturnName)
                       .Replace("{4}", package.FromFirm)
                       .Replace("{5}", package.ReturnAddressLine1)
                       .Replace("{6}", package.ReturnAddressLine2)
                       .Replace("{7}", $"{package.ReturnCity} {package.ReturnState} {package.ReturnZip}")
                       .Replace("{8}", package.ReturnPhone)
                       .Replace("{9}", shipDateForLabel)
                       .Replace("{10}", package.Weight.ToString())
                       .Replace("{11}", package.RecipientName)
                       .Replace("{12}", package.ToFirm)
                       .Replace("{13}", package.AddressLine1)
                       .Replace("{14}", package.AddressLine2)
                       .Replace("{15}", $"{package.City} {package.State} {package.FullZip}")
                       .Replace("{16}", package.FormattedBarcode)
                       .Replace("{17}", humanBarcodeWithSpaces)
                       .Replace("{18}", package.BinCode)
                       .Replace("{19}", package.UspsPermitNumber);

                return zplLabel.ToString().ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create USPS Single Package Label. Exception: {ex}");
                return string.Empty;
            }
        }

        private string GetZplLabelTemplate(string clientName, string shippingMethod)
        {
            var clientTemplate = zplConfiguration.CreatePackageTemplates.FirstOrDefault(x => x.ClientName == clientName);

            return shippingMethod switch
            {
                UspsParcelSelect => clientTemplate.PsOrPslw,
                UspsParcelSelectLightWeight => clientTemplate.PsOrPslw,
                UspsFirstClass => clientTemplate.FirstClassOrPriority,
                UspsPriority => clientTemplate.FirstClassOrPriority,
                _ => string.Empty
            };
        }

        private static (string FormattedShippingMethod, string MarkLetter) GenerateFormattedShippingMethodValues(string shippingMethod)
        {
            return shippingMethod switch
            {
                UspsParcelSelect => ($"USPS PARCEL SELECT", ""),
                UspsParcelSelectLightWeight => ($"USPS PS LIGHTWEIGHT", ""),
                UspsFirstClass => ($"USPS FIRST CLASS", "F"),
                UspsPriority => ($"USPS PRIORITY MAIL", "P"),
                UspsFcz => ($"USPS FIRST CLASS", "F"),
                _ => (string.Empty, string.Empty)
            };
        }

        public string GenerateErrorLabel(string errorLabelMessage, string siteName, string packageId, Package package = null)
        {
            try
            {
                var zplErrorLabelTemplate = zplConfiguration.AutoScanErrorLabelTemplate;

                var zplLabel = zplErrorLabelTemplate
                        .Replace("{PackageId}", packageId)
                        .Replace("{ErrorLabelMessage}", errorLabelMessage)
                        .Replace("{SiteName}", siteName)
                        .Replace("{FormattedLocalDateTime}", package != null ? package.LocalProcessedDate.ToString("yyyy-MM-dd HH:mm.ss") : string.Empty);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create USPS Single Package Error Label. Exception: {ex}");
                return string.Empty;
            }
        }

        public string GenerateThreeLineLabel(string lineOne, string lineTwo, string lineThree)
        {
            try
            {
                var zplErrorLabelTemplate = zplConfiguration.ThreeLineLabelTemplate;

                var zplLabel = zplErrorLabelTemplate
                        .Replace("{LineOne}", lineOne)
                        .Replace("{LineTwo}", lineTwo)
                        .Replace("{LineThree}", lineThree);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create ZPL Label. Exception: {ex}");
                return string.Empty;
            }
        }

        public string GenerateReturnLabel(Package package)
        {
            try
            {
                var zplReturnLabelTemplate = zplConfiguration.AutoScanReturnLabelTemplate;
                var zplLabel = zplReturnLabelTemplate
                   .Replace("{SiteName}", package.SiteName)
                   .Replace("{FormattedLocalDateTime}", package.LocalProcessedDate.ToString("yyyy-MM-dd HH:mm.ss"))
                   .Replace("{PackageId}", package.Id);

                return zplLabel.ToBase64();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create USPS Single Package Return Label. Exception: {ex}");
                return string.Empty;
            }
        }
    }
}
