using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class ShippingContainerEventDataset : Dataset
    {
        public int ShippingContainerDatasetId { get; set; } // [ForeignKey] [Index]
        [StringLength(32)]
        public string ContainerId { get; set; } // [Index]

        public int EventId { get; set; }
        [StringLength(24)]
        public string EventType { get; set; }
        [StringLength(24)]
        public string EventStatus { get; set; }
        [StringLength(180)]
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime LocalEventDate { get; set; }

        [StringLength(32)]
        public string Username { get; set; }
        [StringLength(32)]
        public string MachineId { get; set; }
    }
}
