using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier.Usps
{
	[XmlRoot("RateV4Response")]
	public class UspsRateResponse
	{
		public UspsPackage Package { get; set; }
	}

	public class UspsPackage
	{
		public Postage Postage { get; set; }
		public string Zone { get; set; }
	}

	public class Postage
	{
		public string Rate { get; set; }
	}
}
