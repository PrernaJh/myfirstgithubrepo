using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IRecallStatusRepository
    {
        Task<IList<RecallStatus>> GetRecallStatusesAsync();
    }
}
