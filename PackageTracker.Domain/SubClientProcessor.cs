using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.CosmosDb;

namespace PackageTracker.Domain
{
	public class SubClientProcessor : ISubClientProcessor
	{
		private readonly ILogger<SubClientProcessor> logger;
		private readonly ISubClientRepository subClientRepository;

		public SubClientProcessor(ILogger<SubClientProcessor> logger, ISubClientRepository subClientRepository)
		{
			this.logger = logger;
			this.subClientRepository = subClientRepository;
		}

		public async Task<SubClient> GetSubClientByNameAsync(string subClientName)
		{
			return await subClientRepository.GetSubClientByNameAsync(subClientName);
		}

		public async Task<IEnumerable<SubClient>> GetSubClientsBySiteNameAsync(string siteName)
		{
			return await subClientRepository.GetSubClientsBySiteNameAsync(siteName);
		}

		public async Task<SubClient> GetSubClientByKeyAsync(string subClientKey)
		{
			return await subClientRepository.GetSubClientByKeyAsync(subClientKey);
		}

		public async Task<List<SubClient>> GetSubClientsAsync()
		{
			var subClients = await subClientRepository.GetSubClientsAsync();
			return subClients.ToList();
		}

        public async Task<BatchDbResponse<SubClient>> UpdateSetDatasetProcessed(List<SubClient> subClients)
        {
			return await subClientRepository.UpdateSetDatasetProcessed(subClients);
		}
	}
}
