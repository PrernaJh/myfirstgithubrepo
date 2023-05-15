using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier
{
	[XmlRoot("eVSCertifyResponse")]
	public class UspsLabelResponse
	{
		public string BarcodeNumber { get; set; }
	}
}
