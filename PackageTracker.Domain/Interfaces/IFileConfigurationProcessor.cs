using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IFileConfigurationProcessor
	{
		Task<List<FileConfiguration>> GetAllEndOfDayFileConfigurationsAsync();
	}
}
