using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class ServiceRuleProcessor : IServiceRuleProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IServiceRuleRepository serviceRuleRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly ILogger<ServiceRuleProcessor> logger;

        public ServiceRuleProcessor(
            IActiveGroupProcessor activeGroupProcessor,
            IServiceRuleRepository serviceRuleRepository,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            ILogger<ServiceRuleProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.serviceRuleRepository = serviceRuleRepository;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.logger = logger;
        }

        public async Task<ServiceRule> GetServiceRuleByOverrideMailCode(Package package)
        {
            var useOverridenMailCode = true;
            return await serviceRuleRepository.GetServiceRuleAsync(package, useOverridenMailCode);
        }

        public async Task<FileImportResponse> ProcessServiceRuleFileStream(Stream fileStream, string subClientName)
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
                    ActiveGroupType = ActiveGroupTypeConstants.ServiceRules,
                    Name = subClientName,
                    AddedBy = "System",
                    StartDate = DateTime.Now.AddDays(-1),
                    CreateDate = DateTime.Now,
                    IsEnabled = true
                };

                var fileReadTime = Stopwatch.StartNew();
                var serviceRules = await ReadServiceRuleFileStreamAsync(fileStream, activeGroupId);
                response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"Service Rule file stream rows read: { serviceRules.Count }");

                serviceRuleActiveGroup.Name = subClientName;
                serviceRuleActiveGroup.PartitionKey = subClientName;

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

        private async Task<List<ServiceRule>> ReadServiceRuleFileStreamAsync(Stream stream, string activeGroupId)
        {
            try
            {
                var serviceRules = new List<ServiceRule>();

                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var parts = line.Split('|');
                        if (parts.Length > 22 &&
                            StringHelper.Exists(parts[0]) && parts[0].ToUpper() != "SITE" &&
                            StringHelper.Exists(parts[1]) && parts[1].ToUpper() != "MAILCODE")
                        {
                            serviceRules.Add(new ServiceRule
                            {
                                PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(activeGroupId),
                                ActiveGroupId = activeGroupId,
                                MailCode = parts[1],
                                IsOrmd = parts[2] == "1" ? true : false,
                                IsPoBox = parts[3] == "1" ? true : false,
                                IsOutside48States = parts[4] == "1" ? true : false,
                                IsUpsDas = parts[5] == "1" ? true : false,
                                IsSaturday = parts[6] == "1" ? true : false,
                                IsDduScfBin = parts[7] == "1" ? true : false,
                                MinWeight = decimal.Parse(parts[8]),
                                MaxWeight = decimal.Parse(parts[9]),
                                MinLength = decimal.Parse(parts[10]),
                                MaxLength = decimal.Parse(parts[11]),
                                MinHeight = decimal.Parse(parts[12]),
                                MaxHeight = decimal.Parse(parts[13]),
                                MinWidth = decimal.Parse(parts[14]),
                                MaxWidth = decimal.Parse(parts[15]),
                                MinTotalDimensions = int.Parse(parts[16]),
                                MaxTotalDimensions = int.Parse(parts[17]),
                                ZoneMin = int.Parse(parts[18]),
                                ZoneMax = int.Parse(parts[19]),
                                ShippingCarrier = parts[20],
                                ShippingMethod = parts[21],
                                ServiceLevel = parts[22],
                                IsQCRequired = parts.Length > 23 && parts[23] == "1" ? true : false,
                            });
                        }
                    }

                    return serviceRules;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to read Service Rule file. Exception: { ex }");
                return (new List<ServiceRule>());
            }
        }
    }
}
