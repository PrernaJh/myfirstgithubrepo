using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IWebJobRunProcessor
	{
		Task<WebJobRunRequest> StartWebJob(StartWebJobRequest request);
		Task<WebJobRun> EndWebJob(EndWebJobRequest request);
		Task<WebJobRun> AddWebJobRunAsync(WebJobRunRequest request);
		Task<WebJobRun> UpdateWebJobRunAsync(WebJobRunRequest request);
		Task<WebJobRunResponse> GetMostRecentJobRunBySiteAndJobType(string siteName, string jobType, bool filterBySuccess = false);
		Task<WebJobRunResponse> GetMostRecentJobRunBySubClientAndJobType(string siteName, string subClientName, string jobType, bool filterBySuccess = false);
		Task<WebJobRunResponse> GetMostRecentJobRunByJobType(string jobType);
		Task<bool> CheckForRecentAsnFileImportAsync(string siteName, string subClientName, int warningDurationInHours);
		Task<IEnumerable<WebJobRunResponse>> GetEndOfDayWebJobRunsAsync(string siteName, int daysToFilter);
		Task<List<WebJobRunResponse>> GetAsnImportWebJobRunsBySubClientAsync(string siteName, string subClientName);
		Task<WebJobRunResponse> GetMostRecentJobRunByProcessedDate(string siteName, string subClientName, DateTime processedDate, string jobType, bool filterBySuccess = false);
		Task<List<WebJobRunResponse>> GetWebJobRunsByJobTypeAsync(string jobType);
	}
}
