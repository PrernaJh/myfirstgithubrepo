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
    public class BinDatasetRepository : IBinDatasetRepository
    {
        private readonly IPpgReportsDbContextFactory factory;
        private readonly ILogger<BinDatasetRepository> logger;

        public BinDatasetRepository(IPpgReportsDbContextFactory factory, ILogger<BinDatasetRepository> logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task EraseOldDataAsync(string activeGroupId, DateTime cutoff)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    for (; ; )
                    {
                        var oldItems = context.Set<BinDataset>().Where(x => x.ActiveGroupId == activeGroupId &&
                            x.DatasetCreateDate < cutoff).Take(100);
                        if (!oldItems.Any())
                            break;
                        context.Set<BinDataset>().RemoveRange(oldItems);
                        await context.SaveChangesAsync();

                    }
                    transaction.Commit();
                }
            }
        }

        public async Task<IList<BinDataset>> GetBinDatasetsAsync(string activeGroupId)
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.BinDatasets.AsNoTracking().Where(b => b.ActiveGroupId == activeGroupId).ToListAsync();
            }
        }

        /// <summary>
        /// search for binDataset by activeGroupId and binCode in package search 
        /// </summary>
        /// <param name="activeGroupId"></param>
        /// <param name="binCode"></param>
        /// <returns></returns>
        public async Task<BinDataset> GetBinDatasetsAsync(string activeGroupId, string binCode)
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.BinDatasets.AsNoTracking()
                                        .Where(b => b.ActiveGroupId == activeGroupId && b.BinCode == binCode)
                                        .FirstOrDefaultAsync();
            }
        }

        public async Task<bool> ExecuteBulkInsertAsync(List<BinDataset> binDatasets, string siteName)
        {
            using (var context = factory.CreateDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var now = DateTime.Now;
                    try
                    {
                        foreach (var dataset in binDatasets)
                        {
                            dataset.SiteName = siteName;
                            dataset.DatasetCreateDate = now;
                            dataset.DatasetModifiedDate = now;
                        }

                        await context.BulkInsertAsync(binDatasets);
                        transaction.Commit();
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError($"Exception on bin datasets bulk insert: {ex}");
                        return false;
                    }
                }
            }
        }
    }
}
