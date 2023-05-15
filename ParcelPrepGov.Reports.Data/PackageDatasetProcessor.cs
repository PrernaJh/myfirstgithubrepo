using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Data
{
	public class PackageDatasetProcessor : IPackageDatasetProcessor
	{
		private readonly ILogger<PackageDatasetProcessor> logger;
		private readonly IDatasetProcessor datasetProcessor;
		private readonly IPackageDatasetRepository packageDatasetRepository;
		private readonly IPackageEventDatasetRepository packageEventDatasetRepository;
		private readonly IPackageRepository packageRepository;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public PackageDatasetProcessor(ILogger<PackageDatasetProcessor> logger,
			IDatasetProcessor datasetProcessor,
			IPackageDatasetRepository packageDatasetRepository,
			IPackageEventDatasetRepository packageEventDatasetRepository,
			IPackageRepository packageRepository,
			IWebJobRunProcessor webJobRunProcessor
			)
		{
			this.logger = logger;
			this.datasetProcessor = datasetProcessor;
			this.packageDatasetRepository = packageDatasetRepository;
			this.packageEventDatasetRepository = packageEventDatasetRepository;
			this.packageRepository = packageRepository;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task<ReportResponse> UpdatePackageDatasets(List<PackageDataset> items)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				foreach (var group in items.GroupBy(p => p.SiteName))
				{
					int chunk = 1000;
					var site = new Site() { SiteName = group.Key };
					for (int offset = 0; offset < group.Count(); offset += chunk)
					{
						var datasets = group.Skip(offset).Take(chunk);
						foreach (var duplicates in datasets.GroupBy(p => p.ShippingBarcode))
                        {
							if (duplicates.Count() > 0 && StringHelper.Exists(duplicates.Key))
                            {
								bool first = true;
								foreach(var package in duplicates.OrderByDescending(p => p.LocalProcessedDate))
                                {
									if (! first)
                                    {
										package.ShippingBarcode = null; // Can't have duplicate (real) barcode entries in DB.
										package.HumanReadableBarcode = null;
									}
									first = false;
                                }
                            }
                        }
						var existingItems = await packageDatasetRepository
							.GetDatasetsByTrackingNumberAsync(datasets.Where(p => StringHelper.Exists(p.ShippingBarcode)).ToList());
						var itemsToUpdate = new List<PackageDataset>();
						foreach (var item in existingItems)
						{
							var package = datasets.FirstOrDefault(p => p.ShippingBarcode == item.ShippingBarcode);
							if (package != null && package.CosmosId != item.CosmosId)
							{
								item.ShippingBarcode = null; // Can't have duplicate (real) barcode entries in DB.
								item.HumanReadableBarcode = null;
								itemsToUpdate.Add(item);
							}
						}
						if(itemsToUpdate.Any())
							await packageDatasetRepository.ExecuteBulkUpdateAsync(itemsToUpdate);

						await BulkInsertOrUpdateAsync(site, response, datasets.ToList());
					}
					if (response.IsSuccessful)
						logger.LogInformation($"Number of package datasets inserted: {response.NumberOfDocuments}");
					else
						logger.LogError($"Failed to bulk insert package datasets. Total Failures: {response.NumberOfFailedDocuments}");
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert package datasets. Exception: { ex }");
			}
			return response;
		}

		public async Task<ReportResponse> UpdatePackageDatasets(Site site, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var timeStamps = await datasetProcessor.GetPackageTimeStampsAsync(site.SiteName, lastScanDateTime, nextScanDateTime);
				int chunk = 500;
				int next = 0;
				for (int offset = 0; offset < timeStamps.Count(); offset = next)
				{
					int last = offset + chunk > timeStamps.Count() ? timeStamps.Count() - 1 : offset + chunk - 1;
					// Find all timestamps at the beginning of the next chunk that are the same and include them in this chunk.
					while (last < timeStamps.Count() - 1)
					{
						if (timeStamps.ElementAt(last) != timeStamps.ElementAt(last + 1))
							break;
						last++;
					}
					next = last + 1;
					var packages = await datasetProcessor.GetPackagesForPackageDatasetsAsync(site.SiteName,
						timeStamps.ElementAt(offset),
						timeStamps.ElementAt(last));

					if (packages.Any())
					{
						var packageDatasets = packages.Select(p => CreateDataset(p)).ToList();
						logger.LogInformation($"Number of package datasets to insert: {packageDatasets.Count()}");
						var reportResponse = await UpdatePackageDatasets(packageDatasets);
						response.NumberOfDocuments += reportResponse.NumberOfDocuments;
						if (!reportResponse.IsSuccessful)
                        {
							response.IsSuccessful = false;
							response.Message = reportResponse.Message;
							response.NumberOfFailedDocuments += reportResponse.NumberOfFailedDocuments;
                        }
					}
				}
				if (response.IsSuccessful)
					logger.LogInformation($"Number of package datasets inserted: {response.NumberOfDocuments} for site: { site.SiteName }");
				else
					logger.LogError($"Failed to bulk insert/update package datasets for site: { site.SiteName }. Total Failures: {response.NumberOfFailedDocuments}");
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert package datasets for site: { site.SiteName }. Exception: { ex }");
			}
			return response;
		}

		private async Task BulkInsertOrUpdateAsync(Site site, ReportResponse response, List<PackageDataset> packageDatasets)
		{
			response.NumberOfDocuments += packageDatasets.Count();
			var bulkInsert = await packageDatasetRepository.ExecuteBulkInsertOrUpdateAsync(packageDatasets);
			if (!bulkInsert)
			{
				response.NumberOfFailedDocuments += packageDatasets.Count();
				response.IsSuccessful = false;
			}
		}

		private PackageDataset CreateDataset(Package package)
		{
			var dataset = new PackageDataset
			{
				CosmosId = package.Id,
				CosmosCreateDate = package.CreateDate,

				PackageId = package.PackageId,
				SiteName = package.SiteName,
				ClientName = package.ClientName,
				SubClientName = package.SubClientName,
				ClientFacilityName = package.ClientFacilityName,

				MailCode = package.MailCode,
				PackageStatus = package.PackageStatus,
				RecallStatus = package.RecallStatus,
				ProcessedDate = package.ProcessedDate,
				LocalProcessedDate = package.LocalProcessedDate,
				RecallDate = package.RecallDate,
				ReleaseDate = package.ReleaseDate,

				SiteId = package.SiteId,
				SiteZip = package.SiteZip,
				SiteAddressLineOne = package.SiteAddressLineOne,
				SiteCity = package.SiteCity,
				SiteState = package.SiteState,

				JobId = package.JobId,
				ContainerId = package.ContainerId,
				BinCode = package.BinCode,

				ShippingBarcode = TrackingNumberUtility.GetTrackingNumber(package),
				HumanReadableBarcode = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
				ShippingCarrier = package.ShippingCarrier,
				ShippingMethod = package.ShippingMethod,
				ServiceLevel = package.ServiceLevel,
				Zone = package.Zone,
				Weight = package.Weight,
				Length = package.Length,
				Width = package.Width,
				Depth = package.Depth,
				TotalDimensions = package.TotalDimensions,
				Shape = package.Shape,
				RequestCode = package.RequestCode,
				DropSiteKeyValue = package.DropSiteKeyValue,
				MailerId = package.MailerId,
				Cost = package.Cost,
				Charge = package.Charge,
				BillingWeight = package.BillingWeight,
				ExtraCost = package.ExtraCost.ToString(),
				MarkUpType = package.MarkUpType,

				IsPoBox = package.IsPoBox,
				IsRural = package.IsRural,
				IsUpsDas = package.IsUpsDas,
				IsOutside48States = package.IsOutside48States,
				IsOrmd = package.IsOrmd,
				IsDuplicate = package.IsDuplicate,
				IsSaturday = package.IsSaturday,
				IsDduScfBin = package.IsDduScfBin,
				IsSecondaryContainerCarrier = package.IsSecondaryContainerCarrier,
				IsQCRequired = package.IsQCRequired,

				AsnImportWebJobId = package.AsnImportWebJobId,
				BinGroupId = package.BinGroupId,
				BinMapGroupId = package.BinMapGroupId,
				RateId = package.RateId,
				ServiceRuleId = package.ServiceRuleId,
				ServiceRuleGroupId = package.ServiceRuleGroupId,
				ZoneMapGroupId = package.ZoneMapGroupId,
				FortyEightStatesGroupId = package.ServiceRuleExtensionGroupId,
				UpsGeoDescriptorGroupId = package.UpsGeoDescriptorGroupId,

				RecipientName = package.RecipientName,
				AddressLine1 = package.AddressLine1,
				AddressLine2 = package.AddressLine2,
				AddressLine3 = package.AddressLine3,
				City = package.City,
				State = package.State,
				Zip = package.Zip,
				FullZip = package.FullZip,
				Phone = package.Phone,

				ReturnName = package.ReturnName,
				ReturnAddressLine1 = package.ReturnAddressLine1,
				ReturnAddressLine2 = package.ReturnAddressLine2,
				ReturnCity = package.ReturnCity,
				ReturnState = package.ReturnState,
				ReturnZip = package.ReturnZip,
				ReturnPhone = package.ReturnPhone,

				ZipOverrides = string.Join(",", package.ZipOverrides),
				ZipOverrideGroupIds = string.Join(",", package.ZipOverrideGroupIds),
				DuplicatePackageIds = string.Join(",", package.DuplicatePackageIds),

				ClientShipDate = package.ClientShipDate,
				VisnSiteParent = PackageIdUtility.GetVisnSiteParentId(package.ClientName,package.PackageId)				
			};
			// For consistency with what we were doing before.
			if (package.ShippingCarrier == ShippingCarrierConstants.FedEx &&
				StringHelper.Exists(package.AdditionalShippingData?.HumanReadableTrackingNumber))
			{
				dataset.HumanReadableBarcode = package.AdditionalShippingData.HumanReadableTrackingNumber;
			}

			if (package.PackageStatus != EventConstants.Processed)
			{
				dataset.ShippingBarcode = null; // So doesn't match tracking data
				dataset.HumanReadableBarcode = null;
			}

			foreach (var packageEvent in package.PackageEvents)
			{
				if(packageEvent.EventStatus == EventConstants.Processed 
					&& (packageEvent.EventType == EventConstants.AutoScan || packageEvent.EventType == EventConstants.ManualScan 
						|| packageEvent.EventType == EventConstants.RepeatScan))
                {					
					dataset.ProcessedEventType = packageEvent.EventType;
					dataset.ProcessedMachineId = packageEvent.MachineId;
					dataset.ProcessedUsername = packageEvent.Username;
                }

				dataset.PackageEvents.Add(new PackageEventDataset
				{
					CosmosId = package.Id,
					CosmosCreateDate = package.CreateDate,

					PackageId = package.PackageId,
					SiteName = package.SiteName,

					TrackingNumber = packageEvent.TrackingNumber,
					EventId = packageEvent.EventId,
					EventType = packageEvent.EventType,
					EventStatus = packageEvent.EventStatus,
					Description = StringHelper.Truncate(packageEvent.Description, 180),
					EventDate = packageEvent.EventDate,
					LocalEventDate = packageEvent.LocalEventDate,

					Username = packageEvent.Username,
					MachineId = packageEvent.MachineId
				});
			}
			return dataset;
		}

		public async Task<ReportResponse> MonitorEodPackages(Site site, string userName, DateTime processedDate)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var now = DateTime.Now;
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var eodPackagesAll = await packageRepository.GetPackagesEodOverview(site.SiteName, processedDate);
				int chunk = 1000;
				for (int offset = 0; offset < eodPackagesAll.Count(); offset += chunk)
				{
					var eodPackages = eodPackagesAll.Skip(offset).Take(chunk);
					var packageDatasets = await packageDatasetRepository
						.GetDatasetsByCosmosIdNumberAsync(eodPackages.Select(p => new PackageDataset() { CosmosId = p.Id }).ToList());
					foreach (var package in eodPackages)
					{
						var packageDataset = packageDatasets.FirstOrDefault(p => p.CosmosId == package.Id);
						if (packageDataset != null)
						{
							if (siteLocalTime.Date == packageDataset.LocalProcessedDate.Date)
								packageDataset.ShippedDate = siteLocalTime;
							else
								packageDataset.ShippedDate = packageDataset.LocalProcessedDate.Date.AddHours(20); // 8PM local time on shipped date
							packageDataset.PackageEvents.Add(
								new PackageEventDataset
								{
									CosmosId = package.Id,
									CosmosCreateDate = package.CreateDate,

									PackageId = package.PackageId,
									PackageDatasetId = packageDataset.Id,
									SiteName = package.SiteName,

									EventId = 999, // Set so it won't conflict any actual package events.
									EventType = EventConstants.Shipped,
									EventStatus = EventConstants.Processed,
									Description = "Shipped",
									EventDate = now,
									LocalEventDate = packageDataset.ShippedDate.Value,
									Username = userName,
									MachineId = "System"
								});
						}
					}
					if (packageDatasets.Any())
						await BulkInsertOrUpdateAsync(site, response, packageDatasets.ToList());
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to monitor Eod packages for site: { site.SiteName }. Exception: { ex }");
			}
			return response;
		}

		public async Task<IList<PackageDataset>> GetPackagesWithNoSTC(Site site, int lookbackMin, int lookbackMax)
		{
			IList<PackageDataset> packages = new List<PackageDataset>();
			try
            {
				packages = await packageDatasetRepository.GetDatasetsWithNoStopTheClockScans(site, lookbackMin, lookbackMax);
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to return packages without STC scans. Exception: { ex }");
			}
			return packages;
		}

		private async Task BulkInsertOrUpdateAsync(Site site, ReportResponse response, List<PackageEventDataset> packageEventDatasets)
		{
			response.NumberOfDocuments += packageEventDatasets.Count();
			var bulkInsert = await packageEventDatasetRepository.ExecuteBulkInsertOrUpdateAsync(packageEventDatasets);
			if (!bulkInsert)
			{
				response.NumberOfFailedDocuments += packageEventDatasets.Count();
				response.IsSuccessful = false;
			}
		}
	}
}
