using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class PackageDataset : Dataset
	{
		[StringLength(100)]
		public string PackageId { get; set; } // [Index]
        [StringLength(30)]
		public string ClientName { get; set; }
		[StringLength(30)]
		public string SubClientName { get; set; } // [Index]
		[StringLength(30)]
		public string ClientFacilityName { get; set; } // [Index]

		[StringLength(1)]
		public string MailCode { get; set; }
		[StringLength(24)]
		public string PackageStatus { get; set; } // [Index]
		[StringLength(24)]
		public string RecallStatus { get; set; }
		public DateTime ProcessedDate { get; set; }
		public DateTime LocalProcessedDate { get; set; } // [Index]
		public DateTime? StopTheClockEventDate { get; set; } // [Index]
		public int? IsStopTheClock { get; set; }
		public int? IsUndeliverable { get; set; } // [Index]
		public int? PostalDays { get; set; }
		public int? CalendarDays { get; set; }
		public DateTime? ShippedDate { get; set; }
		public DateTime? RecallDate { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public int? VisnSiteParent { get; set; }

		public DateTime? LastKnownEventDate { get; set; } // [Index]
		[StringLength(120)]
		public string LastKnownEventDescription { get; set; }
		[StringLength(120)]
		public string LastKnownEventLocation { get; set; }
		[StringLength(10)]
		public string LastKnownEventZip { get; set; }

		[StringLength(36)]
		public string SiteId { get; set; }
		[StringLength(5)]
		public string SiteZip { get; set; }
		[StringLength(120)]
		public string SiteAddressLineOne { get; set; }
		[StringLength(30)]
		public string SiteCity { get; set; }
		[StringLength(2)]
		public string SiteState { get; set; }

		[StringLength(36)]
		public string JobId { get; set; }
		[StringLength(36)]
		public string ContainerId { get; set; }
		[StringLength(24)]
		public string BinCode { get; set; }

		[StringLength(100)]
		public string ShippingBarcode { get; set; } // [Index]
		[StringLength(100)]
		public string HumanReadableBarcode { get; set; } // [Index]
		[StringLength(24)]
		public string ShippingCarrier { get; set; }
		[StringLength(60)]
		public string ShippingMethod { get; set; }
		[StringLength(60)]
		public string ServiceLevel { get; set; }
		public int Zone { get; set; }
		public decimal Weight { get; set; } // pounds
		public decimal Length { get; set; } // inches
		public decimal Width { get; set; }
		public decimal Depth { get; set; }
		public decimal TotalDimensions { get; set; }
		[StringLength(24)]
		public string Shape { get; set; }
		[StringLength(24)]
		public string RequestCode { get; set; }
		[StringLength(24)]
		public string DropSiteKeyValue { get; set; }
		[StringLength(24)]
		public string MailerId { get; set; }
		public decimal Cost { get; set; }
		public decimal Charge { get; set; }
		public decimal BillingWeight { get; set; }
		[StringLength(100)]
		public string ExtraCost { get; set; }
		[StringLength(24)]
		public string MarkUpType { get; set; }

		public bool IsPoBox { get; set; }
		public bool IsRural { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsOrmd { get; set; }
		public bool IsDuplicate { get; set; }
		public bool IsSaturday { get; set; }
		public bool IsDduScfBin { get; set; }
		public bool IsSecondaryContainerCarrier { get; set; }
		public bool IsQCRequired { get; set; }


		[StringLength(36)]
		public string AsnImportWebJobId { get; set; }
		[StringLength(36)]
		public string BinGroupId { get; set; }
		[StringLength(36)]
		public string BinMapGroupId { get; set; }
		[StringLength(36)]
		public string RateId { get; set; }
		[StringLength(36)]
		public string ServiceRuleId { get; set; }
		[StringLength(36)]
		public string ServiceRuleGroupId { get; set; }
		[StringLength(36)]
		public string ZoneMapGroupId { get; set; }
		[StringLength(36)]
		public string FortyEightStatesGroupId { get; set; }
		[StringLength(36)]
		public string UpsGeoDescriptorGroupId { get; set; }

		[StringLength(60)]
		public string RecipientName { get; set; }
		[StringLength(120)]
		public string AddressLine1 { get; set; }
		[StringLength(120)]
		public string AddressLine2 { get; set; }
		[StringLength(120)]
		public string AddressLine3 { get; set; }
		[StringLength(60)]
		public string City { get; set; }
		[StringLength(30)]
		public string State { get; set; }
		[StringLength(5)]
		public string Zip { get; set; }
		[StringLength(10)]
		public string FullZip { get; set; }
		[StringLength(24)]
		public string Phone { get; set; }

		[StringLength(60)]
		public string ReturnName { get; set; }
		[StringLength(120)]
		public string ReturnAddressLine1 { get; set; }
		[StringLength(120)]
		public string ReturnAddressLine2 { get; set; }
		[StringLength(30)]
		public string ReturnCity { get; set; }
		[StringLength(30)]
		public string ReturnState { get; set; }
		[StringLength(5)]
		public string ReturnZip { get; set; }
		[StringLength(24)]
		public string ReturnPhone { get; set; }

		public List<PackageEventDataset> PackageEvents { get; set; } = new List<PackageEventDataset>();
		public List<TrackPackageDataset> PackageTracking { get; set; } = new List<TrackPackageDataset>();
		public List<UndeliverableEventDataset> UndeliverableEvents { get; set; } = new List<UndeliverableEventDataset>();
		public string ZipOverrides { get; set; }
		public string ZipOverrideGroupIds { get; set; }
		public string DuplicatePackageIds { get; set; }
        public DateTime? ClientShipDate { get; set; }
		[StringLength(50)]
		public string ProcessedUsername { get; set; }
		[StringLength(50)]
		public string ProcessedEventType { get; set; }
		[StringLength(50)]
		public string ProcessedMachineId { get; set; }
    }
}
