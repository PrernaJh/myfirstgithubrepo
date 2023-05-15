using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class DailyRevenueFile
	{
		public DateTime? MANIFEST_DATE { get; set; }

		public string CUST_NAME { get; set; }
		public string PRODUCT { get; set; }
		public string TRACKING_TYPE { get; set; }
		public int PIECES { get; set; }
		public decimal ASSESSORIAL_COST { get; set; }
		public decimal COST { get; set; }
		public decimal TOTAL_COST { get; set; }
	}
}
