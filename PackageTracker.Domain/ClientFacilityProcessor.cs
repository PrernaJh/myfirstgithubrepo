using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class ClientFacilityProcessor : IClientFacilityProcessor
    {
        private readonly IClientFacilityRepository clientFacilityRepository;

        public ClientFacilityProcessor(IClientFacilityRepository clientFacilityRepository)
        {
            this.clientFacilityRepository = clientFacilityRepository;
        }

        public Task<ClientFacility> GetClientFacility(string name)
        {
            return clientFacilityRepository.GetClientFacilityByName(name);
        }
    }
}
