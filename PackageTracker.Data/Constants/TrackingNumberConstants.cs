using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Constants
{
	public static class TrackingNumberConstants
	{
		public const string FirstClassServiceTypeCode = "001";
		public const string PriorityExpressServiceTypeCode = "701";
		public const string ParcelSelectLightWeightServiceTypeCode = "748";
		public const string ParcelSelectServiceTypeCode = "612";
		public const string PriorityServiceTypeCode = "055";

		//public const string ParcelSelectSignatureServiceTypeCode = "835";
		//public const string FirstClassSignatureServiceTypeCode = "021";
		//public const string PrioritySignatureServiceTypeCode = "108";

		public const string RoutingApplicationIdentifier = "420";
		public const string ChannelApplicationIdentifierNineDigitMid = "92";
		public const string ChannelApplicationIdentifierSixDigitMid = "93";
		public const int UspsCheckDigitMultiplier = 3;
		public const int ContainerCheckDigitMultiplier = 3;
	}
}
