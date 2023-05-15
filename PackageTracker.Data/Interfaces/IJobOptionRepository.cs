using PackageTracker.Data.Models.JobOptions;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IJobOptionRepository : IRepository<JobOption>
	{
		Task<JobOption> GetJobOptionsBySiteAsync(string siteName);
	}
}
