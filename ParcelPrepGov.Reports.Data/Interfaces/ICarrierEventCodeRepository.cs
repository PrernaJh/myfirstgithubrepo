using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface ICarrierEventCodeRepository : IFileManager
    {
        Task<IList<CarrierEventCode>> GetCarrierEventCodesAsync();
        Task EraseOldDataAsync(DateTime cutoff);
        Task ExecuteBulkInsertAsync(List<CarrierEventCode> carrierEventCodes);
    }
}