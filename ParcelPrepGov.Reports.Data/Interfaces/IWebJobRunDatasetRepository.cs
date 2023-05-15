using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IWebJobRunDatasetRepository
    {
        Task<IList<WebJobRunDataset>> GetWebJobRunsByJobTypeAsync(string jobType);
        Task<WebJobRunDataset> AddWebJobRunAsync(WebJobRunDataset webJobRun);
    }
}
