using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IBinDatasetRepository
    {
        Task EraseOldDataAsync(string activeGroupId, DateTime cutoff);
        Task<IList<BinDataset>> GetBinDatasetsAsync(string activeGroupId);
        Task<BinDataset> GetBinDatasetsAsync(string activeGroupId, string binCode);
        Task<bool> ExecuteBulkInsertAsync(List<BinDataset> binDatasets, string siteName);
    }
}
