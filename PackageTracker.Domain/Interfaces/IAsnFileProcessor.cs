using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IAsnFileProcessor
    {
        Task<AsnFileImportResponse> ImportPackages(List<Package> packages, SubClient subClient, bool isDuplicateBlockEnabled, string webJobId);
        Task<(List<Package> Packages, int NumberOfRecords)> ReadCmopFileStreamAsync(Stream stream);
        Task<(List<Package> Packages, int NumberOfRecords)> ReadDalcFileStreamAsync(Stream stream);
    }
}
