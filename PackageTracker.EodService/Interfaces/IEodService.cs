using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IEodService
	{
		Task ProcessEndOfDayPackages();
		Task ProcessEndOfDayContainers();
		Task ProcessEndOfDayPackagesBySite(Site site, DateTime processedDate, bool force = false);
		Task ProcessEndOfDayContainersBySite(Site site, DateTime processedDate, bool force = false);
		Task<bool> ShouldRunJobBeforeFileGeneration(Site site, DateTime processedDate, string webJobConstant);
		Task ResetPackageEod(string message);
		Task ResetContainerEod(string message);
        Task MonitorEod(WebJobSettings webJobSettings);
        Task<(bool, IDictionary<string, IList<WebJobRunResponse>>)> CheckEodComplete(Site site, DateTime dateToProcess, string jobType = null);
		Task<bool> IsEodBlocked(Site site, DateTime dateToProcess, string jobType, string userName, bool sendEmails = false);
		Task<bool> StartEod(string message);
    }
}
