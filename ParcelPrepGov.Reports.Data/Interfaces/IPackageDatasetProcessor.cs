using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface IPackageDatasetProcessor
	{
        Task<ReportResponse> UpdatePackageDatasets(List<PackageDataset> packages);
 		Task<ReportResponse> UpdatePackageDatasets(Site site, DateTime lastScanDateTime, DateTime nextScanDateTime);
        Task<ReportResponse> MonitorEodPackages(Site site, string userName, DateTime processedDate);
        Task<IList<PackageDataset>> GetPackagesWithNoSTC(Site site, int lookbackMin, int lookbackMax);
    }
}
