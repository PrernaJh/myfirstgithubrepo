using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class UspsGtr5Detail
	{
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
		public string PACKAGE_ID { get; set; }
		public static string HOST { get; set; }
		public string PACKAGE_ID_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
			}
		}
		public string TRACKING_NUMBER { get; set; }
		public string TRACKING_NUMBER_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(PACKAGE_CARRIER, TRACKING_NUMBER);
			}
		}
		public string DEST_CITY { get; set; }
		public string DEST_STATE { get; set; }
		public string DEST_ZIP { get; set; }
		public string PRODUCT { get; set; }
		public string USPS_AREA { get; set; }
		public string CARRIER { get; set; }
        [ExcelIgnore]
		public string PACKAGE_CARRIER { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		[DisplayFormatAttribute("DATE_TIME")]
		public DateTime? LAST_KNOWN_DATE { get; set; }
		public string LAST_KNOWN_DATE_STRING
		{
			get
			{
				return LAST_KNOWN_DATE == null ? string.Empty : LAST_KNOWN_DATE.Value.ToString("MM/dd/yyyy hh:mm tt");
			}
		}
		public string LAST_KNOWN_DESC { get; set; }
		public string LAST_KNOWN_LOCATION { get; set; }
		public string LAST_KNOWN_ZIP { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? POSTAL_DAYS { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? CAL_DAYS { get; set; }
		public string VISN { get; set; }
		public string MEDICAL_CENTER_NO { get; set; }
		public string MEDICAL_CENTER_NAME { get; set; }

	}
}

