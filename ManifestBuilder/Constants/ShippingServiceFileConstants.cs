namespace ManifestBuilder
{
	public static class ShippingServiceFileConstants
	{
		//Electronic File  Types
		public const string PostageandTrackingFile = "1"; //used for electronic Verification System (eVS).
		public const string TrackingFile = "2";
		public const string ReturnsService = "3";
		public const string Corrections = "4";

		//Entry Facility Type
		public const string ADC = "A";
		public const string NDC = "B";
		public const string SCF = "S";
		public const string DU = "D";
		public const string ASF = "F";
		public const string ISC = "I";

		//Method of Payment
		public const string PermitSystem = "01";
		public const string FederalAgency = "03";
		public const string PCPostage = "04";
		public const string SmartMeter = "05";
		public const string OtherMeter = "06";
		public const string Stamps = "07";

		public const string PostOfficeofAccountZIPCode = "20260";

		//Postage Type
		public const string Published = "P";
		public const string Customized = "C";
		public const string CommercialPlusPricing = "A";
		public const string CommercialBasedPricing = "B";
		public const string Retail = "R";

		//Unit of Measure  Code
		public const string LBS = "1";
		public const string OZ = "2";
		public const string KILOS = "3";

		//Processing Category Codes
		public const string Cards = "0";
		public const string Letters = "1";
		public const string Flats = "2";
		public const string MachinableParcel = "3";
		public const string IrregularParcel = "4";
		public const string NonMachinableParcel = "5";
		public const string Catalogs = "C";
		public const string OpenAndDistribute = "O";
		public const string Returns = "R";

		//Rate Indicator Codes
		public const string FiveDigitPrice = "DF";
		public const string ThreeDigitPrice = "DE";
		public const string SinglePiece = "SP";
		public const string Pallet = "O5";

		//Destination Rate Indicator Codes
		public const string DestinationAreaDistributionCenter = "A";
		public const string DestinationNetworkDistributionCenter = "B";
		public const string DestinationDeliveryUnit = "D";
		public const string DestinationAuxiliaryServiceFacility = "F";
		public const string InternationalServiceOffice = "I";
		public const string None = "N";
		public const string DestinationSectionalCenterFacility = "S"; //scf

		//Postal Routing Barcode Codes 
		public const string NOBARCODE = "0";
		public const string GS1128BARCODE = "1";

		//filename 
		public const string Filename = "logonid";

		// Class of mail 
		public const string UspsFirstClass = "FC";//First-Class Package Service
		public const string UspsPriority = "PM";//Priority Mail
		public const string UspsParcelSelect = "PS";//Parcel Select
		public const string UspsLightWeight = "LW";//Light Weight
		public const string UspsBPM = "BB";//BoundPrintedMatter
		public const string UspsMarketingMail = "SA";//MarketingMail
		public const string UspsMediaMail = "BS";//MediaMail
		public const string UspsLibraryMail = "BL";//LibraryMail
	}
}
