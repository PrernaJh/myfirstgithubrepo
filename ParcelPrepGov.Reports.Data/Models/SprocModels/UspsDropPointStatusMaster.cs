using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class UspsDropPointStatusMaster
	{
		public string ID { get; set; }
		public string CUST_LOCATION { get; set; }
		[DisplayFormatAttribute("DATE")]
		public DateTime? MANIFEST_DATE { get; set; }
		public string MANIFEST_DATE_STRING
		{
			get
			{
				return MANIFEST_DATE == null ? string.Empty : MANIFEST_DATE.Value.ToString("MM/dd/yyyy");
			}
		}
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		public string PRODUCT { get; set; } 
		public string CARRIER { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_BAGS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PCS_NO_STC { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "PCS_NO_STC", "TOTAL_PCS" })]
		public decimal? PCT_NO_STC { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PCS_NO_SCAN { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "PCS_NO_SCAN", "TOTAL_PCS" })]
		public decimal? PCT_NO_SCAN { get; set; }
	}
}
