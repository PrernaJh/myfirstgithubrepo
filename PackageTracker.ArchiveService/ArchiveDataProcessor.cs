using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.ArchiveService.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Collections.Generic;
using System;
using PackageTracker.Domain.Interfaces;
using System.Threading.Tasks;
using PackageTracker.AzureExtensions;
using Microsoft.Extensions.Configuration;
using PackageTracker.Data.Models.Archive;
using PackageTracker.Data.Utilities;

namespace PackageTracker.ArchiveService
{
    public class ArchiveDataProcessor : IArchiveDataProcessor
    {
        private readonly ILogger<ArchiveDataProcessor> logger;
        private readonly IBlobHelper blobHelper;
        private readonly ICloudFileClientFactory cloudFileClientFactory;
        private readonly IConfiguration config;
        private readonly IFileShareHelper fileShareHelper;

        public ArchiveDataProcessor(ILogger<ArchiveDataProcessor> logger,
            IBlobHelper blobHelper,
            ICloudFileClientFactory cloudFileClientFactory,
            IConfiguration config,
            IFileShareHelper fileShareHelper
            )
        {
            this.logger = logger;
            this.blobHelper = blobHelper;
            this.cloudFileClientFactory = cloudFileClientFactory;
            this.config = config;
            this.fileShareHelper = fileShareHelper;
        }

        public async Task ArchivePackagesAsync(SubClient subClient, DateTime manifestDate, IList<PackageForArchive> packages)
        {
            var archivePath = config.GetSection("PackageArchive").Value;
            if (StringHelper.Exists(archivePath))
            {
                archivePath = $"{archivePath}/{subClient.ClientName}/{subClient.SiteName}/{manifestDate.ToString("yyyyMMdd")}";
                foreach (var package in packages)
                {
                    var strings = new List<string>();
                    strings.Add(JsonUtility<PackageForArchive>.Serialize(package));
                    await blobHelper.UploadListOfStringsToBlobAsync(strings, archivePath, package.PackageId + ".json");
#if DEBUG
                    System.Console.WriteLine($"{archivePath}/{package.PackageId}.json");
#endif
                }
            }
        }
    }
}