namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class PostalPerformance3Digit
	{
		public string ID { get; set; }
		public string ID3 { get; set; }
		public string CMOP { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		public string USPS_AREA { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ZIP3 { get; set; }
		public int? TOTAL_PCS { get; set; }
		public int? TOTAL_PCS_STC { get; set; }
		public int? TOTAL_PCS_NO_STC { get; set; }
		public decimal? STC_SCAN_PCT { get; set; }
		public decimal? AVG_DEL_DAYS { get; set; }
		public int? DAY0_PCS { get; set; }
		public decimal? DAY0_PCT { get; set; }
		public int? DAY1_PCS { get; set; }
		public decimal? DAY1_PCT { get; set; }
		public int? DAY2_PCS { get; set; }
		public decimal? DAY2_PCT { get; set; }
		public int? DAY3_PCS { get; set; }
		public decimal? DAY3_PCT { get; set; }
		public int? DAY4_PCS { get; set; }
		public decimal? DAY4_PCT { get; set; }
		public int? DAY5_PCS { get; set; }
		public decimal? DAY5_PCT { get; set; }
		public int? DAY6_PCS { get; set; }
		public decimal? DAY6_PCT { get; set; }
	}
}
