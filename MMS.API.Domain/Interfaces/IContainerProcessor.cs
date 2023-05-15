using MMS.API.Domain.Models.Containers;
using PackageTracker.Domain.Models;

using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IContainerProcessor
    {
        Task<CreateContainersResponse> CreateContainersAsync(CreateContainersRequest request);
        Task<GetBinCodesResponse> GetBinCodesAsync(string siteName);
        Task<CreateContainerResponse> CreateContainerAsync(CreateContainerRequest request);
        Task<ReprintClosedContainerResponse> ReprintClosedContainerAsync(ReprintClosedContainerRequest request);
        Task<ReprintActiveContainersResponse> ReprintActiveContainersAsync(ReprintActiveContainersRequest request);
    }
}
