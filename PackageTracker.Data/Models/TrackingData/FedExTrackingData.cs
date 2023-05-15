using System;

namespace PackageTracker.Data.Models.TrackingData
{
	public class FedExTrackingData
	{
		public DateTime LastStatusDateTime { get; set; }
		public string LastStatusCode { get; set; }
		public string LastStatusDescription { get; set; }
		public string EventAddress { get; set; }
	}
}
