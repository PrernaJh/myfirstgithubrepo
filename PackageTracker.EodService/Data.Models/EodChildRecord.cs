using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageTracker.EodService.Data.Models
{
	public class EodChildRecord
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }
		[StringLength(36)]
		public string CosmosId { get; set; } // [Index]   
	}
}

