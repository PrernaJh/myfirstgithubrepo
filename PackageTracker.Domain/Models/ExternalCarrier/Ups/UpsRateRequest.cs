using System.Xml.Serialization;

namespace PackageTracker.Domain.Models.ExternalCarrier.Ups
{
	[XmlRoot("AccessRequest")]
	public class UpsAccessRequest
	{
		public string AccessLicenseNumber { get; set; }
		public string UserId { get; set; }
		public string Password { get; set; }
	}

	[XmlRoot("RatingServiceSelectionRequest")]
	public class UpsRateRequest
	{
		public Request Request { get; set; }
		public PickupType PickupType { get; set; }
		public Shipment Shipment { get; set; }
	}

	public class Request
	{
		public string RequestAction { get; set; }
		public string RequestOption { get; set; }
	}

	public class PickupType
	{
		public string Code { get; set; }
		public string Description { get; set; }
	}

	public class Shipment
	{
		public string Description { get; set; }
		public Shipper Shipper { get; set; }
		public ShipTo ShipTo { get; set; }
		public Service Service { get; set; }
		public UpsPackage Package { get; set; }
	}

	public class Shipper
	{
		public string Name { get; set; }
		public string PhoneNumber { get; set; }
		public string ShipperNumber { get; set; }
		public Address Address { get; set; }
	}

	public class ShipTo
	{
		public string CompanyName { get; set; }
		public string PhoneNumber { get; set; }
		public Address Address { get; set; }
	}

	public class Address
	{
		public string AddressLine1 { get; set; }
		public string City { get; set; }
		public string StateProvinceCode { get; set; }
		public string PostalCode { get; set; }
		public string CountryCode { get; set; }
	}

	public class Service
	{
		public string Code { get; set; }
	}

	[XmlRoot("Package")]
	public class UpsPackage
	{
		public PackagingType PackagingType { get; set; }
		public string Description { get; set; }
		public PackageWeight PackageWeight { get; set; }
	}
	public class PackagingType
	{
		public string Code { get; set; }
		public string Description { get; set; }
	}

	public class PackageWeight
	{
		public UnitOfMeasurement UnitOfMeasurement { get; set; }
		public string Weight { get; set; }
	}
	public class UnitOfMeasurement
	{
		public string Code { get; set; }
	}
}
