using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IPackageEventDatasetRepository
    {
        Task<List<PackageSearchEvent>> GetEventDataForPackageDatasetsAsync(IList<PackageDataset> packageDatasets);
        Task<bool> ExecuteBulkInsertOrUpdateAsync(List<PackageEventDataset> packageEventDatasets);
    }
}
