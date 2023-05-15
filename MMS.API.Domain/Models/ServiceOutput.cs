namespace MMS.API.Domain.Models
{
	public class ServiceOutput
	{
		public string ServiceRuleId { get; set; }
		public bool IsQCRequired { get; set; }
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string ServiceLevel { get; set; }
		public string ServiceRuleExtensionId { get; set; }
		public string OverrideGroupId { get; set; }
	}
}
