using System;

namespace PackageTracker.Data.Models.TrackingData
{
	public class UpsTrackingData
	{
		public string ShipperNumber { get; set; }
		public DateTime DeliveryDateTime { get; set; }
		public string ActivityLocationPoliticalDivision2 { get; set; }
		public string ActivityLocationPoliticalDivision1 { get; set; }
		public string ActivityLocationCountryCode { get; set; }
		public string ActivityLocationPostcodePrimaryLow { get; set; }
		public string DeliveryLocationStreetNumberLow { get; set; }
		public string DeliveryLocationStreetName { get; set; }
		public string DeliveryLocationStreetType { get; set; }
		public string DeliveryLocationPoliticalDivision2 { get; set; }
		public string DeliveryLocationPoliticalDivision1 { get; set; }
		public string DeliveryLocationCountryCode { get; set; }
		public string DeliveryLocationPostcodePrimaryLow { get; set; }
		public string DeliveryLocationResidentialAddressIndicator { get; set; }
		public string DeliveryLocationCode { get; set; }
		public string DeliveryLocationDescription { get; set; }
		public string DeliveryLocationSignedForByName { get; set; }
	}
}
