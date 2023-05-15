using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
	public interface IContainerBarcodeProcessor
	{
		Task<string> GenerateBagContainerId(Site site, ShippingContainer container, Bin bin);
		Task<(string Barcode, string HumanReadableBarcode)> GeneratePalletContainerId(Site site, ShippingContainer container);
		Task<string> GeneratePmodBarcode(Site site, ShippingContainer container, Bin bin);
		Task<(string Barcode, string HumanReadableBarcode)> GenerateRegionalCarrierBarcode(Site site, ShippingContainer container, Bin bin);

		Task<string> GenerateOnTracBarcode(Site site, ShippingContainer container);
		string GenerateOnTracPdfBarcode(Site site, ShippingContainer container, string trackingNumber, string recipientContactName, (string City, string State, string FullZip) cityStateZip, bool isSaturday);
	}
}

