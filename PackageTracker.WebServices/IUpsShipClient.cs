using System.Threading.Tasks;
using UpsShipApi;

namespace PackageTracker.WebServices
{
	public interface IUpsShipClient
	{
		Task<ShipmentResponse1> ProcessShipmentAsync(UPSSecurity UPSSecurity, ShipmentRequest ShipmentRequest);
	}
}
