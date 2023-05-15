using System.ComponentModel.DataAnnotations;


namespace MMS.API.Domain.Models
{
	public class ValidatePackageRequest : PackageRequest
	{
		[Required]
		public string ShippingBarcode { get; set; }
	}
}
