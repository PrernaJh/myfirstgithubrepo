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
    public class GeoDescriptorsWebProcessor : IGeoDescriptorsWebProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<GeoDescriptorsWebProcessor> logger;
        private readonly IZipMapRepository zipMapRepository;

        public GeoDescriptorsWebProcessor(IActiveGroupProcessor activeGroupProcessor,
            ILogger<GeoDescriptorsWebProcessor> logger,
            IZipMapRepository zipMapRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.zipMapRepository = zipMapRepository;
        }

        public async Task<List<ZipMap>> GetZipMapsByActiveGroupIdAsync(string activeGroupId)
        {
            var response = await zipMapRepository.GetZipMapsByActiveGroupIdAsync(activeGroupId);
            return response.ToList();
        }

        public async Task<FileImportResponse> ImportZipMaps(List<ZipMap> zipMaps, string username, string name, string startDate, string filename)
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
                    ActiveGroupType = ActiveGroupTypeConstants.UpsGeoDescriptors,
                    StartDate = startDateTime,
                    CreateDate = DateTime.UtcNow,
                    Filename = filename,
                    IsEnabled = true
                };

                foreach (var zipMap in zipMaps)
                {
                    zipMap.ActiveGroupId = activeGroupId;
                }
                var bulkResponse = await zipMapRepository.AddItemsAsync(zipMaps, activeGroupId);
                if (!bulkResponse.IsSuccessful)
                    throw new Exception("Bulk upload failed");
                await activeGroupProcessor.AddActiveGroupAsync(zipActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zone Maps", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing Zone Maps. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}

