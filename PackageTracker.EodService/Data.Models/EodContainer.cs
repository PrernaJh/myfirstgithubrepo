using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageTracker.EodService.Data.Models
{
	public class EodContainer : EodRecord
	{
		[StringLength(100)] 
		public string ContainerId { get; set; } // [index]
		public bool IsContainerClosed { get; set; } // [index]
		[ForeignKey("ContainerDetailRecordId")]
		public int? ContainerDetailRecordId { get; set; } // [index]
		public ContainerDetailRecord ContainerDetailRecord { get; set; }
		[ForeignKey("PmodContainerDetailRecordId")]
		public int? PmodContainerDetailRecordId { get; set; } // [index]
		public PmodContainerDetailRecord PmodContainerDetailRecord { get; set; }
		[ForeignKey("ExpenseRecordId")]
		public int? ExpenseRecordId { get; set; } 
		public ExpenseRecord ExpenseRecord { get; set; }
		[ForeignKey("EvsContainerRecordId")]
		public int? EvsContainerRecordId { get; set; }
		public EvsContainer EvsContainerRecord { get; set; }
		[ForeignKey("EvsPackageRecordId")]
		public int? EvsPackageRecordId { get; set; }
		public EvsPackage EvsPackageRecord { get; set; }
	}
}
