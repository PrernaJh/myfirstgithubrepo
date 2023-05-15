using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Processors
{
	public class ExpenseProcessor : IExpenseProcessor
	{
		private readonly ILogger<ExpenseProcessor> logger;
		private readonly IEodPackageRepository eodPackageRepository;
		private readonly IEodContainerRepository eodContainerRepository;

		public ExpenseProcessor(
            IEodPackageRepository eodPackageRepository,
			IEodContainerRepository eodContainerRepository,
			ILogger<ExpenseProcessor> logger)
		{
			this.logger = logger;
			this.eodPackageRepository = eodPackageRepository;
			this.eodContainerRepository = eodContainerRepository;
		}

		public async Task<FileExportResponse> GenerateExpenseFile(SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId)
		{
			var response = new FileExportResponse();
			var totalWatch = Stopwatch.StartNew();
			var dbWriteWatch = new Stopwatch();

			var dbReadWatch = Stopwatch.StartNew();
			var eodPackageExpenses = new Dictionary<DateTime, List<EodPackage>>();
			for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
			{
				eodPackageExpenses[dateToProcess] = new List<EodPackage>();
				eodPackageExpenses[dateToProcess].AddRange(await eodPackageRepository.GetExpenseRecords(subClient.SiteName, dateToProcess, subClient.Name));
			}

			var eodContainerExpenses = new Dictionary<DateTime, List<EodContainer>>();
			if (subClient.ClientName == ClientSubClientConstants.CmopClientName)
			{
				for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
				{
					eodContainerExpenses[dateToProcess] = new List<EodContainer>();
					eodContainerExpenses[dateToProcess].AddRange(await eodContainerRepository.GetExpenseRecords(subClient.SiteName, dateToProcess));
				}
			}
			dbReadWatch.Stop();

			if (eodPackageExpenses.Any() || eodContainerExpenses.Any())
			{
				for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
				{
					if (eodPackageExpenses.ContainsKey(dateToProcess))
					{
						logger.LogInformation($"Processing {eodPackageExpenses[dateToProcess].Count()} Package Expense records for subClient: {subClient.Name}, date: {dateToProcess}");
						ProcessSinglePackagesIntoGroupedRecords(eodPackageExpenses[dateToProcess], dateToProcess, subClient, webJobId, response);
					}
					if (eodContainerExpenses.ContainsKey(dateToProcess))
					{
						logger.LogInformation($"Processing {eodContainerExpenses[dateToProcess].Count()} Container Expense records for site: {subClient.SiteName}, date: {dateToProcess}");
						ProcessSingleContainersIntoGroupedRecords(eodContainerExpenses[dateToProcess], dateToProcess, subClient.SiteName, webJobId, response);
					}
				}
				response.IsSuccessful = true;
			}
			else
			{
				logger.LogInformation($"No Expense records found for subClient: {subClient.Name}");
				response.IsSuccessful = true;
			}

			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

			return response;
		}
	
		public ExpenseRecord CreatePackageExpenseRecord(Package package)
		{
			var record = new ExpenseRecord
			{
				CosmosId = package.Id,
				SubClientName = package.SubClientName,
				ProcessingDate = package.LocalProcessedDate.ToString("MM/dd/yyyy"),
				BillingReference1 = PackageIdUtility.GetPaddedVisnSiteParentId(package.ClientName, package.PackageId, 5),
				Product = package.ShippingMethod,
				TrackingType = package.ShippingMethod,
				Cost = package.Cost,
				ExtraServiceCost = package.ExtraCost,
				Weight = package.Weight,
				Zone = package.Zone,
			};

			return record;
		}

		public ExpenseRecord CreateContainerExpenseRecord(ShippingContainer container)
		{
			decimal.TryParse(container.Weight, out var weight);

			var record = new ExpenseRecord
			{
				CosmosId = container.Id,
				SubClientName = container.SiteName,
				ProcessingDate = container.LocalProcessedDate.ToString("MM/dd/yyyy"),
				BillingReference1 = container.LocalProcessedDate.ToString("MM/dd/yyyy"),
				Product = container.ShippingMethod,
				TrackingType = container.ShippingMethod,
				Cost = container.Cost,
				ExtraServiceCost = container.Cost,
				Weight = weight,
				Zone = container.Zone
			};

			return record;
		}

		public string[] BuildExpenseHeader()
		{
			return new string[]
			{
				"SUBCLIENT",
				"PROCESSING_DATE",
				"BILLING_REFERENCE1",
				"PRODUCT",
				"TRACKING_TYPE",
				"PIECES",
				"WEIGHT",
				"EXTRA_SERVICE_COST",
				"TOTAL_COST",
				"AVERAGE_WEIGHT",
				"AVERAGE_ZONE"
			};
		}

		public eDataTypes[] GetExcelDataTypes()
		{
			return new eDataTypes[]
			{
				eDataTypes.String, // SUBCLIENT
				eDataTypes.String, // PROCESSING_DATE
				eDataTypes.String, // BILLING_REFERENCE1
				eDataTypes.String, // PRODUCT
				eDataTypes.String, // TRACKING_TYPE
				eDataTypes.Number, // PIECES
				eDataTypes.Number, // WEIGHT
				eDataTypes.Number, // EXTRA_SERVICE_COST
				eDataTypes.Number, // TOTAL_COST
				eDataTypes.Number, // AVERAGE_WEIGHT
				eDataTypes.String // AVERAGE_ZONE
			};
		}


		public static void ProcessSinglePackagesIntoGroupedRecords(IEnumerable<EodPackage> eodPackageExpenses, DateTime dateToProcess, SubClient subClient, string webJobId, FileExportResponse response)
		{
			foreach (var eodGroupByPackageIdFirst5 in eodPackageExpenses
					.GroupBy(x => PackageIdUtility.GetPaddedVisnSiteParentId(subClient.ClientName, x.PackageId, 5)))
			{
				foreach (var eodGroupByFirst5AndProduct in eodGroupByPackageIdFirst5.GroupBy(x => x.ExpenseRecord.Product))
				{
					var weightsForAverage = new List<decimal>();
					var zonesForAverage = new List<int>();
					var sumWeight = 0m;
					var sumTotalCost = 0m;
					var sumExtraServiceCost = 0m;

					var groupedExpense = new GroupedExpenseRecord
					{
						SubClientName = subClient.Name,
						ProcessingDate = dateToProcess.ToString("MM/dd/yyyy"),
						BillingReference1 = eodGroupByPackageIdFirst5.Key,
						Product = eodGroupByFirst5AndProduct.Key,
						TrackingType = eodGroupByFirst5AndProduct.Key,
					};

					foreach (var eodPackage in eodGroupByFirst5AndProduct)
					{
						groupedExpense.Pieces += 1;
						sumWeight += eodPackage.ExpenseRecord.Weight;
						sumTotalCost += eodPackage.ExpenseRecord.Cost;
						weightsForAverage.Add(eodPackage.ExpenseRecord.Weight);
						zonesForAverage.Add(eodPackage.ExpenseRecord.Zone);
						response.NumberOfRecords += 1;
					}

					groupedExpense.SumWeight = sumWeight.ToString();
					groupedExpense.SumTotalCost = sumTotalCost.ToString();
					groupedExpense.SumExtraServiceCost = sumExtraServiceCost.ToString(); // always 0.0 for packages
					groupedExpense.AverageWeight = decimal.Round(weightsForAverage.Average(), 2).ToString();
					groupedExpense.AverageZone = Math.Round(zonesForAverage.Average()).ToString();

					response.FileContents.Add(BuildRecordString(groupedExpense));
				}
			}
		}

		public static void ProcessSingleContainersIntoGroupedRecords(IEnumerable<EodContainer> eodContainerExpenses, DateTime dateToProcess, string siteName, string webJobId, FileExportResponse response)
		{
			foreach (var eodGroupByProduct in eodContainerExpenses.GroupBy(x => x.ExpenseRecord.Product))
			{
				var weightsForAverage = new List<decimal>();
				var zonesForAverage = new List<int>();
				var sumWeight = 0m;
				var sumExtraServiceCost = 0m;
				var sumTotalCost = 0m;

				var groupedExpense = new GroupedExpenseRecord
				{
					SubClientName = siteName,
					ProcessingDate = dateToProcess.ToString("MM/dd/yyyy"),
					BillingReference1 = eodGroupByProduct.Key,
					Product = eodGroupByProduct.Key,
					TrackingType = eodGroupByProduct.Key,
				};

				foreach (var eodContainer in eodGroupByProduct)
				{
					groupedExpense.Pieces += 1;
					sumWeight += eodContainer.ExpenseRecord.Weight;
					sumTotalCost += eodContainer.ExpenseRecord.Cost;
					sumExtraServiceCost += eodContainer.ExpenseRecord.Cost;
					weightsForAverage.Add(eodContainer.ExpenseRecord.Weight);
					zonesForAverage.Add(eodContainer.ExpenseRecord.Zone);
					response.NumberOfRecords += 1;
				}

				groupedExpense.SumWeight = sumWeight.ToString();
				groupedExpense.SumTotalCost = sumTotalCost.ToString();
				groupedExpense.SumExtraServiceCost = sumExtraServiceCost.ToString();
				groupedExpense.AverageWeight = decimal.Round(weightsForAverage.Average(), 2).ToString();
				groupedExpense.AverageZone = Math.Round(zonesForAverage.Average()).ToString();

				response.FileContents.Add(BuildRecordString(groupedExpense));
			}
		}

		private static string BuildRecordString(GroupedExpenseRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.SubClientName);
			recordBuilder.Append(delimiter + record.ProcessingDate);
			recordBuilder.Append(delimiter + record.BillingReference1);
			recordBuilder.Append(delimiter + record.Product);
			recordBuilder.Append(delimiter + record.TrackingType);
			recordBuilder.Append(delimiter + record.Pieces);
			recordBuilder.Append(delimiter + record.SumWeight);
			recordBuilder.Append(delimiter + record.SumExtraServiceCost);
			recordBuilder.Append(delimiter + record.SumTotalCost);
			recordBuilder.Append(delimiter + record.AverageWeight);
			recordBuilder.Append(delimiter + record.AverageZone);
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}
	}
}
