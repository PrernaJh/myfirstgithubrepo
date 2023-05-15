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
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class ServiceRuleExtensionFileProcessor : IServiceRuleExtensionFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IServiceRuleExtensionRepository serviceRuleExtensionRepository;
		private readonly ILogger<ServiceRuleExtensionFileProcessor> logger;

		public ServiceRuleExtensionFileProcessor(IActiveGroupProcessor activeGroupProcessor, IServiceRuleExtensionRepository serviceRuleExtensionRepository, ILogger<ServiceRuleExtensionFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.serviceRuleExtensionRepository = serviceRuleExtensionRepository;
			this.logger = logger;
		}

		public async Task<FileImportResponse> ImportFortyEightStatesFileToDatabase(Stream fileStream, string subClientName)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();
				var fortyEightStatesRules = await ReadFortyEightStatesFileStreamAsync(fileStream);

				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"Forty Eight States file stream rows read: { fortyEightStatesRules.Count()}");

				var activeGroupId = Guid.NewGuid().ToString();
				var fortyEightStatesActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = subClientName,
					ActiveGroupType = ActiveGroupTypeConstants.FortyEightStates,
					AddedBy = "System",
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				foreach (var rule in fortyEightStatesRules)
				{
					rule.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await serviceRuleExtensionRepository.AddItemsAsync(fortyEightStatesRules, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(fortyEightStatesActiveGroup); 
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Service Rule Extensions", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing forty eight states rules. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}

		private async Task<List<ServiceRuleExtension>> ReadFortyEightStatesFileStreamAsync(Stream stream)
		{
			try
			{
				var serviceRuleExtensions = new List<ServiceRuleExtension>();

				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split('|');
						if (parts.Length > 8 && StringHelper.Exists(parts[7]))
						{
							var index = 10;
							var serviceLevel = index < parts.Length ? parts[10] : string.Empty;
							serviceRuleExtensions.Add(new ServiceRuleExtension
							{
								CreateDate = DateTime.Now,
								MailCode = parts[0],
								StateCode = parts[1],
								IsDefault = parts[2] == "1" ? true : false,
								InFedExList = parts[3] == "1" ? true : false,
								InUpsList = parts[4] == "1" ? true : false,
								IsSaturdayDelivery = parts[5] == "1" ? true : false,
								MinWeight = decimal.Parse(parts[6]),
								MaxWeight = decimal.Parse(parts[7]),
								ShippingCarrier = parts[8],
								ShippingMethod = parts[9],
								ServiceLevel = serviceLevel
							});
						}
					}
				}
				return serviceRuleExtensions;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read Forty Eight States file. Exception: { ex }");
				return new List<ServiceRuleExtension>();
			}
		}
	}
}
