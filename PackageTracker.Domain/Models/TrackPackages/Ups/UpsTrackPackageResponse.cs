using System.Collections.Generic;
using System.Xml.Serialization;


namespace PackageTracker.Domain.Models.TrackPackages.Ups
{
	[XmlRoot("QuantumViewResponse")]
	public class UpsTrackPackageResponse
	{
		public QuantumViewEvents QuantumViewEvents { get; set; }
		public string Bookmark { get; set; }

		[XmlElement("Response")]
		public Response Response { get; set; }
	}

	public class Response
	{
		public string TransactionReference { get; set; }
		public int ResponseStatusCode { get; set; }
		public string ResponseStatusDescription { get; set; }

		[XmlElement("Error")]
		public Error Error { get; set; }
	}

	public class Error
	{
		public string ErrorSeverity { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorDescription { get; set; }
	}

	public class QuantumViewEvents
	{
		public QuantumViewEvents() { SubscriptionEvents = new List<SubscriptionEvents>(); }

		public string SubscriberID { get; set; }
		[XmlElement("SubscriptionEvents")]
		public List<SubscriptionEvents> SubscriptionEvents { get; set; }

	}

	public class SubscriptionEvents
	{
		public SubscriptionEvents() { SubscriptionFile = new List<SubscriptionFile>(); }
		public string Name { get; set; }
		public string Number { get; set; }
		public SubscriptionStatus SubscriptionStatus { get; set; }
		[XmlElement("SubscriptionFile")]
		public List<SubscriptionFile> SubscriptionFile { get; set; }
	}
	public class SubscriptionStatus
	{
		public string Code { get; set; }

	}
	public class SubscriptionFile
	{

		public SubscriptionFile() { Delivery = new List<Delivery>(); Manifest = new List<Manifest>(); Generic = new List<Generic>(); }
		//public SubscriptionFile(){ Delivery = new List<Delivery>(); }
		public string FileName { get; set; }
		public StatusType StatusType { get; set; }
		public Origin Origin { get; set; }
		[XmlElement("Delivery")]
		public List<Delivery> Delivery { get; set; }
		[XmlElement("Manifest")]
		public List<Manifest> Manifest { get; set; }
		[XmlElement("Generic")]
		public List<Generic> Generic { get; set; }
	}
	public class StatusType
	{
		public string Code { get; set; }
	}

	public class Origin
	{
		public string ShipperNumber { get; set; }
		public string TrackingNumber { get; set; }
		public string Date { get; set; }
		public string Time { get; set; }
		public ActivityLocation ActivityLocation { get; set; }
	}

	public class ActivityLocation
	{
		public ActivityLocation() { AddressArtifactFormat = new AddressArtifactFormat(); }
		public AddressArtifactFormat AddressArtifactFormat { get; set; }
	}

	public class AddressArtifactFormat
	{
		public string PoliticalDivision2 { get; set; }
		public string PoliticalDivision1 { get; set; }
		public string CountryCode { get; set; }
		public string StreetNumberLow { get; set; }
		public string StreetName { get; set; }
		public string StreetType { get; set; }
		public string PostcodePrimaryLow { get; set; }
		public string ResidentialAddressIndicator { get; set; }

	}

	public class Delivery
	{
		public string ShipperNumber { get; set; }
		public string TrackingNumber { get; set; }
		public string Date { get; set; }
		public string Time { get; set; }
		public ActivityLocation ActivityLocation { get; set; }
		public DeliveryLocation DeliveryLocation { get; set; }
	}

	public class DeliveryLocation
	{
		public DeliveryLocation() { AddressArtifactFormat = new AddressArtifactFormat(); }
		public AddressArtifactFormat AddressArtifactFormat { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public string SignedForByName { get; set; }
	}

	public class Manifest
	{
		public Manifest() { ShipTo = new ShipTo(); }
		public string PickupDate { get; set; }
		public string ScheduledDeliveryDate { get; set; }
		public Shipper Shipper { get; set; }
		public ShipTo ShipTo { get; set; }
		public Service Service { get; set; }
		public Package Package { get; set; }
	}

	public class Shipper
	{
		public Shipper() { Address = new Address(); }
		public Address Address { get; set; }
	}

	public class Address
	{
		public string ConsigneeName { get; set; }
		public string AddressLine1 { get; set; }
		public string City { get; set; }
		public string StateProvinceCode { get; set; }
		public string PostalCode { get; set; }
		public string CountryCode { get; set; }
	}

	public class ShipTo
	{
		public ShipTo() { Address = new Address(); }
		public string AttentionName { get; set; }
		public string LocationID { get; set; }
		public string ReceivingAddressName { get; set; }
		public Address Address { get; set; }
	}


	public class Service
	{
		public string Code { get; set; }
	}
	public class Generic
	{
		public Generic() { ShipmentReferenceNumber = new List<ShipmentReferenceNumber>(); PackageReferenceNumber = new List<PackageReferenceNumber>(); Service = new Service(); ShipTo = new ShipTo(); }
		public string ActivityType { get; set; }
		public string TrackingNumber { get; set; }
		public string ShipperNumber { get; set; }
		[XmlElement("ShipmentReferenceNumber")]
		public List<ShipmentReferenceNumber> ShipmentReferenceNumber { get; set; }
		[XmlElement("PackageReferenceNumber")]
		public List<PackageReferenceNumber> PackageReferenceNumber { get; set; }

		public Service Service { get; set; }
		public Activity Activity { get; set; }
		public BillToAccount BillToAccount { get; set; }
		public ShipTo ShipTo { get; set; }
	}
	public class ShipmentReferenceNumber
	{
		public string Number { get; set; }
		public string Value { get; set; }
	}
	public class PackageReferenceNumber
	{
		public string Number { get; set; }
		public string Value { get; set; }
	}
	public class Activity
	{
		public string Date { get; set; }
		public string Time { get; set; }
	}
	public class BillToAccount
	{
		public string Option { get; set; }
		public string Number { get; set; }
	}
	public class Package
	{
		public PackageServiceOptions PackageServiceOptions { get; set; }
	}

	public class PackageServiceOptions
	{
		public string COD { get; set; }
	}
}