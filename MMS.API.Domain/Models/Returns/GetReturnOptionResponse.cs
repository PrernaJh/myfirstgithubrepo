using PackageTracker.Data.Models.ReturnOptions;
using System.Collections.Generic;

namespace MMS.API.Domain.Models.Returns
{
	public class GetReturnOptionResponse
	{
		public GetReturnOptionResponse()
		{
			ReturnReasons = new List<ReturnReason>();
			ReasonDescriptions = new List<ReasonDescription>();
		}

		public List<ReturnReason> ReturnReasons { get; set; }
		public List<ReasonDescription> ReasonDescriptions { get; set; }
	}
}