using Dapper;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class DatasetRepository 
    {
		private readonly IDbConnection connection;
		private readonly IConfiguration configuration;
		private readonly IPpgReportsDbContextFactory factory;

		private readonly int commandTimeout;

		private int LoadCommandTimeout()
		{
			if (!int.TryParse(configuration.GetSection("SqlCommandTimeout").Value, out var commandTimeout))
				commandTimeout = 300; // seconds

			return commandTimeout;
		}

		public DatasetRepository(IConfiguration configuration, 
			IDbConnection connection,
			IPpgReportsDbContextFactory factory)
        {
			this.configuration = configuration;
			this.connection = connection;
			this.factory = factory;

			commandTimeout = LoadCommandTimeout();
        }

		protected async Task ExecuteQueryAsync(string query)
		{
			try
			{
				await connection.QueryAsync(query, commandTimeout: commandTimeout);
			}
			catch (Exception)
			{
			}
		}

		protected async Task<List<T>> GetResultsAsync<T>(string query)
		{
			IEnumerable<T> response = new List<T>();
			try
			{
				response = await connection.QueryAsync<T>(query, commandTimeout: commandTimeout);
			}
			catch (Exception)
			{
        
			}
			return response.ToList();
		}

		protected async Task EraseOldDataAsync<T>(DateTime cutoff) where T : UspsDataset
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					for (; ; )
					{
						var oldItems = context.Set<T>().Where(x => x.CreateDate < cutoff).Take(100);
						if (!oldItems.Any())
							break;
						context.Set<T>().RemoveRange(oldItems);
						await context.SaveChangesAsync();

					}
					transaction.Commit();
				}
			}
		}

		public async Task ExecuteBulkInsertAsync<T>(List<T> items) where T : class
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					await context.BulkInsertAsync(items);
					transaction.Commit();
				}
			}
		}

		public async Task<T> InsertAsync<T>(T item) where T : class
		{
			using (var context = factory.CreateDbContext())
			{
				var result = await context.AddAsync(item);
				await context.SaveChangesAsync();
				return result.Entity;
			}
		}
	}

}