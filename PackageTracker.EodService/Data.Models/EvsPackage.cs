using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
    public class EvsPackage : EodChildRecord
    {
		public static ManifestBuilder.Package GetManifestBuilderPackage(EvsPackage package)
		{
			return new ManifestBuilder.Package
			{
				ContainerId = package.ContainerId,
				TrackingNumber = package.TrackingNumber,
				ServiceType = (ManifestBuilder.ServiceType) package.ServiceType,
				ProcessingCategory = (ManifestBuilder.ProcessingCategory) package.ProcessingCategory,
				Zone = package.Zone,
				Weight = package.Weight,
				MailerId = package.MailerId,
				Cost = package.Cost,
				IsPoBox = package.IsPoBox,
				RecipientName = package.RecipientName,
				AddressLine1 = package.AddressLine1,
				Zip = package.Zip,
				ReturnAddressLine1 = package.ReturnAddressLine1,
				ReturnCity = package.ReturnCity,
				ReturnState = package.ReturnState,
				ReturnZip = package.ReturnZip,
				EntryZip = package.EntryZip,
				DestinationRateIndicator = package.DestinationRateIndicator,
				EntryFacilityType = (ManifestBuilder.EntryFacilityType) package.EntryFacilityType,
				MailProducerCrid = package.MailProducerCrid,
				ParentMailOwnerMid = package.ParentMailOwnerMid,
				UspsMailOwnerMid = package.UspsMailOwnerMid,
				ParentMailOwnerCrid = package.ParentMailOwnerCrid,
				UspsMailOwnerCrid = package.UspsMailOwnerCrid,
				UspsPermitNo = package.UspsPermitNo,
				UspsPermitNoZip = package.UspsPermitNoZip,
				UspsPaymentMethod = package.UspsPaymentMethod,
				UspsPostageType = package.UspsPostageType,
				UspsCsscNo = package.UspsCsscNo,
				UspsCsscProductNo = package.UspsCsscProductNo,
			};
		}
		[StringLength(100)]
		public string ContainerId { get; set; }
		[StringLength(100)]
		public string TrackingNumber { get; set; }
		public int ServiceType { get; set; }
		public int ProcessingCategory { get; set; }
		public int Zone { get; set; }
		public decimal Weight { get; set; } // pounds
		[StringLength(24)]
		public string MailerId { get; set; }
		public decimal Cost { get; set; }
		public bool IsPoBox { get; set; }
		[StringLength(60)]
		public string RecipientName { get; set; }
		[StringLength(120)]
		public string AddressLine1 { get; set; }
		[StringLength(10)]
		public string Zip { get; set; }
		[StringLength(120)]
		public string ReturnAddressLine1 { get; set; }
		[StringLength(60)]
		public string ReturnCity { get; set; }
		[StringLength(30)]
		public string ReturnState { get; set; }
		[StringLength(10)]
		public string ReturnZip { get; set; }
		[StringLength(10)]
		public string EntryZip { get; set; }
		[StringLength(1)]
		public string DestinationRateIndicator { get; set; }
		public int EntryFacilityType { get; set; }
		[StringLength(10)]
		public string MailProducerCrid { get; set; }
		[StringLength(10)]
		public string ParentMailOwnerMid { get; set; }
		[StringLength(10)]
		public string UspsMailOwnerMid { get; set; }
		[StringLength(10)]
		public string ParentMailOwnerCrid { get; set; }
		[StringLength(10)]
		public string UspsMailOwnerCrid { get; set; }
		[StringLength(10)]
		public string UspsPermitNo { get; set; }
		[StringLength(10)]
		public string UspsPermitNoZip { get; set; }
		[StringLength(2)]
		public string UspsPaymentMethod { get; set; }
		[StringLength(1)]
		public string UspsPostageType { get; set; }
		[StringLength(10)]
		public string UspsCsscNo { get; set; }
		[StringLength(10)]
		public string UspsCsscProductNo { get; set; }
	}
}
