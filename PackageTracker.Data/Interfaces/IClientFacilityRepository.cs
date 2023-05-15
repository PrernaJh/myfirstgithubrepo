using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
    public interface IClientFacilityRepository
    {
        Task<ClientFacility> GetClientFacilityByName(string name);
    }
}
