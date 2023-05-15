namespace PackageTracker.Data.Models
{
	public class OperationalContainer : Entity
	{
		public string SiteName { get; set; }
		public string BinCode { get; set; }
		public string ContainerId { get; set; }
		public string Status { get; set; }
		public bool IsSecondaryCarrier { get; set; }
	}
}
