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
    public class WebJobRunRepository : CosmosDbRepository<WebJobRun>, IWebJobRunRepository
    {
        public WebJobRunRepository(ILogger<WebJobRunRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
            base(logger, configuration, factory)
        { }

        public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

        public override string ContainerName { get; } = CollectionNameConstants.WebJobRuns;

        public async Task<WebJobRun> GetMostRecentJobRunBySiteAndJobType(string siteName, string jobType, bool filterBySuccess)
        {
            var query = $@"SELECT * FROM {ContainerName} wjr 
								WHERE  wjr.jobType = @jobType 
								AND wjr.siteName = @siteName 
								ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@jobType", jobType)
                                                    .WithParameter("@siteName", siteName);
            var results = await GetItemsAsync(queryDefinition, siteName);
            if (filterBySuccess)
            {
                results = results.Where(wjr => wjr.IsSuccessful).OrderByDescending(x => x.CreateDate);
            }
            return results.FirstOrDefault() ?? new WebJobRun();
        }

        public async Task<WebJobRun> GetMostRecentJobRunBySubClientAndJobType(string siteName, string subClientName, string jobType, bool filterBySuccess)
        {
            var query = $@"SELECT * FROM {ContainerName} wjr 
								WHERE  wjr.jobType = @jobType 
								AND wjr.subClientName = @subClientName 
								ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                     .WithParameter("@jobType", jobType)
                                                     .WithParameter("@subClientName", subClientName);
            var results = await GetItemsAsync(queryDefinition, siteName);
            if (filterBySuccess)
            {
                results = results.Where(wjr => wjr.IsSuccessful).OrderByDescending(x => x.CreateDate);
            }
            return results.FirstOrDefault() ?? new WebJobRun();
        }

        public async Task<WebJobRun> GetMostRecentJobRunByJobType(string jobType)
        {
            var query = $@"SELECT TOP 1 * FROM {ContainerName} wjr 
								WHERE  wjr.jobType = @jobType 
								ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@jobType", jobType);
            var results = await GetItemsCrossPartitionAsync(queryDefinition);
            return results.FirstOrDefault() ?? new WebJobRun();
        }

        public async Task<IEnumerable<WebJobRun>> GetWebJobRunsBySiteAndJobTypeAsync(string siteName, List<string> jobTypes, int numberOfDaysToLookback)
        {
            var lookback = DateTime.Now.AddDays(-Math.Abs(numberOfDaysToLookback));
            var query = $@"SELECT * FROM {ContainerName} wjr 
                            WHERE wjr.siteName = @siteName
                                AND ARRAY_CONTAINS(@jobTypes, wjr.jobType) 
							    AND wjr.createDate > @lookback
                            ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@siteName", siteName)
                                                    .WithParameter("@jobTypes", jobTypes)
                                                    .WithParameter("@lookback", lookback);
            var results = await GetItemsAsync(queryDefinition, siteName);
            return results;
        }

        public async Task<IEnumerable<WebJobRun>> GetAsnImportWebJobHistoryAsync(string siteName, string subClientName)
        {
            var lookback = DateTime.Now.AddMonths(-3);
            var asnImportJobType = WebJobConstants.AsnImportJobType;
            var query = $@"SELECT TOP 500 w.siteName, w.subClientName, w.jobName, w.fileDetails, w.username, w.errorMessage, w.isSuccessful, w.createDate, w.localCreateDate FROM {ContainerName} w                            
							WHERE w.siteName = @siteName	
							AND w.subClientName = @subClientName
                            AND  w.jobType = @asnImportJobType
							AND w.createDate > @lookback
                            ORDER BY w.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@lookback", lookback)
                                                    .WithParameter("@siteName", siteName)
                                                    .WithParameter("@subClientName", subClientName)
                                                    .WithParameter("@asnImportJobType", asnImportJobType);
            var results = await GetItemsAsync(queryDefinition, siteName);
            return results;
        }

        public async Task<WebJobRun> GetMostRecentAsnImportWebJobRunAsync(string siteName, string subClientName)
        {
            var lookback = DateTime.Now.AddMonths(-3);
            var asnImportJobType = WebJobConstants.AsnImportJobType;
            var query = $@"SELECT TOP 1 w.siteName, w.subClientName, w.jobName, w.fileDetails, w.username, w.errorMessage, w.isSuccessful, w.createDate, w.localCreateDate FROM {ContainerName} w                            
							WHERE w.subClientName = @subClientName
                            AND  w.jobType = @asnImportJobType
							AND w.createDate > @lookback
                            ORDER BY w.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                     .WithParameter("@lookback", lookback)
                                                     .WithParameter("@subClientName", subClientName)
                                                     .WithParameter("@asnImportJobType", asnImportJobType);
            var results = await GetItemsAsync(queryDefinition, siteName);
            return results.FirstOrDefault() ?? new WebJobRun();
        }

        public async Task<IEnumerable<WebJobRun>> GetWebJobRunsByJobTypeAsync(List<string> jobTypes, int numberOfDaysToLookback)
        {
            var lookback = DateTime.Now.AddDays(-Math.Abs(numberOfDaysToLookback));
            var query = $@"SELECT * FROM {ContainerName} wjr 
		                          WHERE wjr.createDate > @lookback
		                          AND ARRAY_CONTAINS(@jobTypes, wjr.jobType)
		                          ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                     .WithParameter("@lookback", lookback)
                                                     .WithParameter("@jobTypes", jobTypes);
            var results = await GetItemsCrossPartitionAsync(queryDefinition);
            return results;
        }

        public async Task<IEnumerable<WebJobRun>> GetWebJobRunsByJobTypeAsync(string jobType)
        {
            var query = $@"SELECT * FROM {ContainerName} wjr 
		                          WHERE wjr.jobType = @jobType
		                          ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                     .WithParameter("@jobType", jobType);
            var results = await GetItemsCrossPartitionAsync(queryDefinition);
            return results;
        }

        public async Task<WebJobRun> GetMostRecentJobRunByProcessedDate(string siteName, string subClientName, DateTime processedDate, string jobType, bool filterBySuccess = false)
        {
            var lookback = DateTime.Now.AddDays(-7);
            var startDate = processedDate.Date;
            var endDate = processedDate.AddDays(1);
            var query = $@"SELECT * FROM {ContainerName} wjr 
									WHERE  wjr.jobType = @jobType 
									AND wjr.siteName = @siteName";
            if (!string.IsNullOrEmpty(subClientName))
                query += $@"		AND wjr.subClientName = @subClientName";
            query += $@"			AND wjr.processedDate >= @startDate AND wjr.processedDate < @endDate
 									AND wjr.createDate > @lookback
									ORDER BY wjr.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                      .WithParameter("@siteName", siteName)
                                                      .WithParameter("@subClientName", subClientName)
                                                      .WithParameter("@startDate", startDate)
                                                      .WithParameter("@endDate", endDate)
                                                      .WithParameter("@jobType", jobType)
                                                      .WithParameter("@lookback", lookback);
            var results = await GetItemsAsync(queryDefinition, siteName);
            if (filterBySuccess)
            {
                results = results.Where(wjr => wjr.IsSuccessful).OrderByDescending(x => x.CreateDate);
            }
            return results.FirstOrDefault() ?? 
                    new WebJobRun { SiteName = siteName, SubClientName = subClientName, JobType = jobType, ProcessedDate = processedDate };
        }
    }
}