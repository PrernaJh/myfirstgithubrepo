using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
    public class ClientFacilityRepository : CosmosDbRepository<ClientFacility>, IClientFacilityRepository
    {
        public ClientFacilityRepository(ILogger<ClientFacilityRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
            base(logger, configuration, factory)
        { }

        public async Task<ClientFacility> GetClientFacilityByName(string name)
        {
            var query = $@"SELECT TOP 1 * FROM {ContainerName} s WHERE s.name = @name";
            var queryDefinition = new QueryDefinition(query)
                                                   .WithParameter("@name", name);
            var results = await GetItemsAsync(queryDefinition, name);
            return results.FirstOrDefault() ?? new ClientFacility();
        }

        public override string ContainerName { get; } = CollectionNameConstants.ClientFacilities;

        public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateDefaultPartitionKeyString();
    }
}
