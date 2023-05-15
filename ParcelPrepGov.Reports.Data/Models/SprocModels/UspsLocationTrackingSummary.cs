using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class UspsLocationTrackingSummary
    {
        public string LOCATION { get; set; }
        [DisplayFormatAttribute("COUNT")]
        public int? TOTAL_PCS { get; set; }
        [DisplayFormatAttribute("COUNT")]
        public int? DELIVERED_PCS { get; set; }
        [DisplayFormatAttribute("PERCENT", 2, new string[] { "DELIVERED_PCS", "TOTAL_PCS" })]
        public decimal? DELIVERED_PCT { get; set; }
        [DisplayFormatAttribute("AVERAGE", 2, new string[] { "DELIVERED_PCS" })]
        public decimal? AVG_POSTAL_DAYS { get; set; }
        [DisplayFormatAttribute("AVERAGE", 2, new string[] { "DELIVERED_PCS" })]
        public decimal? AVG_CAL_DAYS { get; set; }
        [DisplayFormatAttribute("COUNT")]
        public int? SIGNATURE_PCS { get; set; }
        [DisplayFormatAttribute("COUNT")]
        public int? SIGNATURE_DELIVERED_PCS { get; set; }
        [DisplayFormatAttribute("PERCENT", 2, new string[] { "SIGNATURE_DELIVERED_PCS", "SIGNATURE_PCS" })]
        public decimal? SIGNATURE_DELIVERED_PCT { get; set; }
        [DisplayFormatAttribute("AVERAGE", 2, new string[] { "SIGNATURE_DELIVERED_PCS" })]
        public decimal? SIGNATURE_AVG_POSTAL_DAYS { get; set; }
        [DisplayFormatAttribute("AVERAGE", 2, new string[] { "SIGNATURE_DELIVERED_PCS" })]
        public decimal? SIGNATURE_AVG_CAL_DAYS { get; set; }
        [DisplayFormatAttribute("COUNT")]
        public int? NO_STC_PCS { get; set; }
        [DisplayFormatAttribute("PERCENT", 2, new string[] { "NO_STC_PCS", "TOTAL_PCS" })]
        public decimal? NO_STC_PCT { get; set; }
   }
}
