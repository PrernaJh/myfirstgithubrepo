using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IFileConfigurationRepository : IRepository<FileConfiguration>
	{
		Task<IEnumerable<FileConfiguration>> GetAllFileConfigurations(string siteId, string scheduleType);
	}
}
