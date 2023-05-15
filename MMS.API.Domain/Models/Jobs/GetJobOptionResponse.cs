using PackageTracker.Data.Models.JobOptions;
using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class GetJobOptionResponse
	{
		public GetJobOptionResponse()
		{
			CustomerLocations = new List<CustomerLocation>();
			JobContainerTypes = new List<JobContainerType>();
			MarkUpTypes = new List<MarkUpType>();
			MarkUps = new List<MarkUp>();
			Products = new List<Product>();
			PackageTypes = new List<PackageType>();
			PackageDescriptions = new List<PackageDescription>();
		}

		public List<CustomerLocation> CustomerLocations { get; set; }
		public List<JobContainerType> JobContainerTypes { get; set; }
		public List<MarkUpType> MarkUpTypes { get; set; }
		public List<MarkUp> MarkUps { get; set; }
		public List<Product> Products { get; set; }
		public List<PackageType> PackageTypes { get; set; }
		public List<PackageDescription> PackageDescriptions { get; set; }
	}
}
