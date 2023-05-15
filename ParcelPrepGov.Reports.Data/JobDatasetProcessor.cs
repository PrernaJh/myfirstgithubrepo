using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Data
{
	public class JobDatasetProcessor : IJobDatasetProcessor
	{
		private readonly ILogger<JobDatasetProcessor> logger;
		private readonly IJobUpdateProcessor jobUpdateProcessor;
		private readonly IJobDatasetRepository jobDatasetRepository;
		private readonly IDatasetProcessor datasetProcessor;

		public JobDatasetProcessor(ILogger<JobDatasetProcessor> logger,
			IJobUpdateProcessor jobUpdateProcessor,
			IJobDatasetRepository jobDatasetRepository,
			IDatasetProcessor datasetProcessor
			)
		{
			this.logger = logger;
			this.jobUpdateProcessor = jobUpdateProcessor;
			this.jobDatasetRepository = jobDatasetRepository;
			this.datasetProcessor = datasetProcessor;
		}

		public async Task<ReportResponse> UpdateJobDatasets(Site site)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var jobs = await datasetProcessor.GetJobsForJobDatasetsAsync(site.SiteName);
				var jobDatasets = new List<JobDataset>();
				if (jobs.Any())
				{
					var jobsToUpdate = new List<Job>();
					foreach (var job in jobs)
					{
						CreateDataset(jobDatasets, job);
						jobsToUpdate.Add(job);
					}
					if (jobDatasets.Any() || jobsToUpdate.Any())
					{
						logger.LogInformation($"Number of job datasets to insert: {jobDatasets.Count()}");
						await BulkInsertAndUpdate(site, response, jobDatasets, jobsToUpdate);
					}
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert job datasets for site: { site.SiteName }. Exception: { ex }");
			}
			return response;
		}

		private async Task BulkInsertAndUpdate(Site site, ReportResponse response,
			List<JobDataset> jobDatasets, List<Job> jobs)
		{
			response.NumberOfDocuments = jobDatasets.Count();
			var bulkInsert = await jobDatasetRepository.ExecuteBulkInsertAsync(jobDatasets, site.SiteName);
			if (bulkInsert)
			{
				jobs.ForEach(x => x.IsDatasetProcessed = true);
				var bulkUpdate = await jobUpdateProcessor.UpdateSetDatasetProcessed(jobs);
				if (! bulkUpdate.IsSuccessful)
				{
					response.NumberOfFailedDocuments = bulkUpdate.FailedCount;
					response.IsSuccessful = false;
					logger.LogError($"Failed to bulk update jobs for site: { site.SiteName }. Failures: {response.NumberOfFailedDocuments} Total Expected: {jobs.Count()}");
				}
			}
			else
			{
				response.NumberOfFailedDocuments = jobDatasets.Count();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert job datasets for site: { site.SiteName }. Total Failures: {response.NumberOfFailedDocuments}");
			}
		}

		private void CreateDataset(List<JobDataset> jobDatasets, Job job)
		{
			DateTime.TryParse(job.ManifestDate, out var manifestDate);
			var dataset = new JobDataset
			{
				CosmosId = job.Id,
				CosmosCreateDate = job.CreateDate,
				JobBarcode = job.JobBarcode,
				SubClientName = job.SubClientName,

				ManifestDate = manifestDate,
				MarkUpType = job.MarkUpType,
				MarkUp = job.MarkUp,
				Product = job.Product,
				PackageType = job.PackageType,
				PackageDescription = job.PackageDescription,
				Length = job.Length,
				Width = job.Width,
				Depth = job.Depth,
				MailTypeCode = job.MailTypeCode,

				Username = job.Username,
				MachineId = job.MachineId
			};
			foreach (var container in job.JobContainers)
            {
				dataset.JobContainers.Add(new JobContainerDataset
				{
					CosmosId = job.Id,
					CosmosCreateDate = job.CreateDate,

					JobBarcode = job.JobBarcode,

					NumberOfContainers = container.NumberOfContainers,
					Weight = container.Weight
				});
            }
			jobDatasets.Add(dataset);
		}
	}
}
