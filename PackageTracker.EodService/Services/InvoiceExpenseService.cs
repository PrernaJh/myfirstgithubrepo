using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IExpenseProcessor = PackageTracker.EodService.Interfaces.IExpenseProcessor;
using IInvoiceProcessor = PackageTracker.EodService.Interfaces.IInvoiceProcessor;

namespace PackageTracker.EodService.Services
{
	public class InvoiceExpenseService : IInvoiceExpenseService
	{
		private readonly IBlobHelper blobHelper;
		private readonly IConfiguration config;
		private readonly IEmailService emailService;
		private readonly IEodService eodService;
		private readonly IExpenseProcessor expenseProcessor;
		private readonly IFileShareHelper fileShareHelper;
		private readonly ILogger<InvoiceExpenseService> logger;
		private readonly IInvoiceProcessor invoiceProcessor;
		private readonly ISemaphoreManager semaphoreManager;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public InvoiceExpenseService(ILogger<InvoiceExpenseService> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IEmailService emailService,
			IEodService eodService,
			IExpenseProcessor expenseProcessor,
			IFileShareHelper fileShareHelper,
			IInvoiceProcessor invoiceProcessor,
			ISemaphoreManager semaphoreManager,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.blobHelper = blobHelper;
			this.config = config;
			this.emailService = emailService;
			this.eodService = eodService;
			this.expenseProcessor = expenseProcessor;
			this.fileShareHelper = fileShareHelper;
			this.logger = logger;
			this.invoiceProcessor = invoiceProcessor;
			this.semaphoreManager = semaphoreManager;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ProcessInvoiceFiles(WebJobSettings webJobSettings, string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Invoice incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var weekly = endOfDayQueueMessage.Extra == "WEEKLY";
			var monthly = endOfDayQueueMessage.Extra == "MONTHLY";
			var force = endOfDayQueueMessage.Extra == "FORCE";
			var addPackageRatingElements = webJobSettings.GetParameterBoolValue("AddPackageRatingElements", true);

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: Invoice File Export for site: {siteName}");

			try
			{
				if ((!weekly) && (!monthly) && (! force) &&
					await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.InvoiceExportJobType, username))
				{
					return;
				}

				var subClients = await subClientProcessor.GetSubClientsBySiteNameAsync(site.SiteName);
				foreach (var subClient in subClients)
				{
					await ExportInvoiceFile(dateToProcess, weekly, monthly, username, site, subClient, addPackageRatingElements); // e.g. TUCSON_tecadmin_2021-05-02_WEEKLY for weekly
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export Invoice File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Invoice File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}

		private async Task ExportInvoiceFile(
			DateTime dateToProcess, bool weekly, bool monthly, string username, Site site, SubClient subClient, bool addPackageRatingElements)
		{
			var firstDateToProcess = dateToProcess;
			var lastDateToProcess = dateToProcess;
			var jobType = WebJobConstants.InvoiceExportJobType;
			weekly = weekly && dateToProcess.DayOfWeek == DayOfWeek.Sunday;
			monthly = monthly && dateToProcess.Day == 1;
			if (weekly)
            {
				firstDateToProcess = dateToProcess.AddDays(-7);
				lastDateToProcess = dateToProcess.AddDays(-1);
				jobType = WebJobConstants.InvoiceWeeklyExportJobType;
			}
			else if (monthly)
            {
				firstDateToProcess = dateToProcess.AddMonths(-1);
				lastDateToProcess = dateToProcess.AddDays(-1);
				jobType = WebJobConstants.InvoiceMonthlyExportJobType;
            }

			var fileDetails = new List<FileDetail>();
			var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
			{
				Site = site,
				ProcessedDate = dateToProcess,
				SubClientName = subClient.Name,
				WebJobTypeConstant = jobType,
				JobName = "Invoice File Export",
				Message = "Invoice File Export started",
				Username = username
			});

			var response = await invoiceProcessor.GenerateInvoiceFile(
				subClient, firstDateToProcess, lastDateToProcess, webJobRun.Id, addPackageRatingElements);
			if (response.FileContents.Any())
			{
				var headers = invoiceProcessor.BuildInvoiceHeader(addPackageRatingElements);
				var dataTypes = invoiceProcessor.GetExcelDataTypes(addPackageRatingElements);
				var fileArray = response.FileContents.ToArray();
				var fileName = weekly
					? $"{subClient.Name}_INVOICE_WEEKLY_{dateToProcess:yyyyMMddhhmm}"
					: (monthly
						? $"{subClient.Name}_INVOICE_MONTHLY_{dateToProcess:yyyyMMddhhmm}"
						: $"{subClient.Name}_INVOICE_{dateToProcess:yyyyMMddhhmm}");
				var fileExportPath = $"{config.GetSection("InvoiceExpenseFileExport").Value}/{subClient.ClientName}/{subClient.SiteName}";
				var fileShareName = config.GetSection("InvoiceExpenseFileExportFileShare").Value;

				var maxRows = 1048576;  // Maximum number of rows allowed in an Excel worksheet.
                if (response.NumberOfRecords > maxRows - 1)
                {
					fileName = $"{fileName}.csv";
					var records = new List<string>();
					records.Add($"{String.Join(",", headers)}\n");
					foreach (var record in response.FileContents)
					{
						records.Add(record.Replace("|", ","));
                    }
					await blobHelper.UploadListOfStringsToBlobAsync(records, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to Container", $"{fileExportPath}/{fileName}", response)}");
					if (StringHelper.Exists(config.GetSection("FinancialExpenseInvoice").Value))
					{
						var financialExportPath = $"{config.GetSection("FinancialExpenseInvoice").Value}/{subClient.ClientName}/{subClient.SiteName}";
						await blobHelper.UploadListOfStringsToBlobAsync(records, financialExportPath, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to Container", $"{financialExportPath}/{fileName}", response)}");
					}
					if (StringHelper.Exists(fileShareName))
					{
						await fileShareHelper.UploadListOfStringsToFileShareAsync(records,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
					}
				}
				else
                {
					fileName = $"{fileName}.xlsx";
					var excel = ExcelUtility.GenerateExcel(headers, fileArray, dataTypes, fileName);
					await blobHelper.UploadExcelToBlobAsync(excel, fileExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to Container", $"{fileExportPath}/{fileName}", response)}");

					if (StringHelper.Exists(config.GetSection("FinancialExpenseInvoice").Value))
					{
						var financialExportPath = $"{config.GetSection("FinancialExpenseInvoice").Value}/{subClient.ClientName}/{subClient.SiteName}";
						await blobHelper.UploadExcelToBlobAsync(excel, financialExportPath, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to Container", $"{financialExportPath}/{fileName}", response)}");
					}
					if (StringHelper.Exists(fileShareName))
					{
						await fileShareHelper.UploadExcelToFileShare(excel,
							config.GetSection("AzureFileShareAccountName").Value,
							config.GetSection("AzureFileShareKey").Value,
							fileShareName, fileName);
						logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
					}
				}			

				fileDetails.Add(new FileDetail
				{
					FileName = response.NumberOfRecords != 0 ? fileName : string.Empty,
					FileArchiveName = response.NumberOfRecords != 0 ? fileName : string.Empty,
					NumberOfRecords = response.NumberOfRecords
				});
			}

			await webJobRunProcessor.EndWebJob(new EndWebJobRequest
			{
				WebJobRun = webJobRun,
				IsSuccessful = response.IsSuccessful,
				NumberOfRecords = response.NumberOfRecords,
				Message = "Invoice File Export complete",
				FileDetails = fileDetails
			});

			await eodService.CheckEodComplete(site, dateToProcess);
		}

		public async Task ProcessExpenseFiles(WebJobSettings webJobSettings, string message)
		{
			var endOfDayQueueMessage = QueueUtility.ParseEodProcessQueueMessage(message);
			logger.LogInformation($"EOD: Expense incoming queue message: {message}");
			var siteName = endOfDayQueueMessage.SiteName;
			var username = endOfDayQueueMessage.Username;
			var dateToProcess = endOfDayQueueMessage.TargetDate;
			var weekly = endOfDayQueueMessage.Extra == "WEEKLY";
			var monthly = endOfDayQueueMessage.Extra == "MONTHLY";
			var force = endOfDayQueueMessage.Extra == "FORCE";

			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
			var semaphore = semaphoreManager.GetSemaphore($"{site.EodGroup}_{dateToProcess.Date}");
			await semaphore.WaitAsync();
			logger.LogInformation($"EOD: Expense File Export for site: {siteName}");

			try
			{
				if ((! weekly) && (! monthly) && (! force) &&
					await eodService.IsEodBlocked(site, dateToProcess, WebJobConstants.ExpenseExportJobType, username))
				{
					return;
				}

				var subClients = await subClientProcessor.GetSubClientsBySiteNameAsync(site.SiteName);
				foreach (var subClient in subClients)
				{
					await ExportExpenseFile(dateToProcess, weekly, monthly, username, site, subClient); // e.g. TUCSON_tecadmin_2021-05-02_WEEKLY for weekly
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to export Expense File. Exception: { ex }");
				emailService.SendServiceErrorNotifications("Expense File Processor", ex.ToString());
			}
			finally
			{
				semaphore.Release();
			}
		}

		private async Task ExportExpenseFile(DateTime dateToProcess, bool weekly, bool monthly, string username, Site site, SubClient subClient)
		{
			var firstDateToProcess = dateToProcess;
			var lastDateToProcess = dateToProcess;
			var jobType = WebJobConstants.ExpenseExportJobType;
			weekly = weekly && dateToProcess.DayOfWeek == DayOfWeek.Sunday;
			monthly = monthly && dateToProcess.Day == 1;
			if (weekly)
			{
				firstDateToProcess = dateToProcess.AddDays(-7);
				lastDateToProcess = dateToProcess.AddDays(-1);
				jobType = WebJobConstants.ExpenseWeeklyExportJobType;
			}
			else if (monthly)
			{
				firstDateToProcess = dateToProcess.AddMonths(-1);
				lastDateToProcess = dateToProcess.AddDays(-1);
				jobType = WebJobConstants.ExpenseMonthlyExportJobType;
			}

			var fileDetails = new List<FileDetail>();
			var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
			{
				Site = site,
				ProcessedDate = dateToProcess,
				SubClientName = subClient.Name,
				WebJobTypeConstant = jobType,
				JobName = "Expense File Export",
				Message = "Expense File Export started",
				Username = username
			});

			var response = await expenseProcessor.GenerateExpenseFile(subClient, firstDateToProcess, lastDateToProcess, webJobRun.Id);
			if (response.FileContents.Any())
			{
				var headers = expenseProcessor.BuildExpenseHeader();
				var dataTypes = expenseProcessor.GetExcelDataTypes();
				var fileArray = response.FileContents.ToArray();
				var fileName = weekly
					? $"{subClient.Name}_EXPENSE_WEEKLY_{dateToProcess:yyyyMMddhhmm}.xlsx" 
					: (monthly 
						? $"{subClient.Name}_EXPENSE_MONTHLY_{dateToProcess:yyyyMMddhhmm}.xlsx"
						: $"{subClient.Name}_EXPENSE_{dateToProcess:yyyyMMddhhmm}.xlsx");
				var excel = ExcelUtility.GenerateExcel(headers, fileArray, dataTypes, fileName);

				var fileExportPath = $"{config.GetSection("InvoiceExpenseFileExport").Value}/{subClient.ClientName}/{subClient.SiteName}";
				await blobHelper.UploadExcelToBlobAsync(excel, fileExportPath, fileName);
				logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Expense File Export to Container", $"{fileExportPath}/{fileName}", response)}");

				if (StringHelper.Exists(config.GetSection("FinancialExpenseInvoice").Value))
				{
					var financialExportPath = $"{config.GetSection("FinancialExpenseInvoice").Value}/{subClient.ClientName}/{subClient.SiteName}";
					await blobHelper.UploadExcelToBlobAsync(excel, financialExportPath, fileName);
					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Expense File Export to Container", $"{financialExportPath}/{fileName}", response)}");
				}

				var fileShareName = config.GetSection("InvoiceExpenseFileExportFileShare").Value;
				if (StringHelper.Exists(fileShareName))
				{
					await fileShareHelper.UploadExcelToFileShare(excel,
						config.GetSection("AzureFileShareAccountName").Value,
						config.GetSection("AzureFileShareKey").Value,
						fileShareName, fileName);

					logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileExportResponse("Invoice File Export to FileShare", $"{fileShareName}/{fileName}", response)}");
				}

				fileDetails.Add(new FileDetail
				{
					FileName = response.NumberOfRecords != 0 ? fileName : string.Empty,
					FileArchiveName = response.NumberOfRecords != 0 ? fileName : string.Empty,
					NumberOfRecords = response.NumberOfRecords
				});
			}

			await webJobRunProcessor.EndWebJob(new EndWebJobRequest
			{
				WebJobRun = webJobRun,
				IsSuccessful = response.IsSuccessful,
				NumberOfRecords = response.NumberOfRecords,
				Message = "Invoice File Export complete",
				FileDetails = fileDetails
			});

			await eodService.CheckEodComplete(site, dateToProcess);
		}

		public async Task ProcessPeriodicInvoiceFiles(WebJobSettings webJobSettings)
		{
			var addPackageRatingElements = webJobSettings.GetParameterBoolValue("AddPackageRatingElements", true);
			foreach (var site in await siteProcessor.GetAllSitesAsync())
			{
				var dateToProcess = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var subClients = await subClientProcessor.GetSubClientsBySiteNameAsync(site.SiteName);
				if (dateToProcess.DayOfWeek == DayOfWeek.Sunday)
				{
					foreach (var subClient in subClients)
					{
						await ExportInvoiceFile(dateToProcess, true, false, "System", site, subClient, addPackageRatingElements);
					}
				}
				if (dateToProcess.Day == 1)
				{
					foreach (var subClient in subClients)
					{
						await ExportInvoiceFile(dateToProcess, false, true, "System", site, subClient, addPackageRatingElements);
					}
				}
			}
		}

        public async Task ProcessPeriodicExpenseFiles(WebJobSettings webJobSettings)
        {
			foreach (var site in await siteProcessor.GetAllSitesAsync())
			{
				var dateToProcess = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var subClients = await subClientProcessor.GetSubClientsBySiteNameAsync(site.SiteName);
				if (dateToProcess.DayOfWeek == DayOfWeek.Sunday)
				{
					foreach (var subClient in subClients)
					{
						await ExportExpenseFile(dateToProcess, true, false, "System", site, subClient);
					}
				}
				if (dateToProcess.Day == 1)
				{
					foreach (var subClient in subClients)
					{
						await ExportExpenseFile(dateToProcess, false, true, "System", site, subClient);
					}
				}
			}
		}
    }
}
