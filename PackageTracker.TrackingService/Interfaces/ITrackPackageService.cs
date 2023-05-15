using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.TrackingService.Interfaces
{
    public interface ITrackPackageService
    {
        Task ProcessFedExScanDataFilesAsync(WebJobSettings webJobSettings);
        Task ProcessUpsTrackPackageDataAsync(WebJobSettings webJobSettings);
        Task ProcessUspsScanDataFilesAsync(WebJobSettings webJobSettings);
        Task UpdateMissingPackageUspsScanDataAsync(WebJobSettings webJobSettings);
        Task UpdateMissingContainerUspsScanDataAsync(WebJobSettings webJobSettings);
    }
}
