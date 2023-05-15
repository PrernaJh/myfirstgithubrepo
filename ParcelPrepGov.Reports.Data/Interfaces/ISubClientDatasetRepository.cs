using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface ISubClientDatasetRepository
    {
        Task<IList<SubClientDataset>> GetSubClientDatasetsAsync();
        Task<bool> ExecuteBulkInsertOrUpdateAsync(List<SubClientDataset> subClientDatasets);
    }
}
