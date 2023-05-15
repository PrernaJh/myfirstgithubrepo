using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class Dataset
    {
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }
		public DateTime DatasetCreateDate { get; set; } // [Index]
		public DateTime DatasetModifiedDate { get; set; } // [Index]
		[StringLength(36)]
		public string CosmosId { get; set; } // [Index]   
		public DateTime CosmosCreateDate { get; set; }
		[StringLength(24)]
		public string SiteName { get; set; } // [Index]

	}
}
