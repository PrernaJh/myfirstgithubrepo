using System;
using ParcelPrepGov.Reports.Attributes;


namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class UspsDPSByContainerMaster
	{
		public string ID { get; set; }		
		public string SITE { get; set; }

		[DisplayFormatAttribute("DATE")]
		public DateTime? MANIFEST_DATE { get; set; }

		public string MANIFEST_DATE_STRING
		{
			get
			{
				return MANIFEST_DATE == null ? string.Empty : MANIFEST_DATE.Value.ToString("MM/dd/yyyy");
			}
		}
		public string CONTAINER_ID { get; set; }
		public string BIN_CODE { get; set; }
		public string DROP_SHIP_SITE_KEY { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		public string PRODUCT { get; set; }
		public string CARRIER { get; set; }
		public string CONTAINER_TYPE { get; set; }
		public string TRACKING_NUMBER { get; set; }
		public string LAST_KNOWN_DATE { get; set; }
		public string LAST_KNOWN_DESCRIPTION { get; set; }
		public string LAST_KNOWN_LOCATION { get; set; }
		public string LAST_KNOWN_ZIP { get; set; }

		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_PCS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PCS_NO_STC { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "PCS_NO_STC", "TOTAL_PCS" })]
		public decimal PCT_NO_STC { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? PCS_NO_SCAN { get; set; }
		[DisplayFormatAttribute("PERCENT", 2, new string[] { "PCS_NO_SCAN", "TOTAL_PCS" })]
		public decimal? PCT_NO_SCAN { get; set; }
	}
}
