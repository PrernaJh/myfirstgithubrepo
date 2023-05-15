using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class JobContainerDataset : Dataset
    {
        public int JobDatasetId { get; set; } // [ForeignKey] [Index]
        [StringLength(100)]
        public string JobBarcode { get; set; } // [Index]
        public int NumberOfContainers { get; set; }
        [StringLength(24)]
        public string Weight { get; set; }
    }
}
