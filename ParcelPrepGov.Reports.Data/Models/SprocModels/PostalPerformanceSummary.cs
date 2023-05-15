using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class PostalPerformanceSummary
	{
		public string ID { get; set; }
		public string CUST_LOCATION { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		public string USPS_PRODUCT { get; set; }
		public string USPS_AREA { get; set; }
        public string ENTRY_UNIT_CSZ { get; set; }
        public string ENTRY_UNIT_NAME { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS_STC { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS_NO_STC { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "TOTAL_PCS_STC", "TOTAL_PCS" })]
		public decimal? STC_SCAN_PCT { get; set; }
		[DisplayFormatAttribute("DECIMAL", 2)]
		public decimal? AVG_DEL_DAYS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY0_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY0_PCS", "TOTAL_PCS" })]
		public decimal? DAY0_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY1_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY1_PCS", "TOTAL_PCS" })]
		public decimal? DAY1_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY2_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY2_PCS", "TOTAL_PCS" })]
		public decimal? DAY2_PCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? DAY3_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "DAY3_PCS", "TOTAL_PCS" })]
		public decimal? DAY3_PCT { get; set; }
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
	}
}
