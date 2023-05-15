using System.Collections.Generic;
using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier.FedEx
{
	//TODO: This namespace may need to change in the future
	[XmlRoot(Namespace = "http://fedex.com/ws/rate/v26", ElementName = "RateReply")]
	public class FedExRateResponse
	{
		[XmlElement("RateReplyDetails")]
		public List<RateReplyDetails> RateReplyDetails { get; set; }
	}

	public class RateReplyDetails
	{
		public string ServiceType { get; set; }
		[XmlElement("RatedShipmentDetails")]
		public List<RatedShipmentDetails> RatedShipmentDetails { get; set; }
	}

	public class RatedShipmentDetails
	{
		[XmlElement("ShipmentRateDetail")]
		public ShipmentRateDetail ShipmentRateDetail { get; set; }
	}

	public class ShipmentRateDetail
	{
		[XmlElement("TotalNetFedExCharge")]
		public TotalNetFedExCharge TotalNetFedExCharge { get; set; }
	}
	public class TotalNetFedExCharge
	{
		public string Currency { get; set; }
		public string Amount { get; set; }
	}
}
