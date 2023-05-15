namespace MMS.API.Domain.Models
{
	public class PackageHistoryViewItem
	{
		public string EventSource { get; set; }
		public string EventDate { get; set; }
		public string Description { get; set; }
		public string Weight { get; set; }
		public string PackageStatus { get; set; }
		public string Username { get; set; }
	}
}