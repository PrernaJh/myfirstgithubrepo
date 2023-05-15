using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class WeeklyInvoiceFile
	{
		public string SUBCLIENT { get; set; }
		public DateTime? BILLING_DATE { get; set; }
		public string PACKAGE_ID { get; set; }
		public string TRACKINGNUMBER { get; set; }
		public string BILLING_REFERENCE { get; set; }
		public string BILLING_PRODUCT { get; set; }
		public string MARKUP_DESC { get; set; }
		public string MARKUP_TYPE_DESC { get; set; }
		public decimal BILLING_WEIGHT { get; set; }
		public int? Zone { get; set; }
		public string SIG_COST { get; set; }
		public string PIECE_COST { get; set; }
		public decimal? BILLING_COST { get; set; }
		public decimal? Weight { get; set; }
		public string TOTAL_CUST { get; set; }
	}
}
