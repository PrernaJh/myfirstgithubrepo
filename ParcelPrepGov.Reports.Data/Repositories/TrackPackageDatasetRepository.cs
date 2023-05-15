using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
	public class TrackPackageDatasetRepository : DatasetRepository, ITrackPackageDatasetRepository
	{
		private readonly ILogger<TrackPackageDatasetRepository> logger;

		private readonly IPackageEventDatasetRepository packageEventDatasetRepository;
		private readonly IPpgReportsDbContextFactory factory;

		public TrackPackageDatasetRepository(ILogger<TrackPackageDatasetRepository> logger,
			IConfiguration configuration,
			IDbConnection connection,
			IPackageEventDatasetRepository packageEventDatasetRepository,
			IPpgReportsDbContextFactory factory) : base(configuration, connection, factory)
		{
			this.logger = logger;

			this.packageEventDatasetRepository = packageEventDatasetRepository;
			this.factory = factory;
		}

		public async Task<List<TrackPackageDataset>> GetTrackingDataForPackageDatasetsAsync(IList<PackageDataset> packageDatasets)
		{
			using (var context = factory.CreateDbContext())
			{
				var results = new List<TrackPackageDataset>();
				if (packageDatasets.FirstOrDefault(p => p.PackageStatus == EventConstants.Processed) != null)
				{
					var ids = string.Join(',', packageDatasets.Select(p => p.Id.ToString()));
					var trackPackages = await GetResultsAsync<TrackPackageDataset>($"EXEC getPackageTracking_data '{ids}'");
					trackPackages.RemoveAll(t => t.EventDescription != null && t.EventDescription.Contains("DUPLICATE"));
					// Remove old events if any.  Note: This shouldn't happen anymore, because we always create a new PackageDataset for each CosmosId ...
					trackPackages.RemoveAll(t => t.EventDate < packageDatasets.FirstOrDefault(p => p.Id == t.PackageDatasetId).LocalProcessedDate);
					results.AddRange(trackPackages);
				}

				return results;
			}
		}

		public async Task<bool> ExecuteBulkInsertOrUpdateAsync(List<TrackPackageDataset> items)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var existingItems = items
							.Where(p => p.PackageDatasetId != null)
							.Select(p => new TrackPackageDataset { 
								PackageDatasetId = p.PackageDatasetId, EventCode = p.EventCode, EventDate = p.EventDate, EventDescription = p.EventDescription })
							.ToList();
						await context.BulkReadAsync(existingItems, options =>
							options.UpdateByProperties = new List<string> { "PackageDatasetId", "EventCode", "EventDate", "EventDescription" });
						var existingItems2 = items
							.Where(p => p.ShippingContainerDatasetId != null)
							.Select(p => new TrackPackageDataset { 
								ShippingContainerDatasetId = p.ShippingContainerDatasetId, EventCode = p.EventCode, EventDate = p.EventDate, EventDescription = p.EventDescription })
							.ToList();
						await context.BulkReadAsync(existingItems2, options =>
							options.UpdateByProperties = new List<string> { "ShippingContainerDatasetId", "EventCode", "EventDate", "EventDescription" });
						var now = DateTime.Now;
						items.ForEach(item =>
						{
							var existingItem = item.PackageDatasetId != null ?
								existingItems.FirstOrDefault(p => item.PackageDatasetId == p.PackageDatasetId &&
									item.EventCode == p.EventCode && item.EventDate == p.EventDate && item.EventDescription == p.EventDescription) :
								existingItems2.FirstOrDefault(p => item.ShippingContainerDatasetId == p.ShippingContainerDatasetId &&
									item.EventCode == p.EventCode && item.EventDate == p.EventDate && item.EventDescription == p.EventDescription) ;
							item.Id = (existingItem == null) ? 0 : existingItem.Id;
							item.DatasetCreateDate = (item.Id == 0) ? now : existingItem.DatasetCreateDate;
							item.DatasetModifiedDate = now;
						});

						await context.BulkInsertOrUpdateAsync(items);
						transaction.Commit();
						return true;
					}
					catch (System.Exception ex)
					{
						logger.LogError($"Exception on track package datasets bulk insert: {ex}");
						return false;
					}
				}
			}
		}
	}
}
