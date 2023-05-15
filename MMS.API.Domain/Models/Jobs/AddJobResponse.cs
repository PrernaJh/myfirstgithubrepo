using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class AddJobResponse
	{
		public AddJobResponse()
		{
			LabelFieldValues = new List<LabelFieldValue>();
		}

		public string JobBarcode { get; set; }
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; }
	}
}
