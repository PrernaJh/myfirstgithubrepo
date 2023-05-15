using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class WebJobSettings
	{
		public bool IsEnabled { get; set; }
		public string JobTimer { get; set; }
		public Schedule[] Schedules { get; set; } // Active hours for job, can override in site or subclient.
		public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
	}

	public class Schedule
	{
		public string Days { get; set; }
		public string Hours { get; set; }
	}
}
