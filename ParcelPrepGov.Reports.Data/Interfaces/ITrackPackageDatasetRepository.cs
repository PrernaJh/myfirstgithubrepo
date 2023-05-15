using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface ITrackPackageDatasetRepository
	{
		Task<List<TrackPackageDataset>> GetTrackingDataForPackageDatasetsAsync(IList<PackageDataset> packageDatasets);		
		Task<bool> ExecuteBulkInsertOrUpdateAsync(List<TrackPackageDataset> trackPackageDatasets);
	}
}
