using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class RecallStatus : UspsDataset
    {
        [StringLength(24)]
        public string Status { get; set; }
        [StringLength(80)]
        public string Description { get; set; }
    }
}
