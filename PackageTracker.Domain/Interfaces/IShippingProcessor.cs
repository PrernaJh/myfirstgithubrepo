using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IShippingProcessor
	{
		Task<bool> GetShippingDataAsync(Package package);
		Task<(string Barcode, string Base64Label)> GetUpsShippingDataAsync(Package package, string upsAccountNumber, bool addCustomsData, bool isZpl, bool isDirectDeliveryOnly);
		Task<bool> VoidUpsShipmentAsync(Package package, SubClient subClient);
		Task<bool> AssignUpsCustomsDataToPackage(Package package, SubClient subClient);
	}
}