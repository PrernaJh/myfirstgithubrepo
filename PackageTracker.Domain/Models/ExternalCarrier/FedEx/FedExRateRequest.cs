using System;
using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier.FedEx
{
	[XmlRoot("RateRequest")]
	public class FedExRateRequest
	{
		public WebAuthenticationDetail WebAuthenticationDetail { get; set; }
		public ClientDetail ClientDetail { get; set; }
		public TransactionDetail TransactionDetail { get; set; }
		public Version Version { get; set; }
		public RequestedShipment RequestedShipment { get; set; }
	}

	public class WebAuthenticationDetail
	{
		public UserCredential UserCredential { get; set; }
	}

	public class UserCredential
	{
		public string Key { get; set; }
		public string Password { get; set; }
	}

	public class ClientDetail
	{
		public string AccountNumber { get; set; }
		public string MeterNumber { get; set; }
	}

	public class TransactionDetail
	{
		public string CustomerTransactionId { get; set; }
	}

	public class Version
	{
		public string ServiceId { get; set; }
		public string Major { get; set; }
		public string Intermediate { get; set; }
		public string Minor { get; set; }
	}

	public class RequestedShipment
	{
		public DateTime ShipTimestamp { get; set; }
		public string DropoffType { get; set; }
		public Shipper Shipper { get; set; }
		public Recipient Recipient { get; set; }
		public string RateRequestTypes { get; set; }
		public string PackageCount { get; set; }
		public RequestedPackageLineItems RequestedPackageLineItems { get; set; }
	}

	public class Shipper
	{
		public Address Address { get; set; }
	}

	public class Recipient
	{
		public Address Address { get; set; }
	}
	public class Address
	{
		public string StreetLines { get; set; }
		public string City { get; set; }
		public string StateOrProvinceCode { get; set; }
		public string PostalCode { get; set; }
		public string CountryCode { get; set; }
	}

	public class RequestedPackageLineItems
	{
		public string SequenceNumber { get; set; }
		public string GroupNumber { get; set; }
		public string GroupPackageCount { get; set; }
		public Weight Weight { get; set; }
		public Dimensions Dimensions { get; set; }
	}

	public class Weight
	{
		public string Units { get; set; }
		public string Value { get; set; }
	}

	public class Dimensions
	{
		public string Length { get; set; }
		public string Width { get; set; }
		public string Height { get; set; }
		public string Units { get; set; }
	}

}
