namespace PackageTracker.Domain.Models.FileManagement
{
	public class ServiceRuleManagerResponse
	{
		public string ServiceRuleGroupId { get; set; }
		public string CustomerName { get; set; }
		public string MailCode { get; set; }
		public bool IsOrmd { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsNonContiguousState { get; set; }
		public bool IsUpsDas { get; set; }
		public bool IsSaturday { get; set; }
		public bool IsDduScf { get; set; }
		public decimal MinWeight { get; set; }
		public decimal MaxWeight { get; set; }
		public decimal MinLength { get; set; }
		public decimal MaxLength { get; set; }
		public decimal MinHeight { get; set; }
		public decimal MaxHeight { get; set; }
		public decimal MinWidth { get; set; }
		public decimal MaxWidth { get; set; }
		public decimal MinTotalDimensions { get; set; }
		public decimal MaxTotalDimensions { get; set; }
		public int ZoneMin { get; set; }
		public int ZoneMax { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string ServiceLevel { get; set; }
	}
}
