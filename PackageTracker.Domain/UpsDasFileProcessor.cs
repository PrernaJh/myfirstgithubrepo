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
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class UpsDasFileProcessor : IUpsDasFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IZipOverrideRepository zipOverrideRepository;
		private readonly ILogger<UpsDasFileProcessor> logger;

		public UpsDasFileProcessor(IActiveGroupProcessor activeGroupProcessor, IZipOverrideRepository zipOverrideRepository, ILogger<UpsDasFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.zipOverrideRepository = zipOverrideRepository;
			this.logger = logger;
		}

		public async Task<FileImportResponse> ImportUpsDasFileToDatabase(Stream fileStream)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var fileReadWatch = Stopwatch.StartNew();
				var upsDasZips = await ReadUpsDasFileStreamAsync(fileStream);

				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadWatch.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"UPS DAS Zips file stream rows read: { upsDasZips.Count }");

				var activeGroupId = Guid.NewGuid().ToString();
				var upsDasActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = SiteConstants.AllSites,
					ActiveGroupType = ActiveGroupTypeConstants.ZipsUpsDas,
					AddedBy = "System",
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				foreach (var upsDasZip in upsDasZips)
				{
					upsDasZip.ActiveGroupId = activeGroupId;
				}

				var bulkResponse = await zipOverrideRepository.AddItemsAsync(upsDasZips, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(upsDasActiveGroup);
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


		private async Task<List<ZipOverride>> ReadUpsDasFileStreamAsync(Stream stream)
		{
			try
			{
				var upsDasZips = new List<ZipOverride>();

				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split("\n");

						upsDasZips.Add(new ZipOverride
						{
							ZipCode = parts[0],
							ActiveGroupType = ActiveGroupTypeConstants.ZipsUpsDas,
							CreateDate = DateTime.Now
						});
					}
				}
				return upsDasZips;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read UpsDas file. Exception: { ex }");
				return new List<ZipOverride>();
			}
		}
	}
}
