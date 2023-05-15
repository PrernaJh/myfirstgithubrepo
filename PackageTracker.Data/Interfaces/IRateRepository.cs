using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IRateRepository : IRepository<Rate>
	{
		Task<IEnumerable<Rate>> GetRatesByActiveGroupId(string activeGroupId);
	}
}
