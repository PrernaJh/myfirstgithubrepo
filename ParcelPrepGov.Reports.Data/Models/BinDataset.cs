using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class BinDataset : Dataset
	{
		[StringLength(36)]
		public string ActiveGroupId { get; set; } // [Index]    
		[StringLength(24)]
		public string BinCode { get; set; } // [Index] 

		[StringLength(24)]
		public string LabelListSiteKey { get; set; }
		[StringLength(100)]
		public string LabelListDescription { get; set; }
		[StringLength(5)]
		public string LabelListZip { get; set; }

		[StringLength(24)]
		public string OriginPointSiteKey { get; set; }
		[StringLength(100)]
		public string OriginPointDescription { get; set; }

		[StringLength(24)]
		public string DropShipSiteKeyPrimary { get; set; }
		[StringLength(100)]
		public string DropShipSiteDescriptionPrimary { get; set; }
		[StringLength(120)]
		public string DropShipSiteAddressPrimary { get; set; }
		[StringLength(120)]
		public string DropShipSiteCszPrimary { get; set; }
		[StringLength(24)]
		public string ShippingCarrierPrimary { get; set; }
		[StringLength(60)]
		public string ShippingMethodPrimary { get; set; }
		[StringLength(24)]
		public string ContainerTypePrimary { get; set; }
		[StringLength(24)]
		public string LabelTypePrimary { get; set; }
		[StringLength(24)]
		public string RegionalCarrierHubPrimary { get; set; }
		[StringLength(24)]
		public string DaysOfTheWeekPrimary { get; set; }
		[StringLength(24)]
		public string ScacPrimary { get; set; }
		[StringLength(24)]
		public string AccountIdPrimary { get; set; }

		[StringLength(24)]
		public string BinCodeSecondary { get; set; }
		[StringLength(24)]
		public string DropShipSiteKeySecondary { get; set; }
		[StringLength(100)]
		public string DropShipSiteDescriptionSecondary { get; set; }
		[StringLength(120)]
		public string DropShipSiteAddressSecondary { get; set; }
		[StringLength(120)]
		public string DropShipSiteCszSecondary { get; set; }
		[StringLength(60)]
		public string ShippingMethodSecondary { get; set; }
		[StringLength(24)]
		public string ContainerTypeSecondary { get; set; }
		[StringLength(24)]
		public string LabelTypeSecondary { get; set; }
		[StringLength(24)]
		public string RegionalCarrierHubSecondary { get; set; }
		[StringLength(24)]
		public string DaysOfTheWeekSecondary { get; set; }
		[StringLength(24)]
		public string ScacSecondary { get; set; }
		[StringLength(24)]
		public string AccountIdSecondary { get; set; }
		[StringLength(24)]
		public string ShippingCarrierSecondary { get; set; }

        public bool IsAptb { get; set; }
        public bool IsScsc { get; set; }
    }
}