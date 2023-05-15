using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IEvsCodeRepository : IFileManager
    {
        Task<IList<EvsCode>> GetEvsCodesAsync();
        Task EraseOldDataAsync(DateTime cutoff);
        Task ExecuteBulkInsertAsync(List<EvsCode> evsCodes);
     }
}
