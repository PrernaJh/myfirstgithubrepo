using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageTracker.EodService.Data.Models
{
	public class EodRecord
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }
		public DateTime LocalProcessedDate { get; set; } // [Index]
		public DateTime CreateDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		[StringLength(36)]
		public string CosmosId { get; set; } // [Index]   
		public DateTime CosmosCreateDate { get; set; }
		[StringLength(50)]
		public string SiteName { get; set; } // [Index]
	}
}
