using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface ISubClientProcessor
	{
		Task<SubClient> GetSubClientByNameAsync(string subClientName);
		Task<SubClient> GetSubClientByKeyAsync(string subClientKey);
		Task<IEnumerable<SubClient>> GetSubClientsBySiteNameAsync(string siteName);
		Task<List<SubClient>> GetSubClientsAsync();
		Task<BatchDbResponse<SubClient>> UpdateSetDatasetProcessed(List<SubClient> subClients);
    }
}
