using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class CreateContainerRequest
	{
		public Site Site { get; set; }
		public Bin Bin { get; set; }
		public ZoneMap ZoneMap { get; set; }
		public bool IsReplacement { get; set; }
		public bool IsSecondaryCarrier { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public string ContainerIdToReplace { get; set; }
		public string HumanReadableBarcodeToReplace { get; set; }
		public string SerialNumberToReplace { get; set; }
		public List<Event> EventsToReplace { get; set; } = new List<Event>();
		public string OperationalContainerId { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
    }
}
