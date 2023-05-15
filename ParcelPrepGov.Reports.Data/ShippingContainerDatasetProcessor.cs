using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
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
	public class ShippingContainerDatasetProcessor : IShippingContainerDatasetProcessor
	{
		private readonly ILogger<ShippingContainerDatasetProcessor> logger;
		private readonly IShippingContainerDatasetRepository shippingContainerDatasetRepository;
		private readonly IDatasetProcessor datasetProcessor;

		public ShippingContainerDatasetProcessor(ILogger<ShippingContainerDatasetProcessor> logger,
			IShippingContainerDatasetRepository shippingContainerDatasetRepository,
			IDatasetProcessor datasetProcessor
			)
		{
			this.logger = logger;
			this.shippingContainerDatasetRepository = shippingContainerDatasetRepository;
			this.datasetProcessor = datasetProcessor;
		}

        public async Task<ReportResponse> UpdateShippingContainerDatasets(List<ShippingContainerDataset> items)
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
						foreach (var duplicates in datasets.GroupBy(p => p.UpdatedBarcode))
						{
							if (duplicates.Count() > 0 && StringHelper.Exists(duplicates.Key))
							{
								bool first = true;
								foreach (var item in duplicates.OrderByDescending(p => p.LocalProcessedDate))
								{
									if (!first)
									{
										item.UpdatedBarcode = null; // Can't have duplicate (real) barcode entries in DB.
									}
									first = false;
								}
							}
						}
						var existingItems = await shippingContainerDatasetRepository
							.GetDatasetsByTrackingNumberAsync(datasets.Where(c => StringHelper.Exists(c.UpdatedBarcode)).ToList());
						var itemsToUpdate = new List<ShippingContainerDataset>();
						foreach (var item in existingItems)
						{
							var container = datasets.FirstOrDefault(c => c.UpdatedBarcode == item.UpdatedBarcode);
							if (container != null && container.CosmosId != item.CosmosId)
							{
								item.UpdatedBarcode = null; // Can't have duplicate (real) barcode entries in DB.
								itemsToUpdate.Add(item);
							}
						}
						if (itemsToUpdate.Any())
							await shippingContainerDatasetRepository.ExecuteBulkUpdateAsync(itemsToUpdate);

						await BulkInsertOrUpdateAsync(site, response, datasets.ToList());
					}
					if (response.IsSuccessful)
						logger.LogInformation($"Number of shipping container datasets inserted: {response.NumberOfDocuments}");
					else
						logger.LogError($"Failed to bulk insert shipping container datasets. Total Failures: {response.NumberOfFailedDocuments}");
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert shipping container datasets. Exception: { ex }");
			}
			return response;
		}

		public async Task<ReportResponse> UpdateShippingContainerDatasets(Site site, DateTime lastScanDateTime)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var shippingContainers = await datasetProcessor.GetContainersForContainerDatasetsAsync(site.SiteName, lastScanDateTime);
				if (shippingContainers.Any())
				{
					var shippingContainerDatasets = shippingContainers.Select(c => CreateDataset(c)).ToList();
					logger.LogInformation($"Number of shipping container datasets to insert: {shippingContainerDatasets.Count()}");
					response = await UpdateShippingContainerDatasets(shippingContainerDatasets);
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert shipping container datasets for site: { site.SiteName }. Exception: { ex }");
			}
			return response;
		}

		private async Task BulkInsertOrUpdateAsync(Site site, ReportResponse response, List<ShippingContainerDataset> shippingContainerDatasets)
		{
			response.NumberOfDocuments = shippingContainerDatasets.Count();
			var bulkInsert = await shippingContainerDatasetRepository.ExecuteBulkInsertOrUpdateAsync(shippingContainerDatasets);
			if (! bulkInsert)
			{
				response.NumberOfFailedDocuments = shippingContainerDatasets.Count();
				response.IsSuccessful = false;
			}
		}

		private ShippingContainerDataset CreateDataset(ShippingContainer shippingContainer)
		{
			var dataset = new ShippingContainerDataset
			{
				CosmosId = shippingContainer.Id,
				CosmosCreateDate = shippingContainer.CreateDate,
				LocalCreateDate = shippingContainer.SiteCreateDate,

				ContainerId = shippingContainer.ContainerId,
				Status = shippingContainer.Status,
				SiteName = shippingContainer.SiteName,

				BinCode = shippingContainer.BinCode,
				BinActiveGroupId = shippingContainer.BinActiveGroupId,
				BinCodeSecondary = shippingContainer.BinCodeSecondary,
				ShippingMethod = shippingContainer.ShippingMethod,
				ShippingCarrier = shippingContainer.ShippingCarrier,
				ContainerType = shippingContainer.ContainerType,
				Grouping = shippingContainer.Grouping,
				Weight = shippingContainer.Weight,
				UpdatedBarcode = shippingContainer.CarrierBarcode,
				IsSecondaryCarrier = shippingContainer.IsSecondaryCarrier,
				IsSaturdayDelivery = shippingContainer.IsSaturdayDelivery,
				IsRural = shippingContainer.IsRural,
				IsOutside48States = shippingContainer.IsOutside48States,
				Cost = shippingContainer.Cost,
				Charge = shippingContainer.Charge,
				Zone = shippingContainer.Zone,
				ProcessedDate = shippingContainer.ProcessedDate,
				LocalProcessedDate = shippingContainer.LocalProcessedDate,

				Username = shippingContainer.Username,
				MachineId = shippingContainer.MachineId
			};
			// For FedEx, tracking number is different than the carrier barcode.
			if (StringHelper.Exists(shippingContainer.AdditionalShippingData?.TrackingNumber))
			{
				dataset.UpdatedBarcode = shippingContainer.AdditionalShippingData.TrackingNumber;
			}

			if (shippingContainer.Status != ContainerEventConstants.Closed)
			{
				dataset.UpdatedBarcode = null; // So doesn't match tracking data
			}

			foreach (var containerEvent in shippingContainer.Events)
			{
				dataset.Events.Add(new ShippingContainerEventDataset
				{
					CosmosId = shippingContainer.Id,
					CosmosCreateDate = shippingContainer.CreateDate,

					ContainerId = shippingContainer.ContainerId,
					SiteName = shippingContainer.SiteName,

					EventId = containerEvent.EventId,
					EventType = containerEvent.EventType,
					EventStatus = containerEvent.EventStatus,
					Description = StringHelper.Truncate(containerEvent.Description, 180),
					EventDate = containerEvent.EventDate,
					LocalEventDate = containerEvent.LocalEventDate,

					Username = containerEvent.Username,
					MachineId = containerEvent.MachineId
				});
			}
			return dataset;
		}

		public async Task<IList<ShippingContainerDataset>> GetShippingContainersWithNoSTC(Site site, int lookbackMin, int lookbackMax)
		{
			IList<ShippingContainerDataset> shippingContainers = new List<ShippingContainerDataset>();
			try
			{
				shippingContainers = await shippingContainerDatasetRepository.GetDatasetsWithNoStopTheClockScans(site, lookbackMin, lookbackMax);
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to return shippingContainers without STC scans. Exception: { ex }");
			}
			return shippingContainers;
		}
	}
}
