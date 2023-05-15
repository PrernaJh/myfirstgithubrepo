using MMS.API.Domain.Models.CreatePackage;
using MMS.API.Domain.Utilities;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface ICreatePackageProcessor
    {
        Task<(CreatePackageResponse CreatePackageResponse, PackageTimer Timer)> ProcessCreatePackageRequestAsync(CreatePackageRequest request);
        Task<DeletePackageResponse> DeletePackageAsync(DeletePackageRequest request);
        Task ProcessGenerateCreatePackage(Package package, GenerateCreatePackageRequest request);
    }
}
