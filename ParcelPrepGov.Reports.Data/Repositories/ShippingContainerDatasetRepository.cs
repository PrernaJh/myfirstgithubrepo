using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
	public class ShippingContainerDatasetRepository : DatasetRepository, IShippingContainerDatasetRepository
    {
		private readonly IPpgReportsDbContextFactory factory;
		private readonly ILogger<ShippingContainerDatasetRepository> logger;

		public ShippingContainerDatasetRepository(ILogger<ShippingContainerDatasetRepository> logger,
            IConfiguration configuration,
            IDbConnection connection,
            IPpgReportsDbContextFactory factory): base(configuration, connection, factory)
        {
			this.factory = factory;
			this.logger = logger;
		}

		public async Task<IList<ShippingContainerDataset>> GetDatasetsByTrackingNumberAsync(List<TrackPackage> trackPackages)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var shippingContainerDatasets = trackPackages
                        .Select(t => new ShippingContainerDataset {
                            UpdatedBarcode = t.TrackingNumber
                        })
                        .ToList();
                    // Need to remove duplicates to avoid bulk read exception.
                    var existingItems = new List<ShippingContainerDataset>();
                    foreach (var group in shippingContainerDatasets.GroupBy(c => c.UpdatedBarcode))
                    {
                        existingItems.Add(group.FirstOrDefault());
                    }
                    await context.BulkReadAsync(existingItems, options =>
                        options.UpdateByProperties = new List<string> { "UpdatedBarcode" }
                    );
                    return existingItems.Where(c => c.Id != 0)
                        .OrderByDescending(c => c.DatasetCreateDate)
                        .ToList();
                }
            }
        }

		public async Task<IList<ShippingContainerDataset>> GetDatasetsByContainerIdAsync(List<TrackPackage> trackPackages)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var shippingContainerDatasets = trackPackages
                        .Select(t => new ShippingContainerDataset {
                            ContainerId = t.TrackingNumber,
                            Status = ContainerEventConstants.Closed
                        })
                        .ToList();
                    // Need to remove duplicates to avoid bulk read exception.
                    var existingItems = new List<ShippingContainerDataset>();
                    foreach (var group in shippingContainerDatasets.GroupBy(c => c.ContainerId))
                    {
                        existingItems.Add(group.FirstOrDefault());
                    }
                    await context.BulkReadAsync(existingItems, options =>
                        options.UpdateByProperties = new List<string> { "ContainerId", "Status" }
                    );
                    return existingItems.Where(c => c.Id != 0)
                        .OrderByDescending(c => c.DatasetCreateDate)
                        .ToList();
                }
            }
        }
        public async Task<IList<ShippingContainerDataset>> GetDatasetsByTrackingNumberAsync(List<ShippingContainerDataset> shippingContainers)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var existingItems = shippingContainers.Select(c => new ShippingContainerDataset { UpdatedBarcode = c.UpdatedBarcode }).ToList();
                    await context.BulkReadAsync(existingItems, options =>
                        options.UpdateByProperties = new List<string> { "UpdatedBarcode" }
                    );
                    return existingItems.Where(p => p.Id != 0)
                        .OrderByDescending(p => p.DatasetCreateDate)
                        .ToList();
                }
            }
        }

        public async Task<bool> ExecuteBulkInsertOrUpdateAsync(List<ShippingContainerDataset> items)
		{
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var existingItems = items.Select(c => new ShippingContainerDataset { CosmosId = c.CosmosId }).ToList();
                        await context.BulkReadAsync(existingItems, options =>
                            options.UpdateByProperties = new List<string> { "CosmosId" }
                        );
                        var now = DateTime.Now;
                        items.ForEach(item =>
                        {
                            var existingItem = existingItems.FirstOrDefault(c => item.CosmosId == c.CosmosId);
                            item.Id = existingItem.Id;
                            // Be careful to not overwrite these fields, because there are not in cosmos!
                            if (item.StopTheClockEventDate == null)
                                item.StopTheClockEventDate = existingItem.StopTheClockEventDate;
                            if (item.LastKnownEventDate == null)
                                item.LastKnownEventDate = existingItem.LastKnownEventDate;
                            if (item.LastKnownEventDescription == null)
                                item.LastKnownEventDescription = existingItem.LastKnownEventDescription;
                            if (item.LastKnownEventLocation == null)
                                item.LastKnownEventLocation = existingItem.LastKnownEventLocation;
                            if (item.LastKnownEventZip == null)
                                item.LastKnownEventZip = existingItem.LastKnownEventZip;
                            item.DatasetCreateDate = (item.Id == 0) ? now : existingItem.DatasetCreateDate;
                            item.DatasetModifiedDate = now;
                        });

                        await context.BulkInsertOrUpdateAsync(items, options =>
                        {
                            options.SetOutputIdentity = true;   // Updates datasets with Id
                            options.PreserveInsertOrder = true; // Doesn't clear Events
                        });

                        // BulkInsertOrUpdateAsync is messing up ids????
                        existingItems = items.Select(c => new ShippingContainerDataset { CosmosId = c.CosmosId }).ToList();
                        await context.BulkReadAsync(existingItems, options =>
                            options.UpdateByProperties = new List<string> { "CosmosId" });
                        items.ForEach(item =>
                        {
                            var existingItem = existingItems.FirstOrDefault(p => item.CosmosId == p.CosmosId);
                            item.Id = existingItem.Id;
                        });

                        var events = new List<ShippingContainerEventDataset>();
                        items.ForEach(item =>
                        {
                            item.Events.ForEach(containerEvent =>
                            {
                                containerEvent.ShippingContainerDatasetId = item.Id;
                                containerEvent.DatasetModifiedDate = now;
                                events.Add(containerEvent);
                            });
                        });
                        if (events.Any())
                        {
                            var existingEvents = events.Select(e => new ShippingContainerEventDataset { CosmosId = e.CosmosId, EventId = e.EventId }).ToList();
                            await context.BulkReadAsync(existingEvents, options =>
                                options.UpdateByProperties = new List<string> { "CosmosId", "EventId" }
                            );
                            events.ForEach(containerEvent =>
                            {
                                var existingEvent = existingEvents.FirstOrDefault(e => e.CosmosId == containerEvent.CosmosId && e.EventId == containerEvent.EventId);
                                containerEvent.Id = existingEvent.Id;
                                containerEvent.DatasetCreateDate = (existingEvent.Id == 0) ? now : existingEvent.DatasetCreateDate;

                            });
                            await context.BulkInsertOrUpdateAsync(events);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (System.Exception ex)
					{
						logger.LogError($"Exception on shipping container datasets bulk insert: {ex}");
						return false;
					}
				}
			}
		}
        public async Task<bool> ExecuteBulkUpdateAsync(List<ShippingContainerDataset> shippingContainers)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        shippingContainers.ForEach(x => x.DatasetModifiedDate = now);
                        await context.BulkInsertOrUpdateAsync(shippingContainers);
                        transaction.Commit();
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError($"Exception on shipping container datasets bulk update: {ex}");
                        return false;
                    }
                }
            }
        }

        public async Task<IList<ShippingContainerDataset>> GetDatasetsWithNoStopTheClockScans(Site site, int lookbackMin, int lookbackMax)
        {
            using (var context = factory.CreateDbContext())
            {
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                using (var transaction = context.Database.BeginTransaction())
                {
                    var shippingContainers = context.ShippingContainerDatasets.AsNoTracking()
                        .Where(sc => sc.Status == ContainerEventConstants.Closed &&
                            sc.SiteName == site.SiteName &&
                            sc.ShippingCarrier == ShippingCarrierConstants.Usps &&
                            sc.StopTheClockEventDate == null &&
                            sc.LocalProcessedDate.Date < siteLocalTime.Date.AddDays(-lookbackMin) &&
                            sc.LocalProcessedDate.Date > siteLocalTime.Date.AddDays(-lookbackMax)
                        ).OrderBy(pd => pd.LocalProcessedDate);
                    return await shippingContainers.ToListAsync();
                }
            }
        }

        public async Task<ContainerSearchResultViewModel> GetContainerByBarcode(string barcode, string siteName)
        {
            var result = new ContainerSearchResultViewModel();
            var query = $"EXEC getContainerSearchByBarcode '{barcode}', '{siteName}'";

            var r = await GetResultsAsync<ContainerSearchResultViewModel>(query);
            if (r.Any())
            {
                result = r.FirstOrDefault();
                if(result.MANIFEST_DATE.GetMonthDateYearOnly() == string.Empty)
                {
                    result.MANIFEST_DATE = null;
                }

                result.EVENTS = await GetContainerEventsByContainerId(result.CONTAINER_ID, result.FSC_SITE);                                
                result.PACKAGES = await GetContainerSearchPackages(result.CONTAINER_ID, result.FSC_SITE);
                result.PIECES_IN_CONTAINER = result.PACKAGES.Count();
            }

            return result;
        }

        public async Task<IEnumerable<ContainerEventsViewModel>> GetContainerEventsByContainerId(string containerId, string siteName)
        {
            var query = $"EXEC getContainerSearchEventsByContainerId '{containerId}', '{siteName}'";

            return await GetResultsAsync<ContainerEventsViewModel>(query);
        }

        public async Task<IEnumerable<ContainerSearchPacakgeViewModel>> GetContainerSearchPackages(string containerId, string siteName)
        {
            var items = await GetResultsAsync<ContainerSearchPacakgeViewModel>($"EXEC getContainerSearchPackages '{containerId}', '{siteName}'");
            List<ContainerSearchPacakgeViewModel> packages = await RemoveDuplicatePackages(items);

            return packages;
        }

        private async Task<List<ContainerSearchPacakgeViewModel>> RemoveDuplicatePackages(List<ContainerSearchPacakgeViewModel> items)
        {                 
            var packages = new List<ContainerSearchPacakgeViewModel>();
            foreach (var duplicates in items.GroupBy(p => new { p.PACKAGE_ID, p.SUBCLIENT_NAME}))
            {
                var packageForSite = duplicates.FirstOrDefault(); // linq.GroupBy() preserves order.
                foreach (var package in duplicates.Where(p => p.PACKAGE_STATUS != EventConstants.Blocked && p.PACKAGE_STATUS != EventConstants.Replaced))
                {
                    CheckDuplicatePackages(package, duplicates);
                    if (package.PACKAGE_STATUS != EventConstants.Blocked && package.PACKAGE_STATUS != EventConstants.Replaced)
                    {
                        packageForSite = package;
                        break;
                    }
                }
                packages.Add(packageForSite);
            }
            return packages;
        }
        private void CheckDuplicatePackages(ContainerSearchPacakgeViewModel package, IEnumerable<ContainerSearchPacakgeViewModel> duplicatePackages)
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
                    package.RECALL_DATE = recalledPackage.RECALL_DATE;
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

        public async Task DeleteOlderContainersAsync(string site, DateTime createDate)
        {
            await ExecuteQueryAsync($"EXEC [deleteOlderContainers] '{site}', '{createDate.ToString("yyyy-MM-dd")}'");
        }
    }
}


