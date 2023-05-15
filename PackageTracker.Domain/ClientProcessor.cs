using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class ClientProcessor : IClientProcessor
	{
		private readonly IClientRepository clientRepository;

		public ClientProcessor(IClientRepository clientRepository)
		{
			this.clientRepository = clientRepository;
		}

		public async Task<Client> GetClientByNameAsync(string clientName)
		{
			return await clientRepository.GetClientByNameAsync(clientName);
		}

		public async Task<List<Client>> GetClientsAsync()
		{
			var response = await clientRepository.GetClientsAsync();
			return response.ToList();
		}
	}
}
