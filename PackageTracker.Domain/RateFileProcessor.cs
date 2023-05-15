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
using static PackageTracker.Domain.Utilities.StringHelper;

namespace PackageTracker.Domain
{
	public class RateFileProcessor : IRateFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IRateRepository rateRepository;
		private readonly ISiteProcessor siteProcessor;		
		private readonly ILogger<RateFileProcessor> logger;

		public RateFileProcessor(IActiveGroupProcessor activeGroupProcessor, 
			IRateRepository rateRepository, 
			ISiteProcessor siteProcessor, 
			ILogger<RateFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.rateRepository = rateRepository;
			this.siteProcessor = siteProcessor;			
			this.logger = logger;
		}		

		public async Task<FileImportResponse> ImportRatesFileToDatabase(Stream fileStream, SubClient subClient)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();
				var rates = await ReadRatesFileStreamAsync(fileStream);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"Rates file stream rows read: { rates.Count }");

				var activeGroupId = Guid.NewGuid().ToString();
				var rateActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = subClient.Name,
					ActiveGroupType = ActiveGroupTypeConstants.Rates,					
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				foreach (var rate in rates)
				{
					rate.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await rateRepository.AddItemsAsync(rates, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(rateActiveGroup);
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

		private async Task<List<Rate>> ReadRatesFileStreamAsync(Stream stream)
		{
			try
			{
				var rates = new List<Rate>();

				using (var reader = new StreamReader(stream))
				{
					var passedFirstLine = false;

					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						if (passedFirstLine)
						{
							var parts = line.Split('|');
							if (parts.Length > 27 && 
								StringHelper.Exists(parts[0]) && parts[0].ToUpper() != "CARRIER" && 
								parts[2] == RateFileConstants.Package)
							{
								rates.Add(new Rate
								{
									Carrier = parts[0],
									Service = RateUtility.AssignServiceTypeMapping(parts[1], parts[0]),
									ContainerType = parts[2],
									WeightNotOverOz = ParseIntoDecimalOrReturnZero(parts[3]),
									CostZone1 = ParseIntoDecimalOrReturnZero(parts[4]),
									CostZone2 = ParseIntoDecimalOrReturnZero(parts[5]),
									CostZone3 = ParseIntoDecimalOrReturnZero(parts[6]),
									CostZone4 = ParseIntoDecimalOrReturnZero(parts[7]),
									CostZone5 = ParseIntoDecimalOrReturnZero(parts[8]),
									CostZone6 = ParseIntoDecimalOrReturnZero(parts[9]),
									CostZone7 = ParseIntoDecimalOrReturnZero(parts[10]),
									CostZone8 = ParseIntoDecimalOrReturnZero(parts[11]),
									CostZone9 = ParseIntoDecimalOrReturnZero(parts[12]),
									CostZoneDdu = ParseIntoDecimalOrReturnZero(parts[13]),
									CostZoneScf = ParseIntoDecimalOrReturnZero(parts[14]),
									CostZoneNdc = ParseIntoDecimalOrReturnZero(parts[15]),
									ChargeZone1 = ParseIntoDecimalOrReturnZero(parts[16]),
									ChargeZone2 = ParseIntoDecimalOrReturnZero(parts[17]),
									ChargeZone3 = ParseIntoDecimalOrReturnZero(parts[18]),
									ChargeZone4 = ParseIntoDecimalOrReturnZero(parts[19]),
									ChargeZone5 = ParseIntoDecimalOrReturnZero(parts[20]),
									ChargeZone6 = ParseIntoDecimalOrReturnZero(parts[21]),
									ChargeZone7 = ParseIntoDecimalOrReturnZero(parts[22]),
									ChargeZone8 = ParseIntoDecimalOrReturnZero(parts[23]),
									ChargeZone9 = ParseIntoDecimalOrReturnZero(parts[24]),
									ChargeZoneDdu = ParseIntoDecimalOrReturnZero(parts[25]),
									ChargeZoneScf = ParseIntoDecimalOrReturnZero(parts[26]),
									ChargeZoneNdc = ParseIntoDecimalOrReturnZero(parts[27]),
									CreateDate = DateTime.Now
								});
							}
						}
						else
						{
							passedFirstLine = true;
						}
					}
					return rates;
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read Rates file. Exception: { ex }");
				return new List<Rate>();
			}
		}
	}
}
