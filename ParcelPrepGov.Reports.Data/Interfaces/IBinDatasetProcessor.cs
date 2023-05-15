using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IBinDatasetProcessor
    {
        Task<ReportResponse> UpdateBinDatasets(Site site);
        void CreateDataset(List<BinDataset> binDatasets, Bin bin, ActiveGroup activeGroup);
    }
}

