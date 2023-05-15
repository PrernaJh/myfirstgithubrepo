using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface ISubClientRepository : IRepository<SubClient>
	{
		Task<SubClient> GetSubClientByNameAsync(string subClientName);
		Task<SubClient> GetSubClientByKeyAsync(string subClientKey);
		Task<IEnumerable<SubClient>> GetSubClientsBySiteNameAsync(string siteName);
		Task<IEnumerable<SubClient>> GetSubClientsAsync();
        Task<bool> HaveSubClientsChangedAsync(DateTime lastScanDateTime);
    }
}
