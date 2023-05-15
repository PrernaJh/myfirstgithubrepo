using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IJobDatasetProcessor
    {
        Task<ReportResponse> UpdateJobDatasets(Site site);
    }
}
