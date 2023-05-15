using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class ContainerDetailRecord : EodChildRecord
	{
		[StringLength(100)]
		public string TrackingNumber { get; set; }
		[StringLength(10)]
		public string ShipmentType { get; set; }
		[StringLength(10)]
		public string PickupDate { get; set; } // MM/DD/YYYY
		[StringLength(100)]
		public string ShipReferenceNumber { get; set; }
		[StringLength(10)]
		public string ShipperAccount { get; set; }
		[StringLength(60)]
		public string DestinationName { get; set; }
		[StringLength(120)]
		public string DestinationAddress1 { get; set; }
		[StringLength(120)]
		public string DestinationAddress2 { get; set; }
		[StringLength(30)]
		public string DestinationCity { get; set; }
		[StringLength(30)]
		public string DestinationState { get; set; }
		[StringLength(10)]
		public string DestinationZip { get; set; }
		[StringLength(10)]
		public string DropSiteKey { get; set; }
		[StringLength(60)]
		public string OriginName { get; set; }
		[StringLength(120)]
		public string OriginAddress1 { get; set; }
		[StringLength(120)]
		public string OriginAddress2 { get; set; }
		[StringLength(30)]
		public string OriginCity { get; set; }
		[StringLength(30)]
		public string OriginState { get; set; }
		[StringLength(10)]
		public string OriginZip { get; set; }
		[StringLength(24)] 
		public string Reference1 { get; set; }
		[StringLength(24)] 
		public string Reference2 { get; set; }
		[StringLength(24)] 
		public string Reference3 { get; set; }
		[StringLength(24)] 
		public string CarrierRoute1 { get; set; }
		[StringLength(24)] 
		public string CarrierRoute2 { get; set; }
		[StringLength(24)] 
		public string CarrierRoute3 { get; set; }
		[StringLength(32)]
		public string Weight { get; set; }
		[StringLength(10)]
		public string DeliveryDate { get; set; } // yyyyMMdd
		[StringLength(10)]
		public string ExtraSvcs1 { get; set; }
		[StringLength(10)]
		public string ExtraSvcs2 { get; set; }
		[StringLength(10)]
		public string ExtraSvcs3 { get; set; }
	}
}
