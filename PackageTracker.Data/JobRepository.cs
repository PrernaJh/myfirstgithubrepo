using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class JobRepository : CosmosDbRepository<Job>, IJobRepository
	{
		public JobRepository(ILogger<JobRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Jobs;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(input);

		public async Task<Job> GetJobAsync(string siteName, string jobBarcode)
		{
			var lookback = DateTime.Now.AddDays(-30);
			var query = $@"SELECT TOP 1 * FROM { ContainerName } j 
							WHERE j.jobBarcode = @jobBarcode 
                            AND j.siteName = @siteName
							AND j.createDate > @lookback
							ORDER BY j.createDate DESC";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@jobBarcode", jobBarcode)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsAsync(queryDefinition, jobBarcode);
			return results.FirstOrDefault() ?? new Job();
		}

		public async Task<IEnumerable<Job>> GetJobsForJobDatasetsAsync(string siteName)
		{
			var lookback = DateTime.Now.AddDays(-30);
			var query = $@"SELECT * FROM {ContainerName} j
                           WHERE j.siteName = @siteName
                           AND j.isDatasetProcessed = false
                           AND j.createDate > @lookback";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@lookback", lookback);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

		public async Task<bool> HaveJobsChangedForSiteAsync(string siteName, DateTime lastScanDateTime)
		{
			if (lastScanDateTime.Year == 1)
				return true;
			var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
			var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;

			var query = $@"SELECT TOP 1 j._ts, j.siteName FROM {ContainerName} j
									WHERE j.siteName = @siteName
										AND j._ts >= @unixTimeAtLastScan";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName)
													.WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan);
			var results = await GetTimestampsAsync(queryDefinition);
			return results.Any();
		}
	}
}