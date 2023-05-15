using System.Collections.Generic;

namespace MMS.API.Domain.Models
{
	public class GetPackageHistoryResponse
	{
		public GetPackageHistoryResponse()
		{
			PackageHistoryViewItems = new List<PackageHistoryViewItem>();
		}

		public List<PackageHistoryViewItem> PackageHistoryViewItems { get; set; }
	}
}
