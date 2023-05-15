namespace MMS.API.Domain.Models
{
	public class StartJobRequest
	{
		public string JobBarcode { get; set; }
		public string SiteName { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
