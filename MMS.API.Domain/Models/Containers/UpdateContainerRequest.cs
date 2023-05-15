namespace MMS.API.Domain.Models.Containers
{
	public class UpdateContainerRequest
	{
		public string ContainerId { get; set; }
		public string Weight { get; set; }
		public bool IsSecondaryCarrier { get; set; }
		public bool? IsSaturdayDelivery { get; set; }
		public string SiteName { get; set; }
		public string Username { get; set; }
		public string MachineId { get; set; }
	}
}
