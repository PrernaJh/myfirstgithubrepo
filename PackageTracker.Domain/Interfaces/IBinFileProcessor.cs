using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IBinFileProcessor
    {
        Task<FileImportResponse> ProcessBinFileStream(Stream stream, string siteName, DateTime startDate);
        Task<FileImportResponse> ProcessBinMapFileStream(Stream stream, string siteName);
    }
}
