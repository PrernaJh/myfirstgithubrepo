namespace MMS.API.Domain.Models.OperationalContainers
{
	public class AddOperationalContainerRequest
	{
		public string Id { get; set; }
		public string SiteName { get; set; }
		public string BinCode { get; set; }
		public string ContainerId { get; set; }
		public string Status { get; set; }
		public bool IsSecondaryCarrier { get; set; }
	}
}
