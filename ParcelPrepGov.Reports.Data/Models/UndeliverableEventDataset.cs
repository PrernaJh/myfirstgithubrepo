using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class UndeliverableEventDataset : Dataset
    {
		[StringLength(100)]
		public string PackageId { get; set; }
		public int? PackageDatasetId { get; set; } // [ForeignKey] [Index]

		public DateTime EventDate { get; set; }
		[StringLength(24)]
		public string EventCode { get; set; }
		[StringLength(120)]
		public string EventDescription { get; set; }
		[StringLength(120)]
		public string EventLocation { get; set; }
		[StringLength(10)]
		public string EventZip { get; set; }
	}
}
