using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.ArchiveService.Interfaces;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PackageTracker.ArchiveService
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> logger;

        private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;
        private readonly IEmailService emailService;
        private readonly IReportsRepository reportsRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public ReportService(ILogger<ReportService> logger,
            IBlobHelper blobHelper,
            IConfiguration config,
            IEmailService emailService,
            IReportsRepository reportsRepository,
            ISiteProcessor siteProcessor,
            IWebJobRunProcessor webJobRunProcessor
            )
        {
            this.logger = logger;

            this.blobHelper = blobHelper;
            this.config = config;
            this.emailService = emailService;
            this.reportsRepository = reportsRepository;
            this.siteProcessor = siteProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        public async Task CreateDailyContainerPackageNestingReport(DateTime targetDate, string userName)
        {
            var webJobRun = await webJobRunProcessor.StartWebJob(new StartWebJobRequest
            {
                ProcessedDate = targetDate,
                WebJobTypeConstant = WebJobConstants.DailyContainerNestingReportGenerationJobType,
                JobName = "Daily Container Nesting Report Generation",
                Message = "Daily Container Nesting Report Generation started",
                Username = userName
            });
            var fileDetails = new List<FileDetail>();
            var numberOfRecords = 0;
            var message = string.Empty;
            var isSuccessful = true;
            try
            {
                var sites = await siteProcessor.GetAllSitesAsync();
                var records = new List<BasicContainerPackageNesting>();
                foreach (var site in sites)
                {
                    records.AddRange(
                        await reportsRepository.GetBasicContainerPackageNesting(site.SiteName, targetDate.ToString("yyyy-MM-dd")));
                }
                if (records.Count > 0)
                {
                    var strings = new List<string>();
                    strings.Add(FormatNestingHeaderRecord<BasicContainerPackageNesting>());
                    records.ForEach(r => strings.Add(FormatNestingRecord(r)));
                    var fileExportPath = config.GetSection("DailyContainerReportExport").Value;
                    var fileName = $"{targetDate.ToString("yyyyMMdd")}_BasicContainerPackageNestingReport.txt";
                    await blobHelper.UploadListOfStringsToBlobAsync(strings, fileExportPath, fileName);
                    logger.Log(LogLevel.Information, $"Daily Container Nesting Report Exported to Container", $"{fileExportPath}/{fileName}");
                    numberOfRecords = records.Count;
                    fileDetails.Add(new FileDetail
                    {
                        FileName = fileName,
                        FileArchiveName = $"{fileExportPath}/{fileName}",
                        NumberOfRecords = numberOfRecords
                    });
                }
                message = "Daily Container Nesting Report Generation completed";
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to Generate Daily Container Nesting Report, Manifest Date: {targetDate}. Exception: { ex }");
                emailService.SendServiceErrorNotifications("Report Service: Failed to Generate Daily Container Nesting Report: ", ex.ToString());
                message = ex.Message;
                isSuccessful = false;
            }
            await webJobRunProcessor.EndWebJob(new EndWebJobRequest
            {
                WebJobRun = webJobRun,
                IsSuccessful = isSuccessful,
                NumberOfRecords = numberOfRecords,
                Message = message,
                FileDetails = fileDetails
            });
        }

        private static string FormatNestingHeaderRecord<T>() where T : class
        {
            var cols = new List<string>();
            foreach (var property in typeof(BasicContainerPackageNesting).GetProperties(~BindingFlags.Static))
            {
                if (! property.Name.Contains("HYPERLINK"))
                    cols.Add(property.Name);
            }
            return String.Join('|', cols)+ "\r\n";;
        }
        private static string FormatNestingRecord<T>(T record) where T : class
        {
            var values = new List<string>();
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                if (!property.Name.Contains("HYPERLINK"))
                {
                    var value = record.GetType().GetProperty(property.Name).GetValue(record, null) ?? string.Empty;
                    values.Add(value.ToString());
                }
            }
            return String.Join('|', values) + "\r\n";
        }    
    }
}

