using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IInvoiceProcessor = PackageTracker.EodService.Interfaces.IInvoiceProcessor;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Processors
{
	public class InvoiceProcessor : IInvoiceProcessor
	{
		private readonly ILogger<InvoiceProcessor> logger;
		private readonly IEodPackageRepository eodPackageRepository;

		public InvoiceProcessor(
			IEodPackageRepository eodPackageRepository,
			ILogger<InvoiceProcessor> logger)
		{
			this.logger = logger;
			this.eodPackageRepository = eodPackageRepository;
		}

		public async Task<FileExportResponse> GenerateInvoiceFile(
			SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId, bool addPackageRatingElements)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbWriteWatch = new Stopwatch();

			var dbReadWatch = Stopwatch.StartNew();
			var eodInvoices = new List<EodPackage>();
			for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
			{
				var eodInvoicesForDate = await eodPackageRepository.GetInvoiceRecords(subClient.SiteName, dateToProcess, subClient.Name);
				logger.LogInformation($"Processing {eodInvoicesForDate.Count()} Invoice records for subClient: {subClient.Name}, date: {dateToProcess}");
				eodInvoices.AddRange(eodInvoicesForDate);
			}
			dbReadWatch.Stop();
			logger.LogInformation($"Processing {eodInvoices.Count()} Total Invoice records for subClient: {subClient.Name}");

			if (eodInvoices.Any())
			{
				foreach (var eodPackage in eodInvoices)
				{
					eodPackage.InvoiceRecord.SubClientName = subClient.Name; // This was missing in old data.
					response.FileContents.Add(BuildRecordString(eodPackage.InvoiceRecord, addPackageRatingElements));
					response.NumberOfRecords += 1;
				}
				response.IsSuccessful = true;
			}
			else
			{
				logger.LogInformation($"No Invoice records found for subClient: {subClient.Name}, date range: {firstDateToProcess}-{lastDateToProcess}");
				response.IsSuccessful = true;
			}

			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}

		public InvoiceRecord CreateInvoiceRecord(Package package)
		{
			var record = new InvoiceRecord
			{
				CosmosId = package.Id,
				SubClientName = package.SubClientName,
				BillingDate = package.LocalProcessedDate.ToString("MM/dd/yyyy"),
				PackageId = package.PackageId,
				TrackingNumber = package.Barcode,
				BillingReference1 = PackageIdUtility.GetPaddedVisnSiteParentId(package.ClientName, package.PackageId, 5),
				BillingProduct = package.ShippingMethod,
				BillingWeight = package.BillingWeight.ToString(),
				Zone = package.Zone.ToString(),
				SigCost = package.ExtraCost.ToString(),
				BillingCost = package.Charge.ToString(),
				Weight = package.Weight.ToString(),
				TotalCost = (package.ExtraCost + package.Charge).ToString().ToUpper(),
				IsOutside48States = package.IsOutside48States.ToString().ToUpper(),
				IsRural = package.IsRural.ToString(),
				MarkupType = package.MarkUpType,
			};

			return record;
		}

		public string[] BuildInvoiceHeader(bool addPackageRatingElements)
		{
			var header = new List<string>();
			header.AddRange(new string [] {
				"SUBCLIENT",
				"BILLING_DATE",
				"PACKAGE_ID",
				"TRACKINGNUMBER",
				"BILLING_REFERENCE1",
				"BILLING_PRODUCT",
				"BILLING_WEIGHT",
				"ZONE",
				"SIG_COST",
				"BILLING_COST",
				"WEIGHT",
				"TOTAL_COST",
			});
			if (addPackageRatingElements)
			{
				header.AddRange(new string[] {
					"OUTSIDE_48",
					"IS_RURAL",
					"MARKUP_TYPE"
				});
			}
			return header.ToArray();
		}

		public eDataTypes[] GetExcelDataTypes(bool addPackageRatingElements)
		{
			var dataTypes = new List<eDataTypes>();
			dataTypes.AddRange(new eDataTypes[] 
			{ 
				eDataTypes.String, // SUBCLIENT
				eDataTypes.String, // BILLING_DATE
				eDataTypes.String, // PACKAGE_ID
				eDataTypes.String, // TRACKINGNUMBER
				eDataTypes.String, // BILLING_REFERENCE1
				eDataTypes.String, // BILLING_PRODUCT
				eDataTypes.Number, // BILLING_WEIGHT
				eDataTypes.String, // ZONE
				eDataTypes.Number, // SIG_COST
				eDataTypes.Number, // BILLING_COST
				eDataTypes.Number, // WEIGHT
				eDataTypes.Number, // TOTAL_COST
			});
			if (addPackageRatingElements)
			{
				dataTypes.AddRange(new eDataTypes[] 
				{
					eDataTypes.String, // OUTSIDE_48
					eDataTypes.String, // IS_RURAL
					eDataTypes.String // MARKUP_TYPE
				});
			}
			return dataTypes.ToArray();
		}

		public static string BuildRecordString(InvoiceRecord record, bool addPackageRatingElements)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.SubClientName);
			recordBuilder.Append(delimiter + record.BillingDate);
			recordBuilder.Append(delimiter + record.PackageId);
			recordBuilder.Append(delimiter + record.TrackingNumber);
			recordBuilder.Append(delimiter + record.BillingReference1);
			recordBuilder.Append(delimiter + record.BillingProduct);
			recordBuilder.Append(delimiter + record.BillingWeight);
			recordBuilder.Append(delimiter + record.Zone);
			recordBuilder.Append(delimiter + record.SigCost);
			recordBuilder.Append(delimiter + record.BillingCost);
			recordBuilder.Append(delimiter + record.Weight);
			recordBuilder.Append(delimiter + record.TotalCost);
			if (addPackageRatingElements)
            {
				recordBuilder.Append(delimiter + record.IsOutside48States);
				recordBuilder.Append(delimiter + record.IsRural);
				recordBuilder.Append(delimiter + record.MarkupType);
            }
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}
	}
}
