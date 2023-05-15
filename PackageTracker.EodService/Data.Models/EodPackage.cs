using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageTracker.EodService.Data.Models
{
	public class EodPackage : EodRecord
	{
		[StringLength(100)] 
		public string PackageId { get; set; } // [index]
		[StringLength(100)]
		public string ContainerId { get; set; } // [index]
		[StringLength(100)]
		public string Barcode { get; set; }
		[StringLength(50)]
		public string SubClientName { get; set; }
		public bool IsPackageProcessed { get; set; } // [index]
		[ForeignKey("PackageDetailRecordId")]
		public int? PackageDetailRecordId { get; set; }
		public PackageDetailRecord PackageDetailRecord { get; set; }
		[ForeignKey("ReturnAsnRecordId")]
		public int? ReturnAsnRecordId { get; set; }
		public ReturnAsnRecord ReturnAsnRecord { get; set; }
		[ForeignKey("EvsPackageId")]
		public int? EvsPackageId { get; set; }
		public EvsPackage EvsPackage { get; set; }
		[ForeignKey("InvoiceRecordId")]
		public int? InvoiceRecordId { get; set; }
		public InvoiceRecord InvoiceRecord { get; set; }
		[ForeignKey("ExpenseRecordId")]
		public int? ExpenseRecordId { get; set; }
		public ExpenseRecord ExpenseRecord { get; set; }
    }
}
