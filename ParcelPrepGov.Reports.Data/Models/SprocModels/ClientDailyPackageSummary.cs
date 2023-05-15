using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class ClientDailyPackageSummary
	{
		[DisplayFormatAttribute("DATE")]
		public DateTime? MANIFEST_DATE { get; set; }
		public string MANIFEST_DATE_STRING
		{
			get
			{
				return MANIFEST_DATE == null ? string.Empty : MANIFEST_DATE.Value.ToString("MM/dd/yyyy");
			}
		}
		public string CUST_LOCATION { get; set; }
		public string PRODUCT { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PIECES { get; set; }
		[DisplayFormatAttribute("DECIMAL", 2)]
		public decimal? WEIGHT { get; set; }
	}
}
