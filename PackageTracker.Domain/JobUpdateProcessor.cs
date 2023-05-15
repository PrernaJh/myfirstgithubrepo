using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class JobUpdateProcessor : IJobUpdateProcessor
    {
        private readonly IJobRepository jobRepository;

        public JobUpdateProcessor(IJobRepository jobRepository)
        {
            this.jobRepository = jobRepository;
        }

        public async Task<BatchDbResponse<Job>> UpdateSetDatasetProcessed(List<Job> jobs)
        {
            return await jobRepository.UpdateSetDatasetProcessed(jobs);
        }
    }
}
