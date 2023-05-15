using MMS.API.Domain.Models.AutoScan;
using MMS.API.Domain.Utilities;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IAutoScanProcessor
    {
        Task<(ParcelDataResponse Response, PackageTimer Timer)> ProcessAutoScanPackageRequest(ParcelDataRequest request);
        Task<(NestParcelResponse Response, PackageTimer Timer, string Message)> ProcessNestPackageRequest(NestParcelRequest request);

        Task<ParcelConfirmDataResponse> ConfirmParcelData(ParcelConfirmDataRequest request);
    }
}