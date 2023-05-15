using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IActiveGroupRepository : IRepository<ActiveGroup>
	{
		Task<string> GetCurrentActiveGroupIdAsync(string activeGroupType, string name, DateTime localTime);
		Task<IEnumerable<ActiveGroup>> GetCurrentActiveGroupsWithEndDateAsync(string activeGroupType, string name, DateTime localDateTime);
		Task<IEnumerable<ActiveGroup>> GetActiveGroupsByTypeAsync(string activeGroupType, string name);
		Task<ActiveGroup> GetCurrentActiveGroup(string key, string name);
        Task<bool> HaveActiveGroupsChangedAsync(string activeGroupType, string name, DateTime lastScanDateTime);
    }
}
