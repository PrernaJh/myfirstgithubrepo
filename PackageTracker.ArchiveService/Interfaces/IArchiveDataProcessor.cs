using PackageTracker.Data.Models;
using PackageTracker.Data.Models.Archive;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService.Interfaces
{
    public interface IArchiveDataProcessor
    {
        Task ArchivePackagesAsync(SubClient subClient, DateTime manifestDate, IList<PackageForArchive> packages);
    }
}
