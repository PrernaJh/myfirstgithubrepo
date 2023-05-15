using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.Web.Domain.Models
{
	public class GetBinMapsResponse
	{
		public GetBinMapsResponse()
		{
			BinMaps = new List<BinMap>();
		}
		public List<BinMap> BinMaps { get; set; }
	}
}
