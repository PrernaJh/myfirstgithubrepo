using Microsoft.EntityFrameworkCore;
using PackageTracker.EodService;
using PackageTracker.EodService.Repositories;

namespace PackageTracker.EodService.Interfaces
{
	public interface IEodDbContextFactory
	{
		EodDbContext CreateDbContext();
		EodDbContext CreateDbContext(string[] args);
	}
}