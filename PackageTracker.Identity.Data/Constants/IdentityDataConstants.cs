namespace PackageTracker.Identity.Data.Constants
{
	public static class IdentityDataConstants
	{
		// roles
		public const string Administrator = "ADMINISTRATOR,SYSTEMADMINISTRATOR";
		public const string Supervisor = "SUPERVISOR";
		public const string Operator = "OPERATOR";
		public const string GMAdminSuperQA = "GENERALMANAGER,ADMINISTRATOR,SUPERVISOR,SYSTEMADMINISTRATOR,QUALITYASSURANCE";
		public const string SystemAdministrator = "SYSTEMADMINISTRATOR";
		public const string ScannerService = "SCANNERSERVICE";

		// misc
		public const string SiteKeyValueTypeName = "Site";
		public const string Global = "GLOBAL";
	}
}