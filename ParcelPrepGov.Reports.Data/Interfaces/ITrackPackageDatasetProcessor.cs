using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface ITrackPackageDatasetProcessor
	{
		// Note: This method was only used for Historical data and is therefore it shouldn't be used for anything else.
		Task<ReportResponse> InsertTrackPackageDatasets(List<PackageDataset> packageDatasets, List<TrackPackageDataset> trackPackages);
		Task<ReportResponse> UpdateTrackPackageDatasets(string shippingCarrier, List<TrackPackage> trackPackages);
		int IsStopTheClock(string eventCode, string shippingCarrier);
		int IsUndeliverable(string eventCode, string shippingCarrier);
		void ForceReloadEventCodes();
	}
}
