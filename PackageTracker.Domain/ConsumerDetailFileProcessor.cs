using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class ConsumerDetailFileProcessor : IConsumerDetailFileProcessor
	{
		private readonly ILogger<ConsumerDetailFileProcessor> logger;
		private readonly IPackagePostProcessor packagePostProcessor;

		public ConsumerDetailFileProcessor(ILogger<ConsumerDetailFileProcessor> logger, IPackagePostProcessor packagePostProcessor, ISubClientProcessor subClientProcessor)
		{
			this.logger = logger;
			this.packagePostProcessor = packagePostProcessor;
		}

		public async Task<FileExportResponse> GetConsumerDetailFileAsync(SubClient subClient, string webJobId, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbReadWatch = Stopwatch.StartNew();
			var packages = await packagePostProcessor.GetPackagesForConsumerDetailFile(subClient.Name, lastScanDateTime, nextScanDateTime);
			dbReadWatch.Stop();

			if (packages.Any())
			{
				CreateConsumerDetailRecords(response, packages, subClient, webJobId);
				response.NumberOfRecords = response.FileContents.Count;
			}
			else
			{
				logger.Log(LogLevel.Information, $"Zero packages found for Consumer Detail file");
			}
			response.IsSuccessful = true;
			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}

		public void CreateConsumerDetailRecords(FileExportResponse response, IEnumerable<Package> packages, SubClient subClient, string webJobId)
		{
			foreach (var package in packages)
			{
				var formattedLocalProcessedDate = package.LocalProcessedDate.ToString("MM/dd/yy hh:mm:ss tt");
				var record = new ConsumerDetailFileRecord
				{
					CmopId = subClient.Key,
					PackageId = package.PackageId,
					Carrier = package.ShippingCarrier,
					TrackingNumber = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
					ManifestDate = formattedLocalProcessedDate,
					ShippingDate = formattedLocalProcessedDate,
					Weight = package.Weight.ToString()
				};

				response.FileContents.Add(BuildRecordString(record));
			}
		}

		private static string BuildRecordString(ConsumerDetailFileRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.CmopId);
			recordBuilder.Append(delimiter + record.PackageId);
			recordBuilder.Append(delimiter + record.Carrier);
			recordBuilder.Append(delimiter + record.TrackingNumber);
			recordBuilder.Append(delimiter + record.ManifestDate);
			recordBuilder.Append(delimiter + record.ShippingDate);
			recordBuilder.Append(delimiter + record.Weight);
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}
	}
}
