namespace MMS.API.Domain.Models.Containers
{
	public class ReprintClosedContainerRequest
	{
		public string SiteName { get; set; }
		public string ContainerId { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
