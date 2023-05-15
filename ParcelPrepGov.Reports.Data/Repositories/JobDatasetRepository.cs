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
	public class JobDatasetRepository : IJobDatasetRepository
	{
		private readonly IPpgReportsDbContextFactory factory;
		private readonly ILogger<JobDatasetRepository> logger;

		public JobDatasetRepository(IPpgReportsDbContextFactory factory, ILogger<JobDatasetRepository> logger)
		{
			this.factory = factory;
			this.logger = logger;
		}

		public async Task<string> GetJobBarcodeByCosmosId(string cosmosId)
		{
			var jobBarcode = string.Empty;
			using (var context = factory.CreateDbContext())
			{
				jobBarcode = await context.JobDatasets
					.AsNoTracking()
					.Where(x => x.CosmosId == cosmosId)
					.Select(x => x.JobBarcode)
					.FirstOrDefaultAsync();
			}

			return string.IsNullOrEmpty(jobBarcode) ? string.Empty : jobBarcode;
		}

		public async Task<bool> ExecuteBulkInsertAsync(List<JobDataset> jobDatasets, string siteName)
		{
			using (var context = factory.CreateDbContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						var now = DateTime.Now;
						var jobContainers = new List<JobContainerDataset>();
						foreach (var dataset in jobDatasets)
						{
							dataset.SiteName = siteName;
							dataset.DatasetCreateDate = now;
							dataset.DatasetModifiedDate = now;
							foreach (var container in dataset.JobContainers)
							{
								container.DatasetCreateDate = now;
								container.DatasetModifiedDate = now;
								container.SiteName = siteName;
								jobContainers.Add(container);
							}
						}

						await context.BulkInsertAsync(jobDatasets, options =>
						{
							options.SetOutputIdentity = true; // Updates jobDatasets with Id
							//options.IncludeGraph = true; // Not implemented in EFCore.BulkExtensions?
						});

						foreach (var dataset in jobDatasets)
                        {
							foreach (var container in jobContainers.Where(x => x.JobBarcode == dataset.JobBarcode))
							{
								container.JobDatasetId = dataset.Id;
							}
						}

						if (jobContainers.Any())
							await context.BulkInsertAsync(jobContainers);

						transaction.Commit();
						return true;
					}
					catch (System.Exception ex)
					{
						logger.LogError($"Exception on job datasets bulk insert: {ex}");
						return false;
					}
				}
			}
		}
	}
}
