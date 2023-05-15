namespace ParcelPrepGov.Web.Models
{
	public class NavigationMenuModel
	{
		public bool HasDashboard { get; set; }
		public bool HasUserManagement { get; set; }
		public bool HasServiceManagement { get; set; }
		public bool HasFileManagement { get; set; }
		public bool HasReporting { get; set; }
		public bool HasPackageSearch { get; set; }
		public bool HasRecallRelease { get; set; }
		public bool HasServiceOverride { get; set; }

	}
}
