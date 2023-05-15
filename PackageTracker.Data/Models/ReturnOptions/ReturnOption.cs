using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models.ReturnOptions
{
	public class ReturnOption : Entity
	{
		public ReturnOption()
		{
			ReturnReasons = new List<ReturnReason>();
			ReasonDescriptions = new List<ReasonDescription>();
		}

		public string SiteName { get; set; }
		public bool IsEnabled { get; set; }
		public DateTime StartDate { get; set; }
		public List<ReturnReason> ReturnReasons { get; set; }
		public List<ReasonDescription> ReasonDescriptions { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
