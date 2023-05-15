using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class PmodContainerDetailRecord : EodChildRecord
	{
		[StringLength(50)]
		public string Site { get; set; }
		[StringLength(50)]
		public string PdCust { get; set; } // set to CMOP
		[StringLength(10)]
		public string PdShipDate { get; set; } // MM/dd/yyyy
		[StringLength(10)]
		public string PdVamcId { get; set; } // blank
		[StringLength(100)]
		public string ContainerId { get; set; }
		[StringLength(100)]
		public string PdTrackingNum { get; set; } // carrierBarcode
		[StringLength(24)]
		public string PdShipMethod { get; set; } // PMOD_BAG or PMOD_PALLET
		[StringLength(24)]
		public string PdBillMethod { get; set; } // PMOD_BAG or PMOD_PALLET
		[StringLength(24)]
		public string PdEntryUnitType { get; set; } // SCF or DDU
		[StringLength(24)]
		public string PdShipCost { get; set; } // cost from rate
		[StringLength(24)]
		public string PdBillingCost { get; set; } // charge from rate
		[StringLength(24)]
		public string PdSigCost { get; set; } // 0
		[StringLength(10)]
		public string PdShipZone { get; set; } // postal zone
		[StringLength(10)]
		public string PdZip5 { get; set; } // dropship zip
		[StringLength(32)]
		public string PdWeight { get; set; } // weight from scale in lbs
		[StringLength(32)]
		public string PdBillingWeight { get; set; } // TODO: calculate this
		[StringLength(50)]
		public string PdSortCode { get; set; } // bin code
		[StringLength(24)]
		public string PdMarkupReason { get; set; } // blank
	}
}
