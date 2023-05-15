using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class UspsCarrierDetailMaster
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
		[DisplayNameAttribute("CONTAINER_TYPE")]//This attribute overrides the column header name in the excel
		[DisplayFormatAttribute("CONTAINER TYPE")]//This attribute overrides the column header name in the UI grid
		public string PRODUCT { get; set; }
		public string CARRIER { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? CONT_NO_SCAN { get; set; }
		[DisplayFormatAttribute("COUNT")]
		public int? TOTAL_CONT { get; set; }
	}
}
