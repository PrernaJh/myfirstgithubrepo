using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class CloseContainerResponse
	{
		public string ContainerId { get; set; }
		public string BinCode { get; set; }
		public string ContainerType { get; set; }
		public string Weight { get; set; }
		public bool IsSecondaryCarrier { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
		public bool IsSuccessful { get; set; }
		public string Message { get; set; }
	}
}
