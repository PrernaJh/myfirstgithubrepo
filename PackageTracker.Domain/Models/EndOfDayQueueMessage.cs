using System;

namespace PackageTracker.Domain.Models
{
	public class EndOfDayQueueMessage
	{
		public string SiteName { get; set; }
		public string Username { get; set; }
		public DateTime TargetDate { get; set; }
		public bool UseTargetDate { get; set; }
		public string Extra { get; set; }
	}
}
