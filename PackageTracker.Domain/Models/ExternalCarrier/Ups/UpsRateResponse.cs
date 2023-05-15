using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier.Ups
{
	[XmlRoot("RatingServiceSelectionResponse")]
	public class UpsRateResponse
	{
		public RatedShipment RatedShipment { get; set; }
	}

	public class RatedShipment
	{
		public RatedPackage RatedPackage { get; set; }
	}

	public class RatedPackage
	{
		public TotalCharges TotalCharges { get; set; }
	}

	public class TotalCharges
	{
		public string MonetaryValue { get; set; }
	}
}
