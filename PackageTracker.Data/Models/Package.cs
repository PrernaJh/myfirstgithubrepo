using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
    public class Package : Entity
    {
        public string PackageId { get; set; }
        public string MailCode { get; set; }
        public string JobBarcode { get; set; }
        public string PackageStatus { get; set; }
        public string RecallStatus { get; set; }
        public DateTime ProcessedDate { get; set; }
		public DateTime LocalProcessedDate { get; set; }
		public DateTime LocalCreateDate { get; set; }
		public DateTime? RecallDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
		public DateTime? ShipDate { get; set; }
		public string SiteName { get; set; }
		public string SubClientName { get; set; }
		public string SubClientKey { get; set; }
		public string JobId { get; set; }
		public string ContainerId { get; set; }
		public string BinCode { get; set; }
		public string Barcode { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }

		public List<Event> PackageEvents { get; set; } = new List<Event>();

		public string ClientName { get; set; }
		public string ClientFacilityName { get; set; }
		public string SiteId { get; set; }
        public string SiteZip { get; set; }
        public string SiteAddressLineOne { get; set; }
        public string SiteCity { get; set; }
        public string SiteState { get; set; }
		public string UpsGeoDescriptor { get; set; }        
		public string HumanReadableBarcode { get; set; }
		public string FormattedBarcode { get; set; }
		public string CustomerReferenceNumber { get; set; }
		public string CustomerReferenceNumber2 { get; set; }
		public string CustomerReferenceNumber3 { get; set; }
		public string CustomerReferenceNumber4 { get; set; }
		public string ServiceLevel { get; set; }
		public int Zone { get; set; }
		public decimal Weight { get; set; } // pounds
		public decimal WeightInOunces { get; set; } // pounds
		public decimal Length { get; set; } // inches
		public decimal Width { get; set; }
		public decimal Depth { get; set; }
		public decimal TotalDimensions { get; set; }
		public string Shape { get; set; }
		public string Product { get; set; }
		public string BillOfLading { get; set; }
		public string TimeZone { get; set; }
		public string BusinessRuleType { get; set; }
		public string TrackingRuleType { get; set; }
		public string BinRuleType { get; set; }
		public int Sequence { get; set; }
		public string RequestCode { get; set; }
		public string DropSiteKeyValue { get; set; }
		public string UspsPermitNumber { get; set; }
		public string MailerId { get; set; }
		public string OverrideMailCode { get; set; }		
		public decimal Cost { get; set; }
		public decimal Charge { get; set; }
		public decimal ExtraCost { get; set; }
		public decimal ExtraCharge { get; set; }
		public decimal BillingWeight { get; set; }
		public string MarkUpType { get; set; }

		public bool IsPoBox { get; set; }
		public bool IsRural { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsOrmd { get; set; }
        public bool IsDuplicate { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsDduScfBin { get; set; }
		public bool IsAptbBin { get; set; }
		public bool IsScscBin { get; set; }
		public bool IsSecondaryContainerCarrier { get; set; }
        public bool IsQCRequired { get; set; }
		public bool IsMarkUpTypeCompany { get; set; }
		public bool IsCarrierRecallSent { get; set; }
		public bool IsCreated { get; set; }
		public bool PrintLabel { get; set; }

        public List<string> ZipOverrides { get; set; } = new List<string>();
		
		public bool IsLocked { get; set; }
		public int EodUpdateCounter { get; set; }
		public int EodProcessCounter { get; set; }
		public int SqlEodProcessCounter { get; set; }
		public bool IsUspsEvsProcessed { get; set; }
		public bool IsConsumerDetailFileProcessed { get; set; }
		public bool IsRateAssigned { get; set; }

		public List<string> WebJobIds { get; set; } = new List<string>();
		public string AsnImportWebJobId { get; set; }
		public string BinGroupId { get; set; }
		public string BinMapGroupId { get; set; }
		public string RateId { get; set; }
		public string RateGroupId { get; set; }
		public string ServiceRuleId { get; set; }
		public string ServiceRuleGroupId { get; set; }
		public string OverrideServiceRuleId { get; set; }
		public string ZoneMapGroupId { get; set; }
		public string ServiceRuleExtensionId { get; set; }
		public string ServiceRuleExtensionGroupId { get; set; }
		public string UpsGeoDescriptorGroupId { get; set; }
		public string OverrideServiceRuleGroupId { get; set; }
		public List<string> ZipOverrideIds { get; set; } = new List<string>();
		public List<string> ZipOverrideGroupIds { get; set; } = new List<string>();
		public List<string> DuplicatePackageIds { get; set; } = new List<string>();
		public List<string> HistoricalContainerIds { get; set; } = new List<string>();
		public List<string> HistoricalBinCodes { get; set; } = new List<string>();
		public List<string> HistoricalBinGroupIds { get; set; } = new List<string>();
		public List<string> HistoricalBinMapGroupIds { get; set; } = new List<string>();
		public List<string> HistoricalRepeatScanPackageIds { get; set; } = new List<string>();
		public List<string> HistoricalServiceRuleGroupIds { get; set; } = new List<string>();
		public List<string> HistoricalRateIds { get; set; } = new List<string>();
		public List<string> HistoricalRateGroupIds { get; set; } = new List<string>();
		public int ForceExceptionOverrideLabelTypeId { get; set; }

		public string RecipientName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string FullZip { get; set; }
		public string Phone { get; set; }
		public string ReturnName { get; set; }
		public string ReturnAddressLine1 { get; set; }
		public string ReturnAddressLine2 { get; set; }
		public string ReturnCity { get; set; }
		public string ReturnState { get; set; }
		public string ReturnZip { get; set; }
		public string ReturnPhone { get; set; }
		public string ToFirm { get; set; }
		public string FromFirm { get; set; }

        public List<string> HistoricalBase64Labels { get; set; } = new List<string>();
		public string Base64Label { get; set; }
		public AdditionalShippingData AdditionalShippingData { get; set; }

		public List<CarrierApiError> CarrierApiErrors { get; set; } = new List<CarrierApiError>();
		public int LabelTypeId { get; set; }
		public List<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
		public List<LabelFieldValue> ReturnLabelFieldValues { get; set; } = new List<LabelFieldValue>();

        public DateTime? ClientShipDate { get; set; }
    }
}