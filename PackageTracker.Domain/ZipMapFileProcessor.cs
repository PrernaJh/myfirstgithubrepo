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
	public class ZipMapFileProcessor : IZipMapFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IZipMapRepository zipMapRepository;
		private readonly ILogger<ZipMapFileProcessor> logger;

		public ZipMapFileProcessor(IActiveGroupProcessor activeGroupProcessor, IZipMapRepository zipMapRepository, ILogger<ZipMapFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.zipMapRepository = zipMapRepository;
			this.logger = logger;
		}


		public async Task<FileImportResponse> ImportZipMaps(Stream fileStream, string fileName)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();

				var activeGroupType = string.Empty;

				if (fileName.ToUpper().Contains("LSO"))
				{
					activeGroupType = ActiveGroupTypeConstants.ZipsLso;
				}
				else if (fileName.ToUpper().Contains("ONTR"))
				{
					activeGroupType = ActiveGroupTypeConstants.ZipsOnTrac;
				}
				else
				{
					logger.Log(LogLevel.Error, $"Invalid ZipMap file name: {fileName}");
					throw new Exception();
				}

				var zipMaps = await ReadZipMapFileStreamAsync(fileStream, activeGroupType);
				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"ZipMap file stream rows read: { zipMaps.Count }");

				var activeGroupId = Guid.NewGuid().ToString();
				var zipMapActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = SiteConstants.AllSites,
					ActiveGroupType = activeGroupType,
					AddedBy = "System",
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				foreach (var zipMap in zipMaps)
				{
					zipMap.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await zipMapRepository.AddItemsAsync(zipMaps, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(zipMapActiveGroup);
				response.NumberOfDocumentsImported = bulkResponse.Count;
				response.RequestUnitsConsumed = bulkResponse.RequestCharge;
				response.IsSuccessful = bulkResponse.IsSuccessful;
				response.Message = bulkResponse.Message;
				response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Import Zip Maps", response)}");
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failure while importing zip overrides. Exception: {ex}");
				response.IsSuccessful = false;
				response.Message = ex.Message;
			}
			return response;
		}

		private async Task<List<ZipMap>> ReadZipMapFileStreamAsync(Stream stream, string activeGroupType)
		{
			try
			{
				var zipMaps = new List<ZipMap>();

				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split('|');
						if (parts.Length > 1 && StringHelper.Exists(parts[0]) && Regex.IsMatch(parts[0], @"^[0-9-]+$"))
						{
							zipMaps.Add(new ZipMap
							{
								ZipCode = parts[0],
								Value = parts[1],
								ActiveGroupType = activeGroupType,
								CreateDate = DateTime.Now
							});
						}
					}
				}
				return zipMaps;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read ZipMap file. Exception: { ex }");
				return new List<ZipMap>();
			}
		}
	}
}
