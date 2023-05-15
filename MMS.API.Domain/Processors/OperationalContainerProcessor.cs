using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.OperationalContainers;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class OperationalContainerProcessor : IOperationalContainerProcessor
    {
        private readonly IOperationalContainerRepository operationalContainerRepository;
        private readonly ILogger<IOperationalContainerProcessor> logger;

        public OperationalContainerProcessor(IOperationalContainerRepository operationalContainerRepository, ILogger<IOperationalContainerProcessor> logger)
        {
            this.operationalContainerRepository = operationalContainerRepository;
            this.logger = logger;
        }

        public async Task<AddOperationalContainerResponse> AddOperationalContainerAsync(AddOperationalContainerRequest request)
        {
            try
            {
                var unresolvedPartitionKey = $"{request.SiteName}{request.BinCode}";
                var operationalContainer = await operationalContainerRepository.AddItemAsync(new OperationalContainer
                {
                    Id = request.Id ?? string.Empty,
                    SiteName = request.SiteName,
                    BinCode = request.BinCode,
                    ContainerId = request.ContainerId,
                    Status = request.Status,
                    IsSecondaryCarrier = request.IsSecondaryCarrier
                }, unresolvedPartitionKey);

                return new AddOperationalContainerResponse
                {
                    Id = operationalContainer.Id,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to Add OperationalContainer. Request: {JsonUtility<AddOperationalContainerRequest>.Serialize(request)} Exception: {ex}");
                return new AddOperationalContainerResponse();
            }
        }

        public async Task<UpdateOperationalContainerResponse> UpdateOperationalContainerAsync(OperationalContainer operationalContainer)
        {
            var response = new UpdateOperationalContainerResponse();
            var updatedOperationalContainer = await operationalContainerRepository.UpdateItemAsync(operationalContainer);

            if (StringHelper.Exists(updatedOperationalContainer.Id))
            {
                response.IsSuccessful = true;
            }
            return response;
        }

        public async Task<UpdateOperationalContainerResponse> UpdateOperationalContainerStatus(UpdateOperationalContainerRequest request)
        {
            try
            {
                var unresolvedPartitionKey = $"{request.SiteName}{request.BinCode}";
                var operationalContainer = await operationalContainerRepository.GetItemAsync(request.Id, unresolvedPartitionKey);
                operationalContainer.Status = request.Status;
                await operationalContainerRepository.UpdateItemAsync(operationalContainer);

                return new UpdateOperationalContainerResponse
                {
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to Update OperationalContainer. Request: {JsonUtility<UpdateOperationalContainerRequest>.Serialize(request)} Exception: {ex}");
                return new UpdateOperationalContainerResponse();
            }
        }

        public async Task<UpdateOperationalContainerResponse> UpdateIsSecondaryCarrierAsync(UpdateOperationalContainerRequest request)
        {
            try
            {
                var unresolvedPartitionKey = $"{request.SiteName}{request.BinCode}";
                var operationalContainer = await operationalContainerRepository.GetItemAsync(request.Id, unresolvedPartitionKey);
                operationalContainer.IsSecondaryCarrier = request.IsSecondaryCarrier;
                await operationalContainerRepository.UpdateItemAsync(operationalContainer);

                return new UpdateOperationalContainerResponse
                {
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to Update OperationalContainer. Request: {JsonUtility<UpdateOperationalContainerRequest>.Serialize(request)} Exception: {ex}");
                return new UpdateOperationalContainerResponse();
            }
        }

        public async Task<OperationalContainer> GetMostRecentOperationalContainerAsync(string siteName, string binCode)
        {
            try
            {
                var unresolvedPartitionKey = $"{siteName}{binCode}";
                var response = await operationalContainerRepository.GetMostRecentOperationalContainerAsync(siteName, binCode, unresolvedPartitionKey);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get most recent OperationalContainer. Site: {siteName} BinCode: {binCode} Exception: {ex}");
                return new OperationalContainer();
            }
        }

        public async Task<OperationalContainer> GetActiveOperationalContainerAsync(string siteName, string binCode)
        {
            try
            {
                var unresolvedPartitionKey = $"{siteName}{binCode}";
                var response = await operationalContainerRepository.GetActiveOperationalContainerAsync(siteName, binCode, unresolvedPartitionKey);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get most recent active OperationalContainer. Site: {siteName} BinCode: {binCode} Exception: {ex}");
                return new OperationalContainer();
            }
        }

        public async Task<GetOperationalContainerResponse> GetOperationalContainerAsync(GetOperationalContainerRequest request)
        {
            try
            {
                var response = new GetOperationalContainerResponse();
                var unresolvedPartitionKey = $"{request.SiteName}{request.BinCode}";
                var operationalContainer = await operationalContainerRepository.GetOperationalContainerAsync(request.SiteName, request.ContainerId, unresolvedPartitionKey);

                if (StringHelper.Exists(operationalContainer.Id))
                {
                    response.OperationalContainer = operationalContainer;
                    response.IsSuccessful = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get most recent OperationalContainer. Site: {request.SiteName} BinCode: {request.BinCode} Exception: {ex}");
                return new GetOperationalContainerResponse();
            }
        }
    }
}
