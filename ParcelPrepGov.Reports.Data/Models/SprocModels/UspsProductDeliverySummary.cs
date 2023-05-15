using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class UspsProductDeliverySummary
    {
        public string PRODUCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY3_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY3_PCS", "TOTAL_PCS" })]
		public decimal? DAY3_PCT { get; set; } // <= 3 days
		[DisplayFormatAttribute("COUNT")]
		public int? DAY4_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY4_PCS", "TOTAL_PCS" })]
		public decimal? DAY4_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY5_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY5_PCS", "TOTAL_PCS" })]
		public decimal? DAY5_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY6_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY6_PCS", "TOTAL_PCS" })]
		public decimal? DAY6_PCT { get; set; }
		[DisplayNameAttribute(">= 7 Days PCS")]
		[DisplayFormatAttribute("COUNT")]
		public int? DELAYED_PCS { get; set; } // >= 7 days
		[DisplayNameAttribute(">= 7 Days PCT")]
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DELAYED_PCS", "TOTAL_PCS" })]
		public decimal? DELAYED_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DELIVERED_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DELIVERED_PCS", "TOTAL_PCS" })]
		public decimal? DELIVERED_PCT { get; set; }
		[DisplayFormatAttribute("AVERAGE", 2, new string[] { "DELIVERED_PCS" })]
		public decimal? AVG_POSTAL_DAYS { get; set; }
		[DisplayFormatAttribute("AVERAGE", 2, new string[] { "DELIVERED_PCS" })]
		public decimal? AVG_CAL_DAYS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? NO_STC_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "NO_STC_PCS", "TOTAL_PCS" })]
		public decimal? NO_STC_PCT { get; set; }
	}
}
