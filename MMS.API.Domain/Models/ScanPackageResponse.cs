using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class ScanPackageResponse
	{
		public ScanPackageResponse()
		{
			LabelFieldValues = new List<LabelFieldValue>();
		}

		public string PackageId { get; set; }
		public bool Succeeded { get; set; }
        public bool PrintLabel { get; set; }
        public bool IsQCRequired { get; set; }

		public DateTime ResponseDate { get; set; }
		public string RecipientName { get; set; }
		public string FullAddress { get; set; }
		public string Weight { get; set; }
		public string Carrier { get; set; }
		public string ServiceType { get; set; }
		public string Bin { get; set; }
		public string Barcode { get; set; }
		public string HumanReadableBarcode { get; set; }
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; }
		public string Base64Label { get; set; }
		public string Message { get; set; }
		public string ErrorLabelMessage { get; set; }
	}
}

