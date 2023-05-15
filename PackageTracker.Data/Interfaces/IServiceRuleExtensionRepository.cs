using PackageTracker.Data.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PackageTracker.Data.Interfaces
{
	public interface IServiceRuleExtensionRepository : IRepository<ServiceRuleExtension>
	{
		Task<ServiceRuleExtension> GetFortyEightStatesRuleAsync(Package package);
		Task<ServiceRuleExtension> GetDefaultFortyEightStatesRuleAsync(Package package);
		Task<IEnumerable<ServiceRuleExtension>> GetFortyEightStatesRulesByActiveGroupIdAsync(string activeGroupId);
	}
}
