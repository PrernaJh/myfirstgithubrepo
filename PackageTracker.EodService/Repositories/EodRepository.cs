using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Repositories
{
	public class EodRepository<T> where T : EodRecord
	{
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly IDbConnection connection;
		private readonly IEodDbContextFactory factory;

		private readonly int commandTimeout;

		private int LoadCommandTimeout()
		{
			if (!int.TryParse(configuration.GetSection("SqlCommandTimeout").Value, out var commandTimeout))
				commandTimeout = 300; // seconds

			return commandTimeout;
		}

		public EodRepository(ILogger logger,
			IConfiguration configuration,
			IDbConnection connection,
			IEodDbContextFactory factory)
		{
			this.logger = logger;
			this.configuration = configuration;
			this.connection = connection;
			this.factory = factory;

			commandTimeout = LoadCommandTimeout();
		}

		protected async Task ExecuteQueryAsync(string query)
		{
			await connection.QueryAsync(query, commandTimeout: commandTimeout);
		}

		public async Task<T> GetItemByGuidId(string guidId)
		{
			using (var context = factory.CreateDbContext())
			{
				return await context.Set<T>().AsNoTracking()
					.FirstOrDefaultAsync(e => e.CosmosId == guidId);
			}
		}

		public async Task<IEnumerable<T>> GetEodItems(string siteName, DateTime localProcessedDate)
		{
			using (var context = factory.CreateDbContext())
			{
				var result = await context.Set<T>().AsNoTracking()
					.Where(e => e.SiteName == siteName &&
						e.LocalProcessedDate >= localProcessedDate.Date &&
						e.LocalProcessedDate < localProcessedDate.Date.AddDays(1)
						).ToListAsync();
				return result;
			}
		}
	}
}