using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IZoneProcessor
	{
		Task AssignZonesToListOfPackages(List<Package> packages, Site site, string zoneMapGroupId);
		Task AssignZone(Package package);
	}
}
