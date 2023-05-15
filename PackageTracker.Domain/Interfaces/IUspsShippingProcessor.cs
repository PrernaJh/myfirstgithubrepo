using PackageTracker.Data.Models;

namespace PackageTracker.Domain.Interfaces
{
	public interface IUspsShippingProcessor
	{
		string GenerateUspsBarcode(Package package);
		string GenerateUspsFormattedBarcode(string barcode);
		string GenerateUspsHumanReadableBarcode(string barcode);
	}
}
