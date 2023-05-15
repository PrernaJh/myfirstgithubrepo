using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface IPackageSearchProcessor
	{
		/// <summary>
		/// Gets packages by package id or tracking number, handles multple if string contains commas
		/// </summary>
		/// <param name="packageIdOrBarcode">Barcode is also known as tracking number in our domain.</param>		
		Task<List<PackageDataset>> GetPackageDatasetsByIdOrBarcodeAsync(string packageIdOrBarcode);

		/// <summary>
		/// Gets packages by containerId or tracking number, only handles on container unlike our other functions.
		/// </summary>
		/// <param name="containerIdOrBarcode">Barcode is referring to the containers tracking number.</param>		
		Task<List<PackageDataset>> GetPackagesDatasetsByContainerIdOrBarcodeAsync(string containerIdOrBarcode);
		Task<List<TrackPackageDataset>> GetTrackingDataForPackageDatasetsAsync(List<PackageDataset> packageDataset);
		Task<VisnSite> GetVisnSiteForPackageDatasetAsync(PackageDataset packageDataset);
	}
}
