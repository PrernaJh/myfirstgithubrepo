using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class TrackPackageDataset : Dataset
	{
		[StringLength(100)]
		public string PackageId { get; set; } // [Index]
		public int? PackageDatasetId { get; set; } // [ForeignKey] [Index]

		[StringLength(36)]
		public string ShippingContainerId { get; set; } // [Index]
		public int? ShippingContainerDatasetId { get; set; } // [ForeignKey] [Index]

		[StringLength(24)]
		public string ShippingCarrier { get; set; }
		[StringLength(100)]
		public string TrackingNumber { get; set; } // [Index]
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
