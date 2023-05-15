namespace MMS.API.Domain.Models.CreatePackage
{
	public class CreatePackageResponse : BaseResponse
	{
		public string PackageId { get; set; }
		public string Barcode { get; set; }
		public string Carrier { get; set; }
		public string ShippingMethod { get; set; }
		public string CustomerReferenceNumber { get; set; }
		public string CustomerReferenceNumber2 { get; set; }
		public string CustomerReferenceNumber3 { get; set; }
		public string CustomerReferenceNumber4 { get; set; }
		public string Base64 { get; set; }

	}
}