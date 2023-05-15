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
    public class RateWebProcessor : IRateWebProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly ILogger<RateWebProcessor> logger;
		private readonly IRateRepository rateRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		public RateWebProcessor(IActiveGroupProcessor activeGroupProcessor,
			ILogger<RateWebProcessor> logger,
			IRateRepository rateRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor)
        {
            this.activeGroupProcessor = activeGroupProcessor;
			this.logger = logger;
            this.rateRepository = rateRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
        }

		public async Task<FileImportResponse> ImportListOfNewRates(List<Rate> rates, string startDate, string subClientName, string username, string fileName = null)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

				var activeGroupId = Guid.NewGuid().ToString();
				var ratesActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = subClientName,
					AddedBy = username,
					ActiveGroupType = ActiveGroupTypeConstants.Rates,
					StartDate = DateTime.Parse(startDate),
					CreateDate = DateTime.Now,
					Filename = fileName ?? string.Empty,
					IsEnabled = true
				};

				ratesActiveGroup.Name = subClientName;
				ratesActiveGroup.PartitionKey = subClientName;
				foreach (var rate in rates)
				{
					rate.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await rateRepository.AddItemsAsync(rates, activeGroupId);
				if (!bulkResponse.IsSuccessful)
					throw new Exception("Bulk upload failed");
				await activeGroupProcessor.AddActiveGroupAsync(ratesActiveGroup);
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Rates", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing rates. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}

		public async Task<FileImportResponse> ImportListOfNewContainerRates(List<Rate> rates, string startDate, string siteName, string username, string fileName = null)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);

				var activeGroupId = Guid.NewGuid().ToString();
				var ratesActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = siteName,
					AddedBy = username,
					ActiveGroupType = ActiveGroupTypeConstants.ContainerRates,
					StartDate = DateTime.Parse(startDate),
					CreateDate = DateTime.Now,
					Filename = fileName ?? string.Empty,
					IsEnabled = true
				};

				foreach (var rate in rates)
				{
					rate.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await rateRepository.AddItemsAsync(rates, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(ratesActiveGroup);
				response.DbInsertTime = bulkResponse.ElapsedTime;
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Rates", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing rates. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}

		public async Task<List<ActiveGroup>> GetAllRateActiveGroupsAsync(string subClientName)
        {
            var response = await activeGroupProcessor.GetAllRateActiveGroupsAsync(subClientName);
            return response.ToList();
        }
		
        public async Task<List<Rate>> GetRatesByActiveGroupIdAsync(string activeGroupId)
        {
            var response = await rateRepository.GetRatesByActiveGroupId(activeGroupId);
            return response.ToList();
        }
	}
}
