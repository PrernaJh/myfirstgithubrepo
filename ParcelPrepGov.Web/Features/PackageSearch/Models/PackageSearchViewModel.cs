using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.PackageSearch.Models
{
	public class PackageSearchViewModel
	{
		public string PackageId { get; set; }
		public string Barcode { get; set; }
		public string Ids { get; set; }
		public int IdType { get; set; }
		public bool PackageNotFound { get; set; }
		public PackageSearchResultViewModel PackageSearchResult { get; set; }
	}

	public class PackageSearchResultViewModel
	{
		public string PackageId { get; set; }
		private string _inquiryId;
		public int? PackageDatasetId { get; set; }
        
        public string InquiryId
		{
			get
			{
				return _inquiryId;
			}
			set { _inquiryId = value; }
		}
		public string InquiryIdHyperLink { get; set; }
        public string ServiceRequestNumber { get; set; }
        public string Barcode { get; set; }
		public string RecipientName { get; set; }
		public string RecipientAddress { get; set; }
		public string Carrier { get; set; }
		public string Type { get; set; }
		public decimal Weight { get; set; }
		public string CreateDate { get; set; }
		public List<TrackPackageResultViewModel> PackageTracking { get; set; } = new List<TrackPackageResultViewModel>();
		public string ActivityLocationPoliticalDivision { get; set; }
		public string ScanDateTimeUsps { get; set; }
		public string UspsLocation { get; set; }
		public string DeliveryDate { get; set; }
		public string DeliveryLocationSignedForByName { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string Status { get; set; }
		public string TrackingNumber { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string ShippingBarcode { get; set; }
		public string ShippingBarcodeHyperlink { get; set; }
        public string FscJob { get; set; }
        public string ProcessedDate { get; set; }
		public string PackageStatus { get; set; }
		public string BinCode { get; set; }
		public string SiteName { get; set; }
		public int Zone { get; set; }
		public string ServiceLevel { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsRural { get; set; }
		public bool IsOrmd { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsSaturday { get; set; }
		public bool IsOutside48States { get; set; }
		public bool IsDduScfBin { get; set; }
		public bool IsStopTheClock { get; set; }
		public bool IsUndeliverable { get; set; }

		public string MailCode { get; set; }
		public string MarkupType { get; set; }
		public string CosmosCreateDate { get; set; }
		public string ContainerId { get; set; }

		public string MedicalCenterId { get; set; }
		public string MedicalCenterName { get; set; }
		public string MedicalCenterAddress1 { get; set; }
		public string MedicalCenterAddress2 { get; set; }
		public string MedicalCenterCsz { get; set; }
        public string BinGroupId { get; internal set; }
        public string BinCodeDescription { get; set; }
		public string ClientName { get; set; }

        public string StopTheClockDate { get; set; }
        public string LastKnownDate { get; set; }
        public string LastKnownDescription { get; set; }
        public string LastKnownLocation { get; set; }
        public string LastKnownZip { get; set; }
    }

	public class TrackPackageResultViewModel
	{
		public string ShippingContainerId { get; set; }
		public string ShippingCarrier { get; set; }
		public string TrackingNumber { get; set; }
		public DateTime EventDate { get; set; }
		public string EventCode { get; set; }
		public string EventDescription { get; set; }
		public string EventLocation { get; set; }
		public string EventZip { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}
