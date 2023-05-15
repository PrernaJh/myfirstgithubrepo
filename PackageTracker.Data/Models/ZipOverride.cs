namespace PackageTracker.Data.Models
{
	public class ZipOverride : Entity
	{
		public string ZipCode { get; set; }
		public string ActiveGroupType { get; set; }
		public string ActiveGroupId { get; set; }
		public string FromShippingCarrier { get; set; }
		public string FromShippingMethod { get; set; }
		public string ToShippingCarrier { get; set; }
		public string ToShippingMethod { get; set; }
	}
}
