using PackageTracker.Data.Constants;

namespace PackageTracker.Domain.Utilities
{
    public static class BinFileUtility
    {
        public static string MapShippingCarrier(string shippingCarrier)
        {
            return shippingCarrier.Trim() switch
            {
                "FedEx" => ContainerConstants.FedExCarrier,
                "USPS" => ContainerConstants.UspsCarrier,
                "On Trac" => ContainerConstants.OnTracCarrier,
                "LSO" => ContainerConstants.LsoCarrier,
                "1st Choice" => ContainerConstants.FirstChoice,
                "Mark IV" => ContainerConstants.MarkIV,
                "United Delivery Service" => ContainerConstants.UnitedDeliveryService,
                "CX" => ContainerConstants.Cx,
                "WALTCO INC - GREEN BAY" => ContainerConstants.Waltco,
                "ADL" => ContainerConstants.Adl,
                "GENCO-Charleston" => ContainerConstants.GencoCharleston,
                _ => shippingCarrier,
            };
        }

        public static string MapShippingMethod(string shippingMethod)
        {
            return shippingMethod.Trim() switch
            {
                "Regional Carrier" => ContainerConstants.RegionalCarrier,
                "FedEx Express" => ContainerConstants.FedExExpress,
                "FedEx Ground" => ContainerConstants.FedExGround,
                "LTL" => ContainerConstants.Ltl,
                "First Class" => ContainerConstants.UspsFirstClass,
                "PMOD Bag" => ContainerConstants.UspsPmodBag,
                "PMOD Pallet" => ContainerConstants.UspsPmodPallet,
                "Priority" => ContainerConstants.UspsPriority,
                _ => shippingMethod,
            };
        }
    }
}
