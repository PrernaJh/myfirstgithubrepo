using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications;
using PackageTracker.Communications.Interfaces;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Data;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Processors;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Service.Interfaces;
using PackageTracker.WebServices;
using System;
using System.IO;

namespace PackageTracker.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder();


            var appSettings = "appsettings.json";
#if DEBUG // overwrite appsettings file w/ development version if it exists in the target directory
            var appDevelopmentSettings = "appsettings.Development.json";
            if (File.Exists(appDevelopmentSettings))
            {
                if (File.Exists(appSettings))
                    File.Delete(appSettings);
                File.Copy(appDevelopmentSettings, appSettings);
            }
#endif
            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(appSettings, optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddApplicationInsights();
                logging.AddConsole();
                logging.AddDebug();
                // For some unknown reason the ApplicationInsightsLogProvider doesn't correctly pick this up from appsettings,
                //  so we need to do this:
                logging.AddFilter<ApplicationInsightsLoggerProvider>("",
                    LogLevelFromString(configuration["Logging:ApplicationInsights:LogLevel:Default"]));
            });

            // UPS API client configs
            var upsShipEndpoint = configuration.GetSection("UpsApis").GetSection("ShipEndpointUri").Value;
            var upsShipClient = new UpsShipApi.ShipPortTypeClient(0, upsShipEndpoint ??  "https://localhost");
            var upsVoidEndpoint = configuration.GetSection("UpsApis").GetSection("VoidEndpointUri").Value;
            var upsVoidClient = new UpsVoidApi.VoidPortTypeClient(0, upsVoidEndpoint);

            // FedEx Ship API client configs
            var fedExShipEndpoint = configuration.GetSection("FedExApis").GetSection("ShipEndpointUri").Value;
            var fedExShipClient = new FedExShipApi.ShipPortTypeClient(0, fedExShipEndpoint ?? "https://localhost");

            builder.ConfigureServices(n =>
            {
                // extensions
                n.AddCosmosDbToService(ref configuration, appSettings); // Make sure this is first since it may update 'configuration'
                var cloudStorageAccountSettings = configuration.GetSection("AzureWebJobsStorage").Value;
                n.AddBlobStorageAccount(cloudStorageAccountSettings);
                n.AddFileShareStorageAccount(cloudStorageAccountSettings);
                n.AddQueueStorageAccount(cloudStorageAccountSettings);

                // Add DbContext for Entity Framework and configure context to connect to the Identity db
                n.AddDbContext<PackageTrackerIdentityDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("IdentityDb")));

                // repositories
                n.AddSingleton<IActiveGroupRepository, ActiveGroupRepository>();
                n.AddSingleton<IBinRepository, BinRepository>();
                n.AddSingleton<IBinMapRepository, BinMapRepository>();
                n.AddSingleton<IContainerRepository, ContainerRepository>();
                n.AddSingleton<IClientRepository, ClientRepository>();
                n.AddSingleton<IFileConfigurationRepository, FileConfigurationRepository>();
                n.AddSingleton<IJobOptionRepository, JobOptionRepository>();
                n.AddSingleton<IOperationalContainerRepository, OperationalContainerRepository>();
                n.AddSingleton<IPackageRepository, PackageRepository>();
                n.AddSingleton<IRateRepository, RateRepository>();
                n.AddSingleton<ISiteRepository, SiteRepository>();
                n.AddSingleton<IServiceRuleRepository, ServiceRuleRepository>();
                n.AddSingleton<IServiceRuleExtensionRepository, ServiceRuleExtensionRepository>();
                n.AddSingleton<ISequenceRepository, SequenceRepository>();
                n.AddSingleton<ISubClientRepository, SubClientRepository>();
                n.AddSingleton<IWebJobRunRepository, WebJobRunRepository>();
                n.AddSingleton<IZipMapRepository, ZipMapRepository>();
                n.AddSingleton<IZipOverrideRepository, ZipOverrideRepository>();
                n.AddSingleton<IZoneMapRepository, ZoneMapRepository>();

                // processors
                n.AddSingleton<IActiveGroupProcessor, ActiveGroupProcessor>();
                n.AddSingleton<IBinProcessor, BinProcessor>();
                n.AddSingleton<IBinFileProcessor, BinFileProcessor>();                
                n.AddSingleton<IConsumerDetailFileProcessor, ConsumerDetailFileProcessor>();
                n.AddSingleton<IClientProcessor, ClientProcessor>();
                n.AddSingleton<IEodPostProcessor, EodPostProcessor>();
                n.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>();
                n.AddSingleton<IPackageDuplicateProcessor, PackageDuplicateProcessor>();
                n.AddSingleton<IPackagePostProcessor, PackagePostProcessor>();
                n.AddSingleton<IRecallReleaseProcessor, RecallReleaseProcessor>();
                n.AddSingleton<ISiteProcessor, SiteProcessor>();
                n.AddSingleton<ISequenceProcessor, SequenceProcessor>();
                n.AddSingleton<ISubClientProcessor, SubClientProcessor>();
                n.AddSingleton<IUpsDasFileProcessor, UpsDasFileProcessor>();
                n.AddSingleton<IUpsGeoDescFileProcessor, UpsGeoDescFileProcessor>();
                n.AddSingleton<IUspsShippingProcessor, UspsShippingProcessor>();
                n.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>();
                n.AddSingleton<IZipMapFileProcessor, ZipMapFileProcessor>();
                n.AddSingleton<IZipOverrideProcessor, ZipOverrideProcessor>();
                n.AddSingleton<IZoneProcessor, ZoneProcessor>();
                n.AddSingleton<IZoneFileProcessor, ZoneFileProcessor>();

                // services
                n.AddSingleton<ICreatedPackagePostProcessService, CreatedPackagePostProcessService>();
                n.AddSingleton<IConsumerDetailFileService, ConsumerDetailFileService>();
                n.AddSingleton<IDuplicateAsnCheckerService, DuplicateAsnCheckerService>();
                n.AddSingleton<IPackageRecallJobService, PackageRecallJobService>();
                n.AddSingleton<IShippingProcessor, ShippingProcessor>();
                n.AddSingleton<IUpdateActiveGroupDataService, UpdateActiveGroupDataService>();
                n.AddSingleton<IUpsGeoDescFileService, UpsGeoDescFileService>();
                n.AddSingleton<IWebJobRunsService, WebJobRunsService>();
                n.AddSingleton<IZipMapFileService, ZipMapFileService>();
                n.AddSingleton<IZoneFileService, ZoneFileService>();

                // misc services
                n.AddSingleton<IQueueManager, QueueManager>();
                n.AddSingleton<ISemaphoreManager, SemaphoreManager>();

                // web services
                n.AddSingleton<IFedExShipClient>(fedExShipClient);
                n.AddSingleton<IUpsShipClient>(upsShipClient);
                n.AddSingleton<IUpsVoidClient>(upsVoidClient);

                // external 
                n.AddTransient<IEmailService, EmailService>();
                n.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
                n.AddSingleton<IBlobHelper, BlobHelper>();
                n.AddSingleton<IFileShareHelper, FileShareHelper>();

                // Application Insights.
                n.AddApplicationInsightsTelemetryWorkerService();
            });

            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddTimers();
            });

            using (var host = builder.Build())
            {
                var is64bit = 8 == IntPtr.Size;
                var logger = host.Services.GetService<ILoggerFactory>().CreateLogger("PackageTracker.Service.Program");
                logger.LogInformation($"Service Starting: Is 64 bit: {is64bit}");
                logger.LogInformation($"DatabaseName: {configuration.GetSection("DatabaseName").Value}");

                host.Run();
            }
        }

        static LogLevel LogLevelFromString(string level)
        {
            if (level == "Trace")
                return LogLevel.Trace;
            else if (level == "Debug")
                return LogLevel.Debug;
            else if (level == "Information")
                return LogLevel.Information;
            else if (level == "Warning")
                return LogLevel.Warning;
            else if (level == "Error")
                return LogLevel.Error;
            else if (level == "Critical")
                return LogLevel.Critical;
            else return LogLevel.Information;
        }
    }
}
