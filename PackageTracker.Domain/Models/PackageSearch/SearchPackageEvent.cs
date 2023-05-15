using System;

namespace PackageTracker.Domain.Models.PackageSearch
{
	public class SearchPackageEvent
	{
		public int EventId { get; set; }
		public string EventType { get; set; }
		public string EventStatus { get; set; }
		public string Description { get; set; }
		public string Location { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
		public DateTime EventDate { get; set; }
		public string ContainerId { get; set; }
	}
}
