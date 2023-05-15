using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface ISubClientDatasetProcessor
    {
        Task<ReportResponse> UpdateSubClientDatasets();
        void CreateDataset(List<SubClientDataset> binDatasets, SubClient subClient);
    }
}
