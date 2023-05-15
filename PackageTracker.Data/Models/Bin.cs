namespace PackageTracker.Data.Models
{
	public class Bin : Entity
	{
		public string ActiveGroupId { get; set; }
		public string BinCode { get; set; }
		public string LabelListSiteKey { get; set; }
		public string LabelListDescription { get; set; }
		public string LabelListZip { get; set; }
		public string OriginPointSiteKey { get; set; }
		public string OriginPointDescription { get; set; }
		public string DropShipSiteKeyPrimary { get; set; }
		public string DropShipSiteDescriptionPrimary { get; set; }
		public string DropShipSiteAddressPrimary { get; set; }
		public string DropShipSiteCszPrimary { get; set; }
		public string DropShipSiteNotePrimary { get; set; }
		public string ShippingCarrierPrimary { get; set; }
		public string ShippingMethodPrimary { get; set; }
		public string ContainerTypePrimary { get; set; }
		public string LabelTypePrimary { get; set; }
		public string RegionalCarrierHubPrimary { get; set; }
		public string DaysOfTheWeekPrimary { get; set; }
		public string ScacPrimary { get; set; }
		public string AccountIdPrimary { get; set; }
		public string BinCodeSecondary { get; set; }
		public string DropShipSiteKeySecondary { get; set; }
		public string DropShipSiteDescriptionSecondary { get; set; }
		public string DropShipSiteAddressSecondary { get; set; }
		public string DropShipSiteCszSecondary { get; set; }
		public string DropShipSiteNoteSecondary { get; set; }
		public string ShippingCarrierSecondary { get; set; }
		public string ShippingMethodSecondary { get; set; }
		public string ContainerTypeSecondary { get; set; }
		public string LabelTypeSecondary { get; set; }
		public string RegionalCarrierHubSecondary { get; set; }
		public string DaysOfTheWeekSecondary { get; set; }
		public string ScacSecondary { get; set; }
		public string AccountIdSecondary { get; set; }
		public bool IsAptb { get; set; }
		public bool IsScsc { get; set; }
	}
}
