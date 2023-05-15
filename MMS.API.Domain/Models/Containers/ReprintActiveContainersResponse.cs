using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class ReprintActiveContainersResponse
	{
		public List<CreateContainer> Containers { get; set; } = new List<CreateContainer>();		
		public List<string> FailedBinCodes { get; set; } = new List<string>();
	}
}
