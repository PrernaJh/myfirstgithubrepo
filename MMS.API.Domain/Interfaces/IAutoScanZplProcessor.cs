using PackageTracker.Data.Models;

namespace MMS.API.Domain.Interfaces
{
	public interface IAutoScanZplProcessor
	{
		string GenerateUspsLabel(Package package);
		string GenerateUpsGroundLabel(Package package, string upsBase64);
		string GenerateUpsAirLabel(Package package, string upsBase64);
		string GenerateErrorLabel(string errorLabelMessage, string siteName, string packageId, string timeZone = null);
	}
}