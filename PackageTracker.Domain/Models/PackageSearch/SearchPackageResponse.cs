using System.Collections.Generic;

namespace PackageTracker.Domain.Models.PackageSearch
{
	public class SearchPackageResponse
	{
		public SearchPackageResponse()
		{
			SearchPackageList = new List<SearchPackage>();
		}

		public List<SearchPackage> SearchPackageList { get; set; }
	}
}