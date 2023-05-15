using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models
{
	public class UpdateQueueMessage
	{
		public string Name { get; set; }
		public int DaysToLookback { get; set; }
	}
}
