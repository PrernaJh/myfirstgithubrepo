namespace PackageTracker.Data.Constants
{
	public static class BarcodeConstants
	{
		public const string PriorityExpressServiceTypeCode = "701";

		public const string ParcelSelectDelConServiceTypeCode = "612";
		public const string ParcelSelectLightWeightDelConServiceTypeCode = "748";
		public const string FirstClassDelConServiceTypeCode = "001";
		public const string PriorityDelConServiceTypeCode = "055";

		public const string ParcelSelectSignatureServiceTypeCode = "615";
		public const string ParcelSelectLightWeightSignatureServiceTypeCode = "835";
		public const string FirstClassSignatureServiceTypeCode = "021";
		public const string PrioritySignatureServiceTypeCode = "108";

		public const string RoutingApplicationIdentifier = "420";

		public const string ChannelApplicationIdentifierNineDigitMid = "92";
		public const string ChannelApplicationIdentifierSixDigitMid = "93";
		public const string GS1RoutingCodeApplicationIdentifier = "403";
		public const string GS1SerialNumberApplicationIdentifier = "21";	

		public const int UspsCheckDigitMultiplier = 3;
		public const int ContainerCheckDigitMultiplier = 3;
	}
}
