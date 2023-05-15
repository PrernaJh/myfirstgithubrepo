using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;

namespace PackageTracker.EodService.Repositories
{
    public class EodContainerRepository : EodRepository<EodContainer>, IEodContainerRepository
    {
        private readonly ILogger<EodContainerRepository> logger;
        private readonly IEodDbContextFactory factory;

        public EodContainerRepository(ILogger<EodContainerRepository> logger,
            IConfiguration configuration,
            IDbConnection connection,
            IEodDbContextFactory factory) : base(logger, configuration, connection, factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public async Task<int> CountOldEodContainersAsync(string siteName, DateTime cutoff)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Count(e => e.SiteName == siteName &&
                                e.LocalProcessedDate.Date < cutoff.Date);
                    return result;
                }
            }
        }

        public async Task<BatchDbResponse> DeleteOldEodContainersAsync(string siteName, DateTime cutoff, int chunkSize)
        {
            var stopWatch = Stopwatch.StartNew();
            var response = new BatchDbResponse() { IsSuccessful = true };
            try
            {
                await ExecuteQueryAsync($"EXEC [DeleteOldEodContainers] '{siteName}', '{cutoff.ToString("yyyy-MM-dd")}',  {chunkSize}");
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Exception on Eod Container bulk delete: {ex}");
                response.IsSuccessful = false;
                response.Message = $"Exception on Eod Container bulk delete: {ex.Message}";
            }
            response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
            return response;
        }

        public async Task<BatchDbResponse> DeleteEodContainers(string siteName, DateTime localProcessedDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var response = new BatchDbResponse() { IsSuccessful = true };
            try
            {
                await ExecuteQueryAsync($"EXEC [DeleteEodContainers] '{siteName}', '{localProcessedDate.ToString("yyyy-MM-dd")}'");
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Exception on Eod Container bulk insert: {ex}");
                response.IsSuccessful = false;
                response.Message = $"Exception on Eod Container bulk delete: {ex.Message}";
            }
            response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
            return response;
        }

        public async Task<BatchDbResponse> UpsertEodContainersAsync(IEnumerable<EodContainer> containers, int chunkSize = 500)
        {
            var stopWatch = Stopwatch.StartNew();
            var response = new BatchDbResponse() { IsSuccessful = true, Count = containers.Count() };
            for (int offset = 0; offset < containers.Count(); offset += chunkSize)
            {
                var items = containers.Skip(offset).Take(chunkSize);
                using (var context = factory.CreateDbContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var existingItems = items.Select(p => new EodContainer { CosmosId = p.CosmosId }).ToList();
                            await context.BulkReadAsync(existingItems, options =>
                                options.UpdateByProperties = new List<string> { "CosmosId" });
                            var now = DateTime.Now;
                            var containerDetailRecords = new List<ContainerDetailRecord>();
                            var pmodContainerDetailRecords = new List<PmodContainerDetailRecord>();
                            var evsPackageRecords = new List<EvsPackage>();
                            var evsContainerRecords = new List<EvsContainer>();
                            var expenseRecords = new List<ExpenseRecord>();
                            items.ToList().ForEach(item =>
                            {
                                var existingItem = existingItems.FirstOrDefault(p => item.CosmosId == p.CosmosId);
                                item.Id = existingItem.Id;
                                if (item.Id == 0)
                                    item.CreateDate = now;
                                else
                                    item.CreateDate = existingItem.CreateDate;
                                item.ModifiedDate = now;
                                item.ContainerDetailRecordId = existingItem.ContainerDetailRecordId;
                                if (item.ContainerDetailRecord != null)
                                {
                                    if (item.ContainerDetailRecordId.HasValue)
                                        item.ContainerDetailRecord.Id = item.ContainerDetailRecordId.Value;
                                    containerDetailRecords.Add(item.ContainerDetailRecord);
                                }
                                item.PmodContainerDetailRecordId = existingItem.PmodContainerDetailRecordId;
                                if (item.PmodContainerDetailRecord != null)
                                {
                                    if (item.PmodContainerDetailRecordId.HasValue)
                                        item.PmodContainerDetailRecord.Id = item.PmodContainerDetailRecordId.Value;
                                    pmodContainerDetailRecords.Add(item.PmodContainerDetailRecord);
                                }
                                item.ExpenseRecordId = existingItem.ExpenseRecordId;
                                if (item.ExpenseRecord != null)
                                {
                                    if (item.ExpenseRecordId.HasValue)
                                        item.ExpenseRecord.Id = item.ExpenseRecordId.Value;
                                    expenseRecords.Add(item.ExpenseRecord);
                                }
                                item.EvsContainerRecordId = existingItem.EvsContainerRecordId;
                                if (item.EvsContainerRecord != null)
                                {
                                    if (item.EvsContainerRecordId.HasValue)
                                        item.EvsContainerRecord.Id = item.EvsContainerRecordId.Value;
                                    evsContainerRecords.Add(item.EvsContainerRecord);
                                }
                                item.EvsPackageRecordId = existingItem.EvsPackageRecordId;
                                if (item.EvsPackageRecord != null)
                                {
                                    if (item.EvsPackageRecordId.HasValue)
                                        item.EvsPackageRecord.Id = item.EvsPackageRecordId.Value;
                                    evsPackageRecords.Add(item.EvsPackageRecord);
                                }
                            });

                            await context.BulkInsertOrUpdateAsync(containerDetailRecords, options => { options.SetOutputIdentity = true; });
                            await context.BulkInsertOrUpdateAsync(pmodContainerDetailRecords, options => { options.SetOutputIdentity = true; });
                            await context.BulkInsertOrUpdateAsync(expenseRecords, options => { options.SetOutputIdentity = true; });
                            await context.BulkInsertOrUpdateAsync(evsContainerRecords, options => { options.SetOutputIdentity = true; });
                            await context.BulkInsertOrUpdateAsync(evsPackageRecords, options => { options.SetOutputIdentity = true; });
                            foreach (var item in items)
                            {
                                var containerDetailRecord = containerDetailRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
                                item.ContainerDetailRecordId = containerDetailRecord == null || item.ContainerDetailRecordId.HasValue ? item.ContainerDetailRecordId : containerDetailRecord.Id;
                                var pmodContainerDetailRecord = pmodContainerDetailRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
                                item.PmodContainerDetailRecordId = pmodContainerDetailRecord == null || item.PmodContainerDetailRecordId.HasValue ? item.PmodContainerDetailRecordId : pmodContainerDetailRecord.Id;
                                var ExpenseRecord = expenseRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
                                item.ExpenseRecordId = ExpenseRecord == null || item.ExpenseRecordId.HasValue ? item.ExpenseRecordId : ExpenseRecord.Id;
                                var evsContainerRecord = evsContainerRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
                                item.EvsContainerRecordId = evsContainerRecord == null || item.EvsContainerRecordId.HasValue ? item.EvsContainerRecordId : evsContainerRecord.Id;
                                var evsPackageRecord = evsPackageRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
                                item.EvsPackageRecordId = evsPackageRecord == null || item.EvsPackageRecordId.HasValue ? item.EvsPackageRecordId : evsPackageRecord.Id;
                            }

                            await context.BulkInsertOrUpdateAsync(items.ToList());
                            transaction.Commit();
                        }
                        catch (System.Exception ex)
                        {
                            if (chunkSize == 1)  
                            {
                                logger.LogError($"Exception on EodContainer insert/update for container {containers.First().ContainerId}");
                            }
                            else
                            {
                                logger.LogError($"Exception on EodContainer bulk insert/update: {ex}");
                                foreach (var container in containers)
                                {
                                    var singleContainer = new List<EodContainer>();
                                    singleContainer.Add(container);
                                    await UpsertEodContainersAsync(singleContainer, 1);
                                }
                            }
                            response.Message = "Exception on EodContainer bulk insert/update";
                            response.IsSuccessful = false;
                            break;
                        }
                    }
                }
            }
            response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
            return response;
        }

        public async Task<EodContainer> GetEodContainerByGuidId(string guidContainerId)
        {
            var result = await GetItemByGuidId(guidContainerId);
            return result ?? new EodContainer();
        }

        public async Task<IEnumerable<EodContainer>> GetEodContainers(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                            .Include(e => e.ContainerDetailRecord)
                            .Include(e => e.PmodContainerDetailRecord)
                            .Include(e => e.ExpenseRecord)
                            .Include(e => e.EvsContainerRecord)
                            .Include(e => e.EvsPackageRecord)
                        .ToList();
                    return result;
                }
            }
        }

        public async Task<IEnumerable<EodContainer>> GetEodOverview(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                        .ToList();
                    return result;
                }
            }
        }

        public async Task<IEnumerable<EodContainer>> GetContainerDetails(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                            .Include(e => e.ContainerDetailRecord)
                        .ToList();
                    return result.Where(e => e.ContainerDetailRecord != null);
                }
            }
        }
 
        public async Task<IEnumerable<EodContainer>> GetEvsEodContainers(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                            .Include(e => e.EvsContainerRecord)
                            .Include(e => e.EvsPackageRecord)
                       .ToList();
                    return result.Where(e => e.EvsContainerRecord != null || e.EvsPackageRecord != null);
                }
            }
        }

        public async Task<IEnumerable<EodContainer>> GetExpenseRecords(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                            .Include(e => e.ExpenseRecord)
                        .ToList();
                    return result.Where(e => e.ExpenseRecord != null);
                }
            }
        }

        public async Task<IEnumerable<EodContainer>> GetPmodContainerDetails(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = context.EodContainers.AsNoTracking()
                        .Where(e => e.SiteName == siteName && e.IsContainerClosed &&
                                e.LocalProcessedDate.Date == localProcessedDate.Date)
                            .Include(e => e.PmodContainerDetailRecord)
                        .ToList();
                    return result.Where(e => e.PmodContainerDetailRecord != null);
                }
            }
        }

        public async Task<IEnumerable<EodContainer>> GetReferencedContainers(string siteName, DateTime localProcessedDate)
        {
            await Task.CompletedTask;
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (var context = factory.CreateDbContext())
                {
                    var result = from c in context.EodContainers
                                    join p in context.EodPackages on c.ContainerId equals p.ContainerId
                                    where c.SiteName == siteName && c.IsContainerClosed &&
                                        c.LocalProcessedDate.Date == localProcessedDate.Date
                                        && p.SiteName == siteName && p.IsPackageProcessed &&
                                        p.LocalProcessedDate.Date == localProcessedDate.Date
                                 select c;
                    return result.Distinct().ToList();
                }
            }
        }
    }
}
