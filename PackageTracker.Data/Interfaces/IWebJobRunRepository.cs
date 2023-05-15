using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IWebJobRunRepository : IRepository<WebJobRun>
	{
		Task<WebJobRun> GetMostRecentJobRunBySiteAndJobType(string siteName, string jobType, bool filterBySuccess = false);
		Task<WebJobRun> GetMostRecentJobRunBySubClientAndJobType(string siteName, string subClientName, string jobType, bool filterBySuccess = false);
		Task<IEnumerable<WebJobRun>> GetAsnImportWebJobHistoryAsync(string siteName, string subClientName);
		Task<IEnumerable<WebJobRun>> GetWebJobRunsByJobTypeAsync(List<string> jobTypes, int numberOfDaysToLookback);
		Task<IEnumerable<WebJobRun>> GetWebJobRunsByJobTypeAsync(string jobType);
		Task<IEnumerable<WebJobRun>> GetWebJobRunsBySiteAndJobTypeAsync(string siteName, List<string> jobTypes, int numberOfDaysToLookback);
		Task<WebJobRun> GetMostRecentJobRunByJobType(string jobType);
		Task<WebJobRun> GetMostRecentAsnImportWebJobRunAsync(string siteName, string subClientName);
		Task<WebJobRun> GetMostRecentJobRunByProcessedDate(string siteName, string subClientName, DateTime processedDate, string jobType, bool filterBySuccess = false);
	}
}