using DevExpress.Spreadsheet;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Models.Archive;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Reports.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
	public class PackageDatasetRepository : DatasetRepository, IPackageDatasetRepository
	{
		private readonly ILogger<PackageDatasetRepository> logger;

		private readonly IPpgReportsDbContextFactory factory;
		private readonly IRecallStatusRepository recallStatusRepository;

		public PackageDatasetRepository(ILogger<PackageDatasetRepository> logger,
			IConfiguration configuration,
			IDbConnection connection,
			IRecallStatusRepository recallStatusRepository,
			IPpgReportsDbContextFactory factory) : base(configuration, connection, factory)
		{
			this.logger = logger;

			this.factory = factory;
			this.recallStatusRepository = recallStatusRepository;
		}


		public async Task<PackageDataset> GetPackageDatasetAsync(string packageIdOrBarcode)
		{
			var response = await GetPackageDatasetsForSearchAsync(packageIdOrBarcode);
			return response.FirstOrDefault();
		}

		public async Task<IList<PackageDataset>> GetPackageDatasetsForContainerSearchAsync(string containerIdOrBarcode, string subclientName)
		{
			var items = await GetResultsAsync<PackageDataset>($"EXEC getContainerSearch_detail '{containerIdOrBarcode}' '{subclientName}'");
			var packages = new List<PackageDataset>();
			foreach (var duplicates in items.GroupBy(p => new { p.PackageId, p.SubClientName }))
			{
				var packageForSite = duplicates.FirstOrDefault(); // These were sorted by "ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC"
																  // and linq.GroupBy() preserves this order.
				foreach (var package in duplicates.Where(p => p.PackageStatus != EventConstants.Blocked && p.PackageStatus != EventConstants.Replaced))
				{
					CheckDuplicatePackages(package, duplicates);
					if (package.PackageStatus != EventConstants.Blocked && package.PackageStatus != EventConstants.Replaced)
					{
						packageForSite = package;
						break;
					}
				}
				packages.Add(packageForSite);
			}
			return packages;
		}

		public async Task<IList<PackageDataset>> GetPackageDatasetsForSearchAsync(string packageIdOrBarcodes)
		{
			var items = await GetResultsAsync<PackageDataset>($"EXEC getPackageSearch_master '{packageIdOrBarcodes}'");
			var packages = new List<PackageDataset>();
			foreach (var duplicates in items.GroupBy(p => new { p.PackageId, p.SubClientName }))
			{
				var packageForSite = duplicates.FirstOrDefault(); // These were sorted by "ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC"
																  // and linq.GroupBy() preserves this order.
				foreach (var package in duplicates.Where(p => p.PackageStatus != EventConstants.Blocked && p.PackageStatus != EventConstants.Replaced))
				{
					CheckDuplicatePackages(package, duplicates);
					if (package.PackageStatus != EventConstants.Blocked && package.PackageStatus != EventConstants.Replaced)
					{
						packageForSite = package;
						break;
					}
				}
				packages.Add(packageForSite);
			}
			return packages;
		}
	
		public async Task<IList<PackageSearchExportModel>> GetPackageDataForExportAsync(string packageIdOrBarcodes, string ticketHost)
		{
			var recallStatuses = await recallStatusRepository.GetRecallStatusesAsync();
			var items = await GetResultsAsync<PackageSearchExportModel>($"EXEC getPackageSearch_data '{packageIdOrBarcodes}'");
			var packages = new List<PackageSearchExportModel>();
			foreach (var duplicates in items.GroupBy(p => new { p.PACKAGE_ID, p.CUST_LOCATION }))
			{
				var packageForSite = duplicates.FirstOrDefault(); // These were sorted by "ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC"
																  // and linq.GroupBy() preserves this order.


				foreach (var package in duplicates.Where(p => p.PACKAGE_STATUS != EventConstants.Blocked && p.PACKAGE_STATUS != EventConstants.Replaced))
                {
					CheckDuplicatePackages(package, duplicates);
					if (package.PACKAGE_STATUS != EventConstants.Blocked && package.PACKAGE_STATUS != EventConstants.Replaced)
                    {
						packageForSite = package;
						break;
                    }
				}
				if (packageForSite.PACKAGE_STATUS != EventConstants.Processed)
				{
					packageForSite.CARRIER = string.Empty;
					packageForSite.TRACKING_NUMBER = string.Empty;
					packageForSite.PRODUCT = string.Empty;
					packageForSite.DATE_SHIPPED = null;
				}
				var replacementStatus = recallStatuses.FirstOrDefault(x => x.Status == packageForSite.RECALL_STATUS);
				if (replacementStatus != null)
				{
					packageForSite.RECALL_STATUS = replacementStatus.Description;
                }
				
                packageForSite.INQUIRY_ID_HYPERLINK =
                    HyperLinkFormatter.FormatHelpDeskHyperLink(ticketHost, packageForSite.INQUIRY_ID, packageForSite.PACKAGE_ID, packageForSite.TRACKING_NUMBER,
                    packageForSite.SiteName, packageForSite.ID, packageForSite.SHIPPING_CARRIER, false);

                packages.Add(packageForSite);
			}

			// Re-order packages in the same order as the id search list.
			var orderedPackages = new List<PackageSearchExportModel>();
			foreach (var id in packageIdOrBarcodes.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
				var match = packages.Where(p => p.PACKAGE_ID == id 
					|| (StringHelper.Exists(p.TRACKING_NUMBER) && id.Contains(p.TRACKING_NUMBER))).ToList<PackageSearchExportModel>();

				if (match.Count() > 0)
					orderedPackages.AddRange(match);
				else if (id.Trim() != string.Empty)
					orderedPackages.Add(new PackageSearchExportModel { PACKAGE_ID = id });
            }
			return orderedPackages;
		}

		public async Task<byte[]> ExportPackageDataToSpreadSheet(string host, string ticketHost, string packageIdOrBarcodes)
        {
			PackageSearchExportModel.HOST = host;

			var workbook = Utility.WorkbookExtensions.CreateWorkbook();
			var workSheetIndex = 0;
			var workSheetsProcessed = 0;
			var data = await GetPackageDataForExportAsync(packageIdOrBarcodes, ticketHost);			
			workbook.ImportDataToWorkSheets(ref workSheetIndex, data);

			while (workSheetsProcessed < workbook.Worksheets.Count())
			{ 
				workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<PackageSearchExportModel>(host, null);
			}
			
			workbook.Calculate(); 		

			return await workbook.SaveDocumentAsync(DocumentFormat.Xlsx);
		}

		public async Task<IList<PackageDataset>> GetDatasetsByTrackingNumberAsync(List<TrackPackage> trackPackages)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					var packageDatasets = trackPackages
						.Select(t => new PackageDataset
						{
							ShippingBarcode = t.TrackingNumber
						})
						.ToList();
					// Need to remove duplicates to avoid bulk read exception.
					var existingItems = new List<PackageDataset>();
					foreach (var group in packageDatasets.GroupBy(p => p.ShippingBarcode))
					{
						existingItems.Add(group.FirstOrDefault());
					}
					await context.BulkReadAsync(existingItems, options =>
						options.UpdateByProperties = new List<string> { "ShippingBarcode" }
					);
					return existingItems.Where(p => p.Id != 0)
						.OrderByDescending(p => p.DatasetCreateDate)
						.ToList();
				}
			}
		}

		public async Task<IList<PackageDataset>> GetDatasetsByTrackingNumberAsync(List<PackageDataset> packages)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					var existingItems = packages.Select(p => new PackageDataset { ShippingBarcode = p.ShippingBarcode }).ToList();
					await context.BulkReadAsync(existingItems, options =>
						options.UpdateByProperties = new List<string> { "ShippingBarcode" }
					);
					return existingItems.Where(p => p.Id != 0)
						.OrderByDescending(p => p.DatasetCreateDate)
						.ToList();
				}
			}
		}

		public async Task<IList<PackageDataset>> GetProcessedPackagesBySiteAndPackageIdAsync(string siteName, List<PackageDataset> packages)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					var existingItems = packages
						.Select(p => new PackageDataset { SiteName = siteName, PackageId = p.PackageId, PackageStatus = EventConstants.Processed }).ToList();
					await context.BulkReadAsync(existingItems, options =>
						options.UpdateByProperties = new List<string> { "SiteName", "PackageId", "PackageStatus" }
					);
					return existingItems.Where(p => p.Id != 0)
						.OrderByDescending(p => p.DatasetCreateDate)
						.ToList();
				}
			}
		}

		public async Task<IList<PackageDataset>> GetDatasetsByCosmosIdNumberAsync(List<PackageDataset> packages)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					var existingItems = packages.Select(p => new PackageDataset { CosmosId = p.CosmosId }).ToList();
					await context.BulkReadAsync(existingItems, options =>
						options.UpdateByProperties = new List<string> { "CosmosId" }
					);
					return existingItems.Where(p => p.Id != 0)
						.OrderByDescending(p => p.DatasetCreateDate)
						.ToList();
				}
			}
		}

		public async Task<IList<PackageDataset>> GetDatasetsWithNoStopTheClockScans(Site site, int lookbackMin, int lookbackMax)
		{
			using (var context = factory.CreateDbContext())
			{
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				using (var transaction = context.Database.BeginTransaction())
				{
					var packages = context.PackageDatasets.AsNoTracking()
						.Where(pd => pd.PackageStatus == EventConstants.Processed &&
							pd.SiteName == site.SiteName &&
							pd.ShippingCarrier == ShippingCarrierConstants.Usps &&
							pd.StopTheClockEventDate == null &&
							pd.LocalProcessedDate.Date < siteLocalTime.Date.AddDays(-lookbackMin) &&
							pd.LocalProcessedDate.Date > siteLocalTime.Date.AddDays(-lookbackMax)
						).OrderBy(pd => pd.LocalProcessedDate);
					return await packages.ToListAsync();
				}
			}
		}

		public async Task<bool> ExecuteBulkInsertOrUpdateAsync(List<PackageDataset> items)
		{
			if (items.Count == 0)
				return true;
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var existingItems = items.Select(p => new PackageDataset { CosmosId = p.CosmosId }).ToList();
						await context.BulkReadAsync(existingItems, options =>
							options.UpdateByProperties = new List<string> { "CosmosId" });
						var now = DateTime.Now;
						items.ForEach(item =>
						{
							var existingItem = existingItems.FirstOrDefault(p => item.CosmosId == p.CosmosId);
							item.Id = existingItem.Id;
							// Be careful to not overwrite these fields, because there are not in cosmos!
							if (item.StopTheClockEventDate == null)
								item.StopTheClockEventDate = existingItem.StopTheClockEventDate;
							if (item.IsStopTheClock == null)
								item.IsStopTheClock = existingItem.IsStopTheClock;
							if (item.IsUndeliverable == null)
								item.IsUndeliverable = existingItem.IsUndeliverable;
							if (item.PostalDays == null)
								item.PostalDays = existingItem.PostalDays;
							if (item.CalendarDays == null)
								item.CalendarDays = existingItem.CalendarDays;
							if (item.VisnSiteParent == null)
								item.VisnSiteParent = existingItem.VisnSiteParent;
							if (item.LastKnownEventDate == null)
								item.LastKnownEventDate = existingItem.LastKnownEventDate;
							if (item.LastKnownEventDescription == null)
								item.LastKnownEventDescription = existingItem.LastKnownEventDescription;
							if (item.LastKnownEventLocation == null)
								item.LastKnownEventLocation = existingItem.LastKnownEventLocation;
							if (item.LastKnownEventZip == null)
								item.LastKnownEventZip = existingItem.LastKnownEventZip;
							if (item.ShippedDate == null)
								item.ShippedDate = existingItem.ShippedDate;
				
							item.DatasetCreateDate = (item.Id == 0) ? now : existingItem.DatasetCreateDate;
							item.DatasetModifiedDate = now;
						});

						await context.BulkInsertOrUpdateAsync(items, options =>
						{
							options.SetOutputIdentity = true;   // Updates datasets with Id
							options.PreserveInsertOrder = true; // Doesn't clear PackageEvents or UndeliverableEvents
						});

						// BulkInsertOrUpdateAsync is messing up ids????
						existingItems = items.Select(p => new PackageDataset { CosmosId = p.CosmosId }).ToList();
						await context.BulkReadAsync(existingItems, options =>
							options.UpdateByProperties = new List<string> { "CosmosId" });
						items.ForEach(item =>
						{
							var existingItem = existingItems.FirstOrDefault(p => item.CosmosId == p.CosmosId);
							item.Id = existingItem.Id;
						});

						var events = new List<PackageEventDataset>();
						items.ForEach(item =>
						{
							item.PackageEvents.ForEach(packageEvent =>
							{
								packageEvent.PackageDatasetId = item.Id;
								packageEvent.DatasetModifiedDate = now;
								events.Add(packageEvent);
							});
						});
						if (events.Any())
						{
							var existingEvents = events.Select(e => new PackageEventDataset { CosmosId = e.CosmosId, EventId = e.EventId }).ToList();
							await context.BulkReadAsync(existingEvents, options =>
								options.UpdateByProperties = new List<string> { "CosmosId", "EventId" }
							);
							events.ForEach(packageEvent =>
							{
								var existingEvent = existingEvents.FirstOrDefault(e => e.CosmosId == packageEvent.CosmosId && e.EventId == packageEvent.EventId);
								packageEvent.Id = existingEvent.Id;
								packageEvent.DatasetCreateDate = (existingEvent.Id == 0) ? now : existingEvent.DatasetCreateDate;
							});
							await context.BulkInsertOrUpdateAsync(events);
						}

						var undeliverableEvents = new List<UndeliverableEventDataset>();
						items.ForEach(item =>
						{
							item.UndeliverableEvents.ForEach(undeliverableEvent =>
							{
								undeliverableEvent.PackageDatasetId = item.Id;
								undeliverableEvent.DatasetModifiedDate = now;
								undeliverableEvents.Add(undeliverableEvent);
							});
						});
						if (undeliverableEvents.Any())
						{
							var existingEvents = undeliverableEvents.Select(
								e => new UndeliverableEventDataset { CosmosId = e.CosmosId, EventCode = e.EventCode, EventDate = e.EventDate }).ToList();
							await context.BulkReadAsync(existingEvents, options =>
								options.UpdateByProperties = new List<string> { "CosmosId", "EventCode", "EventDate" }
							);
							undeliverableEvents.ForEach(undeliverableEvent =>
							{
								var existingEvent = existingEvents.FirstOrDefault(e => e.CosmosId == undeliverableEvent.CosmosId && 
									e.EventCode == undeliverableEvent.EventCode && e.EventDate == undeliverableEvent.EventDate);
								undeliverableEvent.Id = existingEvent.Id;
								undeliverableEvent.DatasetCreateDate = (existingEvent.Id == 0) ? now : existingEvent.DatasetCreateDate;
							});
							await context.BulkInsertOrUpdateAsync(undeliverableEvents);
						}

						transaction.Commit();
						return true;
					}
					catch (System.Exception ex)
					{
						logger.LogError($"Exception on package datasets bulk insert, Count: {items.Count}, " +
                            $"First: {items.First().PackageId}/{items.First().ShippingBarcode}, " +
                            $"Last: {items.Last().PackageId}/{items.Last().ShippingBarcode}: {ex}");
						return false;
					}
				}
			}
		}
		
		public async Task<bool> ExecuteBulkUpdateAsync(List<PackageDataset> packages)
		{
			if (packages.Count == 0)
				return true;
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var now = DateTime.Now;
						packages.ForEach(x => x.DatasetModifiedDate = now);
						await context.BulkInsertOrUpdateAsync(packages);
						transaction.Commit();
						return true;
					}
					catch (System.Exception ex)
					{
						logger.LogError($"Exception on package datasets bulk update: {ex}");
						return false;
					}
				}
			}
		}

        public async Task<IEnumerable<PackageFromStatus>> GetPackagesFromStatusAsync(string subClient, string packageStatus, string startDate, string endDate)
        {
			return await GetResultsAsync<PackageFromStatus>($"EXEC getRptRecallReleaseSummary_details '{subClient}', '{packageStatus}', '{startDate}', '{endDate}'");
		}

        public async Task<IEnumerable<PackageFromStatus>> GetRecallReleasePackages(string subClient, string startDate, string endDate)
        {
			return await GetResultsAsync<PackageFromStatus>($"EXEC [getRptRecallReleaseSummary_export] '{subClient}', '{startDate}', '{endDate}'");
		}

        public async Task<PackageDataset> FindOldestPackageForArchiveAsync(string subClient, DateTime startDate)
        {
			var packages = await GetResultsAsync<PackageDataset>($"EXEC [getOldestPackageForArchive] '{subClient}', '{startDate.ToString("yyyy-MM-dd")}'");
			return packages.Any() ? packages[0] : null;
		}

		public async Task<IList<PackageForArchive>> GetPackagesForArchiveAsync(string subClient, DateTime manifestDate)
        {
			return await GetResultsAsync<PackageForArchive>($"EXEC [getPackageDataForArchive] '{subClient}', '{manifestDate.ToString("yyyy-MM-dd")}'");
		}

        public async Task DeleteArchivedPackagesAsync(string subClient, DateTime manifestDate)
        {
			await ExecuteQueryAsync($"EXEC [deleteArchivedPackages] '{subClient}', '{manifestDate.ToString("yyyy-MM-dd")}'");
		}

        public async Task DeleteOlderPackagesAsync(string subClient, bool isPackageProcessed, DateTime createDate)
        {
			if (isPackageProcessed)
				await ExecuteQueryAsync($"EXEC [deleteOlderPackages] '{subClient}', 1, '{createDate.ToString("yyyy-MM-dd")}'");
			else
				await ExecuteQueryAsync($"EXEC [deleteOlderPackages] '{subClient}', 0, '{createDate.ToString("yyyy-MM-dd")}'");
		}

		private void CheckDuplicatePackages(PackageSearchExportModel package, IEnumerable<PackageSearchExportModel> duplicatePackages)
		{
			var recalledPackages = duplicatePackages.Where(x => x.ID != package.ID &&
														x.PACKAGE_STATUS == EventConstants.Recalled
														).ToList();
			var duplicatesWhichBlockImport = duplicatePackages.Where(x => x.ID != package.ID && (
														x.PACKAGE_STATUS == EventConstants.Processed ||
														x.PACKAGE_STATUS == EventConstants.Released
														)).ToList();
			var recalledPackage = recalledPackages.FirstOrDefault();
			if (recalledPackage != null && package.PACKAGE_STATUS == EventConstants.Imported)
			{
				if (recalledPackage.RECALL_STATUS == EventConstants.RecallCreated || recalledPackage.RECALL_STATUS == EventConstants.Imported)
				{
					recalledPackage.PACKAGE_STATUS = EventConstants.Replaced;
					package.PACKAGE_STATUS = EventConstants.Recalled;
					package.DATE_RECALLED = recalledPackage.DATE_RECALLED;
					package.RECALL_STATUS = EventConstants.Imported;
				}
				else
				{
					duplicatesWhichBlockImport.Add(recalledPackage);
				}
			}
			if (duplicatesWhichBlockImport.Any())
			{
				package.PACKAGE_STATUS = EventConstants.Blocked;
			}
		}
		private void CheckDuplicatePackages(PackageDataset package, IEnumerable<PackageDataset> duplicatePackages)
		{
			var recalledPackages = duplicatePackages.Where(x => x.Id != package.Id &&
														x.PackageStatus == EventConstants.Recalled
														).ToList();
			var duplicatesWhichBlockImport = duplicatePackages.Where(x => x.Id != package.Id && (
														x.PackageStatus == EventConstants.Processed ||
														x.PackageStatus == EventConstants.Released
														)).ToList();
			var recalledPackage = recalledPackages.FirstOrDefault();
			if (recalledPackage != null && package.PackageStatus == EventConstants.Imported)
			{
				if (recalledPackage.RecallStatus == EventConstants.RecallCreated || recalledPackage.RecallStatus == EventConstants.Imported)
				{
					recalledPackage.PackageStatus = EventConstants.Replaced;
					package.PackageStatus = EventConstants.Recalled;
					package.RecallDate = recalledPackage.RecallDate;
					package.RecallStatus = EventConstants.Imported;
				}
				else
				{
					duplicatesWhichBlockImport.Add(recalledPackage);
				}
			}
			if (duplicatesWhichBlockImport.Any())
			{
				package.PackageStatus = EventConstants.Blocked;
			}
		}
	}
}

