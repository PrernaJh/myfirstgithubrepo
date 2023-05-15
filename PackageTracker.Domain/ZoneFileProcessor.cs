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
	public class ZoneFileProcessor : IZoneFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IZoneMapRepository zoneMapRepository;
		private readonly ILogger<ZoneFileProcessor> logger;

		public ZoneFileProcessor(IActiveGroupProcessor activeGroupProcessor, IZoneMapRepository zoneMapRepository, ILogger<ZoneFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.zoneMapRepository = zoneMapRepository;
			this.logger = logger;
		}

		public async Task<FileImportResponse> ImportZoneFileToDatabase(Stream fileStream)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();
				var zoneMaps = await ReadZoneFileStreamAsync(fileStream);

				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"Zone file stream rows read: { zoneMaps.Count }");

				var activeGroupId = Guid.NewGuid().ToString();
				var zoneMapsActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = SiteConstants.AllSites,
					ActiveGroupType = ActiveGroupTypeConstants.ZoneMaps,
					AddedBy = "System",
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.UtcNow,
					IsEnabled = true
				};

				foreach (var zoneMap in zoneMaps)
				{
					zoneMap.ActiveGroupId = activeGroupId;
				}
				
				var bulkResponse = await zoneMapRepository.AddItemsAsync(zoneMaps, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(zoneMapsActiveGroup);
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zone Maps", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing zone maps. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}

		public async Task<ICollection<ZoneMap>> ReadZoneFileStreamAsync(Stream stream)
		{
			try
			{
				var zoneMaps = new List<ZoneMap>();

				using (var reader = new StreamReader(stream))
				{
					var passedFirstLine = false;

					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						if (passedFirstLine)
						{
							if (line.Length > 3 && Regex.IsMatch(line.Substring(0, 3), @"[0-9]"))
							{
								zoneMaps.Add(new ZoneMap
								{
									ZipFirstThree = line.Substring(0, 3),
									ZoneMatrix = line.Substring(3),
									CreateDate = DateTime.Now
								});
                            }
						}
						else
						{
							passedFirstLine = true;
						}
					}
				}
				return zoneMaps;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read Zone file. Exception: { ex }");
				return new List<ZoneMap>();
			}
		}
	}
}
