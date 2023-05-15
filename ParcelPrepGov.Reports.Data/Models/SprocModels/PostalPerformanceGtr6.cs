using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class PostalPerformanceGtr6
	{
		public string ID { get; set; }
		public string ID3 { get; set; }
		public string ID5 { get; set; }
		public DateTime? DATE_SHIPPED { get; set; }
		public string MANIFEST_DATE_STRING
		{
			get
			{
				return DATE_SHIPPED == null ? string.Empty : DATE_SHIPPED.Value.ToString("MM/dd/yyyy");
			}
		}
		public string CUST_LOCATION { get; set; }
		public string PACKAGE_ID { get; set; }
		public string TRACKING_NUMBER { get; set; }
		public string DEST_CITY { get; set; }
		public string DEST_STATE { get; set; }
		public string DEST_ZIP { get; set; }
		public string PRODUCT { get; set; }
		public string CARRIER { get; set; }
		public string USPS_AREA { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string LAST_KNOWN_DESC { get; set; }
		public DateTime? LAST_KNOWN_DATE { get; set; }
		public string LAST_KNOWN_LOCATION { get; set; }
		public string LAST_KNOWN_ZIP { get; set; }
		public string VISN { get; set; }
		public string PHARM_DIV_NO { get; set; }
		public string PHARM_DIV { get; set; }
	}
}
