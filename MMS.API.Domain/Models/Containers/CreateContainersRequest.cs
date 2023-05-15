using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
	public class CreateContainersRequest
	{
		public string SiteName { get; set; }
		public string NumberOfCopies { get; set; }
		public List<string> BinCodes { get; set; } = new List<string>();
		public bool IsSecondaryCarrier { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
