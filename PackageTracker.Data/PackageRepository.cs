using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class PackageRepository : CosmosDbRepository<Package>, IPackageRepository
	{
		public PackageRepository(ILogger<PackageRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Packages;

		public override string ResolvePartitionKeyString(string packageId) => PartitionKeyUtility.GeneratePackagePartitionKeyString(packageId);

		public async Task<Package> GetPackageByPackageId(string packageId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var blocked = EventConstants.Blocked;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId                                                             
															AND p.siteName = @siteName
															AND p.packageStatus != @blocked
                                                            AND p.createDate > @lookback
                                                            ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@blocked", blocked)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<Package> GetProcessedPackageByPackageId(string packageId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var processed = EventConstants.Processed;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId 
                                                            AND p.siteName = @siteName 
															AND p.packageStatus = @processed
															AND p.processedDate > @lookback
															ORDER BY p.processedDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@processed", processed)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<Package> GetImportedOrProcessedPackage(string packageId, string siteName)
		{
			var imported = EventConstants.Imported;
			var processed = EventConstants.Processed;
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId                                                             
															AND p.siteName = @siteName
															AND p.packageStatus IN (@imported, @processed)
                                                            AND p.createDate > @lookback
                                                            ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@imported", imported)
													.WithParameter("@processed", processed)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<Package> GetProcessedOrReleasedPackage(string packageId, string siteName)
		{
			var released = EventConstants.Released;
			var processed = EventConstants.Processed;
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId                                                             
															AND p.siteName = @siteName
															AND p.packageStatus IN (@released, @processed)
                                                            AND p.createDate > @lookback
                                                            ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@released", released)
													.WithParameter("@processed", processed)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<Package> GetCreatedPackage(string packageId, string siteName)
		{
			var created = EventConstants.Created;
			var lookback = DateTime.Now.AddDays(-60);
            var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId
			                                                         AND p.siteName = @siteName
			                                                         AND p.packageStatus = @created
			                                                         AND p.createDate > @lookback
			                                                         ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@created", created)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<IEnumerable<Package>> GetPackagesByPackageId(string packageId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var blocked = EventConstants.Blocked;
			var replaced = EventConstants.Replaced;
			var query = $@"SELECT * FROM {ContainerName} p 
				WHERE p.packageId = @packageId
					AND p.siteName = @siteName
					AND p.packageStatus != @blocked
					AND p.packageStatus != @replaced
					AND p.createDate > @lookback
				ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@blocked", blocked)
													.WithParameter("@replaced", replaced)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results;
		}

		public async Task<Package> GetPackageForConfirmParcelData(string packageId)
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId
																	ORDER BY p.processedDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<Package> GetReturnPackage(string packageId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var imported = EventConstants.Imported;
			var processed = EventConstants.Processed;
			var released = EventConstants.Released;
			var recalled = EventConstants.Recalled;
			var exception = EventConstants.Exception;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.packageId = @packageId
                                                            AND p.siteName = @siteName
															AND p.packageStatus IN (@imported, @processed, @released, @recalled, @exception)
															AND p.createDate > @lookback
															ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@packageId", packageId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback)
													.WithParameter("@imported", imported)
													.WithParameter("@processed", processed)
													.WithParameter("@released", released)
													.WithParameter("@recalled", recalled)
													.WithParameter("@exception", exception);
			var results = await GetItemsAsync(queryDefinition, packageId);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<IEnumerable<Package>> GetPackagesEodOverview(string siteName, DateTime targetDate)
		{
			var startDate = targetDate.Date;
			var endDate = startDate.AddDays(1);
			var processed = EventConstants.Processed;
			var query = $@"SELECT p.id, p.packageId, p.siteName, p.subClientName, p.barcode, p.createDate FROM {ContainerName} p 
			                        WHERE p.siteName = @siteName
									AND p.packageStatus = @processed
									AND p.localProcessedDate >= @startDate
									AND p.localProcessedDate < @endDate";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate)
													.WithParameter("@processed", processed);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForEndOfDayProcess(string siteName, DateTime lastScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-14).Date;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 500 * FROM {ContainerName} p 
			                        WHERE p.siteName = @siteName
									AND p._ts >= @unixTimeAtLastScan
									AND p.localProcessedDate > @lookback
			                        AND p.eodUpdateCounter > p.eodProcessCounter";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForSqlEndOfDayProcess(string siteName, DateTime targetDate, DateTime lastScanDateTime)
		{
			var startDate = targetDate.Date;
			var endDate = startDate.AddDays(1);
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = startDate;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 500 * FROM {ContainerName} p 
			                        WHERE p.siteName = @siteName
									AND p._ts >= @unixTimeAtLastScan
									AND p.localProcessedDate >= @startDate
									AND p.localProcessedDate < @endDate
			                        AND ((IS_DEFINED(p.sqlEodProcessCounter) = true AND p.eodUpdateCounter > p.sqlEodProcessCounter)
										OR   (IS_DEFINED(p.sqlEodProcessCounter) = false AND p.eodUpdateCounter > 0))";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}
		public async Task<IEnumerable<Package>> GetPackagesForRateAssignment(string subClientName)
		{
			var lookback = DateTime.Now.AddDays(-14);
			var processed = EventConstants.Processed;
			var query = $@"SELECT * FROM {ContainerName} p
							WHERE p.processedDate > @lookback
			                AND p.packageStatus = @processed
						    AND p.subClientName = @subClientName
							AND p.isRateAssigned = false";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@processed", processed)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForConsumerDetailFile(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-1);
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var processed = EventConstants.Processed;
			var query = $@"SELECT * FROM {ContainerName} p 
                           WHERE p.subClientName = @subClientName
								AND p.packageStatus = @processed
  								AND p.processedDate >= @lastScanDateTime
  								AND p.processedDate < @nextScanDateTime";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@lastScanDateTime", lastScanDateTime)
													.WithParameter("@nextScanDateTime", nextScanDateTime)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@processed", processed);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForCreatePackagePostProcessing(string subClientName, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-1);
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var created = EventConstants.Created;
			var query = $@"SELECT * FROM {ContainerName} p 
                           WHERE p.subClientName = @subClientName
								AND p.isCreated = true
 								AND p.packageStatus = @created
 								AND p.createDate >= @lastScanDateTime
  								AND p.createDate < @nextScanDateTime";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@lastScanDateTime", lastScanDateTime)
													.WithParameter("@nextScanDateTime", nextScanDateTime)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@created", created);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}
		public async Task<IEnumerable<Package>> GetPackagesForUspsEvsFile(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate)
		{
			var processed = EventConstants.Processed;
			var carrier = ShippingCarrierConstants.Usps.ToUpper();
			var query = $@"SELECT * FROM {ContainerName} p
                           WHERE p.siteName = @siteName
                           AND p.shippingCarrier = @carrier
                           AND p.localProcessedDate > @lookbackStartDate
						   AND p.localProcessedDate < @lookbackEndDate
                           AND p.isUspsEvsProcessed = false
						   AND p.packageStatus = @processed";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@processed", processed)
													.WithParameter("@lookbackStartDate", lookbackStartDate)
													.WithParameter("@lookbackEndDate", lookbackEndDate)
													.WithParameter("@carrier", carrier);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<int>> GetPackageTimeStampsAsync(string siteName, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			var lookback = DateTime.Now.AddDays(-7);
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			if (lastScanDateTime.Year == 1)
				lastScanDateTime = lookback;
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var unixTimeAtNextScan = ((int)(nextScanDateTime - startOfUnixEpoch).TotalSeconds) + 1;
			var query = $@"SELECT p._ts, p.siteName FROM {ContainerName} p
								WHERE p.siteName = @siteName
									AND p._ts >= @unixTimeAtLastScan
									AND p._ts < @unixTimeAtNextScan
								ORDER BY p._ts";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan)
													.WithParameter("@unixTimeAtNextScan", unixTimeAtNextScan)
													.WithParameter("@lookback", lookback);
			var results = await GetTimestampsAsync(queryDefinition);
			return results;
		}

		public async Task<bool> HavePackagesChangedForSiteAsync(string siteName, DateTime lastScanDateTime)
		{
			if (lastScanDateTime.Year == 1)
				return true;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
			var query = $@"SELECT TOP 1 p._ts, p.siteName FROM {ContainerName} p
									WHERE p.siteName = @siteName
										AND p._ts >= @unixTimeAtLastScan";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan);
			var results = await GetTimestampsAsync(queryDefinition);
			return results.Any();		
		}

		public async Task<IEnumerable<Package>> GetPackagesForPackageDatasetsAsync(string siteName, int firstTimestamp, int lastTimeStamp)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT * FROM {ContainerName} p
								WHERE p.siteName = @siteName
									AND p._ts >= @firstTimestamp
									AND p._ts <= @lastTimeStamp
									AND p.createDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@firstTimestamp", firstTimestamp)
													.WithParameter("@lastTimeStamp", lastTimeStamp)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<Package> GetPackageByTrackingNumberAndSiteName(string barcode, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var processed = EventConstants.Processed;
			var query = $@"SELECT TOP 1 * FROM {ContainerName} p WHERE p.barcode = @barcode 
                                                            AND p.siteName = @siteName 
															AND p.packageStatus = @processed
															AND p.processedDate > @lookback
															ORDER BY p.processedDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@barcode", barcode)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback)
													.WithParameter("@processed", processed);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results.FirstOrDefault() ?? new Package();
		}

		public async Task<IEnumerable<Package>> GetRecalledPackages(string subClientName)
		{
			var recalled = EventConstants.Recalled;
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT * FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
                           AND p.packageStatus = @recalled
						   AND p.createDate > @lookback
						   ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@recalled", recalled)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetReleasedPackages(string subClientName)
		{
			var released = EventConstants.Released;
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT * FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
                           AND p.packageStatus = @released
						   AND p.createDate > @lookback
						   ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@released", released)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesToRecallByPartial(string subClientName, string partialPackageId)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var imported = EventConstants.Imported;
			var query = $@"SELECT TOP 20 p.packageId FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
						   AND p.createDate > @lookback
						   AND STARTSWITH(p.packageId, @partialPackageId)
                           AND p.packageStatus = @imported
						   ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@imported", imported)
													.WithParameter("@partialPackageId", partialPackageId)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForDuplicateAsnChecker(string siteName)
		{
			var lookback = DateTime.Now.AddDays(-14);
			var imported = EventConstants.Imported;
			var query = $@"SELECT * FROM {ContainerName} p 
                           WHERE p.siteName = @siteName
                           AND p.packageStatus = @imported
                           AND p.createDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@imported", imported)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetImportedPackagesBySite(string siteName)
		{
			var imported = EventConstants.Imported;
			var lookback = DateTime.Now.AddDays(-3);
			var query = $@"SELECT * FROM {ContainerName} p
							WHERE p.siteName = @siteName
								AND p.packageStatus = @imported
								AND p.createDate > @lookback
						ORDER BY p.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@imported", imported)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesWithOutdatedBinData(int daysToLookback, int maxCount, 
			string subClientName, string binGroupId, string binMapGroupId)
		{
			var imported = EventConstants.Imported;
			var recalled = EventConstants.Recalled;
			var released = EventConstants.Released;
			var lookback = DateTime.Now.AddDays(-daysToLookback);
			var query = $@"SELECT TOP @maxCount * FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
								AND p.packageStatus IN (@imported, @recalled, @released)
								AND (p.binGroupId != @binGroupId OR p.binMapGroupId != @binMapGroupId)
								AND IS_DEFINED(p.zip)
								AND p.createDate > @lookback
						   ORDER BY p.createDate";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@maxCount", maxCount)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@binGroupId", binGroupId)
													.WithParameter("@binMapGroupId", binMapGroupId)
													.WithParameter("@imported", imported)
													.WithParameter("@recalled", recalled)
													.WithParameter("@released", released)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesForRateUpdate(string subClientName, int daysToLookback)
		{
			var processed = EventConstants.Processed;
			var startDate = DateTime.Now.AddDays(-daysToLookback);
			var endDate = DateTime.Now.AddDays(-(daysToLookback - 1));
			var query = $@"SELECT * FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
                           AND p.packageStatus = @processed
						   AND p.isRateAssigned = true	
                           AND p.processedDate > @startDate
                           AND p.processedDate <= @endDate";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@processed", processed)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetImportedOrReleasedPackagesBySubClient(string subClientName, int daysToLookback)
		{
			var imported = EventConstants.Imported;
			var released = EventConstants.Released;
			var lookback = DateTime.Now.AddDays(-daysToLookback);
			var query = $@"SELECT * FROM {ContainerName} p
                           WHERE p.subClientName = @subClientName
                           AND p.packageStatus IN (@imported, @released)
                           AND p.createDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@imported", imported)
													.WithParameter("@released", released)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesByContainerAsync(string containerId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT * FROM {ContainerName} p
							WHERE p.containerId = @containerId
							AND p.siteName = @siteName 
							AND p.processedDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackageIdsByContainerAsync(string containerId, string siteName)
		{
			var lookback = DateTime.Now.AddDays(-60);
			var query = $@"SELECT p.id FROM {ContainerName} p
							WHERE p.containerId = @containerId
							AND p.siteName = @siteName 
							AND p.processedDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetProcessedPackagesByDate(DateTime targetDate, 
			string clientName, string siteName, string subClientName, string shippingCarrier, int count)
        {
			var processed = EventConstants.Processed;
			var startDate = targetDate.Date;
			var endDate = startDate.AddDays(1);
			var query = count == 0 ? "SELECT" : $"SELECT TOP {count}";
			query += $@" * FROM {ContainerName} p
							WHERE p.packageStatus = @processed
									AND p.localProcessedDate >= @startDate 
									AND p.localProcessedDate < @endDate";
			if (clientName != null)
				query += " AND p.clientName = @clientName";
			if (siteName != null)
				query += " AND p.siteName = @siteName";
			if (subClientName != null)
				query += " AND p.subClientName = @subClientName";
			if (shippingCarrier != null)
				query += " AND p.shippingCarrier = @shippingCarrier";
			query += " ORDER BY p.packageId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@processed", processed)
													.WithParameter("@clientName", clientName)
													.WithParameter("@siteName", siteName)
													.WithParameter("@subClientName", subClientName)
													.WithParameter("@shippingCarrier", shippingCarrier)
													.WithParameter("@startDate", startDate)
													.WithParameter("@endDate", endDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<IEnumerable<Package>> GetPackagesToResetEod(string siteName, DateTime lookbackStartDate, DateTime lookbackEndDate)
		{
			var processed = EventConstants.Processed;
			var query = $@"SELECT p.id, p.partitionKey, p.localProcessedDate, p.eodUpdateCounter, p.eodProcessCounter FROM {ContainerName} p 
			                        WHERE p.siteName = @siteName
									AND p.localProcessedDate > @lookbackStartDate
									AND p.localProcessedDate < @lookbackEndDate
			                        AND p.packageStatus = @processed";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@processed", processed)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookbackStartDate", lookbackStartDate)
													.WithParameter("@lookbackEndDate", lookbackEndDate);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

        public async Task<int> CountPackagesForContainerAsync(string containerId, string siteName)
        {
			var lookback = DateTime.Now.AddDays(-60);
			var processed = EventConstants.Processed;
			var query = $@"SELECT p.id FROM {ContainerName} p
							WHERE p.containerId = @containerId
							AND p.siteName = @siteName 
							AND p.packageStatus = @processed
							AND p.processedDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@containerId", containerId)
													.WithParameter("@processed", processed)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results.Count();
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesEodProcessed(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
                var updateOperations = new List<PatchOperation>
                {
                    PatchOperation.Set("/eodProcessCounter", package.EodProcessCounter),
                    PatchOperation.Set("/isRateAssigned", package.IsRateAssigned),
                    PatchOperation.Set("/cost", package.Cost),
                    PatchOperation.Set("/charge", package.Charge),
                    PatchOperation.Set("/extraCost", package.ExtraCost),
                    PatchOperation.Set("/extraCharge", package.ExtraCharge),
                    PatchOperation.Set("/billingWeight", package.BillingWeight),
                    PatchOperation.Set("/rateId", package.RateId ?? string.Empty),
                    PatchOperation.Set("/rateGroupId", package.RateGroupId ?? string.Empty),
                    PatchOperation.Set("/webJobIds", package.WebJobIds)
                };
                updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesSqlEodProcessed(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
                var updateOperations = new List<PatchOperation>
                {
                    PatchOperation.Set("/sqlEodProcessCounter", package.SqlEodProcessCounter),
                    PatchOperation.Set("/isRateAssigned", package.IsRateAssigned),
                    PatchOperation.Set("/cost", package.Cost),
                    PatchOperation.Set("/charge", package.Charge),
                    PatchOperation.Set("/extraCost", package.ExtraCost),
                    PatchOperation.Set("/extraCharge", package.ExtraCharge),
                    PatchOperation.Set("/billingWeight", package.BillingWeight),
                    PatchOperation.Set("/rateId", package.RateId ?? string.Empty),
                    PatchOperation.Set("/rateGroupId", package.RateGroupId ?? string.Empty),
                    PatchOperation.Set("/webJobIds", package.WebJobIds)
                };
                updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesForRateUpdate(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isRateAssigned", package.IsRateAssigned),
					PatchOperation.Set("/cost", package.Cost),
					PatchOperation.Set("/charge", package.Charge),
					PatchOperation.Set("/extraCost", package.ExtraCost),
					PatchOperation.Set("/extraCharge", package.ExtraCharge),
					PatchOperation.Set("/billingWeight", package.BillingWeight),
					PatchOperation.Set("/rateId", package.RateId ?? string.Empty),
					PatchOperation.Set("/rateGroupId", package.RateGroupId ?? string.Empty),
					PatchOperation.Set("/packageEvents", package.PackageEvents),
					PatchOperation.Set("/historicalRateIds", package.HistoricalRateIds),
					PatchOperation.Set("/historicalRateGroupIds", package.HistoricalRateGroupIds),
					PatchOperation.Set("/webJobIds", package.WebJobIds)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesForCreatePackagePostProcess(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isRural", package.IsRural),
					PatchOperation.Set("/isUpsDas", package.IsUpsDas),
					PatchOperation.Set("/zipOverrides", package.ZipOverrides),
					PatchOperation.Set("/zipOverrideIds", package.ZipOverrideIds),
					PatchOperation.Set("/zipOverrideGroupIds", package.ZipOverrideGroupIds),
					PatchOperation.Set("/webJobIds", package.WebJobIds)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}		
		public async Task<BatchDbResponse<Package>> UpdatePackagesForRecallRelease(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/packageStatus", package.PackageStatus),
					PatchOperation.Set("/recallStatus", package.RecallStatus ?? String.Empty),
					PatchOperation.Set("/recallDate",  package.RecallDate ?? new DateTime()),
					PatchOperation.Set("/releaseDate", package.ReleaseDate ?? new DateTime()),
					PatchOperation.Set("/eodUpdateCounter", package.EodUpdateCounter),
					PatchOperation.Set("/packageEvents", package.PackageEvents)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesSetBinData(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isDduScfBin", package.IsDduScfBin),
					PatchOperation.Set("/isAptbBin", package.IsAptbBin),
					PatchOperation.Set("/isScscBin", package.IsScscBin),
					PatchOperation.Set("/binCode", package.BinCode),
					PatchOperation.Set("/binGroupId", package.BinGroupId),
					PatchOperation.Set("/binMapGroupId", package.BinMapGroupId),
					PatchOperation.Set("/historicalBinCodes", package.HistoricalBinCodes),
					PatchOperation.Set("/historicalBinGroupIds", package.HistoricalBinGroupIds),
					PatchOperation.Set("/historicalBinMapGroupIds", package.HistoricalBinMapGroupIds)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesSetServiceRuleGroupIds(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/serviceRuleGroupId", package.ServiceRuleGroupId),
					PatchOperation.Set("/historicalServiceRuleGroupIds", package.HistoricalServiceRuleGroupIds),
					PatchOperation.Set("/webJobIds", package.WebJobIds)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		public async Task<BatchDbResponse<Package>> UpdatePackagesSetContainer(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/containerId", package.ContainerId),
					PatchOperation.Set("/binCode", package.BinCode),
					PatchOperation.Set("/isSecondaryContainerCarrier", package.IsSecondaryContainerCarrier),
					PatchOperation.Set("/isRateAssigned", package.IsRateAssigned),
					PatchOperation.Set("/eodUpdateCounter", package.EodUpdateCounter),
					PatchOperation.Set("/packageEvents", package.PackageEvents),
					PatchOperation.Set("/historicalBinCodes", package.HistoricalBinCodes),
					PatchOperation.Set("/historicalContainerIds", package.HistoricalContainerIds)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}
		
		public async Task<BatchDbResponse<Package>> UpdatePackagesSetIsSecondaryContainer(IEnumerable<Package> packages)
		{
			var updateItems = new Dictionary<Package, ICollection<PatchOperation>>();
			foreach (var package in packages)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isSecondaryContainerCarrier", package.IsSecondaryContainerCarrier),
					PatchOperation.Set("/isRateAssigned", package.IsRateAssigned),
					PatchOperation.Set("/eodUpdateCounter", package.EodUpdateCounter),
					PatchOperation.Set("/packageEvents", package.PackageEvents)
				};
				updateItems[package] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}
	}
}
