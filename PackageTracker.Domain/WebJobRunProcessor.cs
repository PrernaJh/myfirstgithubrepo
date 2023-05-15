using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class WebJobRunProcessor : IWebJobRunProcessor
    {
        private readonly ILogger<WebJobRunProcessor> logger;
        private readonly IFileConfigurationProcessor fileConfigurationProcessor;
        private readonly IWebJobRunRepository webJobRunRepository;

        public WebJobRunProcessor(ILogger<WebJobRunProcessor> logger,
            IFileConfigurationProcessor fileConfigurationProcessor, IWebJobRunRepository webJobRunRepository)
        {
            this.logger = logger;
            this.fileConfigurationProcessor = fileConfigurationProcessor;
            this.webJobRunRepository = webJobRunRepository;
        }

        public async Task<WebJobRunRequest> StartWebJob(StartWebJobRequest request)
        {
            var siteName = StringHelper.Exists(request.Site?.SiteName) ?
                request.Site.SiteName : SiteConstants.AllSites;
            var localCreateDate = siteName == SiteConstants.AllSites
                ? DateTime.UtcNow
                : TimeZoneUtility.GetLocalTime(request.Site.TimeZone);
            var createDate = DateTime.UtcNow; // Using UtcNow rather than Now, allows local debugging
            var webJobRunId = Guid.NewGuid().ToString();
            var webJobRun = new WebJobRunRequest
            {
                Id = webJobRunId,
                SiteName = siteName,
                SubClientName = request.SubClientName,
                JobType = request.WebJobTypeConstant,
                ProcessedDate = request.ProcessedDate,
                JobName = request.JobName,
                Username = StringHelper.Exists(request.Username) ? request.Username : "System",
                Message = request.Message,
                InProgress = true,
                LocalCreateDate = localCreateDate,
                CreateDate = createDate
            };
            await AddWebJobRunAsync(webJobRun);
            return webJobRun;
        }

        public async Task<WebJobRun> EndWebJob(EndWebJobRequest request)
        {
            var now = DateTime.UtcNow; // Using UtcNow rather than Now, allows local debugging
            if (request.FileDetails.Any())
            {
                request.WebJobRun.FileDetails.AddRange(request.FileDetails);
            }

            request.WebJobRun.InProgress = false;
            request.WebJobRun.Message = request.Message;
            request.WebJobRun.NumberOfRecords = request.NumberOfRecords;
            request.WebJobRun.IsSuccessful = request.IsSuccessful;
            request.WebJobRun.TimeElapsed = (now - request.WebJobRun.CreateDate).TotalSeconds.ToString();

            return await UpdateWebJobRunAsync(request.WebJobRun);
        }

        public async Task<WebJobRun> AddWebJobRunAsync(WebJobRunRequest request)
        {
            var webJobRun = RequestToWebJobRun(request);
            return await webJobRunRepository.AddItemAsync(webJobRun, webJobRun.SiteName);
        }

        public async Task<WebJobRun> UpdateWebJobRunAsync(WebJobRunRequest request)
        {
            var webJobRun = RequestToWebJobRun(request);
            webJobRun.PartitionKey = webJobRunRepository.ResolvePartitionKeyString(webJobRun.SiteName);
            return await webJobRunRepository.UpdateItemAsync(webJobRun);
        }

        public async Task<bool> CheckForRecentAsnFileImportAsync(string siteName, string subClientName, int warningDurationInHours)
        {
            var asnWebJobRun = await webJobRunRepository.GetMostRecentAsnImportWebJobRunAsync(siteName, subClientName);
            if (asnWebJobRun.FileDetails.Any())
            {
                var timeElapsed = DateTime.Now - asnWebJobRun.CreateDate;
                return timeElapsed.TotalHours > warningDurationInHours;
            }

            return false;
        }

        public async Task<IEnumerable<WebJobRunResponse>> GetEndOfDayWebJobRunsAsync(string siteName, int numberOfDaysToLookback)
        {
            try
            {                
                var eodWebJobs = new List<WebJobRun>();
                var endOfDayFileConfigurations = await fileConfigurationProcessor.GetAllEndOfDayFileConfigurationsAsync();
                var webJobTypesToQuery = new List<string>();
                foreach (var fileConfiguration in endOfDayFileConfigurations)
                {
                    webJobTypesToQuery.Add(fileConfiguration.WebJobType);
                }
                if (siteName == SiteConstants.AllSites)
                {
                    var response = await webJobRunRepository.GetWebJobRunsByJobTypeAsync(webJobTypesToQuery, numberOfDaysToLookback);
                    eodWebJobs.AddRange(response);
                }
                else
                {
                    var response = await webJobRunRepository.GetWebJobRunsBySiteAndJobTypeAsync(siteName, webJobTypesToQuery, numberOfDaysToLookback);
                    eodWebJobs.AddRange(response);
                }
                return eodWebJobs.Select(x => WebJobRunToResponse(x));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to get webJobRuns for site: {siteName} Exception: {ex}");
                return new List<WebJobRunResponse>();
            }
        }

        public async Task<List<WebJobRunResponse>> GetAsnImportWebJobRunsBySubClientAsync(string siteName, string subClientName)
        {
            var webJobTypesToQuery = new List<string>
            {
                WebJobConstants.AsnImportJobType
            };
            var stopwatch = Stopwatch.StartNew();
            var eodWebJobs = await webJobRunRepository.GetAsnImportWebJobHistoryAsync(siteName, subClientName);
            stopwatch.Stop();
            logger.LogInformation($"Asn History query elapsed ms: {stopwatch.ElapsedMilliseconds}");
            return eodWebJobs.Select(x => WebJobRunToResponse(x)).ToList();
        }

        public async Task<WebJobRunResponse> GetMostRecentJobRunBySiteAndJobType(string siteName, string jobType, bool filterBySuccess)
        {
            var webJobRun = await webJobRunRepository.GetMostRecentJobRunBySiteAndJobType(siteName, jobType, filterBySuccess);
            return WebJobRunToResponse(webJobRun);

        }

        public async Task<WebJobRunResponse> GetMostRecentJobRunBySubClientAndJobType(string siteName, string subClientName, string jobType, bool filterBySuccess)
        {
            var webJobRun = await webJobRunRepository.GetMostRecentJobRunBySubClientAndJobType(siteName, subClientName, jobType, filterBySuccess);
            return WebJobRunToResponse(webJobRun);
        }

        public async Task<WebJobRunResponse> GetMostRecentJobRunByJobType(string jobType)
        {
            var webJobRun = await webJobRunRepository.GetMostRecentJobRunByJobType(jobType);
            return WebJobRunToResponse(webJobRun);
        }

        public async Task<WebJobRunResponse> GetMostRecentJobRunByProcessedDate(string siteName, string subClientName, DateTime processedDate, string jobType, bool filterBySuccess)
        {
            var webJobRun = await webJobRunRepository.GetMostRecentJobRunByProcessedDate(siteName, subClientName, processedDate, jobType, filterBySuccess);
            return WebJobRunToResponse(webJobRun);
        }

        public async Task<List<WebJobRunResponse>> GetWebJobRunsByJobTypeAsync(string jobType)
        {
            var jobReturn = await webJobRunRepository.GetWebJobRunsByJobTypeAsync(jobType);
            var list = new List<WebJobRunResponse>();
            for (int i = 0; i < jobReturn.Count(); i++)
            {
                list.Add(WebJobRunToResponse(jobReturn.ElementAt(i)));
            }
            return list;
        }

        private static WebJobRun RequestToWebJobRun(WebJobRunRequest request)
        {
            return new WebJobRun
            {
                Id = request.Id,
                SiteName = StringHelper.Exists(request.SiteName) ? request.SiteName : SiteConstants.AllSites,
                ClientName = request.ClientName ?? string.Empty,
                SubClientName = request.SubClientName ?? string.Empty,
                JobType = request.JobType,
                JobName = request.JobName ?? string.Empty,
                ProcessedDate = request.ProcessedDate,
                FileDetails = request.FileDetails,
                NumberOfRecords = request.NumberOfRecords,
                Username = request.Username,
                Message = request.Message,
                IsSuccessful = request.IsSuccessful,
                InProgress = request.InProgress,
                CreateDate = request.CreateDate.Year == 1 ? DateTime.Now : request.CreateDate,
                LocalCreateDate = request.LocalCreateDate,
                TimeElapsed = request.TimeElapsed,
                BulkResponse = request.BulkResponse,
            };
        }

        private static WebJobRunResponse WebJobRunToResponse(WebJobRun run)
        {
            return new WebJobRunResponse
            {
                SiteName = run.SiteName,
                ClientName = run.ClientName,
                SubClientName = run.SubClientName,
                JobType = run.JobType,
                JobName = run.JobName,
                ProcessedDate = run.ProcessedDate,
                FileDetails = run.FileDetails,
                NumberOfRecords = run.NumberOfRecords,
                Username = run.Username,
                Message = run.Message,
                IsSuccessful = run.IsSuccessful,
                InProgress = run.InProgress,
                CreateDate = run.CreateDate,
                LocalCreateDate = run.LocalCreateDate,
                TimeElapsed = run.TimeElapsed
            };
        }
    }
}