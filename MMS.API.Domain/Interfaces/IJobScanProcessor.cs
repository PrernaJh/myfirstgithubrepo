using MMS.API.Domain.Models;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IJobScanProcessor
    {
        Task<bool> GetJobDataForPackageScan(Package package);
        Task<bool> GetJobDataForCreatePackageScan(Package package);
        Task<GetJobOptionResponse> GetJobOptionAsync(string siteName);
        Task<AddJobResponse> AddJobAsync(AddJobRequest job);
        Task<StartJobResponse> GetStartJob(StartJobRequest request);
    }
}
