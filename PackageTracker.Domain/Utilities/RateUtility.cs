using PackageTracker.Data.Constants;
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace PackageTracker.Domain.Utilities
{
    public static class RateUtility
    {
        public static string AssignServiceTypeMapping(string serviceType, string carrier)
        {
            return serviceType.Trim() switch
            {
                "PS" => UspsParcelSelect,
                "PSLW" => UspsParcelSelectLightWeight,
                "FCM" => UspsFirstClass,
                "PRIORITY" => UspsPriority,
                "EXPRESS" => UspsPriorityExpress,
                "GROUND" => UpsGround,
                "2ND DAY AIR" => UpsSecondDayAir,
                "NEXT DAY AIR SAVER" => UpsNextDayAirSaver,
                "NEXT DAY AIR" => carrier == FedEx ? FedExPriorityOvernight : UpsNextDayAir,
                _ => serviceType,
            };
        }


        public static string AssignContainerServiceTypeMapping(string serviceType, string containerType)
        {
            return serviceType.Trim() switch
            {
                "PMOD" => containerType == ContainerConstants.ContainerTypeBag ? ContainerConstants.PmodBag : ContainerConstants.PmodPallet,
                _ => serviceType,
            };
        }
    }
}
