using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IJobDatasetRepository
    {
        Task<bool> ExecuteBulkInsertAsync(List<JobDataset> jobDatasets, string siteName);
        Task<string> GetJobBarcodeByCosmosId(string cosmosId);
    }
}
