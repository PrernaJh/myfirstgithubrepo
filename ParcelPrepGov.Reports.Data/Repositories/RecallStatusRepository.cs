
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
    public class RecallStatusRepository : IRecallStatusRepository
    {
        private readonly IPpgReportsDbContextFactory factory;
        private readonly ILogger<RecallStatusRepository> logger;

        public RecallStatusRepository(IPpgReportsDbContextFactory factory, ILogger<RecallStatusRepository> logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task<IList<RecallStatus>> GetRecallStatusesAsync()
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.RecallStatuses.AsNoTracking().ToListAsync();
            }
        }
    }
}
