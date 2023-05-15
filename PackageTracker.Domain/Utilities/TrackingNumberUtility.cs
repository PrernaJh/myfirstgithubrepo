using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;

namespace PackageTracker.Domain.Utilities
{
    // FedEx tracking number is different than the package.Barcode
    public static class TrackingNumberUtility
    {
        public static string GetTrackingNumber(Package package)
        {
            var trackingNumber = string.Empty;

            if (package.ShippingCarrier == ShippingCarrierConstants.FedEx
                && StringHelper.Exists(package.AdditionalShippingData?.TrackingNumber))
            {
                trackingNumber = package.AdditionalShippingData.TrackingNumber;
            }
            else
            {
                trackingNumber = package.Barcode;
            }

            return trackingNumber;
        }
       
        public static string GetHumanReadableTrackingNumber(Package package)
        {
            var trackingNumber = string.Empty;

            if (package.ShippingCarrier == ShippingCarrierConstants.FedEx &&
                StringHelper.Exists(package.AdditionalShippingData?.TrackingNumber))
            {
                trackingNumber = package.AdditionalShippingData.TrackingNumber;
            }
            else if (package.ShippingCarrier == ShippingCarrierConstants.Usps)
            {
                trackingNumber = package.HumanReadableBarcode;
            }
            else
            {
                trackingNumber = package.Barcode;
            }

            return trackingNumber;
        }
    }
}
