using System;

namespace PackageTracker.Data.Models
{
	public class SubClient : Entity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Key { get; set; }
		public string ClientName { get; set; }
		public string SiteName { get; set; }
		public string MailerId { get; set; } // TODO: Remove?
		public bool IsEnabled { get; set; }

		public DateTime StartDate { get; set; }

		// Consumer detail file settings
		public bool SendConsumerDetailFile { get; set; }
		public string ConsumerDetailFileExportLocation { get; set; }


		// ASN import settings
		public string AsnFileTrigger { get; set; }
		public string AsnImportLocation { get; set; }
		public string AsnImportFormat { get; set; }

		// ASN return file export settings
		public string AsnExportLocation { get; set; }
		public string AsnExportFileNameFormat { get; set; }
		public string AsnExportFormat { get; set; }

		// Account settings
		public FedExCredentials FedExCredentials { get; set; }

		public string FedexShipmentDescription { get; set; }
		public string FedexShipmentCurrencyCode { get; set; }
		public string FedexShipmentMonetaryValue { get; set; }

		public string UpsAccountNumber { get; set; }
		public string UpsShipmentDescription { get; set; }
		public string UpsShipmentCurrencyCode { get; set; }
		public string UpsShipmentMonetaryValue { get; set; }
		public bool UpsDirectDeliveryOnly { get; set; }

		public string UspsImpbMid { get; set; }
		public string ParentMailOwnerMid { get; set; }
		public string ParentMailOwnerCrid { get; set; }
		public string UspsMailOwnerMid { get; set; }
		public string UspsMailOwnerCrid { get; set; }
		public string UspsPermitNo { get; set; }
		public string UspsPermitNoZip { get; set; }
		public string UspsPaymentAccountNo { get; set; }
		public string UspsPaymentMethod { get; set; }
		public string UspsPostageType { get; set; }
		public string UspsCsscNo { get; set; }
		public string UspsCsscProductNo { get; set; }
		public string UspsApiUserId { get; set; }
		public string UspsApiSourceId { get; set; }

		public decimal SignatureCost { get; set; }
		public decimal SignatureCharge { get; set; }


		public Schedule[] Schedules { get; set; } // Active hours for subClient.
    }
}