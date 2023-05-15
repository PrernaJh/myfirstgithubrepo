using MMS.API.Domain.Models.Containers;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IContainerUpdateProcessor
    {
        Task<CloseContainerResponse> CloseContainerAsync(CloseContainerRequest request);
        Task<DeleteContainerResponse> DeleteContainerAsync(DeleteContainerRequest request);
        Task<UpdateContainerResponse> UpdateContainerAsync(UpdateContainerRequest request);
        Task<ReplaceContainerResponse> ReplaceContainerAsync(ReplaceContainerRequest request);
    }
}
