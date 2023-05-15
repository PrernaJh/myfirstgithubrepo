using System;
using System.Collections.Generic;
namespace PackageTracker.Domain.Models.PackageSearch
{
	public class SearchPackage
	{
		public string PackageId { get; set; }
		public string Barcode { get; set; }
		public string ActivityLocationPoliticalDivision { get; set; }
		public string ScanDateTimeUsps { get; set; }
		public string UspsLocation { get; set; }
		public string DeliveryDate { get; set; }
		public string DeliveryLocationSignedForByName { get; set; }
		public string RecipientName { get; set; }
		public string RecipientAddress { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public decimal Weight { get; set; }
		public string Status { get; set; }
		public string TrackingNumber { get; set; }
		public DateTime CreateDate { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public List<SearchPackageEvent> PackageEvents { get; set; }
		public List<TrackPackageDatasetModel> PackageTracking { get; set; }


		public string ShippingBarcode { get; set; }
		public DateTime ProcessedDate { get; set; }
		public string PackageStatus { get; set; }
		public string BinCode { get; set; }
		public string SiteName { get; set; }
		public int Zone { get; set; }
		public string ServiceLevel { get; set; }

		public bool IsPoBox { get; set; }
		public bool IsRural { get; set; }
		public bool IsOrmd { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsSaturday { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsDduScfBin { get; set; }
		public string MailCode { get; set; }
		public DateTime DatasetCreateDate { get; set; }

		public string ContainerId { get; set; }
	}


	public class TrackPackageDatasetModel
	{
		public string Id { get; set; }
		public string rptindex { get; set; }
		public string PackageId { get; set; } // [Index]
		public string ShippingContainerId { get; set; } // [Index]
		public string ShippingCarrier { get; set; }
		public string TrackingNumber { get; set; } // [Index]
		public DateTime EventDate { get; set; }
		public string EventCode { get; set; }
		public string EventDescription { get; set; }
		public string EventLocation { get; set; }
		public string EventZip { get; set; }
	}
}
