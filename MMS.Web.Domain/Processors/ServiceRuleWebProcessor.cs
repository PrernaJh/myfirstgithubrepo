using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Processors
{
    public class ServiceRuleWebProcessor : IServiceRuleWebProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IServiceRuleRepository serviceRuleRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly ILogger<ServiceRuleWebProcessor> logger;

        public ServiceRuleWebProcessor(
            IActiveGroupProcessor activeGroupProcessor,
            IServiceRuleRepository serviceRuleRepository,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            ILogger<ServiceRuleWebProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.serviceRuleRepository = serviceRuleRepository;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.logger = logger;
        }

        public async Task<List<ServiceRule>> GetServiceRulesByActiveGroupIdAsync(string activeGroupId)
        {
            var response = await serviceRuleRepository.GetServiceRulesByActiveGroupIdAsync(activeGroupId);

            return response.ToList();
        }

        public async Task<FileImportResponse> ImportListOfNewServiceRules(List<ServiceRule> serviceRules, string startDate, string subClientName, string username, string filename = null)
        {
            var response = new FileImportResponse();
            try
            {
                var totalWatch = Stopwatch.StartNew();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

                var activeGroupId = Guid.NewGuid().ToString();
                var serviceRuleActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = subClientName,
                    AddedBy = username,
                    ActiveGroupType = ActiveGroupTypeConstants.ServiceRules,
                    StartDate = DateTime.Parse(startDate),
                    Filename = filename,
                    CreateDate = DateTime.Now,
                    IsEnabled = true
                };

                serviceRuleActiveGroup.Name = subClientName;
                serviceRuleActiveGroup.PartitionKey = subClientName;

                foreach (var serviceRule in serviceRules)
                {
                    serviceRule.ActiveGroupId = activeGroupId;
                }

                var bulkResponse = await serviceRuleRepository.AddItemsAsync(serviceRules, activeGroupId);
                if (!bulkResponse.IsSuccessful)
                    throw new Exception("Bulk upload failed");
                await activeGroupProcessor.AddActiveGroupAsync(serviceRuleActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Service Rules", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing service rules. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<FileImportResponse> ImportListOfNewBusinessRules(List<ServiceRule> serviceRules, string startDate, string subClientName, string username, string filename = null)
        {
            var response = new FileImportResponse();
            try
            {
                var totalWatch = Stopwatch.StartNew();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

                var activeGroupId = Guid.NewGuid().ToString();
                var serviceRuleActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = subClientName,
                    AddedBy = username,
                    ActiveGroupType = ActiveGroupTypeConstants.FortyEightStates,
                    StartDate = DateTime.Parse(startDate),
                    Filename = filename,
                    CreateDate = DateTime.Now,
                    IsEnabled = true
                };

                serviceRuleActiveGroup.Name = subClientName;
                serviceRuleActiveGroup.PartitionKey = subClientName;

                foreach (var serviceRule in serviceRules)
                {
                    serviceRule.ActiveGroupId = activeGroupId;
                }

                var bulkResponse = await serviceRuleRepository.AddItemsAsync(serviceRules, activeGroupId);
                if (bulkResponse.IsSuccessful)
                    await activeGroupProcessor.AddActiveGroupAsync(serviceRuleActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Service Rules", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing service rules. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
