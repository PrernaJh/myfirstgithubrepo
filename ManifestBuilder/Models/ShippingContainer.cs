using System;
using System.Collections.Generic;

namespace ManifestBuilder
{
	public class ShippingContainer
	{
		public string ContainerId { get; set; } // [Index]
		public ShippingCarrier ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public ContainerType ContainerType { get; set; }
		public string CarrierBarcode { get; set; }
		public string EntryZip { get; set; }
		public EntryFacilityType EntryFacilityType { get; set; }
	}
}
