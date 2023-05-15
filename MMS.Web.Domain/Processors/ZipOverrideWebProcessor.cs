using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Interfaces;
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
    public class ZipOverrideWebProcessor : IZipOverrideWebProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<ZipOverrideWebProcessor> logger;
        private readonly IZipOverrideRepository zipOverrideRepository;

        public ZipOverrideWebProcessor(IActiveGroupProcessor activeGroupProcessor,
                ILogger<ZipOverrideWebProcessor> logger,
                IZipOverrideRepository zipOverrideRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.zipOverrideRepository = zipOverrideRepository;
        }

        public async Task<List<ZipOverride>> GetZipOverridesByActiveGroupIdAsync(string activeGroupId)
        {
            var response = await zipOverrideRepository.GetZipOverridesByActiveGroupId(activeGroupId);
            return response.ToList();
        }

        public async Task<FileImportResponse> ImportZipOverrides(List<ZipOverride> zipOverrides, string username, string name, string startDate, string filename = null)
        {
            var response = new FileImportResponse();
            try
            {
                DateTime.TryParse(startDate, out var startDateTime);
                var totalWatch = Stopwatch.StartNew();

                var activeGroupId = Guid.NewGuid().ToString();
                var zipActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = name,
                    AddedBy = username,
                    ActiveGroupType = zipOverrides.FirstOrDefault().ActiveGroupType,
                    StartDate = startDateTime,
                    CreateDate = DateTime.Now,
                    Filename = filename,
                    IsEnabled = true
                };

                logger.Log(LogLevel.Information, $"Zip Override file stream rows read: { zipOverrides.Count }");

                foreach (var zipOverride in zipOverrides)
                {
                    zipOverride.ActiveGroupId = activeGroupId;
                }
                var bulkResponse = await zipOverrideRepository.AddItemsAsync(zipOverrides, activeGroupId);
                if (!bulkResponse.IsSuccessful)
                    throw new Exception("Bulk upload failed");
                await activeGroupProcessor.AddActiveGroupAsync(zipActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zip Overrides", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing Zip Overrides. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }


}
