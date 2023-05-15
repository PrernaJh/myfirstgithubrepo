using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class SiteUserManagementViewModel
	{
		public string SiteName { get; set; }
		public List<string> AssignableSiteLocations { get; set; }
		public bool AllSites { get; set; }
	}
}
