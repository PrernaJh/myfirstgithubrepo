using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IClientRepository : IRepository<Client>
	{
		Task<Client> GetClientByNameAsync(string clientName);
		Task<IEnumerable<Client>> GetClientsAsync();
	}
}
