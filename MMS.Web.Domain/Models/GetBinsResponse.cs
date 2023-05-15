using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.Web.Domain.Models
{
	public class GetBinsResponse
	{
		public GetBinsResponse()
		{
			Bins = new List<Bin>();
		}

		public List<Bin> Bins { get; set; }
	}
}
