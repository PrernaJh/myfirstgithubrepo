using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class ReprintClosedContainerResponse
	{
		public string ContainerId { get; set; }
		public string ContainerType { get; set; }
		public string Weight { get; set; }
		public string HumanReadableBarcode { get; set; }
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
	}
}
