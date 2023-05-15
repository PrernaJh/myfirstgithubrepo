using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.TrackPackages.Ups
{
	[XmlRoot("AccessRequest")]
	public class UpsAccessRequest
	{
		public string AccessLicenseNumber { get; set; }
		public string UserId { get; set; }
		public string Password { get; set; }
	}

	[XmlRoot("QuantumViewRequest")]
	public class UpsTrackPackageRequest
	{
		public Request Request { get; set; }

		public string Bookmark { get; set; }
		public SubscriptionRequest SubscriptionRequest { get; set; }
	}

	public class Request
	{
		public string RequestAction { get; set; }
	}
	public class SubscriptionRequest
	{
		public string Name { get; set; }
		public DateTimeRange DateTimeRange { get; set; }
	}
	public class DateTimeRange
	{
		public string BeginDateTime { get; set; }
		public string EndDateTime { get; set; }
	}
}
