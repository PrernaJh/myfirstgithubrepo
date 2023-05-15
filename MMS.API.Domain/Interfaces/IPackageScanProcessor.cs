using MMS.API.Domain.Models;
using MMS.API.Domain.Utilities;
using PackageTracker.Domain.Utilities;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IPackageScanProcessor
    {
        Task<(ScanPackageResponse ScanPackageResponse, PackageTimer Timer)> ProcessScanPackageRequest(ScanPackageRequest request, bool isRepeatScan = false);
        Task<ValidatePackageResponse> ProcessValidatePackageRequest(ValidatePackageRequest request);
        Task<ReprintPackageResponse> ReprintPackageAsync(ReprintPackageRequest request);
        Task<GetPackageHistoryResponse> GetPackageEvents(GetPackageHistoryRequest request);
        Task<ForceExceptionResponse> ProcessForceExceptionRequest(ForceExceptionRequest request);
    }
}
