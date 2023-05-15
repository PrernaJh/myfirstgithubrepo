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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class ZipOverrideProcessor : IZipOverrideProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IZipOverrideRepository zipOverrideRepository;
		private readonly ILogger<ZipOverrideProcessor> logger;

		public ZipOverrideProcessor(IActiveGroupProcessor activeGroupProcessor, IZipOverrideRepository zipOverrideRepository, ILogger<ZipOverrideProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.zipOverrideRepository = zipOverrideRepository;
			this.logger = logger;
		}
	
		public async Task AssignZipOverridesForListOfPackages(List<Package> packages, List<string> activeGroupIds)
		{
			var groupedPackages = packages.GroupBy(x => x.Zip);

			foreach (var groupOfPackages in groupedPackages)
			{
				foreach (var activeGroupId in activeGroupIds)
				{
					var zipOverride = await zipOverrideRepository.GetZipOverrideByZipCodeAsync(groupOfPackages.Key, activeGroupId);

					if (StringHelper.Exists(zipOverride.Id))
					{
						foreach (var package in groupOfPackages)
						{
							package.ZipOverrides.Add(zipOverride.ActiveGroupType);
							package.ZipOverrideIds.Add(zipOverride.Id);
							package.ZipOverrideGroupIds.Add(activeGroupId);

							if (zipOverride.ActiveGroupType == ActiveGroupTypeConstants.ZipsUpsDas)
							{
								package.IsUpsDas = true;
							}
							else if (zipOverride.ActiveGroupType == ActiveGroupTypeConstants.ZipsUspsRural)
							{
								package.IsRural = true;
							}
						}
					}
				}
			}
		}		

		public async Task<FileImportResponse> ImportZipCarrierOverrideFileToDatabase(Stream fileStream, string fileName, string subClientName)
		{
			var response = new FileImportResponse();
            try
            {
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();
				var zipCarrierOverrides = await ReadZipCarrierOverrideFileStreamAsync(fileStream);

				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"ZipCarrierOverride Zips file stream rows read: { zipCarrierOverrides.Count }");

				var activeGroupId = Guid.NewGuid().ToString();
				var zipCarrierOverrideActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = subClientName,
					Filename = fileName,
					ActiveGroupType = ActiveGroupTypeConstants.ZipCarrierOverride,
					AddedBy = "System",
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				foreach (var zipCarrierOverride in zipCarrierOverrides)
				{
					zipCarrierOverride.ActiveGroupId = activeGroupId;
				}
				var bulkResponse = await zipOverrideRepository.AddItemsAsync(zipCarrierOverrides, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(zipCarrierOverrideActiveGroup);
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zip Overrides", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing zip overrides. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}
			
		private async Task<List<ZipOverride>> ReadZipCarrierOverrideFileStreamAsync(Stream stream)
		{
			try
			{
				var zipCarrierOverrides = new List<ZipOverride>();

				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split("|");
						if (parts.Length > 4 && StringHelper.Exists(parts[0]) && Regex.IsMatch(parts[0], @"^[0-9-]+$"))
							zipCarrierOverrides.Add(new ZipOverride
						{
							ZipCode = parts[0],
							FromShippingCarrier = parts[1],
							FromShippingMethod = parts[2],
							ToShippingCarrier = parts[3],
							ToShippingMethod = parts[4],
							ActiveGroupType = ActiveGroupTypeConstants.ZipCarrierOverride,
							CreateDate = DateTime.Now
						});
					}
				}
				return zipCarrierOverrides;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read ZipCarrierOverride file. Exception: { ex }");
				return new List<ZipOverride>();
			}
		}
	}
}
