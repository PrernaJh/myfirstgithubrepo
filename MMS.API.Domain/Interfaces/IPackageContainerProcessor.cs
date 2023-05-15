using MMS.API.Domain.Models.Containers;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IPackageContainerProcessor
    {
        Task AssignPackageContainerData(Package package, bool isAutoScan = false);
        Task<AssignContainerResponse> AssignPackageNewContainerAsync(AssignContainerRequest request);
        Task<AssignContainerResponse> AssignPackageActiveContainerAsync(AssignContainerRequest request);
        void UpdatePackageNewContainer(string username, string machineId, Package package, ShippingContainer newContainer, ShippingContainer oldContainer);
    }
}
