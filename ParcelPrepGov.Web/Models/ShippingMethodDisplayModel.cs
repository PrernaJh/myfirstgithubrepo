using System.Collections.Generic;

namespace ParcelPrepGov.Web.Models
{
    public class ShippingMethodDisplayModel
    {
        public string CarrierConstant { get; set; }
        public Dictionary<string, string> ShippingMethods { get; set; }
    }
}
