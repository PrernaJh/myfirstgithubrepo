using PackageTracker.Data.Models;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IShippingContainerDatasetRepository
    {
        Task<IList<ShippingContainerDataset>> GetDatasetsByTrackingNumberAsync(List<TrackPackage> trackPackages);
        Task<IList<ShippingContainerDataset>> GetDatasetsByContainerIdAsync(List<TrackPackage> trackPackages);
        Task<IList<ShippingContainerDataset>> GetDatasetsByTrackingNumberAsync(List<ShippingContainerDataset> shippingContainers);
        Task<bool> ExecuteBulkInsertOrUpdateAsync(List<ShippingContainerDataset> shippingContainerDatasets);
        Task<bool> ExecuteBulkUpdateAsync(List<ShippingContainerDataset> itemsToUpdate);
        Task<IList<ShippingContainerDataset>> GetDatasetsWithNoStopTheClockScans(Site site, int lookbackMin, int lookbackMax);
        Task<ContainerSearchResultViewModel> GetContainerByBarcode(string barcode, string siteName);
        Task<IEnumerable<ContainerSearchPacakgeViewModel>> GetContainerSearchPackages(string containerId, string siteName);
        Task<IEnumerable<ContainerEventsViewModel>> GetContainerEventsByContainerId(string containerId, string siteName);
        Task DeleteOlderContainersAsync(string site, DateTime date);
    }
}
