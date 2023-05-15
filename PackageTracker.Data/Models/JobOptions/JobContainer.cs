using System.Collections.Generic;

namespace PackageTracker.Data.Models.JobOptions
{
	public class JobContainer
	{
		public JobContainer()
		{
			JobContainerTypes = new List<JobContainerType>();
		}

		public List<JobContainerType> JobContainerTypes { get; set; }
		public int NumberOfContainers { get; set; }
		public string Weight { get; set; }
	}
}
