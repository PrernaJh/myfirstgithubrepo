using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
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
	public class PackageEventDatasetRepository : DatasetRepository, IPackageEventDatasetRepository
	{
		private readonly ILogger<PackageEventDatasetRepository> logger;

		private readonly IPpgReportsDbContextFactory factory;

		public PackageEventDatasetRepository(ILogger<PackageEventDatasetRepository> logger,
			IConfiguration configuration,
			IDbConnection connection,
			IPpgReportsDbContextFactory factory) : base(configuration, connection, factory)
		{
			this.factory = factory;
			this.logger = logger;
		}

        public async Task<List<PackageSearchEvent>> GetEventDataForPackageDatasetsAsync(IList<PackageDataset> packageDatasets)
        {
			using (var context = factory.CreateDbContext())
			{
				var firstPackage = packageDatasets
					.OrderBy(p => p.CosmosCreateDate)
					.FirstOrDefault();
				var result = new List<PackageSearchEvent>();
				if (firstPackage != null)
				{
					var ids = string.Join(',', packageDatasets.Select(p => p.CosmosId));
					result = await GetResultsAsync<PackageSearchEvent>(
						$"EXEC getPackageEvent_data '{ids}', '{firstPackage.CosmosCreateDate.ToString("yyyy-MM-dd")}'");
					// Remove old events if any.  This shouldn't happen anymore, but there may be old data like this.
					result.RemoveAll(e => e.EventDate.Date < packageDatasets.FirstOrDefault(p => p.CosmosId == e.CosmosId).CosmosCreateDate.Date);
				}
				return result.OrderByDescending(t => t.EventDate).ToList();
			}
		}

		public async Task<bool> ExecuteBulkInsertOrUpdateAsync(List<PackageEventDataset> events)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						if (events.Any())
						{
							var now = DateTime.Now;
							var existingEvents = events.Select(e => new PackageEventDataset { CosmosId = e.CosmosId, EventId = e.EventId }).ToList();
							await context.BulkReadAsync(existingEvents, options =>
								options.UpdateByProperties = new List<string> { "CosmosId", "EventId" }
							);
							events.ForEach(packageEvent =>
							{
								var existingEvent = existingEvents.FirstOrDefault(e => e.CosmosId == packageEvent.CosmosId && e.EventId == packageEvent.EventId);
								packageEvent.Id = existingEvent.Id;
								packageEvent.DatasetCreateDate = (existingEvent.Id == 0) ? now : existingEvent.DatasetCreateDate;
								packageEvent.DatasetModifiedDate = now;
							});
							await context.BulkInsertOrUpdateAsync(events);
							transaction.Commit();
						}
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
	}
}
