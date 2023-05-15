using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService.Interfaces
{
    public interface IHistoricalDataService
    {
        Task FileImportWatcher(WebJobSettings webJobSettings);
    }
}
