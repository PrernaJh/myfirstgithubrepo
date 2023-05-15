using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports
{
	public class PackageSearchProcessor : IPackageSearchProcessor
	{
		private readonly IVisnSiteRepository visnSiteRepository;
		private readonly IPackageDatasetRepository packageDatasetRepository;
		private readonly ITrackPackageDatasetRepository trackPackageDatasetRepository;

		public PackageSearchProcessor(IVisnSiteRepository visnSiteRepository,
			IPackageDatasetRepository packageDatasetRepository, 
			ITrackPackageDatasetRepository trackPackageDatasetRepository)
		{
			this.visnSiteRepository = visnSiteRepository;
			this.packageDatasetRepository = packageDatasetRepository;
			this.trackPackageDatasetRepository = trackPackageDatasetRepository;
		}

		public async Task<List<PackageDataset>> GetPackageDatasetsByIdOrBarcodeAsync(string packageIdOrBarcode)
		{
			var packages = await packageDatasetRepository.GetPackageDatasetsForSearchAsync(packageIdOrBarcode);
			return packages.GroupBy(p => p.SubClientName).Select(g => g.First()).ToList();
		}

        public async Task<List<PackageDataset>> GetPackagesDatasetsByContainerIdOrBarcodeAsync(string containerIdOrBarcode)
        {
			var packages = await packageDatasetRepository.GetPackageDatasetsForSearchAsync(containerIdOrBarcode);
			return packages.GroupBy(p => p.SubClientName).Select(g => g.First()).ToList();
		}

        public async Task<List<TrackPackageDataset>> GetTrackingDataForPackageDatasetsAsync(List<PackageDataset> packageDatasets)
		{
			var response = await trackPackageDatasetRepository.GetTrackingDataForPackageDatasetsAsync(packageDatasets);
			return response;
		}

		public async Task<VisnSite> GetVisnSiteForPackageDatasetAsync(PackageDataset packageDataset)
        {
			return await visnSiteRepository.GetVisnSiteForPackageDatasetAsync(packageDataset);
        }
    }
}
