using PackageTracker.Data.Models;

namespace MMS.API.Domain.Interfaces
{
    public interface ICreatePackageZplProcessor
    {
        string GenerateUspsLabel(Package package, string shippingMethod);
        string GenerateErrorLabel(string errorLabelMessage, string siteName, string packageId, Package package = null);
        string GenerateThreeLineLabel(string lineOne, string lineTwo, string lineThree);
        string GenerateReturnLabel(Package package);
    }
}
