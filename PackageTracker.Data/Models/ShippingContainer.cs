using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class ShippingContainer : Entity
	{
		public string ContainerId { get; set; } // [Index]
		public string OperationalContainerId { get; set; }
		public string HumanReadableBarcode { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }
		public string BinActiveGroupId { get; set; }
		public string BinCode { get; set; }
		public string BinCodeSecondary { get; set; }
		public string BinLabelType { get; set; }
		public string ShippingMethod { get; set; }
		public string ShippingMethodOverride { get; set; }
		public string ShippingCarrier { get; set; }
		public string Status { get; set; }
		public string ContainerType { get; set; }
		public string Grouping { get; set; }
		public string Weight { get; set; }
		public string BillingWeight { get; set; }
		public decimal WeightInOz { get; set; }
		public string SerialNumber { get; set; }
		public string ClosedSerialNumber { get; set; }
		public string CarrierBarcode { get; set; }
		public string HumanReadableCarrierBarcode { get; set; }
		public string DropShipSiteDescription { get; set; }
		public string DropShipSiteAddress { get; set; }
		public string DropShipSiteCsz { get; set; }
		public string DropShipSiteNote { get; set; }
		public string RegionalCarrierHub { get; set; }
		public string ZoneMapActiveGroupId { get; set; }
		public bool IsSecondaryCarrier { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public bool IsRural { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsRateAssigned { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
		public int LabelTypeId { get; set; }
		public int ClosedLabelTypeId { get; set; }
		public bool IsUspsEvsFileProcessed { get; set; }
		public decimal Cost { get; set; }
		public decimal Charge { get; set; }
		public string RateId { get; set; }
		public string RateGroupId { get; set; }
		public int Zone { get; set; }
		public string Base64Label { get; set; }
		public AdditionalShippingData AdditionalShippingData{ get; set; }
		public DateTime SiteCreateDate { get; set; }
        public DateTime ProcessedDate { get; set; }
		public DateTime LocalProcessedDate { get; set; }

		public int EodUpdateCounter { get; set; }
		public int EodProcessCounter { get; set; }
		public int SqlEodProcessCounter { get; set; }
		public List<string> WebJobIds{ get; set; } = new List<string>();
		public List<string> HistoricalRateIds { get; set; } = new List<string>();
		public List<string> HistoricalRateGroupIds { get; set; } = new List<string>();
        public List<CarrierApiError> CarrierApiErrors { get; set; } = new List<CarrierApiError>();
		public List<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
		public List<LabelFieldValue> ClosedLabelFieldValues { get; set; } = new List<LabelFieldValue>();
		public List<Event> Events { get; set; } = new List<Event>();

	}
}
