using PackageTracker.Data.Models.TrackingData;

namespace PackageTracker.Data.Models
{
	public class TrackPackage : Entity
	{
		public string ShippingCarrier { get; set; }
		public string TrackingNumber { get; set; }
		public string SiteName { get; set; }

		public FedExTrackingData FedExTrackingData { get; set; }
		public UpsTrackingData UpsTrackingData { get; set; }
		public UspsTrackingData UspsTrackingData { get; set; }
	}
}
