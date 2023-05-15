using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IClientFacilityProcessor
    {
        Task<ClientFacility> GetClientFacility(string name);
    }
}
