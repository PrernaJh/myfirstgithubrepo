namespace PackageTracker.Data.Models
{
	public class ServiceOverride
	{
		public string OldShippingCarrier { get; set; }
		public string OldShippingMethod { get; set; }
		public string NewShippingCarrier { get; set; }
		public string NewShippingMethod { get; set; }
	}
}
