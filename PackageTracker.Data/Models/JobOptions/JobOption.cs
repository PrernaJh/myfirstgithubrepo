using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models.JobOptions
{
	public class JobOption : Entity
	{
		public JobOption()
		{
			CustomerLocations = new List<CustomerLocation>();
			MarkUpTypes = new List<MarkUpType>();
			MarkUps = new List<MarkUp>();
			Products = new List<Product>();
			PackageTypes = new List<PackageType>();
			PackageDescriptions = new List<PackageDescription>();
			JobContainerTypes = new List<JobContainerType>();
		}

		public string SiteName { get; set; }
		public bool IsEnabled { get; set; }
		public DateTime StartDate { get; set; }
		public List<CustomerLocation> CustomerLocations { get; set; }
		public List<MarkUpType> MarkUpTypes { get; set; }
		public List<MarkUp> MarkUps { get; set; }
		public List<Product> Products { get; set; }
		public List<PackageType> PackageTypes { get; set; }
		public List<PackageDescription> PackageDescriptions { get; set; }
		public List<JobContainerType> JobContainerTypes { get; set; }
	}
}
