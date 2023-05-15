using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
    public class EvsContainer : EodChildRecord
    {
		public static ManifestBuilder.ShippingContainer GetManifestBuilderShippingContainer(EvsContainer container)
		{
			return new ManifestBuilder.ShippingContainer
			{
				ContainerId = container.ContainerId,
				ShippingCarrier = (ManifestBuilder.ShippingCarrier) container.ShippingCarrier,
				ShippingMethod = container.ShippingMethod,
				ContainerType = (ManifestBuilder.ContainerType) container.ContainerType,
				CarrierBarcode = container.CarrierBarcode,
				EntryZip = container.EntryZip,
				EntryFacilityType = (ManifestBuilder.EntryFacilityType) container.EntryFacilityType,
			};
		}
		[StringLength(100)]
		public string ContainerId { get; set; } // [Index]
		[StringLength(24)]
		public int ShippingCarrier { get; set; }
		[StringLength(60)]
		public string ShippingMethod { get; set; }
		public int ContainerType { get; set; }
		[StringLength(100)]
		public string CarrierBarcode { get; set; }
		[StringLength(10)]
		public string EntryZip { get; set; }
		public int EntryFacilityType { get; set; }
	}
}