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
    public class ServiceRuleExtensionWebProcessor : IServiceRuleExtensionWebProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IServiceRuleExtensionRepository serviceRuleExtensionRepository;
		private readonly ILogger<ServiceRuleExtensionWebProcessor> logger;

		public ServiceRuleExtensionWebProcessor(IActiveGroupProcessor activeGroupProcessor, 
											 IServiceRuleExtensionRepository serviceRuleExtensionRepository, 
											 ILogger<ServiceRuleExtensionWebProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.serviceRuleExtensionRepository = serviceRuleExtensionRepository;
			this.logger = logger;
		}

        
		public async Task<List<ServiceRuleExtension>> GetExtendedServiceRulesByActiveGroupIdAsync(string activeGroupId)
		{
			var response = await serviceRuleExtensionRepository.GetFortyEightStatesRulesByActiveGroupIdAsync(activeGroupId);

			return response.ToList();
		}

        

        public async Task<FileImportResponse> ImportListOfNewExtendedServiceRules(List<ServiceRuleExtension> extendedServiceRules, string startDate, string subClientName, string username, string filename = null)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();

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
				foreach (var extendedServiceRule in extendedServiceRules)
				{
					extendedServiceRule.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await serviceRuleExtensionRepository.AddItemsAsync(extendedServiceRules, activeGroupId);
				if (!bulkResponse.IsSuccessful)
					throw new Exception("Bulk upload failed");
				await activeGroupProcessor.AddActiveGroupAsync(serviceRuleActiveGroup);
				response.DbInsertTime = bulkResponse.ElapsedTime;
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Service Rule Extensions", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing service rule extensions. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}
	}
}
