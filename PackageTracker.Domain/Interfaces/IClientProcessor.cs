using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IClientProcessor
	{
		Task<Client> GetClientByNameAsync(string clientName);
		Task<List<Client>> GetClientsAsync();
	}
}