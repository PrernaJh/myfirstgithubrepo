using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Interfaces;
using MMS.Web.Domain.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileManagement;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Processors
{
    public class BinWebProcessor : IBinWebProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IBinRepository binRepository;
        private readonly IBinMapRepository binMapRepository;
        private readonly ILogger<BinWebProcessor> logger;

        public BinWebProcessor(
            IActiveGroupProcessor activeGroupProcessor,
            IBinRepository binRepository,
            IBinMapRepository binMapRepository,
            ILogger<BinWebProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.binRepository = binRepository;
            this.binMapRepository = binMapRepository;
            this.logger = logger;
        }

        public async Task<GetBinsResponse> GetActiveBinsByActiveGroupIdAsync(string activeGroupId)
        {
            var response = new GetBinsResponse();

            response.Bins.AddRange(await binRepository.GetBinsByActiveGroupIdAsync(activeGroupId));

            return response;
        }

        public async Task<GetBinMapsResponse> GetActiveBinMapsByActiveGroupIdAsync(string activeGroupId)
        {
            var response = new GetBinMapsResponse();

            response.BinMaps.AddRange(await binMapRepository.GetBinMapsByActiveGroupIdAsync(activeGroupId));

            return response;
        }

        public async Task<FileImportResponse> ImportBinsAndBinMaps(ImportBinsAndBinMapsRequest request)
        {
            var response = new FileImportResponse() { IsSuccessful = true };
            if (request.Bins != null && request.Bins.Any())
            {
                response = await ImportNewBins(request.Bins, request.SiteName, request.UserName, request.StartDate, request.Filename);
            }
            if (response.IsSuccessful && request.BinMaps != null && request.BinMaps.Any())
            {
                response = await ImportNewBinMaps(request.BinMaps, request.SubClientName, request.UserName, request.StartDate, request.Filename);
            }
            return response;
        }

        private async Task<FileImportResponse> ImportNewBins(List<Bin> bins, string siteName, string username, string startDate, string filename)
        {
            var response = new FileImportResponse();
            try
            {
                DateTime.TryParse(startDate, out var startDateTime);
                var totalWatch = Stopwatch.StartNew();

                var activeGroupId = Guid.NewGuid().ToString();
                var binActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = siteName,
                    AddedBy = username,
                    ActiveGroupType = ActiveGroupTypeConstants.Bins,
                    StartDate = startDateTime,
                    CreateDate = DateTime.Now,
                    Filename = filename,
                    IsEnabled = true
                };

                logger.Log(LogLevel.Information, $"Bin file stream rows read: { bins.Count }");


                foreach (var bin in bins)
                {
                    bin.ShippingCarrierPrimary = BinFileUtility.MapShippingCarrier(bin.ShippingCarrierPrimary);
                    bin.ShippingMethodPrimary = BinFileUtility.MapShippingMethod(bin.ShippingMethodPrimary);
                    bin.ShippingMethodSecondary = BinFileUtility.MapShippingMethod(bin.ShippingMethodSecondary);

                    bin.ActiveGroupId = activeGroupId;
                }

                var bulkResponse = await binRepository.AddItemsAsync(bins, activeGroupId);
                if (!bulkResponse.IsSuccessful)
                    throw new Exception("Bulk upload failed");
                await activeGroupProcessor.AddActiveGroupAsync(binActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Bins", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing bins. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private async Task<FileImportResponse> ImportNewBinMaps(List<BinMap> binMaps, string subClientName, string username, string startDate, string filename)
        {
            var response = new FileImportResponse();
            try
            {
                DateTime.TryParse(startDate, out var startDateTime);
                var totalWatch = Stopwatch.StartNew();

                var activeGroupId = Guid.NewGuid().ToString();
                var binMapActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = subClientName,
                    AddedBy = username,
                    ActiveGroupType = ActiveGroupTypeConstants.BinMaps,
                    StartDate = startDateTime,
                    CreateDate = DateTime.Now,
                    Filename = filename,
                    IsEnabled = true
                };

                logger.Log(LogLevel.Information, $"Bin Map file stream rows read: { binMaps.Count }");

                foreach (var binMap in binMaps)
                {
                    binMap.ActiveGroupId = activeGroupId;
                }

                var bulkResponse = await binMapRepository.AddItemsAsync(binMaps, activeGroupId);
                if (bulkResponse.IsSuccessful)
                    await activeGroupProcessor.AddActiveGroupAsync(binMapActiveGroup);
                response.DbInsertTime = bulkResponse.ElapsedTime;
                response.NumberOfDocumentsImported = bulkResponse.Count;
                response.RequestUnitsConsumed = bulkResponse.RequestCharge;
                response.IsSuccessful = bulkResponse.IsSuccessful;
                response.Message = bulkResponse.Message;
                response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
                logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Bin Maps", response)}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failure while importing bins maps. Exception: {ex}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
