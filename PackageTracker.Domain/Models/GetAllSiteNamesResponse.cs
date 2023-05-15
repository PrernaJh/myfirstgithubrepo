using System.Collections.Generic;

namespace PackageTracker.Domain.Models
{
	public class GetAllSiteNamesResponse
	{
		public GetAllSiteNamesResponse()
		{
			SiteNames = new List<string>();
		}

		public List<string> SiteNames { get; set; }
	}
}
