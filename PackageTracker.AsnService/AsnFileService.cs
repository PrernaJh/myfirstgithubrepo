using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PackageTracker.AsnService
{
    public class AsnFileService : IAsnFileService
    {
        private readonly ILogger<AsnFileService> logger;

        private readonly IAsnFileProcessor asnFileProcessor;
        private readonly IBlobHelper blobHelper;
        private readonly ICloudFileClientFactory cloudFileClientFactory;
        private readonly IConfiguration config;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IEmailService emailService;
        private readonly IWebJobRunProcessor webJobRunProcessor;

        public AsnFileService(ILogger<AsnFileService> logger,
            IAsnFileProcessor asnFileProcessor, IBlobHelper blobHelper, ICloudFileClientFactory cloudFileClientFactory, IConfiguration config,
            IEmailService emailService, ISiteProcessor siteProcessor, ISubClientProcessor subClientProcessor, IWebJobRunProcessor webJobRunProcessor)
        {
            this.logger = logger;

            this.asnFileProcessor = asnFileProcessor;
            this.blobHelper = blobHelper;
            this.cloudFileClientFactory = cloudFileClientFactory;
            this.config = config;
            this.emailService = emailService;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.webJobRunProcessor = webJobRunProcessor;
        }

        public async Task ProcessAsnFilesAsync(WebJobSettings webJobSettings)
        {
            var subClients = await subClientProcessor.GetSubClientsAsync();
            var sites = await siteProcessor.GetAllSitesAsync();

            foreach (var subClient in subClients)
            {
                var site = sites.FirstOrDefault(s => s.SiteName == subClient.SiteName);
                if (webJobSettings.IsDuringScheduledHours(site, subClient))
                {
                    await ProcessAsnFilesAsync(subClient, site.TimeZone, webJobSettings);
                }
            }
        }

        private async Task ProcessAsnFilesAsync(SubClient subClient, string timeZone, WebJobSettings webJobSettings)
        {
            var isDuplicateBlockEnabled = webJobSettings.GetParameterBoolValue("IsDuplicateBlockEnabled");
            var fileArchivePath = $"{config.GetSection("AsnFileArchive").Value}/{subClient.ClientName}/{subClient.SiteName}";
            var fileShare = subClient.AsnImportLocation;
            var fileClient = cloudFileClientFactory.GetCloudFileClient();
            var storageCredentials = new StorageCredentials(config.GetSection("AzureFileShareAccountName").Value, config.GetSection("AzureFileShareKey").Value);
            var directory = fileClient.GetShareReference(fileShare).GetRootDirectoryReference();
            var token = new FileContinuationToken();
            var files = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            var cloudFilesToProcess = new List<CloudFile>();
            var fileDetails = new List<FileDetail>();
            var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

            if (files.Results.Count() > 0)
            {
                logger.Log(LogLevel.Information, $"Total number of files in shared import folder for SubClient {subClient.Name}: {files.Results.Count()}");

                var packages = new List<Package>();
                var importResponse = new AsnFileImportResponse();

                foreach (var file in files.Results)
                {
                    var cloudFile = new CloudFile(file.Uri, storageCredentials);
                    var fileName = cloudFile.Name;
                    logger.Log(LogLevel.Information, $"Attempting to match SubClient { subClient.Name } with file:  { fileName }");

                    if (ImportMatch(subClient, fileName) && cloudFilesToProcess.Count < 10)
                    {
                        cloudFilesToProcess.Add(cloudFile);

                        logger.Log(LogLevel.Information, $"Match found. Beginning processing for {subClient.Name}. ASN file: { fileName } Duplicate Block Enabled: {isDuplicateBlockEnabled}");
                    }
                    else
                    {
                        break;
                    }
                }

                if (cloudFilesToProcess.Any())
                {
                    var processedFiles = new List<CloudFile>();
                    foreach (var cloudFile in cloudFilesToProcess)
                    {
                        try
                        {
                            long fileSize = 0;
                            using (var fileStream = await cloudFile.OpenReadAsync())
                            {
                                fileSize = fileStream.Length;
                                Thread.Sleep(1000);
                            }

                            var numberOfRecords = 0;
                            using (var fileStream = await cloudFile.OpenReadAsync())
                            {
                                if (fileStream.Length > fileSize)
                                    continue; // File is being written, so wait till later ...
                                if (subClient.AsnImportFormat == FileFormatConstants.CmopAsnImportFormat1)
                                {
                                    var fileReadResponse = await asnFileProcessor.ReadCmopFileStreamAsync(fileStream);
                                    packages.AddRange(fileReadResponse.Packages);
                                    numberOfRecords = fileReadResponse.NumberOfRecords;

                                }
                                else if (subClient.AsnImportFormat == FileFormatConstants.DalcAsnImportFormat1)
                                {
                                    var fileReadResponse = await asnFileProcessor.ReadDalcFileStreamAsync(fileStream);
                                    packages.AddRange(fileReadResponse.Packages);
                                    numberOfRecords = fileReadResponse.NumberOfRecords;
                                }
                            }
                            logger.Log(LogLevel.Information, $"ASN file {cloudFile.Name} stream rows read: { numberOfRecords } for SubClient {subClient.Name}");

                            processedFiles.Add(cloudFile);
                            fileDetails.Add(new FileDetail
                            {
                                FileName = cloudFile.Name,
                                NumberOfRecords = numberOfRecords
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Error, $"Exception trying to read ASN file {cloudFile.Name} for SubClient {subClient.Name}: {ex}");
                        }
                    }
                    if (processedFiles.Any())
                    {
                        var webJobId = Guid.NewGuid().ToString();
                        if (packages.Any())
                            importResponse = await asnFileProcessor.ImportPackages(packages, subClient, isDuplicateBlockEnabled, webJobId);
                        else
                            importResponse.IsSuccessful = true; // Empty file processed.

                        foreach (var cloudFile in processedFiles)
                        {
                            var (archiveSuccess, archiveMessage) = (false, string.Empty);
                            // Need to use a new file stream, because the old one is positioned at the end of the file!
                            using (var fileStream = await cloudFile.OpenReadAsync())
                            {
                                (archiveSuccess, archiveMessage) = await blobHelper.ArchiveToBlob(fileStream, cloudFile.Name, fileArchivePath);
                            }

                            try
                            {
                                var ftpArgs = config.GetSection("CmopAsnFtpArchive").Get<FtpArgs>();
                                if (webJobSettings.GetParameterBoolValue("EnableCmopAsnFtpArchive") &&
                                    ftpArgs != null && subClient.ClientName == ClientSubClientConstants.CmopClientName)
                                {
                                    // Need to use a new file stream, because the old one is positioned at the end of the file!
                                    using (var fileStream = await cloudFile.OpenReadAsync())
                                    {
                                        using (var ftpHelper = new SftpHelper(ftpArgs))
                                        {
                                            await ftpHelper.UploadStreamToFileAsync(fileStream, blobHelper.GenerateArchiveFileName(cloudFile.Name));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error while archiving ASN file: {cloudFile.Name} to SFTP site, Ex: {ex}");
                                emailService.SendServiceErrorNotifications("ASN File Processor", $"Error while archiving ASN file: {cloudFile.Name} to SFTP site. See logs for details.");
                            }

                            if (archiveSuccess)
                            {
                                fileDetails.FirstOrDefault(x => x.FileName == cloudFile.Name).FileArchiveName = archiveMessage;
                                await cloudFile.DeleteAsync();
                            }
                            else
                            {
                                logger.LogError($"Error while archiving ASN file: {cloudFile.Name}. See logs for details.");
                                emailService.SendServiceErrorNotifications("ASN File Processor", $"Error while archiving ASN file: {cloudFile.Name}. See logs for details.");
                            }

                            if (!importResponse.IsSuccessful)
                            {
                                emailService.SendServiceErrorNotifications("ASN File Processor", $"Error while importing ASN records for subclient: {subClient.Name}. See logs for details.");
                            }

                            // Send email about blocked packages.
                            var blockedPackages = packages.Where(p => p.PackageStatus == EventConstants.Blocked);
                            if (blockedPackages.Any())
                            {
                                var ids = new List<string>();
                                blockedPackages.ToList().ForEach(p => ids.Add(p.PackageId));
                                var text = String.Join(", ", ids);

                                emailService.SendServiceAlertNotifications($"Alert: ASN File Import: Blocked Packages for: {subClient.Name}",
                                    $"Blocked Package Ids: {text}.",
                                    site.CriticalAlertEmailList,
                                    site.CriticalAlertSmsList);
                            }
                        }

                        logger.Log(LogLevel.Information, $"{LogFileUtility.LogAsnFileImportResponse("ASN File Import", importResponse)}");

                        await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
                        {
                            Id = webJobId,
                            SiteName = subClient.SiteName,
                            ClientName = subClient.ClientName,
                            SubClientName = subClient.Name,
                            JobName = "ASN File Import",
                            JobType = WebJobConstants.AsnImportJobType,
                            Username = "System",
                            FileDetails = fileDetails,
                            Message = string.Empty,
                            IsSuccessful = importResponse.IsSuccessful,
                            LocalCreateDate = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneUtility.ConvertTimeZoneConstantsToParameter(timeZone)))
                        });
                        if (!importResponse.IsSuccessful)
                        {
                            var fileNames = string.Join(',', fileDetails.Select(d => " " + d.FileName).ToArray());
                            emailService.SendServiceAlertNotifications($"Alert: ASN File Import: Failed: {subClient.Name}",
                                $"File Name(s): {fileNames}",
                                site.CriticalAlertEmailList,
                                site.CriticalAlertSmsList);
                        }
                    }
                }
            }
        }

        private bool ImportMatch(SubClient subClient, string fileName)
        {
            try
            {
                if (fileName.EndsWith(".filepart", StringComparison.InvariantCultureIgnoreCase)) // Skip partially uploaded files
                    return false;
                var response = AsnFileUtility.MatchFileName(subClient.AsnFileTrigger, fileName);
                if (response)
                {
                    logger.Log(LogLevel.Information, $"Matched input file name: {fileName} using ASN File Trigger: {subClient.AsnFileTrigger}");
                }
                return response;
            }
            catch
            {
                logger.Log(LogLevel.Information, $"Unable to match input file name: {fileName} using ASN File Trigger: {subClient.AsnFileTrigger}");
                return false;
            }
        }
    }
}

