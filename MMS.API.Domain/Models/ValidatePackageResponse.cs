namespace MMS.API.Domain.Models
{
	public class ValidatePackageResponse
	{
		public bool IsValid { get; set; }
		public string ShippingBarcode { get; set; }
		public string PackageId { get; set; }
		public string BinCode { get; set; }
		public string FullAddress { get; set; }
		public string Message { get; set; }
		public string RecipientName { get; set; }

	}
}
