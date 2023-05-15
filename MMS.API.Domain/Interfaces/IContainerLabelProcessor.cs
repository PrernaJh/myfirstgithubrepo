using MMS.API.Domain.Models.Containers;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IContainerLabelProcessor
    {
        Task<ShippingContainer> GetCreateContainerLabelData(ShippingContainer container, Site site, Bin bin, CreateContainerRequest createContainerRequest);
        Task<FedExShippingDataResponse> GetClosedContainerLabelData(ShippingContainer container, Site site, Bin bin);
    }
}
