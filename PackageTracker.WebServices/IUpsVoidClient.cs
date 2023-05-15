using System.Threading.Tasks;
using UpsVoidApi;

namespace PackageTracker.WebServices
{
	public interface IUpsVoidClient
	{
		Task<VoidShipmentResponse1> ProcessVoidAsync(UPSSecurity UPSSecurity, VoidShipmentRequest VoidShipmentRequest);
	}
}