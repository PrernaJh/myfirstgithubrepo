namespace PackageTracker.Data.Constants
{
	public static class ErrorLabelConstants
	{
		// packages

		public const string InvalidPackageId = "INVALID PACKAGE ID";
		public const string InvalidWeight = "INVALID WEIGHT";
		public const string InvalidJob = "INVALID JOB";		
		public const string ServiceRuleNotFound = "SERVICE RULE NOT FOUND";		
		public const string CarrierDataError = "CARRIER DATA ERROR";
		public const string PackageRecalled = "PACKAGE RECALLED";
		public const string OperatorGenerated = "OPERATOR GENERATED";
		public const string InvalidStatus = "INVALID STATUS";

		// FSC messages

		public const string ReturnToCustomer = "RETURN TO CUSTOMER";
		public const string Recall = "(RC)";

        // single packages

        public const string SubClientError = "SUBCLIENT DATA ERROR";
		public const string BinError = "BIN DATA ERROR";
		public const string ServiceError = "SERVICE DATA ERROR";
		public const string TrackingNumberError = "TRACKING NUMBER ERROR";
		public const string LabelError = "LABEL ERROR";
		public const string SortCodeChange = "SORT CODE CHANGE";

		// containers

		public const string BinFailure = "BIN NOT FOUND";
		public const string ContainerQueryFailure = "CONTAINER ID NOT FOUND";

		// shared

		public const string EndOfDayProcessed = "END OF DAY PROCESSED";

	}
}
