using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IZipMapRepository : IRepository<ZipMap>
	{
		Task<ZipMap> GetZipMapValueAsync(string activeGroupId, string zipCode);
		Task<IEnumerable<ZipMap>> GetZipMapsByActiveGroupIdAsync(string activeGroupId);

	}
}
