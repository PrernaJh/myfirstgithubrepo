namespace MMS.API.Domain.Models.OperationalContainers
{
	public class UpdateOperationalContainerRequest
	{
		public string Id { get; set; }
		public string SiteName { get; set; }
		public string BinCode { get; set; }
        public string Status { get; set; }
        public bool IsSecondaryCarrier { get; set; }
	}
}
