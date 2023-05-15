using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IJobUpdateProcessor
    {
        Task<BatchDbResponse<Job>> UpdateSetDatasetProcessed(List<Job> jobs);
    }
}
