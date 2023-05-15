using Microsoft.Extensions.Configuration;
using PackageTracker.Data.Constants;
using PackageTracker.Identity.Data.Models;

namespace PackageTracker.Domain.Utilities
{
    public static class HyperLinkFormatter
    {
        public static string FormatHelpDeskHyperLink(string host, string inquiryId,
            string packageId, string shippingTrackingNumber, string siteName, int? packageDatasetId, string carrier, bool validateInputs)
        {
            var url = string.Empty;
            if (inquiryId == null)
            {
                url = string.Empty;                    
            }
            else
            {
                url = $"{host}/Ticket/{inquiryId}";
            }

            return url;
        }
        public static string FormatTrackingHyperLink(string carrier, string trackingNumber)
        {
            string url = string.Empty;
            if (carrier == ShippingCarrierConstants.Usps)
            {
                url = $"https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1={trackingNumber}";
            }
            else if (carrier == ShippingCarrierConstants.FedEx)
            {
                url = $"https://www.fedex.com/fedextrack/no-results-found?trknbr={trackingNumber}";
            }
            else if (carrier == ShippingCarrierConstants.Ups)
            {
                url = $"https://www.ups.com/track?loc=null&tracknum={trackingNumber}&requester=WT/trackdetails";
            }
            return url;
        }
        public static string FormatPackageHyperLink(string host, string packageId)
        {
            var url = host == null
                ? $"/PackageSearch?packageId={packageId}"
                : $"https://{host}/PackageSearch?packageId={packageId}";
            return url;
        }
        public static string FormatContainerHyperLink(string host, string containerId)
        {
            var url = host == null
                ? $"/ContainerSearch?containerId={containerId}"
                : $"https://{host}/ContainerSearch?containerId={containerId}";
            return url;
        }    
    }
}
