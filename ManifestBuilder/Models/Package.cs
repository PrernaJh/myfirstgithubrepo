using System;
using System.Collections.Generic;

namespace ManifestBuilder
{
    public class Package 
    {
		public string ContainerId { get; set; }
		public string TrackingNumber { get; set; }
		public ServiceType ServiceType { get; set; }

		public ProcessingCategory ProcessingCategory { get; set; }
		public int Zone { get; set; }
		public decimal Weight { get; set; } // pounds
		public string MailerId { get; set; }	
		public decimal Cost { get; set; }
		public bool IsPoBox { get; set; }
		public string RecipientName { get; set; }
		public string AddressLine1 { get; set; }
		public string Zip { get; set; }
		public string ReturnAddressLine1 { get; set; }
		public string ReturnCity { get; set; }
		public string ReturnState { get; set; }
		public string ReturnZip { get; set; }
		public string EntryZip { get; set; }
		public string DestinationRateIndicator { get; set; }
		public EntryFacilityType EntryFacilityType { get; set; }
		public string MailProducerCrid { get; set; }

		public string ParentMailOwnerMid { get; set; }
		public string UspsMailOwnerMid { get; set; }
		public string ParentMailOwnerCrid { get; set; }
		public string UspsMailOwnerCrid { get; set; }
		public string UspsPermitNo { get; set; }
		public string UspsPermitNoZip { get; set; }
		public string UspsPaymentMethod { get; set; }
		public string UspsPostageType { get; set; }
		public string UspsCsscNo { get; set; }
		public string UspsCsscProductNo { get; set; }
	}
}