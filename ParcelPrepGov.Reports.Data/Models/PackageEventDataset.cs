using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class PackageEventDataset : Dataset
    {
        public int PackageDatasetId { get; set; } // [ForeignKey] [Index]
        [StringLength(100)]
        public string PackageId { get; set; } // [Index]
        [StringLength(100)]
        public string TrackingNumber { get; set; }

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
