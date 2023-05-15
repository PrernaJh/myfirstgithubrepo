namespace MMS.API.Domain.Models.Containers
{
    public class ReplaceContainerRequest
    {
		public string OldContainerId { get; set; }
		public string NewContainerId { get; set; }
		public string SiteName { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
