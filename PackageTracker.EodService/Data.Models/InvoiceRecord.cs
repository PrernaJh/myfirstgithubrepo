using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class InvoiceRecord : EodChildRecord
	{
		[StringLength(50)]
		public string SubClientName { get; set; }
		[StringLength(100)]
		public string PackageId { get; set; } // package.PackageId
		[StringLength(10)]
		public string BillingDate { get; set; } // package.LocalProcessedDate MM/DD/YYYY
		[StringLength(100)]
		public string TrackingNumber { get; set; } // Barcode
		[StringLength(10)]
		public string BillingReference1 { get; set; } // VisnSiteParent
		[StringLength(60)]
		public string BillingProduct { get; set; } // ShippingMethod
		[StringLength(32)]
		public string BillingWeight { get; set; } // BillingWeight
		[StringLength(10)]
		public string Zone { get; set; } // Zone
		[StringLength(24)]
		public string SigCost { get; set; } // ExtraCost
		[StringLength(24)]
		public string BillingCost { get; set; } // Charge
		[StringLength(32)]
		public string Weight { get; set; } // Weight
		[StringLength(24)]
		public string TotalCost { get; set; } // ExtraCost + Charge
		[StringLength(5)]
		public string IsOutside48States { get; set; } // IsOutside48States.ToString().Toupper()
		[StringLength(5)]
		public string IsRural { get; set; } // IsRural.ToString().Toupper()
		[StringLength(24)]
		public string MarkupType { get; set; } // package.MarkUpType
	}
}
