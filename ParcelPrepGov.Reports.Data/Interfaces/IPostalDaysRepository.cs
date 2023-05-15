using Microsoft.AspNetCore.Http;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IPostalDaysRepository : IFileManager
    {
        int CalculateCalendarDays(DateTime stopTheClockEventDate, DateTime manifestDate);
        int CalculatePostalDays(DateTime stopTheClockEventDate, DateTime manifestDate, string shippingMethod);

        Task EraseOldDataAsync(DateTime cutoff);
        Task ExecuteBulkInsertAsync(List<PostalDays> postalDays);
        Task<IList<PostalDays>> GetPostalDaysAsync();
    }
}
