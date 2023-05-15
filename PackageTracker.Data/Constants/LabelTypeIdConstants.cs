namespace PackageTracker.Data.Constants
{
	public static class LabelTypeIdConstants
	{
		// packages
		public const int Error = 0;
		public const int UspsPackage = 1;
		public const int UpsShipping = 2;
		public const int FedexShipping = 3;
		public const int ReturnToSender = 4;
		public const int AutoScan = 15;
		public const int SortCodeChange = 17;

		// containers
		public const int CreateContainer = 5;
		public const int PmodContainer = 6;
		public const int ThirdPartyShipping = 7;
		public const int FedexExpressContainer = 8;
		public const int FedexGroundContainer = 9;
		public const int OnTrac = 11;
		public const int ThirdPartyShippingGS1 = 18;

		//public const int PmodBag = 5;
		//public const int PmodPallet = 6;
		//public const int UspsPallet = 7;
		//public const int ThirdPartyShipping = 8;
		//public const int UspsBag = 9;
		//public const int LsoBag = 11;
		//public const int LsoPallet = 12;
		//public const int OnTracBag = 13;
		//public const int OnTracPallet = 14;

		// jobs
		public const int JobReceivingTicket = 10;
	}
}
