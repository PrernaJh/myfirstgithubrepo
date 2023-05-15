using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class PackageDetailRecord : EodChildRecord
	{
		/// <summary>
		//  Site Name
		/// </summary>
		[StringLength(50)]
		public string MmsLocation { get; set; }
		/// <summary>
		/// Company Name - CMOP, DALC
		/// </summary>
		[StringLength(10)]
		public string Customer { get; set; }
		/// <summary>
		/// Ship Date  MM/DD/YYYY
		/// </summary>
		[StringLength(10)]
		public string ShipDate { get; set; }
		/// <summary>
		/// Left 5 digits of package ID
		/// </summary>
		[StringLength(10)]
		public string VamcId { get; set; }
		/// <summary>
		/// The Package ID in the ASN file from CMOP
		/// </summary>
		[StringLength(100)]
		public string PackageId { get; set; }
		/// <summary>
		/// Appears to be the tracking number assigned by USPS, FedEx or UPS
		/// </summary>
		[StringLength(100)]
		public string TrackingNumber { get; set; }
		/// <summary>
		/// Ship method: 
		/// FIRST CLASS
		/// IRREGULAR
		/// PRIORITY MAIL
		/// UPS - NEXT DAY AIR SAVER
		/// UPS - GROUND
		/// UPS - NEXT DAY AIR
		/// PRIORITY MAIL(PMOD)
		/// UPS - 2ND DAY AIR

		/// </summary>
		[StringLength(60)]
		public string ShipMethod { get; set; }
		/// Bill method: 
		/// FIRST CLASS
		/// IRREGULAR
		/// PRIORITY MAIL
		/// UPS - NEXT DAY AIR SAVER
		/// UPS - GROUND
		/// UPS - NEXT DAY AIR
		/// PRIORITY MAIL(PMOD)
		/// UPS - 2ND DAY AIR
		[StringLength(60)]
		public string BillMethod { get; set; }
		/// <summary>
		/// Entry Unit Type
		/// SCF
		/// DDU
		/// NDC
		/// ASF
		/// </summary>
		[StringLength(10)]
		public string EntryUnitType { get; set; }
		/// <summary>
		/// Dollar amount we paid to USPS
		/// </summary>
		[StringLength(24)]
		public string ShipCost { get; set; }
		/// <summary>
		/// Dollar amount we billed CMOP
		/// </summary>
		[StringLength(24)]
		public string BillingCost { get; set; }
		/// <summary>
		/// Signature Cost - Fixed - 2 decimals
		/// </summary>
		[StringLength(24)]
		public string SignatureCost { get; set; }
		/// <summary>
		/// Ship Zone - No format - just the zone, no leading zeros
		/// </summary>
		[StringLength(10)]
		public string ShipZone { get; set; }
		/// <summary>
		/// 5 digit ZIP code
		/// </summary>
		[StringLength(10)]
		public string ZipCode { get; set; }
		/// <summary>
		/// Weight from scale in lbs
		/// </summary>
		[StringLength(32)]
		public string Weight { get; set; }
		/// <summary>
		/// Billed weight in ounces
		/// </summary>
		[StringLength(32)]
		public string BillingWeight { get; set; }
		/// <summary>
		/// Sort code - this is the bin location at the facility
		/// </summary>
		[StringLength(60)]
		public string SortCode { get; set; }
		[StringLength(24)]
		public string MarkupType { get; set; } // package.MarkUpType
		[StringLength(5)]
		public string IsOutside48States { get; set; } // IsOutside48States ? "Y" : "N"
		[StringLength(5)]
		public string IsRural { get; set; } // IsRural ? "Y" : "N"
	}
}