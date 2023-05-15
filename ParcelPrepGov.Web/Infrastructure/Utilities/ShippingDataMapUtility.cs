using PackageTracker.Data.Constants;
using ParcelPrepGov.Web.Models;
using System.Collections.Generic;

namespace ParcelPrepGov.Web.Infrastructure.Utilities
{
    public static class ShippingDataMapUtility
    {
        public const string UspsDescription = "USPS";
        public const string UpsDescription = "UPS";
        public const string FedExDescription = "FedEx";
        public const string UspsFirstClassDescription = "First Class";
        public const string UspsPriorityDescription = "Priority Mail";
        public const string UspsPslwDescription = "Parcel Select Lightweight";
        public const string UspsPsDescription = "Parcel Select";
        public const string UspsPriorityExpressDescription = "Priority Express";
        public const string UpsGround = "Ground";
        public const string UpsNextDayAir = "Next Day Air";
        public const string UpsNextDayAirSaver = "Next Day Air Saver";
        public const string UpsSecondDayAir = "Second Day Air";
        public const string FedExPriorityOvernight = "Priority Overnight";
        public const string FedExGround = "Ground";

        public static List<ShippingCarrierDisplayModel> GetShippingCarrierConstantsDescriptions()
        {
            var shippingCarrierConstantDescriptions = new List<ShippingCarrierDisplayModel>();

            var uspsShipCarrierViewModel = new ShippingCarrierDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.Usps,
                CarrierDescription = UspsDescription
            };

            shippingCarrierConstantDescriptions.Add(uspsShipCarrierViewModel);

            var upsShipCarrierViewModel = new ShippingCarrierDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.Ups,
                CarrierDescription = UpsDescription
            };

            shippingCarrierConstantDescriptions.Add(upsShipCarrierViewModel);

            var fedExShipCarrierViewModel = new ShippingCarrierDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.FedEx,
                CarrierDescription = FedExDescription
            };

            shippingCarrierConstantDescriptions.Add(fedExShipCarrierViewModel);

            return shippingCarrierConstantDescriptions;
        }

        public static List<ShippingMethodDisplayModel> GetShippingMethodDisplayConstantsByCarrier()
        {
            var shippingMethodsByCarrier = new List<ShippingMethodDisplayModel>();

            var uspsShipMethodDisplayModel = new ShippingMethodDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.Usps,
                ShippingMethods = new Dictionary<string, string> {
                    { ShippingMethodConstants.UspsFirstClass, UspsFirstClassDescription },
                    { ShippingMethodConstants.UspsParcelSelectLightWeight, UspsPslwDescription},
                    { ShippingMethodConstants.UspsParcelSelect, UspsPsDescription},
                    { ShippingMethodConstants.UspsPriority, UspsPriorityDescription },
                    { ShippingMethodConstants.UspsPriorityExpress, UspsPriorityExpressDescription }
                }
            };

            shippingMethodsByCarrier.Add(uspsShipMethodDisplayModel);

            var upsShipMethodDisplayModel = new ShippingMethodDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.Ups,
                ShippingMethods = new Dictionary<string, string> {
                    { ShippingMethodConstants.UpsGround, UpsGround },
                    { ShippingMethodConstants.UpsNextDayAir, UpsNextDayAir},
                    { ShippingMethodConstants.UpsNextDayAirSaver, UpsNextDayAirSaver },
                    { ShippingMethodConstants.UpsSecondDayAir, UpsSecondDayAir }
                }
            };

            shippingMethodsByCarrier.Add(upsShipMethodDisplayModel);

            var fedExShipMethodDisplayModel = new ShippingMethodDisplayModel
            {
                CarrierConstant = ShippingCarrierConstants.FedEx,
                ShippingMethods = new Dictionary<string, string> {
                    { ShippingMethodConstants.FedExGround, FedExGround },
                    { ShippingMethodConstants.FedExPriorityOvernight, FedExPriorityOvernight}
                }
            };

            shippingMethodsByCarrier.Add(fedExShipMethodDisplayModel);

            return shippingMethodsByCarrier;
        }
    }
}
