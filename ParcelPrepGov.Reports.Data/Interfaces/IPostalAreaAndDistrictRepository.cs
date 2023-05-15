using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IPostalAreaAndDistrictRepository : IFileManager
    {
        Task ExecuteBulkInsertAsync(List<PostalAreaAndDistrict> items);
        Task EraseOldDataAsync(DateTime cutoff);
    }
}
