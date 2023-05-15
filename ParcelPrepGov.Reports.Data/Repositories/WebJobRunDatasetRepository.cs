using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
      public class WebJobRunDatasetRepository : DatasetRepository, IWebJobRunDatasetRepository
      {
        private readonly IPpgReportsDbContextFactory factory;
        private readonly ILogger<WebJobRunDatasetRepository> logger;

        public WebJobRunDatasetRepository(
            IConfiguration config,
            IDbConnection connection,
            IPpgReportsDbContextFactory factory, 
            ILogger<WebJobRunDatasetRepository> logger) :
            base(config, connection, factory)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task<IList<WebJobRunDataset>> GetWebJobRunsByJobTypeAsync(string jobType)
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.WebJobRunDatasets.AsNoTracking()
                    .Where(w => w.JobType == jobType)
                    .OrderByDescending(w => w.DatasetCreateDate)
                    .ToListAsync();
            }
        }

        public async Task<WebJobRunDataset> AddWebJobRunAsync(WebJobRunDataset webJobRun)
        {
            webJobRun.DatasetCreateDate = webJobRun.DatasetModifiedDate = DateTime.UtcNow;
            return await InsertAsync(webJobRun);
        }
   }
}
