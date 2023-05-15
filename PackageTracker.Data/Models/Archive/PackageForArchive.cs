using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Models.Archive
{
    public class PackageForArchive
    {
		public string PackageId { get; set; }
		public string SiteName { get; set; }
		public string ClientName { get; set; }
		public string ClientFacilityName { get; set; }
		public string SubClientName { get; set; }

		public string MailCode { get; set; }
		public string PackageStatus { get; set; }
		public DateTime LocalProcessedDate { get; set; }
		public string ShippingBarcode { get; set; }
		public string HumanReadableBarcode { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string ServiceLevel { get; set; }

		public int Zone { get; set; }
		public decimal Weight { get; set; } // pounds
		public decimal Length { get; set; } // inches
		public decimal Width { get; set; }
		public decimal Depth { get; set; }
		public decimal TotalDimensions { get; set; }
		public string MailerId { get; set; }
		public decimal Cost { get; set; }
		public decimal Charge { get; set; }
		public decimal ExtraCost { get; set; }
		public decimal BillingWeight { get; set; }
		public string MarkUpType { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsRural { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsOrmd { get; set; }

		public string RecipientName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string FullZip { get; set; }
		public string Phone { get; set; }

		public DateTime? CosmosCreateDate { get; set; }
		public DateTime? StopTheClockEventDate { get; set; }
		public DateTime? ShippedDate { get; set; }
		public string RecallStatus { get; set; }
		public DateTime? RecallDate { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public DateTime? LastKnownEventDate { get; set; }
		public string LastKnownEventDescription { get; set; }
		public string LastKnownEventLocation { get; set; }
		public string LastKnownEventZip { get; set; }
		public int? PostalDays { get; set; }
		public int? CalendarDays { get; set; }
		public int? IsStopTheClock { get; set; }
		public int? IsUndeliverable { get; set; }

		public string BinCode { get; set; }
		public string ContainerId { get; set; }
		public string ContainerType { get; set; }
		public bool IsSecondaryContainerCarrier { get; set; }
		public string ContainerBarcode { get; set; }
		public string ContainerShippingCarrier { get; set; }
		public string ContainerShippingMethod { get; set; }
		public DateTime? ContainerLastKnownEventDate { get; set; }
		public string ContainerLastKnownEventDescription { get; set; }
		public string ContainerLastKnownEventLocation { get; set; }
		public string ContainerLastKnownEventZip { get; set; }
	}
}
