using Microsoft.WindowsAzure.Storage.File;
using PackageTracker.Data.Models;
using PackageTracker.Data.Models.Archive;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService.Interfaces
{
    public interface IHistoricalDataProcessor
    {
        Task<(FileReadResponse readRespose, List<PackageForArchive> packages, ExcelWorkSheet exceptions)>
            ImportHistoricalDataFileAsync(WebJobSettings webJobSettings, Stream stream, DateTime fileDate);
    }
}
