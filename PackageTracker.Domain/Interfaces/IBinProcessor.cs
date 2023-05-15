using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PackageTracker.Domain.Interfaces
{
    public interface IBinProcessor
    {
        Task<List<Bin>> GetBinsByActiveGroupIdAsync(string binGroupId);
        Task<List<Bin>> GetBinCodesAsync(string binGroupId);
        Task<Bin> GetBinByBinCodeAsync(string binCode, string binGroupId);
        Task AssignPackageToCurrentBinAsync(Package package);
        Task AssignCreatePackageBinAsync(Package package, bool isInitialCreate, int daysPlus = 0);
        Task<bool> VerifyCreatedPackageBinOnScan(Package package);
        Task AssignBinsForListOfPackagesAsync(List<Package> packages, string binActiveGroupId, string binMapActiveGroupId, bool isUpdate = false);
    }
}
