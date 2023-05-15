using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IShippingContainerDatasetProcessor
    {
        Task<ReportResponse> UpdateShippingContainerDatasets(Site site, DateTime lastScanDateTime);
        Task<ReportResponse> UpdateShippingContainerDatasets(List<ShippingContainerDataset> shippingContainerDatasets);
        Task<IList<ShippingContainerDataset>> GetShippingContainersWithNoSTC(Site site, int v1, int v2);
    }
}