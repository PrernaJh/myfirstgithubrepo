using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IJobRepository : IRepository<Job>
	{
		Task<Job> GetJobAsync(string siteName, string jobBarcode);
		Task<IEnumerable<Job>> GetJobsForJobDatasetsAsync(string siteName);
        Task<bool> HaveJobsChangedForSiteAsync(string siteName, DateTime lastScanDateTime);
    }
}
