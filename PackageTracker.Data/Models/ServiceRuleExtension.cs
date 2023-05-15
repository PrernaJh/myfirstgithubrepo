namespace PackageTracker.Data.Models
{
	public class ServiceRuleExtension : Entity
	{
		public string ActiveGroupId { get; set; }
		public string MailCode { get; set; }
		public string StateCode { get; set; }
		public bool IsDefault { get; set; }
		public bool InFedExList { get; set; }
		public bool InUpsList { get; set; }
		public bool IsSaturdayDelivery { get; set; }
		public decimal MinWeight { get; set; }
		public decimal MaxWeight { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string ServiceLevel { get; set; }
	}
}
