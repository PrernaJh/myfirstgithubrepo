using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class UspsCarrierDetailDetail
	{
		public string ID { get; set; }
		public string LOCATION { get; set; }
		[DisplayFormatAttribute("DATE")]
		public DateTime? MANIFEST_DATE { get; set; }
		public string MANIFEST_DATE_STRING
		{
			get
			{
				return MANIFEST_DATE == null ? string.Empty : MANIFEST_DATE.Value.ToString("MM/dd/yyyy");
			}
		}
		public string ENTRY_UNIT_KEY { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		public string PRODUCT { get; set; }
		public string CARRIER { get; set; }
		public string TRACKING_NUMBER { get; set; }
		public string TRACKING_NUMBER_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(CARRIER, TRACKING_NUMBER);
			}
		}
		public string CONTAINER_LABEL { get; set; }
		[DisplayFormatAttribute("DECIMAL", 2)]
		public string WEIGHT { get; set; }
		[DisplayFormatAttribute("DATE_TIME")]
		public DateTime? LAST_KNOWN_DATE { get; set; }
		public string LAST_KNOWN_DESC { get; set; }
		public string LAST_KNOWN_LOCATION { get; set; }
		public string LAST_KNOWN_ZIP { get; set; } 
	}
}
