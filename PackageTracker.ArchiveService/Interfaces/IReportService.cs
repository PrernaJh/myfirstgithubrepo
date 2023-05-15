using System;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService.Interfaces
{
    public interface IReportService
    {
        Task CreateDailyContainerPackageNestingReport(DateTime targetDate, string userName);
    }
}
