using FedExShipApi;
using System.Threading.Tasks;

namespace PackageTracker.WebServices
{
	public interface IFedExShipClient
	{
		Task<processShipmentResponse> processShipmentAsync(ProcessShipmentRequest ProcessShipmentRequest);
	}
}
