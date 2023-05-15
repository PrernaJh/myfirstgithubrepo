using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ParcelPrepGov.Reports.Models
{
    public class ShippingContainerDataset : Dataset
    {
		[StringLength(36)]
		public string ContainerId { get; set; } // [Index]
		[StringLength(24)]
		public string Status { get; set; } // [Index]

		[StringLength(24)]
		public string BinCode { get; set; }
		[StringLength(36)]
		public string BinActiveGroupId { get; set; }
		[StringLength(24)]
		public string BinCodeSecondary { get; set; }
		[StringLength(60)]
		public string ShippingMethod { get; set; }
		[StringLength(24)]
		public string ShippingCarrier { get; set; }
		[StringLength(24)]
		public string ContainerType { get; set; }
		[StringLength(2)]
		public string Grouping { get; set; }
		[StringLength(24)]
		public string Weight { get; set; }
		[StringLength(100)]
		public string UpdatedBarcode { get; set; } // [Index]
		[StringLength(32)]
		public bool IsSecondaryCarrier { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public bool IsRural { get; set; }
		public bool IsOutside48States { get; set; }
		public decimal Cost { get; set; }
		public decimal Charge { get; set; }
		public int Zone { get; set; }
		public DateTime ProcessedDate { get; set; }
		public DateTime LocalProcessedDate { get; set; }
		public DateTime? StopTheClockEventDate { get; set; } // [Index]
		public DateTime? LastKnownEventDate { get; set; } // [Index]
        public DateTime? LocalCreateDate { get; set; }

        [StringLength(120)]
		public string LastKnownEventDescription { get; set; }
		[StringLength(120)]
		public string LastKnownEventLocation { get; set; }
		[StringLength(10)]
		public string LastKnownEventZip { get; set; }

		public string Username { get; set; }
		[StringLength(32)]
		public string MachineId { get; set; }

		public List<ShippingContainerEventDataset> Events { get; set; } = new List<ShippingContainerEventDataset>();
		public List<TrackPackageDataset> Tracking { get; set; } = new List<TrackPackageDataset>();
	}
}
