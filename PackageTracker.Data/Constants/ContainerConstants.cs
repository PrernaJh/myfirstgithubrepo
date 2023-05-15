namespace PackageTracker.Data.Constants
{
	public static class ContainerConstants
	{
		public const string PmodBag = "PMOD_BAG";
		public const string PmodPallet = "PMOD_PALLET";
		public const string ThirdPartyBag = "3RD_PARTY_BAG";
		public const string ThirdPartyPallet = "3RD_PARTY_PALLET";
		public const string ThirdPartyBagGS1 = "3RD_PARTY_BAG_GS1";
		public const string ThirdPartyPalletGS1 = "3RD_PARTY_PALLET_GS1";
		public const string UspsOriginPallet = "USPS_ORIGIN_PALLET";
		public const string LsoBag = "LSO_BAG";
		public const string LsoPallet = "LSO_PALLET";
		public const string OnTracBag = "ONTRAC_BAG";
		public const string OnTracPallet = "ONTRAC_PALLET";
		public const string ContainerTypeBag = "BAG";
		public const string ContainerTypePallet = "PALLET";
		public const string ContainerTypePackage = "PACKAGE";

		// shipment types
		public const string PalletShipmentTypeId = "510";
		public const string BagShipmentTypeId = "520";

		// container shipping carriers from bins
		public const string FedExCarrier = "FEDEX";
		public const string UspsCarrier = "USPS";
		public const string UpsCarrier = "UPS";
		public const string OnTracCarrier = "ONTRAC";
		public const string LsoCarrier = "LSO";
		public const string FirstChoice = "1ST_CHOICE";
		public const string MarkIV = "MARKIV";
		public const string UnitedDeliveryService = "UNITED_DELIVERY_SERVICE";
		public const string Cx = "CX";
		public const string Waltco = "WALTCO_GREEN_BAY";
		public const string Adl = "ADL";
		public const string GencoCharleston = "GENCO_CHARLESTON";
		public const string Hackbarth = "HACKBARTH";
		public const string LocalUsps = "LOCAL_USPS";

		// container shipping methods from bins
		public const string RegionalCarrier = "REGIONAL_CARRIER";
		public const string FedExExpress = "EXPRESS"; // this supports legacy bins and defaults to Express Standard
		public const string FedExExpressStandard = "EXPRESS_STANDARD";
		public const string FedExExpressPriority = "EXPRESS_PRIORITY";
		public const string FedExGround = "GROUND";
		public const string Ltl = "LTL";
		public const string UspsFirstClass = "FIRST_CLASS";
		public const string UspsPmodBag = "PMOD_BAG";
		public const string UspsPmodPallet = "PMOD_PALLET";
		public const string UspsPriority = "PRIORITY";

	}
}