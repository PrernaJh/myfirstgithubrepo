using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class ExpenseRecord : EodChildRecord
	{
		[StringLength(50)]
		public string SubClientName { get; set; }
		[StringLength(10)]
		public string ProcessingDate { get; set; } // localProcessedDate MM/dd/yyyy
		[StringLength(10)]
		public string BillingReference1 { get; set; } // leftmost 5 digits of packageId || localProcessedDate
		[StringLength(60)]
		public string Product { get; set; } // shippingMethod
		[StringLength(60)]
		public string TrackingType { get; set; } // shippingMethod
		public decimal Cost { get; set; }
		public decimal ExtraServiceCost { get; set; }
		public decimal Weight { get; set; }
		public int Zone { get; set; }
	}
}
