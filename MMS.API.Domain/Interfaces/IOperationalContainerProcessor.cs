using MMS.API.Domain.Models.OperationalContainers;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IOperationalContainerProcessor
    {
        Task<AddOperationalContainerResponse> AddOperationalContainerAsync(AddOperationalContainerRequest request);
        Task<UpdateOperationalContainerResponse> UpdateOperationalContainerAsync(OperationalContainer operationalContainer);
        Task<UpdateOperationalContainerResponse> UpdateOperationalContainerStatus(UpdateOperationalContainerRequest request);
        Task<OperationalContainer> GetMostRecentOperationalContainerAsync(string siteName, string binCode);
        Task<UpdateOperationalContainerResponse> UpdateIsSecondaryCarrierAsync(UpdateOperationalContainerRequest request);
        Task<GetOperationalContainerResponse> GetOperationalContainerAsync(GetOperationalContainerRequest request);
        Task<OperationalContainer> GetActiveOperationalContainerAsync(string siteName, string binCode);
    }
}
