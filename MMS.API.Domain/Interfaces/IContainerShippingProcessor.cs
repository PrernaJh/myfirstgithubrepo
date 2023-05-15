using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
	public interface IContainerShippingProcessor
	{
		Task<FedExShippingDataResponse> GetFedexContainerShippingData(ShippingContainer container);
	}
}
