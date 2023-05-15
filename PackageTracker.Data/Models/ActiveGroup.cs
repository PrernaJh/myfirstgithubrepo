using System;

namespace PackageTracker.Data.Models
{
	public class ActiveGroup : Entity
	{
		public string Name { get; set; }
		public string AddedBy { get; set; }
		public string ActiveGroupType { get; set; }
		public bool IsEnabled { get; set; }
		public string Filename { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public ServiceOverride ServiceOverride { get; set; }
	}
}
