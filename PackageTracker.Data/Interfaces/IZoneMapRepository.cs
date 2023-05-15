using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IZoneMapRepository : IRepository<ZoneMap>
	{
		Task<IEnumerable<ZoneMap>> GetZoneMapsByActiveGroupIdAsync(string activeGroupId);
		Task<ZoneMap> GetZoneMapAsync(string zipFirstThree, string activeGroupId);
	}
}
