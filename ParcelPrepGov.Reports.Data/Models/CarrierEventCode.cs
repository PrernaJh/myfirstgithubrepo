using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class CarrierEventCode : UspsDataset
    {
        [StringLength(24)]
        public string ShippingCarrier { get; set; }

        [StringLength(10)]
        public string Code { get; set; }
        [StringLength(50)]
        public string Description { get; set; }
        public int IsStopTheClock { get; set; }
        public int IsUndeliverable { get; set; }
    }
}
