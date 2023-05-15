using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;

namespace PackageTracker.EodService.Processors
{
	public class ContainerDetailProcessor : IContainerDetailProcessor
	{
		private readonly IEodContainerRepository eodContainerRepository;
		private readonly ILogger<ContainerDetailProcessor> logger;

		public ContainerDetailProcessor(
			IEodContainerRepository eodContainerRepository,
			ILogger<ContainerDetailProcessor> logger)
		
		{
			this.eodContainerRepository = eodContainerRepository;
			this.logger = logger;
		}

		public async Task<FileExportResponse> ExportContainerDetailFile(Site site, DateTime dateToProcess, string webJobId)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbReadWatch = Stopwatch.StartNew();
			var dbWriteWatch = new Stopwatch();

			var eodContainerDetails = await eodContainerRepository.GetContainerDetails(site.SiteName, dateToProcess);
			dbReadWatch.Stop();

			if (eodContainerDetails.Any())
			{
				logger.LogInformation($"Processing {eodContainerDetails.Count()} Container Detail records for site: {site.SiteName}, date: {dateToProcess}");

				foreach (var eodContainer in eodContainerDetails.Where(x=>x.ContainerDetailRecord != null))
				{
					response.FileContents.Add(BuildRecordString(eodContainer.ContainerDetailRecord));
					response.NumberOfRecords += 1;
				}
				response.IsSuccessful = true;
			}
			else
			{
				logger.LogInformation($"No Container Detail records found for site: {site.SiteName}, date: {dateToProcess}");
				response.IsSuccessful = true;
			}

			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}

		public async Task<FileExportResponse> ExportPmodContainerDetailFile(Site site, DateTime dateToProcess, string webJobId)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbReadWatch = Stopwatch.StartNew();
			var dbWriteWatch = new Stopwatch();

			var eodPmodContainerDetails = await eodContainerRepository.GetPmodContainerDetails(site.SiteName, dateToProcess);
			dbReadWatch.Stop();

			if (eodPmodContainerDetails.Any())
			{
				logger.LogInformation($"Processing {eodPmodContainerDetails.Count()} PMOD Container Detail records for site: {site.SiteName}, date: {dateToProcess}");

				foreach (var eodPmodContainer in eodPmodContainerDetails.Where(x => x.PmodContainerDetailRecord != null))
				{
					response.FileContents.Add(BuildPmodRecordString(eodPmodContainer.PmodContainerDetailRecord));
					response.NumberOfRecords += 1;
				}
				response.IsSuccessful = true;
			}
			else
			{
				logger.LogInformation($"No PMOD Container Detail records found for site: {site.SiteName}, date: {dateToProcess}");
				response.IsSuccessful = true;
			}

			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}

		public ContainerDetailRecord CreateContainerDetailRecord(Site site, Bin bin, ShippingContainer container)
		{
			var siteCustomData = site.SiteCustomData.FirstOrDefault(x => x.CustomDataType == SiteConstants.MercuryGateCustomDataType); // Custom data for Mercury Gate
			var customDataArray = siteCustomData.CustomData.Split(',');
			var isPrimary = !container.IsSecondaryCarrier;
			var isSaturdayDelivery = container.SiteCreateDate.AddDays(1).DayOfWeek == DayOfWeek.Saturday;
			var csvToParse = isPrimary ? bin.DropShipSiteCszPrimary : bin.DropShipSiteCszSecondary;
			var parsedCsv = AddressUtility.ParseCityStateZip(csvToParse);
			var trackingNumber = string.Empty;

			trackingNumber = container.CarrierBarcode;

			return new ContainerDetailRecord
			{
				CosmosId = container.Id,
				TrackingNumber = trackingNumber,
				ShipmentType = MapContainerType(container),
				PickupDate = container.LocalProcessedDate.ToString("yyyyMMdd"),
				ShipReferenceNumber = string.Empty,
				ShipperAccount = site.ShipperAccount,
				DestinationName = isPrimary ? bin.DropShipSiteDescriptionPrimary : bin.DropShipSiteDescriptionSecondary,
				DestinationAddress1 = isPrimary ? bin.DropShipSiteAddressPrimary : bin.DropShipSiteAddressSecondary,
				DestinationAddress2 = string.Empty,
				DestinationCity = parsedCsv.City,
				DestinationState = parsedCsv.State,
				DestinationZip = AddressUtility.TrimZipToFirstFive(parsedCsv.FullZip),
				DropSiteKey = isPrimary ? bin.DropShipSiteKeyPrimary : bin.DropShipSiteKeySecondary,
				OriginName = customDataArray[0],
				OriginAddress1 = customDataArray[1],
				OriginAddress2 = customDataArray[2],
				OriginCity = customDataArray[3],
				OriginState = customDataArray[4],
				OriginZip = customDataArray[5],
				Reference1 = isPrimary ? bin.ScacPrimary : bin.ScacSecondary,
				Reference2 = string.Empty,
				Reference3 = string.Empty,
				CarrierRoute1 = isPrimary ? bin.RegionalCarrierHubPrimary : bin.RegionalCarrierHubSecondary,
				CarrierRoute2 = string.Empty,
				CarrierRoute3 = string.Empty,
				Weight = container.Weight ?? "0",
				DeliveryDate = container.LocalProcessedDate.AddDays(1).ToString("yyyyMMdd"),
				ExtraSvcs1 = "9002",
				ExtraSvcs2 = isSaturdayDelivery ? "9001" : string.Empty,
				ExtraSvcs3 = string.Empty
			};
		}

		public PmodContainerDetailRecord CreatePmodContainerDetailRecord(ShippingContainer container)
		{
			var binCode = container.IsSecondaryCarrier ? container.BinCodeSecondary : container.BinCode;

			return new PmodContainerDetailRecord
			{
				CosmosId = container.Id,
				Site = container.SiteName,
				PdCust = "CMOP",
				PdShipDate = container.LocalProcessedDate.ToString("MM/dd/yyyy"),
				PdVamcId = string.Empty,
				ContainerId = container.ContainerId,
				PdTrackingNum = container.CarrierBarcode,
				PdShipMethod = container.ShippingMethod,
				PdBillMethod = container.ShippingMethod,
				PdEntryUnitType = IsDduOrScf(binCode),
				PdShipCost = container.Cost.ToString(),
				PdBillingCost = container.Charge.ToString(),
				PdSigCost = "0",
				PdShipZone = container.Zone.ToString(),
				PdZip5 = AddressUtility.TrimZipToFirstFive(AddressUtility.ParseCityStateZip(container.DropShipSiteCsz).FullZip),
				PdWeight = container.Weight,
				PdBillingWeight = StringHelper.Exists(container.BillingWeight) ? container.BillingWeight : "0",
				PdSortCode = binCode,
				PdMarkupReason = string.Empty
			};
		}

		public static string BuildRecordString(ContainerDetailRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.TrackingNumber ?? string.Empty);
			recordBuilder.Append(delimiter + record.ShipmentType ?? string.Empty);
			recordBuilder.Append(delimiter + record.PickupDate ?? string.Empty);
			recordBuilder.Append(delimiter + record.ShipReferenceNumber ?? string.Empty);
			recordBuilder.Append(delimiter + record.ShipperAccount ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationName ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationAddress1 ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationAddress2 ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationCity ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationState ?? string.Empty);
			recordBuilder.Append(delimiter + record.DestinationZip ?? string.Empty);
			recordBuilder.Append(delimiter + record.DropSiteKey ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginName ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginAddress1 ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginAddress2 ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginCity ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginState ?? string.Empty);
			recordBuilder.Append(delimiter + record.OriginZip ?? string.Empty);
			recordBuilder.Append(delimiter + record.Reference1 ?? string.Empty);
			recordBuilder.Append(delimiter + record.Reference2 ?? string.Empty);
			recordBuilder.Append(delimiter + record.Reference3 ?? string.Empty);
			recordBuilder.Append(delimiter + record.CarrierRoute1 ?? string.Empty);
			recordBuilder.Append(delimiter + record.CarrierRoute2 ?? string.Empty);
			recordBuilder.Append(delimiter + record.CarrierRoute3 ?? string.Empty);
			recordBuilder.Append(delimiter + record.Weight ?? string.Empty);
			recordBuilder.Append(delimiter + record.DeliveryDate ?? string.Empty);
			recordBuilder.Append(delimiter + record.ExtraSvcs1 ?? string.Empty);
			recordBuilder.Append(delimiter + record.ExtraSvcs2 ?? string.Empty);
			recordBuilder.Append(delimiter + record.ExtraSvcs3 ?? string.Empty);
			recordBuilder.AppendLine();
			//recordBuilder.Append("\n") //  -- append only "\n" rather than "\r\n"

			return recordBuilder.ToString();
		}

		public static string BuildPmodRecordString(PmodContainerDetailRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.Site ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdCust ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdShipDate ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdVamcId ?? string.Empty);
			recordBuilder.Append(delimiter + record.ContainerId ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdTrackingNum ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdShipMethod ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdBillMethod ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdEntryUnitType ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdShipCost ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdBillingCost ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdSigCost ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdShipZone ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdZip5 ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdWeight ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdBillingWeight ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdSortCode ?? string.Empty);
			recordBuilder.Append(delimiter + record.PdMarkupReason ?? string.Empty);
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}

		private static string MapContainerType(ShippingContainer container)
		{
			var response = string.Empty;

			if (container.ContainerType == ContainerConstants.ContainerTypeBag)
			{
				response = ContainerConstants.BagShipmentTypeId;
			}
			else if (container.ContainerType == ContainerConstants.ContainerTypePallet)
			{
				response = ContainerConstants.PalletShipmentTypeId;
			}

			return response;
		}

		private static string IsDduOrScf(string binCode)
		{
			var response = string.Empty;

			if (binCode.Substring(0, 1) == "D")
			{
				response = "DDU";
			}
			else if (binCode.Substring(0, 1) == "S")
			{
				response = "SCF";
			}
			return response;
		}
	}
}
