using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class ReturnAsnRecord : EodChildRecord
	{
		[StringLength(100)]
		public string ParcelId { get; set; }
		[StringLength(10)]
		public string SiteCode { get; set; }
		[StringLength(24)]
		public string PackageWeight { get; set; }
		[StringLength(10)]
		public string ProductCode { get; set; }
		[StringLength(1)]
		public string Over84Flag { get; set; }
		[StringLength(1)]
		public string Over108Flag { get; set; }
		[StringLength(1)]
		public string NonMachinableFlag { get; set; }
		[StringLength(1)]
		public string DelCon { get; set; }
		[StringLength(1)]
		public string Signature { get; set; }
		[StringLength(10)]
		public string CustomerNumber { get; set; }
		[StringLength(50)]
		public string BolNumber { get; set; }
		[StringLength(10)]
		public string PackageCreateDateDayMonthYear { get; set; } // MMddyyyy
		[StringLength(10)]
		public string PackageCreateDateHourMinuteSecond { get; set; } // HHmmss
		[StringLength(10)]
		public string ZipDestination { get; set; }
		[StringLength(100)]
		public string PackageBarcode { get; set; }
		[StringLength(10)]
		public string Zone { get; set; }
		[StringLength(24)]
		public string TotalShippingCharge { get; set; }
		[StringLength(24)]
		public string ConfirmationSurcharge { get; set; }
		[StringLength(24)]
		public string NonMachinableSurcharge { get; set; }
		[StringLength(5)]
		public string IsOutside48States { get; set; } // IsOutside48States ? "Y" : "N"
		[StringLength(5)]
		public string IsRural { get; set; } // IsRural ? "Y" : "N"
		[StringLength(24)]
		public string MarkupType { get; set; } // package.MarkUpType
	}
}
