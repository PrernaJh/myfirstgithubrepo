using System;

namespace PackageTracker.Domain.Models.FileProcessing
{
	public class ConsumerDetailFileRecord
	{
		public string CmopId { get; set; }
		public string PackageId { get; set; }
		public string Carrier { get; set; }
		public string TrackingNumber { get; set; }
		public string ManifestDate { get; set; }
		public string ShippingDate { get; set; }
		public string Weight { get; set; }
	}
}
