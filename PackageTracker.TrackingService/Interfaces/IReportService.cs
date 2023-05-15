using System.Threading.Tasks;

namespace PackageTracker.TrackingService.Interfaces
{
	public interface IReportService
	{
		Task UpdateBinDatasets();
		Task UpdateSubClientDatasets();
		Task UpdateJobDatasets();
		Task UpdatePackageDatasets();
		Task UpdateShippingContainerDatasets();
        Task MonitorEodPackages(string message);
    }
}
