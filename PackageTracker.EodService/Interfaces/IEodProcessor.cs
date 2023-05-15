using PackageTracker.Data.Models;
using PackageTracker.EodService.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IEodProcessor
	{
		Task<(bool IsSuccessful, int NumberOfRecords)> ProcessEndOfDayPackagesAsync(Site site, string webJobId, DateTime targetDate, DateTime lastScanDateTime);
		Task<(bool IsSuccessful, int NumberOfRecords)> ProcessEndOfDayContainersAsync(Site site, string webJobId, DateTime targetDate, DateTime lastScanDateTime);
		Task<(bool IsSuccessful, int NumberOfRecords)> CheckForDuplicateEndOfDayPackagesAsync(Site site, DateTime dateToCheck, string webJobId);
		Task<(bool IsSuccessful, int NumberOfRecords)> CheckForDuplicateEndOfDayContainersAsync(Site site, DateTime dateToCheck, string webJobId);
        Task<(bool IsSuccessful, int NumberOfRecords)> DeleteObsoleteContainersAsync(Site site, DateTime dateToProcess, WebJobRun webJob);
		Task<(bool IsSuccessful, int NumberOfRecords)> DeleteEmptyContainersAsync(Site site, DateTime dateToCheck, WebJobRun webJob);
		Task ResetPackageEod(Site site, DateTime targetDate, bool updateDatabase = true, bool cleanupFirst = true);
		Task ResetContainerEod(Site site, DateTime targetDate, bool updateDatabase = true, bool cleanupFirst = true);
		EodPackage GenerateEodPackage(Site site, string webJobId, SubClient subClient, Package packageToImport);
		void GenerateEodPackageFileRecords(Site site, SubClient subClient, EodPackage eodPackage, Package package, 
			Dictionary<string, string> primaryBinEntryMaps, Dictionary<string, string> secondaryBinEntryZipMaps);
		EodContainer GenerateEodContainer(Site site, string webJobId, ShippingContainer container);
		void GenerateEodContainerFileRecords(Site site, List<Bin> activeBins, EodContainer eodContainer, ShippingContainer container,
					Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps);       
		Task<List<Bin>> GetUniqueBinsByActiveGroups(IEnumerable<ShippingContainer> containers);
    }
}
