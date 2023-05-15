using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class PackageInquiry
    {
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }
		[StringLength(24)]
		public string SiteName { get; set; } // [Index]       
		public int InquiryId { get; set; } // [Index]
        [StringLength(50)]
        public string ServiceRequestNumber { get; set; }
        public int PackageDatasetId { get; set; } // [ForeignKey] [Index]
		[StringLength(50)]
        public string PackageId { get; set; } // [Index]       
    }
}
