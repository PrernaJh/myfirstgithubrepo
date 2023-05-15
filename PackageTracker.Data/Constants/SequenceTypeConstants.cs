namespace PackageTracker.Data.Constants
{
	public static class SequenceTypeConstants
	{
		public const string Package = "PACKAGE";
		public const string SinglePackage = "SINGLE_PACKAGE";
		public const string Container = "CONTAINER";
		public const string Pallet = "PALLET";
		public const string EvsFile = "EVSFILE";
		public const string EvsFileName = "EVSFILENAME";
		public const string Job = "JOB";
		public const string OnTrac = "ONTRAC";
		public const string Pmod = "PMOD_CONTAINER";
		public const string Regional = "REGIONAL_CARRIER";

		public const string FourDigitMaxSequence = "9998";
		public const string FiveDigitMaxSequence = "99998";
		public const string SixDigitMaxSequence = "999998";
		public const string SevenDigitMaxSequence = "9999998";		
		public const string NineDigitMaxSequence = "999999998";		
		public const string SinglePackageMaxSequence = "1099999998";
		public const string SinglePackageStartSequence = "1000000000";
		public const int AsnImportMaxSequence = 999999998;
		// asn import new start as of 3/2/2022: 10000000 - 10 million
	}
}
