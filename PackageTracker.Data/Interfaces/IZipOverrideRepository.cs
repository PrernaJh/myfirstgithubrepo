using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IZipOverrideRepository : IRepository<ZipOverride>
	{
		Task<ZipOverride> GetZipOverrideByZipCodeAsync(string zipCode, string zipOverrideGroupId);
		Task<IEnumerable<ZipOverride>> GetZipOverridesByActiveGroupId(string activeGroupId);
		Task<ZipOverride> GetZipCarrierOverrideAsync(Package package, ServiceRule serviceRule, string activeGroupId);
	}
}
