using Microsoft.AspNetCore.Http;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IVisnSiteRepository : IFileManager
    {
        Task EraseOldDataAsync(DateTime cutoff);
        Task ExecuteBulkInsertAsync(List<VisnSite> visnSites);
        Task<VisnSite> GetVisnSiteForPackageDatasetAsync(PackageDataset packageDataset); 
    }
}
