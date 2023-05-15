using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.EodService.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces

{
	public interface IEvsFileProcessor
	{
		Task<FileExportResponse> GenerateEvsEodFile(Site site, DateTime manifestDate);
		Task<FileExportResponse> GeneratePmodEvsEodFile(Site site, DateTime manifestDate);
		Task<Dictionary<string, string>> GenerateBinEntryZipMap(Site site, List<Package> packages, bool isSecondaryContainerCarrier);
		Task<Dictionary<string, string>> GenerateBinEntryZipMap(Site site, List<ShippingContainer> containers, bool isSecondaryContainerCarrier);
		public EvsPackage GenerateManifestPackage(Site site, SubClient subClient, Package package,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps);
		public EvsContainer GenerateManifestContainer(Site site, ShippingContainer container,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps);
		public EvsPackage GenerateManifestPackageForContainer(Site site, ShippingContainer container,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps);
	}
}

