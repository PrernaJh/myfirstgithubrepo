using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IContainerRateFileProcessor
    {
        Task<FileImportResponse> ImportContainerRatesFileToDatabase(Stream fileStream, Site site);
    }
}
