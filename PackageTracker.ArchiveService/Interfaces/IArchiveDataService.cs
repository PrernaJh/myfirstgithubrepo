using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService.Interfaces
{
    public interface IArchiveDataService
    {
        Task ArchivePackagesAsync(WebJobSettings webJobSettings);
    }
}
