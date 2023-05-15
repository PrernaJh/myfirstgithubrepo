namespace PackageTracker.Data.Constants
{
	public static class ServiceConstants
	{
		// job option constants
		public const string Irregular = "IRREGULAR";
		public const string Ormd = "ORMD";

		// USPS request codes
		public const string UspsFirstClassRequestCode = "FIRST CLASS";

		// UPS request codes
		public const string UpsNextDayAirRequestCode = "01";
		public const string UpsNextDayAirSaverRequestCode = "13";
		public const string UpsSecondDayAirRequestCode = "02";
		public const string UpsGroundRequestCode = "03";
		public const string UpsDelConRequestCode = "1";
		public const string UpsSignatureRequestCode = "2";
		public const string UpsIrregularRequestCode = "IR";
		public const string UpsMachinableRequestCode = "MA";
	}
}
