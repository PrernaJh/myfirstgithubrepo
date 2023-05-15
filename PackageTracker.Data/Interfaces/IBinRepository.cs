using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IBinRepository : IRepository<Bin>
	{
		Task<IEnumerable<Bin>> GetBinsByActiveGroupIdAsync(string binGroupId);
		Task<IEnumerable<Bin>> GetBinCodesAsync(string binGroupId);
		Task<Bin> GetBinByBinCodeAsync(string binCode, string binGroupId);
	}
}
