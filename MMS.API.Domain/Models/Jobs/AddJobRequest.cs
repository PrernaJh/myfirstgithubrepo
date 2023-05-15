using PackageTracker.Data.Models.JobOptions;
using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class AddJobRequest
	{
		public AddJobRequest()
		{
			JobContainers = new List<JobContainer>();
		}

		public string SiteName { get; set; }
		public string ManifestDate { get; set; }
		public CustomerLocation CustomerLocation { get; set; }
		public MarkUpType MarkUpType { get; set; }
		public MarkUp MarkUp { get; set; }
		public Product Product { get; set; }
		public PackageType PackageType { get; set; }
		public PackageDescription PackageDescription { get; set; }
		public string Reference { get; set; }
		public string BillOfLading { get; set; }
		public string SerialNumber { get; set; }
        public bool PrintPackageLabel { get; set; }
        public List<JobContainer> JobContainers { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
