using PackageTracker.Data.Models;

namespace MMS.API.Domain.Models.Containers
{
	public class CreateContainerResponse
	{
		public ShippingContainer ShippingContainer { get; set; }
		public bool IsSuccessful { get; set; }
	}
}
