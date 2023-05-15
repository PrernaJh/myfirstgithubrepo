namespace PackageTracker.Data.Constants
{
	public static class EventConstants
	{
		// package status

        public const string Imported = "IMPORTED";
        public const string Processed = "PROCESSED";
        public const string Recalled = "RECALLED";
        public const string Released = "RELEASED";
        public const string Exception = "EXCEPTION";
        public const string Replaced = "REPLACED";
        public const string Blocked = "BLOCKED";
		public const string Repeat = "REPEAT";
		public const string Created = "CREATED";
		public const string Deleted = "DELETED";

		// recall status
		public const string RecallCreated = "CREATED";
		public const string RecallScanned = "SCANNED";

		// package event types

		public const string FileImport = "FILEIMPORT";
		public const string Reprint = "REPRINT";
		public const string AutoScan = "AUTOSCAN";
		public const string ManualScan = "MANUALSCAN";
		public const string RepeatScan = "REPEATSCAN";
		public const string RateAssigned = "RATEASSIGNED";
		public const string RateUpdated = "RATEUPDATED";
		public const string ContainerAssigned = "CONTAINERASSIGNED";
		public const string ForcedException = "FORCEDEXCEPTION";
		public const string ManualRecall = "MANUALRECALL";
		public const string ManualRelease = "MANUALRELEASE";
		public const string ManualReturn = "MANUALRETURN";
		public const string ManualDelete = "MANUALDELETE";
		public const string EodProcessed = "EODPROCESSED";
		public const string Shipped = "SHIPPED";
		public const string CreateSinglePackage = "CREATESINGLEPACKAGE";
		public const string ProcessSinglePackae = "PROCESSSINGLEPACKAGE";
		public const string ReturnSinglePackage = "RETURNSINGLEPACKAGE";
		public const string DeleteSinglePackage = "DELETESINGLEPACKAGE";

		// job
		public const string JobScan = "JOBSCAN";
		public const string JobStarted = "STARTED";
	}
}