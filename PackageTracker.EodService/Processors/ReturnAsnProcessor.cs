using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PackageTracker.Data.Constants.ServiceLevelConstants;
using IReturnAsnProcessor = PackageTracker.EodService.Interfaces.IReturnAsnProcessor;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Processors
{
	public class ReturnAsnProcessor : IReturnAsnProcessor
	{
		private readonly IEodPackageRepository eodPackageRepository;
		private readonly ILogger<ReturnAsnProcessor> logger;

		public ReturnAsnProcessor(
			IEodPackageRepository eodPackageRepository,
			ILogger<ReturnAsnProcessor> logger)
		{
			this.eodPackageRepository = eodPackageRepository;
			this.logger = logger;
		}

		public async Task<FileExportResponse> GenerateReturnAsnFile(
			Site site, SubClient subClient, DateTime dateToProcess, string webJobId, bool isSimplified, bool addPackageRatingElements)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbWriteWatch = new Stopwatch();
			var dbReadWatch = Stopwatch.StartNew();
			
			var eodPackageReturnAsns = await eodPackageRepository.GetReturnAsns(site.SiteName, dateToProcess, subClient.Name);
			dbReadWatch.Stop();
			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);

			if (eodPackageReturnAsns.Any())
			{
				logger.LogInformation($"Processing {eodPackageReturnAsns.Count()} Return ASN records for subClient: {subClient.Name}, date: {dateToProcess.Date}");
				foreach (var eodPackage in eodPackageReturnAsns)
				{
					if (isSimplified)
					{
						response.FileContents.Add(BuildSimplifiedRecordString(eodPackage.ReturnAsnRecord));
					}
					else
					{
						response.FileContents.Add(BuildRecordString(eodPackage.ReturnAsnRecord, addPackageRatingElements));
					}
					response.NumberOfRecords += 1;
				}
				response.IsSuccessful = true;
			}
			else
			{
				logger.LogInformation($"No Return ASN records found for subClient: {subClient.Name}, date: {dateToProcess.Date}");
				response.IsSuccessful = true;
			}

			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}

		public ReturnAsnRecord CreateReturnAsnRecord(Package package)
		{
			return new ReturnAsnRecord
			{
				CosmosId = package.Id,
				ParcelId = package.PackageId ?? string.Empty,
				SiteCode = PackageIdUtility.GetVisnSiteParentId(package.ClientName, package.PackageId).ToString(),
				PackageWeight = package.Weight.ToString(),
				ProductCode = GetProductCode(package),
				Over84Flag = "",
				Over108Flag = "",
				NonMachinableFlag = "N",
				DelCon = package.ServiceLevel == DeliveryConfirmation ? "Y" : "N",
				Signature = package.ServiceLevel == Signature ? "Y" : "N",
				CustomerNumber = package.SubClientKey,
				BolNumber = package.BillOfLading ?? string.Empty,
				PackageCreateDateDayMonthYear = package.LocalProcessedDate.ToString("MMddyyyy") ?? string.Empty,
				PackageCreateDateHourMinuteSecond = package.LocalProcessedDate.ToString("HHmmss") ?? string.Empty,
				ZipDestination = package.Zip ?? string.Empty,
				PackageBarcode = TrackingNumberUtility.GetHumanReadableTrackingNumber(package),
				Zone = package.Zone.ToString(),
				TotalShippingCharge = (package.Charge + package.ExtraCharge).ToString("F2"),
				ConfirmationSurcharge = string.Empty,
				NonMachinableSurcharge = string.Empty,
				IsOutside48States = package.IsOutside48States ? "Y" : "N",
				IsRural = package.IsRural ? "Y" : "N",
				MarkupType = package.MarkUpType,
			};
		}

		public ReturnAsnRecord CreateReturnAsnShortRecord(Package package)
		{
			return new ReturnAsnRecord
			{
				CosmosId = package.Id,
				ParcelId = package.PackageId ?? string.Empty,
				PackageBarcode = package.Barcode ?? string.Empty
			};
		}

		private static string GetProductCode(Package package)
		{
			var response = string.Empty;

			if (package.ShippingCarrier == ShippingCarrierConstants.Usps)
			{
				response = package.ShippingMethod switch
				{
					ShippingMethodConstants.UspsParcelSelectLightWeight => "10",
					ShippingMethodConstants.UspsParcelSelect => "11",
					ShippingMethodConstants.UspsFirstClass => "80",
					ShippingMethodConstants.UspsFcz => "80",
					ShippingMethodConstants.UspsPriority => "90",
					ShippingMethodConstants.UspsPriorityExpress => string.Empty,
					_ => string.Empty
				};
			}
			else if (package.ShippingCarrier == ShippingCarrierConstants.Ups)
			{
				response = package.ShippingMethod switch
				{
					ShippingMethodConstants.UpsGround => "75",
					ShippingMethodConstants.UpsSecondDayAir => "76",
					ShippingMethodConstants.UpsNextDayAir => "78",
					ShippingMethodConstants.UpsNextDayAirSaver => "79",
					_ => string.Empty
				};
			}
			else if (package.ShippingCarrier == ShippingCarrierConstants.FedEx)
			{
				response = package.ShippingMethod switch
				{
					ShippingMethodConstants.FedExPriorityOvernight => "61",
					ShippingMethodConstants.FedExGround => string.Empty,
					_ => string.Empty
				};
			}

			return response;
		}

		public static string BuildRecordString(ReturnAsnRecord record, bool addPackageRatingElements)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.ParcelId);
			recordBuilder.Append(delimiter + record.SiteCode);
			recordBuilder.Append(delimiter + record.PackageWeight);
			recordBuilder.Append(delimiter + record.ProductCode);
			recordBuilder.Append(delimiter + record.Over84Flag);
			recordBuilder.Append(delimiter + record.Over108Flag);
			recordBuilder.Append(delimiter + record.NonMachinableFlag);
			recordBuilder.Append(delimiter + record.DelCon);
			recordBuilder.Append(delimiter + record.Signature);
			recordBuilder.Append(delimiter + record.CustomerNumber);
			recordBuilder.Append(delimiter + record.BolNumber);
			recordBuilder.Append(delimiter + record.PackageCreateDateDayMonthYear);
			recordBuilder.Append(delimiter + record.PackageCreateDateHourMinuteSecond);
			recordBuilder.Append(delimiter + record.ZipDestination);
			recordBuilder.Append(delimiter + record.PackageBarcode);
			recordBuilder.Append(delimiter + record.Zone);
			recordBuilder.Append(delimiter + record.TotalShippingCharge);
			recordBuilder.Append(delimiter + record.ConfirmationSurcharge);
			recordBuilder.Append(delimiter + record.NonMachinableSurcharge);
			if (addPackageRatingElements)
            {
				recordBuilder.Append(delimiter + record.IsOutside48States);
				recordBuilder.Append(delimiter + record.IsRural);
				recordBuilder.Append(delimiter + record.MarkupType);
            }
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}

		public static string BuildSimplifiedRecordString(ReturnAsnRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.ParcelId);
			recordBuilder.Append(delimiter + record.PackageBarcode);
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}
	}


}
