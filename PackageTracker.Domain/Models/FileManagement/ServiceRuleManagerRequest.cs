namespace PackageTracker.Domain.Models.FileManagement
{
	public class ServiceRuleManagerRequest
	{
		public string MailCode { get; set; }
		public string CustomerName { get; set; }
		public bool IsOrmd { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsNonContiguousState { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsSaturday { get; set; }
		public bool IsDduScf { get; set; }
		public int Zone { get; set; }
		public decimal Weight { get; set; } // ounces
		public decimal Length { get; set; } // inches
		public decimal Width { get; set; }
		public decimal Height { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }
		public decimal TotalDimensions { get; set; }
	}
}
