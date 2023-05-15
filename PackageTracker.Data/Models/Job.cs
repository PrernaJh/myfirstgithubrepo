using PackageTracker.Data.Models.JobOptions;
using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class Job : Entity
	{
		public string JobBarcode { get; set; }
		public string SiteName { get; set; }
		public string ManifestDate { get; set; }
		public string ClientName { get; set; }
		public string SubClientName { get; set; }
		public string SubClientDescription { get; set; }
		public string MarkUpType { get; set; }
		public string MarkUp { get; set; }
		public string Product { get; set; }
		public string PackageType { get; set; }
		public string PackageDescription { get; set; }
		public decimal Length { get; set; }
		public decimal Width { get; set; }
		public decimal Depth { get; set; }
		public string MailTypeCode { get; set; }
		public string Reference { get; set; }
		public string BillOfLading { get; set; }
		public string SerialNumber { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }        
        public int LabelTypeId { get; set; }
		public List<JobContainer> JobContainers { get; set; } = new List<JobContainer>();
		public List<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
		public List<Event> JobEvents { get; set; } = new List<Event>();
	}
}