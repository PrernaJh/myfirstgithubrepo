using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class CarrierDetail
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
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string ENTRY_UNIT_TYPE { get; set; }
		[DisplayFormatAttribute("CONTAINER TYPE")]		//This attribute overrides the column header name in the UI grid
		public string CONTAINER_TYPE { get; set; }
		public string CARRIER { get; set; }


		[DisplayNameAttribute("TOTAL_PCS")]				//This attribute overrides the column header name in the excel
		[DisplayFormatAttribute("TOTAL PCS")]			//This attribute overrides the column header name in the UI grid
		public int? TOTAL_PIECES { get; set; }

		[DisplayNameAttribute("PCS_NO_SCAN")]			//This attribute overrides the column header name in the excel
		[DisplayFormatAttribute("PCS NO SCAN")]			//This attribute overrides the column header name in the UI grid
		public int? TOTAL_PCS_NO_SCAN { get; set; }

        public string BIN_CODE { get; set; }

        public string CONTAINER_ID { get; set; }
		public string CONTAINER_WEIGHT { get; set; }
		public string CONTAINER_TRACKING_NUMBER { get; set; }

		[DisplayNameAttribute("SITE_KEY")]				//This attribute overrides the column header name in the excel
		[DisplayFormatAttribute("SITE KEY")]			//This attribute overrides the column header name in the UI grid
		public string DROP_SHIP_SITE_KEY { get; set; }

		[DisplayFormatAttribute("LAST KNOWN DATE")]
		public DateTime? LAST_KNOWN_DATE { get; set; }
		public string LAST_KNOWN_DATE_STRING
		{
			get
			{
				return LAST_KNOWN_DATE == null ? string.Empty : LAST_KNOWN_DATE.Value.ToString("MM/dd/yyyy");
			}
		}

		public string LAST_KNOWN_DESC { get; set; }
		public string LAST_KNOWN_LOCATION { get; set; }
		public string LAST_KNOWN_ZIP { get; set; }

	}
}
