using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class UpsGeoDescFileProcessor : IUpsGeoDescFileProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IZipMapRepository zipMapRepository;
        private readonly ILogger<UpsGeoDescFileProcessor> logger;

        public UpsGeoDescFileProcessor(IActiveGroupProcessor activeGroupProcessor, IZipMapRepository zipMapRepository, ILogger<UpsGeoDescFileProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.zipMapRepository = zipMapRepository;
            this.logger = logger;
        }

        public async Task<FileImportResponse> ImportUpsGeoDescFileToDatabase(Stream fileStream, Site site)
        {
            var response = new FileImportResponse();

            try
            {
                var totalWatch = Stopwatch.StartNew();
                var fileReadWatch = Stopwatch.StartNew();
                var geoDescritors = await ReadGeoDescFileStreamAsync(fileStream);

                response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"Geo descriptor file stream rows read: { geoDescritors.Count }");

                var activeGroupId = Guid.NewGuid().ToString();
                var activeGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = site.SiteName,
                    ActiveGroupType = ActiveGroupTypeConstants.UpsGeoDescriptors,
                    StartDate = DateTime.Now.AddDays(-1),
                    CreateDate = DateTime.Now,
                    IsEnabled = true
                };

                foreach (var gd in geoDescritors)
                {
                    gd.ActiveGroupId = activeGroupId;
                }

                var bulkResponse = await zipMapRepository.AddItemsAsync(geoDescritors, activeGroupId);
                if (bulkResponse.IsSuccessful)
                    await activeGroupProcessor.AddActiveGroupAsync(activeGroup);
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zip Maps", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing geo descriptors. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private async Task<List<ZipMap>> ReadGeoDescFileStreamAsync(Stream stream)
        {
            try
            {
                var geoDescriptors = new List<ZipMap>();

                using (var reader = new StreamReader(stream))
                {
                    var passedFirstLine = false;

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (passedFirstLine)
                        {
                            var parts = line.Split('|');
                            if (parts.Length > 2 && StringHelper.Exists(parts[0]) && Regex.IsMatch(parts[0], @"^[0-9-]+$"))
                            {
                                geoDescriptors.Add(new ZipMap
                                {
                                    ZipCode = parts[0],
                                    Value = parts[2]
                                });
                            }
                        }
                        else
                        {
                            passedFirstLine = true;
                        }
                    }
                    return geoDescriptors;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to read Geo Descriptor file. Exception: { ex }");
                return new List<ZipMap>();
            }
        }
    }
}
