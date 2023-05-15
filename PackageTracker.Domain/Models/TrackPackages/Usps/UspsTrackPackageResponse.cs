using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace PackageTracker.Domain.Models.TrackPackages.Usps
{
	[XmlRoot("TrackResponse")]
	public class UspsTrackPackageResponse
	{
		[XmlElement]
		public List<TrackInfo> TrackInfo { get; set; }

	}

	public class TrackInfo
	{
		[XmlAttribute]
		public string ID {get; set;}
		[XmlElement]
		public List<TrackDetail> TrackDetail { get; set; }
		public TrackDetail TrackSummary { get; set; }
		public Error Error { get; set; }
	}

	public class TrackDetail
    {
		public string DeliveryAttributeCode { get; set; }
		public string EventTime { get; set; }
		public string EventDate { get; set; }
		public string Event { get; set; }
		public string EventCity { get; set; }
		public string EventState { get; set; }
		public string EventStatusCategory { get; set; }
		public string EventPartner { get; set; }
		public string EventZIPCode { get; set; }
		public string EventCountry { get; set; }
		public string FirmName { get; set; }
		public string Name { get; set; }
		public bool AuthorizedAgent { get; set; }
		public string EventCode { get; set; }
		public string ActionCode { get; set; }
		public string ReasonCode { get; set; }
		public bool GeoCertified { get; set; }

		public DateTime EventDateTime()
        {
			DateTime eventDateTime = new DateTime();
			DateTime.TryParse(EventDate + " " + EventTime, out eventDateTime);
			return eventDateTime;
        }
	}

	public class Error
    {
		public string Description { get; set; }
	}
}