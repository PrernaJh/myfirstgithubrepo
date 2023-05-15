using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class ReprintPackageResponse
	{
		public ReprintPackageResponse()
		{
			LabelFieldValues = new List<LabelFieldValue>();
		}

		public string Carrier { get; set; }
		public string ServiceType { get; set; }
		public string Bin { get; set; }
		public string Barcode { get; set; }
		public string HumanReadableBarcode { get; set; }
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; }
		public string Base64Label { get; set; }
        public string RecipientName { get; set; }
        public string FullAddress { get; set; }
        public bool IsReprintDisabled { get; set; }
    }
}
