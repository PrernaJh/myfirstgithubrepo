using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IServiceRuleRepository : IRepository<ServiceRule>
	{
		Task<ServiceRule> GetServiceRuleAsync(Package package, bool useOverridenMailCode = false);
		Task<IEnumerable<ServiceRule>> GetServiceRulesByActiveGroupIdAsync(string activeGroupId);
	}
}
