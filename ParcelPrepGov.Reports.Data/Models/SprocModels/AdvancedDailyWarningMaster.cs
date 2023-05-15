using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class AdvancedDailyWarningMaster	{
		public string ID { get; set; }
		public string SITE_CODE { get; set; }
		[DisplayFormatAttribute("DATE")]
		public DateTime? DATE_SHIPPED { get; set; }
		public string MANIFEST_DATE_STRING
		{
			get
			{
				return DATE_SHIPPED == null ? string.Empty : DATE_SHIPPED.Value.ToString("MM/dd/yyyy");
			}
		}
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PCS_NO_SCAN { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "PCS_NO_SCAN", "TOTAL_PCS" })]
		public decimal? PCT_NO_SCAN { get; set; }
	}
}
