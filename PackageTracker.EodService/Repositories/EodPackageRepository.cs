using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
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
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Repositories
{
	public class EodPackageRepository : EodRepository<EodPackage>, IEodPackageRepository
	{
		private readonly ILogger<EodPackageRepository> logger;
		private readonly IEodDbContextFactory factory;

		public EodPackageRepository(ILogger<EodPackageRepository> logger,
			IConfiguration configuration,
			IDbConnection connection,
			IEodDbContextFactory factory) : base(logger, configuration, connection, factory)
		{
			this.logger = logger;
			this.factory = factory;
		}

		public async Task<int> CountOldEodPackagesAsync(string siteName, DateTime cutoff)
		{
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Count(e => e.SiteName == siteName &&
								e.LocalProcessedDate.Date < cutoff.Date);
					return result;
				}
			}
		}
		
		public async Task<BatchDbResponse> DeleteOldEodPackagesAsync(string siteName, DateTime cutoff, int chunkSize)
        {
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse() { IsSuccessful = true };
			try
			{
				await ExecuteQueryAsync($"EXEC [DeleteOldEodPackages] '{siteName}', '{cutoff.ToString("yyyy-MM-dd")}', {chunkSize}");
			}
			catch (System.Exception ex)
			{
				logger.LogError($"Exception on Eod Package bulk delete: {ex}");
				response.IsSuccessful = false;
				response.Message = $"Exception on Eod Package bulk delete {ex.Message}";
			}
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

        public async Task<BatchDbResponse> DeleteEodPackages(string siteName, DateTime localProcessedDate)
        {
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse() { IsSuccessful = true };
			try
			{
				await ExecuteQueryAsync($"EXEC [DeleteEodPackages] '{siteName}', '{localProcessedDate.ToString("yyyy-MM-dd")}'");
			}
			catch (System.Exception ex)
			{
				logger.LogError($"Exception on Eod Package bulk insert: {ex}");
				response.IsSuccessful = false;
				response.Message = $"Exception on Eod Package bulk delete {ex.Message}";
			}
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<BatchDbResponse> UpsertEodPackagesAsync(IEnumerable<EodPackage> packages, int chunkSize = 500)
		{
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse() { IsSuccessful = true, Count = packages.Count() };
			for (int offset = 0; offset < packages.Count(); offset += chunkSize)
			{
				var items = packages.Skip(offset).Take(chunkSize);
				using (var context = factory.CreateDbContext())
				{
					using (var transaction = context.Database.BeginTransaction())
					{
						try
						{
							var existingItems = items.Select(p => new EodPackage { CosmosId = p.CosmosId }).ToList();
							await context.BulkReadAsync(existingItems, options =>
								options.UpdateByProperties = new List<string> { "CosmosId" });
							var now = DateTime.Now;
							var packageDetailRecords = new List<PackageDetailRecord>();
							var returnAsnRecords = new List<ReturnAsnRecord>();
							var evsPackages = new List<EvsPackage>();
							var invoiceRecords = new List<InvoiceRecord>();
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
								item.PackageDetailRecordId = existingItem.PackageDetailRecordId;
								if (item.PackageDetailRecord != null)
								{
									if (item.PackageDetailRecordId.HasValue)
										item.PackageDetailRecord.Id = item.PackageDetailRecordId.Value;
									packageDetailRecords.Add(item.PackageDetailRecord);
								}
								item.ReturnAsnRecordId = existingItem.ReturnAsnRecordId;
								if (item.ReturnAsnRecord != null)
								{
									if (item.ReturnAsnRecordId.HasValue)
										item.ReturnAsnRecord.Id = item.ReturnAsnRecordId.Value;
									returnAsnRecords.Add(item.ReturnAsnRecord);
								}
								item.EvsPackageId = existingItem.EvsPackageId;
								if (item.EvsPackage != null)
								{
									if (item.EvsPackageId.HasValue)
										item.EvsPackage.Id = item.EvsPackageId.Value;
									evsPackages.Add(item.EvsPackage);
								}
								item.InvoiceRecordId = existingItem.InvoiceRecordId;
								if (item.InvoiceRecord != null)
								{
									if (item.InvoiceRecordId.HasValue)
										item.InvoiceRecord.Id = item.InvoiceRecordId.Value;
									invoiceRecords.Add(item.InvoiceRecord);
								}
								item.ExpenseRecordId = existingItem.ExpenseRecordId;
								if (item.ExpenseRecord != null)
								{
									if (item.ExpenseRecordId.HasValue)
										item.ExpenseRecord.Id = item.ExpenseRecordId.Value;
									expenseRecords.Add(item.ExpenseRecord);
								}
							});

							await context.BulkInsertOrUpdateAsync(packageDetailRecords, options => { options.SetOutputIdentity = true; });
							await context.BulkInsertOrUpdateAsync(returnAsnRecords, options => { options.SetOutputIdentity = true; });
							await context.BulkInsertOrUpdateAsync(evsPackages, options => { options.SetOutputIdentity = true; });
							await context.BulkInsertOrUpdateAsync(invoiceRecords, options => { options.SetOutputIdentity = true; });
							await context.BulkInsertOrUpdateAsync(expenseRecords, options => { options.SetOutputIdentity = true; });
							foreach (var item in items)
							{
								var packageDetailRecord = packageDetailRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
								item.PackageDetailRecordId = packageDetailRecord == null || item.PackageDetailRecordId.HasValue ? item.PackageDetailRecordId : packageDetailRecord.Id;
								var returnAsnRecord = returnAsnRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
								item.ReturnAsnRecordId = returnAsnRecord == null || item.ReturnAsnRecordId.HasValue ? item.ReturnAsnRecordId : returnAsnRecord.Id;
								var evsPackage = evsPackages.FirstOrDefault(p => p.CosmosId == item.CosmosId);
								item.EvsPackageId = evsPackage == null || item.EvsPackageId.HasValue ? item.EvsPackageId : evsPackage.Id;
								var invoiceRecord = invoiceRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
								item.InvoiceRecordId = invoiceRecord == null || item.InvoiceRecordId.HasValue ? item.InvoiceRecordId : invoiceRecord.Id;
								var expenseRecord = expenseRecords.FirstOrDefault(p => p.CosmosId == item.CosmosId);
								item.ExpenseRecordId = expenseRecord == null || item.ExpenseRecordId.HasValue ? item.ExpenseRecordId : expenseRecord.Id;
							}

							await context.BulkInsertOrUpdateAsync(items.ToList());
							transaction.Commit();
						}
						catch (System.Exception ex)
						{
							if (chunkSize == 1)
							{
								logger.LogError($"Exception on EodPackage insert/update for package {packages.First().PackageId}");
							}
							else
							{
								logger.LogError($"Exception on EodPackage bulk insert/update: {ex}");
								foreach (var package in packages)
								{
									var singlePackage = new List<EodPackage>();
									singlePackage.Add(package);
									await UpsertEodPackagesAsync(singlePackage, 1);
								}
							}
							response.Message = "Exception on EodPackage bulk insert/update";
							response.IsSuccessful = false;
							break;
						}
					}
				}
			}
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<EodPackage> GetEodPackageByGuidId(string guidPackageId)
        {
			var result = await GetItemByGuidId(guidPackageId);
			return result ?? new EodPackage();
		}

 		public async Task<IEnumerable<EodPackage>> GetEodPackages(string siteName, DateTime localProcessedDate)
		{
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.PackageDetailRecord)
							.Include(e => e.ReturnAsnRecord)
							.Include(e => e.EvsPackage)
							.Include(e => e.InvoiceRecord)
							.Include(e => e.ExpenseRecord)
						.ToList();
					return result;
				}
			}
		}

        public async Task<IEnumerable<EodPackage>> GetEodOverview(string siteName, DateTime localProcessedDate)
        {
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
						.ToList();
					return result;
				}
			}
		}

		public async Task<IEnumerable<EodPackage>> GetEvsEodPackages(string siteName, DateTime localProcessedDate)
		{
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.EvsPackage)
						.ToList();
					return result.Where(e => e.EvsPackage != null);
				}
			}
		}

        public async Task<IEnumerable<EodPackage>> GetExpenseRecords(string siteName, DateTime localProcessedDate, string subClientName)
        {
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.SubClientName == subClientName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.ExpenseRecord)
						.ToList();
					return result.Where(e => e.ExpenseRecord != null);
				}
			}
		}

        public async Task<IEnumerable<EodPackage>> GetInvoiceRecords(string siteName, DateTime localProcessedDate, string subClientName)
        {
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.SubClientName == subClientName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.InvoiceRecord)
						.ToList();
					return result.Where(e => e.InvoiceRecord != null);
				}
			}
		}

        public async Task<IEnumerable<EodPackage>> GetPackageDetails(string siteName, DateTime localProcessedDate)
        {
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.PackageDetailRecord)
						.ToList();
					return result.Where(e => e.PackageDetailRecord != null);
				}
			}
		}

        public async Task<IEnumerable<EodPackage>> GetReturnAsns(string siteName, DateTime localProcessedDate, string subClientName)
        {
			await Task.CompletedTask;
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
			}))
			{
				using (var context = factory.CreateDbContext())
				{
					var result = context.EodPackages.AsNoTracking()
						.Where(e => e.SiteName == siteName && e.SubClientName == subClientName && e.IsPackageProcessed &&
								e.LocalProcessedDate.Date == localProcessedDate.Date)
							.Include(e => e.ReturnAsnRecord)
						.ToList();
					return result.Where(e => e.ReturnAsnRecord != null);
				}
			}
		}
    }
}


