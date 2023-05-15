using System;

namespace PackageTracker.Data.Models.TrackingData
{
	public class UspsTrackingData
	{
		public string ElectronicFileNumber { get; set; }
		public string MailerID { get; set; }
		public string MailerName { get; set; }
		public string DestinationZipCode { get; set; }
		public string DestinationZipPlus4 { get; set; }
		public string ScanningFacilityZip { get; set; }
		public string ScanningFacilityName { get; set; }
		public string EventCode { get; set; }
		public string EventName { get; set; }
		public DateTime EventDateTime { get; set; }
		public string MailOwnerMailerID { get; set; }
		public string CustomerReferenceNumber { get; set; }
		public string DestinationCountryCode { get; set; }
		public string RecipientName { get; set; }
		public string OriginalLabel { get; set; }
		public string UnitOfMeasureCode { get; set; }
		public string Weight { get; set; }
		public DateTime DeliveryDateTime { get; set; }
		public string LogisticsManagerMailerID { get; set; }
	}
}
