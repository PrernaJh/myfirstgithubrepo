using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.ArchiveService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PackageTracker.Data.Models.Archive;

namespace PackageTracker.ArchiveService
{
    public class HistoricalDataService : IHistoricalDataService
    {
        private readonly ILogger<HistoricalDataService> logger;
        private readonly IArchiveDataProcessor archiveDataProcessor;
        private readonly IBlobHelper blobHelper;
        private readonly ICloudFileClientFactory cloudFileClientFactory;
        private readonly IConfiguration config;
        private readonly IEmailService emailService;
        private readonly IFileShareHelper fileShareHelper;
        private readonly IHistoricalDataProcessor historicalDataProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public HistoricalDataService(ILogger<HistoricalDataService> logger,
            IArchiveDataProcessor archiveDataProcessor,
            IBlobHelper blobHelper,
            ICloudFileClientFactory cloudFileClientFactory,
            IConfiguration config,
            IEmailService emailService,
            IFileShareHelper fileShareHelper,
            IHistoricalDataProcessor historicalDataProcessor,
            ISubClientProcessor subClientProcessor,
            IWebJobRunProcessor webJobRunProcessor
            )
        {
            this.logger = logger;
            this.archiveDataProcessor = archiveDataProcessor;
            this.blobHelper = blobHelper;
            this.cloudFileClientFactory = cloudFileClientFactory;
            this.config = config;
            this.emailService = emailService;
            this.fileShareHelper = fileShareHelper;
            this.historicalDataProcessor = historicalDataProcessor;
            this.subClientProcessor = subClientProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        public async Task FileImportWatcher(WebJobSettings webJobSettings)
        {
            var fileArchivePath = config.GetSection("HistoricalDataArchive").Value;
            var fileShare = config.GetSection("HistoricalDataImportFileShare").Value;
            var fileClient = cloudFileClientFactory.GetCloudFileClient();
            var directory = fileClient.GetShareReference(fileShare).GetRootDirectoryReference();
            var token = new FileContinuationToken();
            var files = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (var file in files.Results)
            {
                try
                {
                    logger.Log(LogLevel.Information, $"Processing Historical Data file: { file.Uri }");
                    var totalTimeStopwatch = new Stopwatch();
                    totalTimeStopwatch.Start();
                    var response = new FileImportResponse();
                    var webJobId = Guid.NewGuid().ToString();
                    var storageCredentials = new StorageCredentials(config.GetSection("AzureFileShareAccountName").Value, config.GetSection("AzureFileShareKey").Value);
                    var cloudFile = new CloudFile(file.Uri, storageCredentials);
                    var fileName = cloudFile.Name;
                    long fileSize = 0;
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        fileSize = fileStream.Length;
                        Thread.Sleep(1000);
                    }
                    FileReadResponse readResponse;
                    List<PackageForArchive> packages;
                    ExcelWorkSheet exceptions;
                    DateTime fileDate;
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        if (fileStream.Length > fileSize)
                            continue; // File is being written, so wait till later ...
                        var root = Path.GetFileNameWithoutExtension(fileName);
                        var datestring = root.Substring(root.Length - 6) + " 120000";
                        DateTime.TryParseExact(datestring, "MMddyy HHmmss", null, DateTimeStyles.AssumeUniversal, out fileDate);
                        (readResponse, packages, exceptions) =
                            await historicalDataProcessor.ImportHistoricalDataFileAsync(webJobSettings, fileStream, fileDate);
                        response.FileReadTime = readResponse.FileReadTime;
                        response.IsSuccessful = readResponse.IsSuccessful;
                        response.Message = readResponse.Message;
                    }
                    if (response.IsSuccessful)
                    {
                        foreach (var group in packages.GroupBy(p => p.SubClientName))
                        {
                            var subClient = await subClientProcessor.GetSubClientByNameAsync(group.Key);
                            if (StringHelper.Exists(subClient.Name))
                                await archiveDataProcessor.ArchivePackagesAsync(subClient, fileDate, group.ToList());
                        }
                    }
                    var archiveSuccess = false;
                    var archiveFileName = fileName;
                    fileSize = 0;
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        fileSize = fileStream.Length;
                        (archiveSuccess, archiveFileName) = await blobHelper.ArchiveToBlob(fileStream, fileName, fileArchivePath);
                    }
                    if (archiveSuccess)
                    {
                        await cloudFile.DeleteAsync();
                        try
                        {
                            var exportfileShare = config.GetSection("HistoricalDataExportFileShare").Value;
                            if (StringHelper.Exists(exportfileShare) && exceptions != null)
                            {
                                var exportFileName = blobHelper.GenerateArchiveFileName(fileName.Replace(".xlsx", $"_Exceptions[{exceptions.RowCount}].xlsx"));
                                await fileShareHelper.UploadWorkSheet(exceptions,
                                    config.GetSection("AzureFileShareAccountName").Value, config.GetSection("AzureFileShareKey").Value,
                                    exportfileShare, exportFileName, fileSize);
                                exceptions.Dispose();
                            }
                        } catch (Exception ex)
                        {
                            logger.LogError($"Can't export exceptions file for historical data file: {fileName}, Ex: {ex}");
                        }
                    }
                    totalTimeStopwatch.Stop();
                    response.TotalTime = TimeSpan.FromMilliseconds(totalTimeStopwatch.ElapsedMilliseconds);
                    if (!response.IsSuccessful)
                    {
                        emailService.SendServiceErrorNotifications("Historical Data File Processor: Error while processing file: {fileName}.", $"See logs for details.");
                    }

                    await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                    {
                        Id = webJobId,
                        SiteName = string.Empty,
                        ClientName = string.Empty,
                        SubClientName = string.Empty,
                        JobName = "Historical Package Data File Import",
                        JobType = WebJobConstants.HistoricalDataJobType,
                        Username = "System",
                        FileDetails = new List<FileDetail> { 
                            new FileDetail { 
                                FileName = fileName,
                                FileArchiveName = archiveFileName,
                                NumberOfRecords = (int)response.NumberOfDocumentsImported 
                            } 
                        },
                        Message = string.Empty,
                        IsSuccessful = response.IsSuccessful
                    });

                    logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("Historical Data File Import", response)}");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, $"Failed to process Historical Data File: {file.Uri.AbsoluteUri}. Exception: { ex }");
                    emailService.SendServiceErrorNotifications("Error: Historical Data File Processor, Filepath: { file.Uri.AbsoluteUri }", ex.ToString());
                }
            }

            logger.Log(LogLevel.Information, $"Processed {files.Results.Count()} files.");
        }

    }
}
