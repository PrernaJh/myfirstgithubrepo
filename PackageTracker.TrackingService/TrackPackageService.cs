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
using ParcelPrepGov.Reports.Interfaces;
using PackageTracker.TrackingService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace PackageTracker.TrackingService
{
    public class TrackPackageService : ITrackPackageService
    {
        private readonly ILogger<TrackPackageService> logger;
        private readonly IBlobHelper blobHelper;
        private readonly ICloudFileClientFactory cloudFileClientFactory;
        private readonly IConfiguration config;
        private readonly IEmailService emailService;
        private readonly IPackageDatasetProcessor packageDatasetProcessor;
        private readonly IPackageSearchProcessor packageSearchProcessor;
        private readonly IShippingContainerDatasetProcessor shippingContainerDatasetProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly ITrackPackageDatasetProcessor trackPackageDatasetProcessor;
        private readonly ITrackPackageProcessor trackPackageProcessor;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public TrackPackageService(ILogger<TrackPackageService> logger,
            IBlobHelper blobHelper,
            ICloudFileClientFactory cloudFileClientFactory,
            IConfiguration config,
            IEmailService emailService,
            IPackageDatasetProcessor packageDatasetProcessor,
            IPackageSearchProcessor packageSearchProcessor,
            IShippingContainerDatasetProcessor shippingContainerDatasetProcessor,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            ITrackPackageDatasetProcessor trackPackageDatasetProcessor,
            ITrackPackageProcessor trackPackageProcessor,
            IWebJobRunProcessor webJobRunProcessor
            )
        {
            this.logger = logger;
            this.blobHelper = blobHelper;
            this.cloudFileClientFactory = cloudFileClientFactory;
            this.config = config;
            this.emailService = emailService;
            this.packageDatasetProcessor = packageDatasetProcessor;
            this.packageSearchProcessor = packageSearchProcessor;
            this.shippingContainerDatasetProcessor = shippingContainerDatasetProcessor;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.trackPackageDatasetProcessor = trackPackageDatasetProcessor;
            this.trackPackageProcessor = trackPackageProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        /* archaic
        public async Task ProcessFedExTrackPackageDataAsync(WebJobSettings webJobSettings)
        {
            foreach (var site in await siteProcessor.GetAllSitesAsync())
            {
                if (webJobSettings.IsDuringScheduledHours(site, await subClientProcessor.GetSubClientsAsync()))
                {
                    var isSuccessful = true;
                    var message = string.Empty;
                    try
                    {
                        var response = await trackPackageProcessor.ImportFedexTrackingDataAsync(site);
                        isSuccessful = response.IsSuccessful;
                    }
                    catch (Exception ex)
                    {
                        isSuccessful = false;
                        message = $"FedEx track packages for site: {site.SiteName} failed, Exception: {ex.Message}";
                        logger.Log(LogLevel.Error, message);
                    }
                    await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                    {
                        SiteName = site.SiteName,
                        JobName = "FedEx Track Package",
                        JobType = WebJobConstants.FedExTrackPackageJobType,
                        Username = "System",
                        ErrorMessage = message,
                        IsSuccessful = isSuccessful
                    });
                }
            }
        }
        */

        public async Task ProcessFedExScanDataFilesAsync(WebJobSettings webJobSettings)
        {
            if (!webJobSettings.IsDuringScheduledHours(await siteProcessor.GetAllSitesAsync(), await subClientProcessor.GetSubClientsAsync()))
                return;
            trackPackageDatasetProcessor.ForceReloadEventCodes();
            var fileArchivePath = config.GetSection("FedExScanDataArchive").Value;
            var fileShare = config.GetSection("FedExScanDataImportFileShare").Value;
            var fileClient = cloudFileClientFactory.GetCloudFileClient();
            var directory = fileClient.GetShareReference(fileShare).GetRootDirectoryReference();
            var token = new FileContinuationToken();
            var files = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            var processedFiles = new List<CloudFile>();
            foreach (var file in files.Results)
            {
                try
                {
                    var totalTimeStopwatch = new Stopwatch();
                    totalTimeStopwatch.Start();
                    var response = new FileImportResponse();
                    var webJobId = Guid.NewGuid().ToString();
                    var storageCredentials = new StorageCredentials(config.GetSection("AzureFileShareAccountName").Value, config.GetSection("AzureFileShareKey").Value);
                    var cloudFile = new CloudFile(file.Uri, storageCredentials);
                    var fileName = cloudFile.Name;

                    var trackPackages = new List<TrackPackage>();
                    long fileSize = 0;
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        fileSize = fileStream.Length;
                        Thread.Sleep(1000);
                    }
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        if (fileStream.Length > fileSize)
                            continue; // File is being written, so wait till later ...
                        logger.Log(LogLevel.Information, $"Processing FedEx Scan Data file: { file.Uri }");
                        processedFiles.Add(cloudFile);
                        var readRespose = new FileReadResponse();
                        (readRespose, trackPackages) = await trackPackageProcessor.ImportFedExTrackingFileAsync(webJobSettings, fileStream);
                        response.FileReadTime = readRespose.FileReadTime;
                        response.IsSuccessful = readRespose.IsSuccessful;
                        response.Message = readRespose.Message;
                    }
                    if (response.IsSuccessful)
                    {
                        var reportResponse = await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(ShippingCarrierConstants.FedEx, trackPackages);
                        response.IsSuccessful = reportResponse.IsSuccessful;
                        response.Message = reportResponse.Message;
                        response.NumberOfDocumentsImported = reportResponse.NumberOfDocuments;
                    }

                    // Need to use a new file stream, because the old one is positioned at the end of the file!
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        await blobHelper.ArchiveToBlob(fileStream, fileName, fileArchivePath);
                    }
                    await cloudFile.DeleteAsync();
                    totalTimeStopwatch.Stop();
                    response.TotalTime = TimeSpan.FromMilliseconds(totalTimeStopwatch.ElapsedMilliseconds);
                    if (!response.IsSuccessful)
                    {
                        emailService.SendServiceErrorNotifications("FedEx Scan Data File Processor", $"Error while processing file: {fileName}. See logs for details.");
                    }

                    await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                    {
                        Id = webJobId,
                        SiteName = string.Empty,
                        ClientName = string.Empty,
                        SubClientName = string.Empty,
                        JobName = "FedEx Scan Data File Import",
                        JobType = WebJobConstants.FedExTrackPackageJobType,
                        Username = "System",
                        FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName, NumberOfRecords = (int)response.NumberOfDocumentsImported } },
                        Message = string.Empty,
                        IsSuccessful = response.IsSuccessful
                    });

                    logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("FedEx Scan Data File Import", response)}");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, $"Failed to process FedEx Scan Data File: {file.Uri.AbsoluteUri}. Exception: { ex }");
                    emailService.SendServiceErrorNotifications("Error: FedEx Scan Data File Processor", $"Filepath: { file.Uri.AbsoluteUri } Exception: {ex}");
                }
            }

            logger.Log(LogLevel.Information, $"Processed {processedFiles.Count} files.");
        }

        public async Task ProcessUpsTrackPackageDataAsync(WebJobSettings webJobSettings)
        {
            if (!webJobSettings.IsDuringScheduledHours(await siteProcessor.GetAllSitesAsync(), await subClientProcessor.GetSubClientsAsync()))
                return;
            trackPackageDatasetProcessor.ForceReloadEventCodes();
            var totalTimeStopwatch = new Stopwatch();
            totalTimeStopwatch.Start();
            var response = new XmlImportResponse();
            try
            {
                var trackPackages = new List<TrackPackage>();
                (response, trackPackages) = await trackPackageProcessor.ImportUpsTrackingDataAsync(webJobSettings);
                if (response.IsSuccessful)
                {
                    var reportResponse = await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(ShippingCarrierConstants.Ups, trackPackages);
                    response.IsSuccessful = reportResponse.IsSuccessful;
                    response.Message = reportResponse.Message;
                    response.NumberOfDocumentsImported = reportResponse.NumberOfDocuments;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to track packages from UPS. Exception: { ex }");
                response.Message = ex.Message;
                response.IsSuccessful = false;
            }
            await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
            {
                SiteName = SiteConstants.AllSites,
                JobName = "UPS Track Package",
                JobType = WebJobConstants.UpsTrackPackageJobType,
                Username = "System",
                Message = response.Message,
                IsSuccessful = response.IsSuccessful
            });
            totalTimeStopwatch.Stop();
            var totalTime = TimeSpan.FromMilliseconds(totalTimeStopwatch.ElapsedMilliseconds);
            logger.Log(LogLevel.Information, $"{LogFileUtility.LogXmlImportResponse("UPS Track Package", response, totalTime)}");
        }

        public async Task ProcessUspsScanDataFilesAsync(WebJobSettings webJobSettings)
        {
            if (!webJobSettings.IsDuringScheduledHours(await siteProcessor.GetAllSitesAsync(), await subClientProcessor.GetSubClientsAsync()))
                return;
            trackPackageDatasetProcessor.ForceReloadEventCodes();
            var fileArchivePath = config.GetSection("UspsScanDataArchive").Value;
            var fileShare = config.GetSection("UspsScanDataImportFileShare").Value;
            var fileClient = cloudFileClientFactory.GetCloudFileClient();
            var directory = fileClient.GetShareReference(fileShare).GetRootDirectoryReference();
            var token = new FileContinuationToken();
            var files = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            var processedFiles = new List<CloudFile>();
            foreach (var file in files.Results)
            {
                try
                {
                    var totalTimeStopwatch = new Stopwatch();
                    totalTimeStopwatch.Start();
                    var response = new FileImportResponse() { IsSuccessful = true };
                    var webJobId = Guid.NewGuid().ToString();
                    var storageCredentials = new StorageCredentials(config.GetSection("AzureFileShareAccountName").Value, config.GetSection("AzureFileShareKey").Value);
                    var cloudFile = new CloudFile(file.Uri, storageCredentials);
                    var fileName = cloudFile.Name;
                    if (Regex.IsMatch(fileName, webJobSettings.GetParameterStringValue("FileNameMatchPattern")))
                    {
                        long fileSize = 0;
                        using (var fileStream = await cloudFile.OpenReadAsync())
                        {
                            fileSize = fileStream.Length;
                            Thread.Sleep(1000);
                        }
                        using (var fileStream = await cloudFile.OpenReadAsync())
                        {
                            if (fileStream.Length > fileSize)
                                continue; // File is being written, so wait till later ...
                            logger.Log(LogLevel.Information, $"Processing USPS Scan Data file: { file.Uri }");
                            processedFiles.Add(cloudFile);
                            int chunk = 10000;
                            for (; ; )
                            {
                                var trackPackages = new List<TrackPackage>();
                                var readRespose = new FileReadResponse();
                                (readRespose, trackPackages) = await trackPackageProcessor.ImportUspsTrackingFileAsync(webJobSettings, fileStream, chunk);
                                response.FileReadTime += readRespose.FileReadTime;
                                if (! response.IsSuccessful)
                                    response.Message = readRespose.Message;
                                if (trackPackages.Count == 0)
                                    break;
                                if (response.IsSuccessful)
                                {
                                    var reportResponse = await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(ShippingCarrierConstants.Usps, trackPackages);
                                    response.NumberOfDocumentsImported += reportResponse.NumberOfDocuments;
                                    if (! reportResponse.IsSuccessful)
                                    {
                                        response.IsSuccessful = false;
                                        response.Message = reportResponse.Message;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        long fileSize = 0;
                        using (var fileStream = await cloudFile.OpenReadAsync())
                        {
                            fileSize = fileStream.Length;
                            Thread.Sleep(1000);
                        }
                        using (var fileStream = await cloudFile.OpenReadAsync())
                        {
                            if (fileStream.Length > fileSize)
                                continue; // File is being written, so wait till later ...
                        }
                        logger.Log(LogLevel.Information, $"Archiving USPS Data file: { file.Uri }");
                    }

                    // Need to use a new file stream, because the old one is positioned at the end of the file!
                    using (var fileStream = await cloudFile.OpenReadAsync())
                    {
                        await blobHelper.ArchiveToBlob(fileStream, fileName, fileArchivePath);
                    }
                    await cloudFile.DeleteAsync();
                    totalTimeStopwatch.Stop();
                    response.TotalTime = TimeSpan.FromMilliseconds(totalTimeStopwatch.ElapsedMilliseconds);
                    if (!response.IsSuccessful)
                    {
                        emailService.SendServiceErrorNotifications("USPS Scan Data File Processor", $"Error while processing file: {fileName}. See logs for details.");
                    }

                    await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                    {
                        Id = webJobId,
                        SiteName = string.Empty,
                        ClientName = string.Empty,
                        SubClientName = string.Empty,
                        JobName = "USPS Scan Data File Import",
                        JobType = WebJobConstants.UspsTrackPackageJobType,
                        Username = "System",
                        FileDetails = new List<FileDetail> { new FileDetail { FileName = fileName, NumberOfRecords = (int)response.NumberOfDocumentsImported } },
                        Message = string.Empty,
                        IsSuccessful = response.IsSuccessful
                    });

                    logger.Log(LogLevel.Information, $"{LogFileUtility.LogFileImportResponse("USPS Scan Data File Import", response)}");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, $"Failed to process USPS Scan Data File: {file.Uri.AbsoluteUri}. Exception: { ex }");
                    emailService.SendServiceErrorNotifications("Error: USPS Scan Data File Processor", $"Filepath: { file.Uri.AbsoluteUri } Exception: {ex}");
                }
            }

            logger.Log(LogLevel.Information, $"Processed {processedFiles.Count} files.");
        }

        public async Task UpdateMissingPackageUspsScanDataAsync(WebJobSettings webJobSettings)
        {
            trackPackageDatasetProcessor.ForceReloadEventCodes();
            var subClients = await subClientProcessor.GetSubClientsAsync();
            foreach (var site in await siteProcessor.GetAllSitesAsync())
            {
                var packagesToUpdate = await packageDatasetProcessor.GetPackagesWithNoSTC(site,
                    webJobSettings.GetParameterIntValue("LookbackMin", 5), webJobSettings.GetParameterIntValue("LookbackMax", 12));
                logger.LogInformation($"USPS Packages without STC: {packagesToUpdate.Count()} for site: {site.SiteName}");
                var shippingCarrier = ShippingCarrierConstants.Usps;
                foreach (var group in packagesToUpdate.GroupBy(p => p.LocalProcessedDate.Date))
                {
                    var trackPackages = new List<TrackPackage>();
                    foreach (var package in group)
                    {
                        var subClient = subClients.FirstOrDefault(s => s.Name == package.SubClientName);
                        if (subClient != null)
                        {
                            var response = await trackPackageProcessor
                                .GetUspsTrackingData(package.ShippingBarcode, subClient.UspsApiUserId, subClient.UspsApiSourceId);
                            trackPackages.AddRange(trackPackageProcessor.ImportUspsTrackPackageResponse(response, package.ShippingBarcode));
                        }
                    }
                    trackPackages.RemoveAll(tp => trackPackageDatasetProcessor.IsStopTheClock(tp.UspsTrackingData.EventCode, shippingCarrier) == 0);
                    await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(shippingCarrier, trackPackages);
                    logger.LogInformation($"Updated Packages USPS Tracking data: {trackPackages.Count()} for site: {site.SiteName}, for manifest date: {group.Key}");
                }
            }
        }

        public async Task UpdateMissingContainerUspsScanDataAsync(WebJobSettings webJobSettings)
        {
            trackPackageDatasetProcessor.ForceReloadEventCodes();
            foreach (var site in await siteProcessor.GetAllSitesAsync())
            {
                var shippingContainersToUpdate = await shippingContainerDatasetProcessor.GetShippingContainersWithNoSTC(site,
                    webJobSettings.GetParameterIntValue("LookbackMin", 5), webJobSettings.GetParameterIntValue("LookbackMax", 12));
                logger.LogInformation($"USPS Shipping Containers without STC: {shippingContainersToUpdate.Count()} for site: {site.SiteName}");
                var shippingCarrier = ShippingCarrierConstants.Usps;
                foreach (var group in shippingContainersToUpdate.GroupBy(c => c.LocalProcessedDate.Date))
                {
                    var trackPackages = new List<TrackPackage>();
                    foreach (var shippingContainer in group)
                    {
                        var response = await trackPackageProcessor
                            .GetUspsTrackingData(shippingContainer.UpdatedBarcode ?? shippingContainer.ContainerId, site.UspsApiUserId, site.UspsApiSourceId);
                        trackPackages.AddRange(trackPackageProcessor.ImportUspsTrackPackageResponse(response, shippingContainer.UpdatedBarcode));
                    }
                    trackPackages.RemoveAll(tp => trackPackageDatasetProcessor.IsStopTheClock(tp.UspsTrackingData.EventCode, shippingCarrier) == 0);
                    await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(shippingCarrier, trackPackages);
                    logger.LogInformation($"Updated Shipping Containers USPS Tracking data: {trackPackages.Count()} for site: {site.SiteName}, for manifest date: {group.Key}");
                }
            }
        }
    }
}

