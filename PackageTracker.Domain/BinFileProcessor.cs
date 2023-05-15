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
	public class BinFileProcessor : IBinFileProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinRepository binRepository;
		private readonly IBinMapRepository binMapRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly ILogger<BinFileProcessor> logger;

		public BinFileProcessor(IActiveGroupProcessor activeGroupProcessor, IBinRepository binRepository, IBinMapRepository binMapRepository, ISiteProcessor siteProcessor, ISubClientProcessor subClientProcessor, ILogger<BinFileProcessor> logger)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.binRepository = binRepository;
			this.binMapRepository = binMapRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.logger = logger;
		}

		public async Task<FileImportResponse> ProcessBinFileStream(Stream fileStream, string siteName, DateTime startDate)
		{
			var response = new FileImportResponse();
            try
            {
				var totalWatch = Stopwatch.StartNew();
				var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);

				var activeGroupId = Guid.NewGuid().ToString();
				var binActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = site.SiteName,
					AddedBy = "System",
					ActiveGroupType = ActiveGroupTypeConstants.Bins,
					StartDate = startDate,
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				var fileReadTime = Stopwatch.StartNew();
				var bins = await ReadBinFileStreamAsync(fileStream, activeGroupId);
				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"Bin file stream rows read: { bins.Count }");

				var bulkResponse = await binRepository.AddItemsAsync(bins, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(binActiveGroup);
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

		public async Task<FileImportResponse> ProcessBinMapFileStream(Stream fileStream, string subClientName)
		{
			var response = new FileImportResponse();
			try
			{
				var totalWatch = Stopwatch.StartNew();
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

				var activeGroupId = Guid.NewGuid().ToString();
				var binMapActiveGroup = new ActiveGroup
				{
					Id = activeGroupId,
					Name = subClient.Name,
					AddedBy = "System",
					ActiveGroupType = ActiveGroupTypeConstants.BinMaps,
					StartDate = DateTime.Now.AddDays(-1),
					CreateDate = DateTime.Now,
					IsEnabled = true
				};

				var fileReadTime = Stopwatch.StartNew();
				var binMaps = await ReadBinMapFileStreamAsync(fileStream, activeGroupId);
				response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
				logger.Log(LogLevel.Information, $"Bin Map file stream rows read: { binMaps.Count }");

				var bulkResponse = await binMapRepository.AddItemsAsync(binMaps, activeGroupId);
				if (bulkResponse.IsSuccessful)
					await activeGroupProcessor.AddActiveGroupAsync(binMapActiveGroup);
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

		private async Task<List<Bin>> ReadBinFileStreamAsync(Stream stream, string activeGroupId)
		{
			try
			{
				var bins = new List<Bin>();

				using var reader = new StreamReader(stream);
				var passedFirstLine = false;

				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();

					if (passedFirstLine)
					{
						var parts = line.Split('|');
						if (parts.Length > 29 && StringHelper.Exists(parts[0]))
						{
							bins.Add(new Bin
							{
								ActiveGroupId = activeGroupId,
								CreateDate = DateTime.Now,
								BinCode = parts[0].ToUpper(),
								LabelListSiteKey = parts[1].ToUpper(),
								LabelListDescription = parts[2].ToUpper(),
								LabelListZip = parts[3].ToUpper(),
								OriginPointSiteKey = parts[4].ToUpper(),
								OriginPointDescription = parts[5].ToUpper(),
								DropShipSiteKeyPrimary = parts[6].ToUpper(),
								DropShipSiteDescriptionPrimary = parts[7].ToUpper(),
								DropShipSiteAddressPrimary = parts[8].ToUpper(),
								DropShipSiteCszPrimary = parts[9].ToUpper(),
								ShippingCarrierPrimary = BinFileUtility.MapShippingCarrier(parts[10]),
								ShippingMethodPrimary = BinFileUtility.MapShippingMethod(parts[11]),
								ContainerTypePrimary = parts[12].ToUpper(),
								LabelTypePrimary = parts[13].ToUpper(),
								RegionalCarrierHubPrimary = parts[14].ToUpper(),
								DaysOfTheWeekPrimary = parts[15].ToUpper(),
								ScacPrimary = parts[16].ToUpper(),
								AccountIdPrimary = parts[17].ToUpper(),
								BinCodeSecondary = parts[18].ToUpper(),
								DropShipSiteKeySecondary = parts[19].ToUpper(),
								DropShipSiteDescriptionSecondary = parts[20].ToUpper(),
								DropShipSiteAddressSecondary = parts[21].ToUpper(),
								DropShipSiteCszSecondary = parts[22].ToUpper(),
								ShippingMethodSecondary = BinFileUtility.MapShippingMethod(parts[23]),
								ContainerTypeSecondary = parts[24].ToUpper(),
								LabelTypeSecondary = parts[25].ToUpper(),
								RegionalCarrierHubSecondary = parts[26].ToUpper(),
								DaysOfTheWeekSecondary = parts[27].ToUpper(),
								ScacSecondary = parts[28].ToUpper(),
								AccountIdSecondary = parts[29].ToUpper()
							});
						}
					}
					else
					{
						passedFirstLine = true;
					}
				}

				return bins;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read Bin file. Exception: { ex }");
				return (new List<Bin>());
			}
		}

		private async Task<List<BinMap>> ReadBinMapFileStreamAsync(Stream stream, string activeGroupId)
		{
			try
			{
				var binMaps = new List<BinMap>();

				using var reader = new StreamReader(stream);
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split('|');
						if (parts.Length > 1 && StringHelper.Exists(parts[1]) &&
							StringHelper.Exists(parts[0]) && Regex.IsMatch(parts[0], @"^[0-9-]+$"))
						{
							binMaps.Add(new BinMap
							{
								ActiveGroupId = activeGroupId,
								CreateDate = DateTime.Now,
								ZipCode = parts[0],
								BinCode = parts[1]
							});
						}
					}
					return binMaps;
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read Bin Maps file. Exception: { ex }");
				return (new List<BinMap>());
			}
		}		
	}
}
