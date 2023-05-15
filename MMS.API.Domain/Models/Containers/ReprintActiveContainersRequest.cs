using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class ReprintActiveContainersRequest
	{
		public string SiteName { get; set; }
		public List<string> BinCodes { get; set; } = new List<string>();
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
