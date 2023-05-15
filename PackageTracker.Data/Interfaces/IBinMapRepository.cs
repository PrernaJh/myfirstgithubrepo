using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IBinMapRepository : IRepository<BinMap>
	{
		Task<BinMap> GetBinMapByZip(string zipCode, string activeGroupId);
		Task<IEnumerable<BinMap>> GetBinMapsByActiveGroupIdAsync(string activeGroupId);
	}
}
