using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class SubClientDatasetRepository : ISubClientDatasetRepository
    {
        private readonly IPpgReportsDbContextFactory factory;
        private readonly ILogger<SubClientDatasetRepository> logger;

        public SubClientDatasetRepository(IPpgReportsDbContextFactory factory, ILogger<SubClientDatasetRepository> logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task<IList<SubClientDataset>> GetSubClientDatasetsAsync()
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.SubClientDatasets.AsNoTracking().ToListAsync();
            }
        }

        public async Task<bool> ExecuteBulkInsertOrUpdateAsync(List<SubClientDataset> items)
        {
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var existingItems = items.Select(s => new SubClientDataset { Name = s.Name }).ToList();
						await context.BulkReadAsync(existingItems, options =>
							options.UpdateByProperties = new List<string> { "Name" });
						var now = DateTime.Now;
						items.ForEach(item =>
						{
							var existingItem = existingItems.FirstOrDefault(s => item.Name == s.Name);
							item.Id = existingItem.Id;
							item.DatasetCreateDate = (item.Id == 0) ? now : existingItem.DatasetCreateDate;
							item.DatasetModifiedDate = now;
						});

						await context.BulkInsertOrUpdateAsync(items);

						transaction.Commit();
						return true;
					}
					catch (System.Exception ex)
					{
						logger.LogError($"Exception on subClient datasets bulk insert: {ex}");
						return false;
					}
				}
			}
		}
	}
}
