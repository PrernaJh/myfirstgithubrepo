namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class SearchServiceRulesViewModel
	{
		public string CustomerName { get; set; }
		public string MailCode { get; set; }
		public double Weight { get; set; }
		public double Length { get; set; }
		public double Height { get; set; }
		public double Width { get; set; }
		public double TotalDimensions { get; set; }
		public int Zone { get; set; }

		public bool IsOrmd { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsNonContiguousState { get; set; }
		public bool IsUpsDas { get; set; }
	}

	public class SearchServiceRulesResponseViewModel
	{
		public string ShippingCarrier { get; set; }
		public string ShippingMethod { get; set; }
		public string ServiceLevel { get; set; }
		public string Title { get; set; }
	}

}
