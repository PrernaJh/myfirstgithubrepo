using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class AdditionalShippingData
	{
		// FedEx
		public string TrackingNumber { get; set; }
		public string OriginId { get; set; }
		public string Cad { get; set; }
		public string Ursa { get; set; }
		public string FormId { get; set; }
		public string AirportId { get; set; }
		public string StateAndCountryCode { get; set; }
		public string FormattedDeliveryDate { get; set; }
		public string FormattedShippingMethod { get; set; }
		public string FormattedServiceDescriptor { get; set; }
		public string FormattedServiceDescriptorLetter { get; set; }
		public string AstraPlannedServiceLevel { get; set; }
		public string HumanReadableTrackingNumber { get; set; }
		public string OperationalSystemId { get; set; }
		public string HumanReadableSecondaryBarcode { get; set; }

		// FedEx Customs Data
		public string FedexShipperAccountNumber{ get; set; }
		public string FedexShipperCountryCode { get; set; }
		public string FedexShipmentDescription { get; set; }
		public string FedexShipmentCurrencyCode { get; set; }
		public string FedexShipmentMonetaryValue { get; set; }

		// Ups
		public string UpsShipperAttentionName{ get; set; }
		public string UpsShipperPhone { get; set; }
		public string UpsShipmentDescription { get; set; }
		public string UpsShipmentCurrencyCode { get; set; }
		public string UpsShipmentMonetaryValue { get; set; }
		public string UpsShipmentCountryCode { get; set; }        
    }
}
