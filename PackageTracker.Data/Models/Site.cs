using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class Site : Entity
	{
		public string SiteName { get; set; }
		public string Zip { get; set; }
		public string ZipPlusFour { get; set; }
		public string Description { get; set; }
		public string PermitNumber { get; set; }
		public string ShipperAccount { get; set; }
		public string MailProducerMid { get; set; }
		public string MailProducerCrid { get; set; }
		public string SackMailerId { get; set; }
		public string SackCrid { get; set; }
		public string PalletMailerId { get; set; }
		public string PalletCrid { get; set; }
		public string PmodPalletPermitNumber { get; set; }
		public string AlternateCarrierBarcodePrefix { get; set; } // Tucson - OnTrac, Dallas - LSO
		public bool IsEnabled { get; set; }
		public string FullAddress { get; set; }
		public string AddressLineOne { get; set; }
		public string AddressLineTwo { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string TimeZone { get; set; }
		public string UspsApiUserId { get; set; }
		public string UspsApiSourceId { get; set; }
		public string UspsPermitNo { get; set; }
		public string UspsPaymentMethod { get; set; }
		public string UspsPermitNoZip { get; set; }
		public string UspsPostageType { get; set; }
		public string UspsCsscNo { get; set; }
		public string UspsCsscProductNo { get; set; }
		public string UpsShipperAttentionName { get; set; }
		public string UpsShipperPhone { get; set; }
		public string EvsId { get; set; }
		public string EodGroup { get; set; }

		public List<BinOverride> BinOverrides { get; set; } = new List<BinOverride>();
		public List<string> CriticalAlertEmailList { get; set; } = new List<string>();
		public List<string> CriticalAlertSmsList { get; set; } = new List<string>();
		public List<string> EodSummaryEmailList { get; set; } = new List<string>();
		public List<SiteCustomData> SiteCustomData { get; set; } = new List<SiteCustomData>();
		
		// Account settings
		public FedExCredentials FedExCredentials { get; set; }
		public string FedexShipmentDescription { get; set; }
		public string FedexShipmentCurrencyCode { get; set; }
		public string FedexShipmentMonetaryValue { get; set; }

		public Schedule[] Schedules { get; set; } // Active hours for site, can override in subclient.
	}
}
