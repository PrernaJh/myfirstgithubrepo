using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models.Returns
{
	public class ReturnPackageResponse
	{
		public ReturnPackageResponse()
		{
			LabelFieldValues = new List<LabelFieldValue>();
		}

		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; }
	}
}