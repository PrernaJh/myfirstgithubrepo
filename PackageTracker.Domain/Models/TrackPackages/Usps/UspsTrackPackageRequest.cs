using System.Xml.Serialization;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.TrackPackages.Usps
{
	[XmlRoot("TrackFieldRequest")]
	public class UspsTrackPackageRequest
	{
		public UspsTrackPackageRequest()
        {
			TrackID = new List<TrackID>();
        }
		[XmlAttribute]
		public string USERID { get; set; }
		public string Revision { get; set; }
		public string ClientIp { get; set; }
		public string SourceId { get; set; }
		[XmlElement]
		public List<TrackID> TrackID { get; set; }
	}

	public class TrackID
    {
		[XmlAttribute]
		public string ID { get; set; }
    }
}
