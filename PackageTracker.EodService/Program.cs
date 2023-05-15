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
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data;
using PackageTracker.EodService.Interfaces;
using PackageTracker.EodService.Repositories;
using PackageTracker.EodService.Services;
using System;
using System.IO;
using IContainerDetailProcessor = PackageTracker.EodService.Interfaces.IContainerDetailProcessor;
using IEodProcessor = PackageTracker.EodService.Interfaces.IEodProcessor;
using IEvsFileProcessor = PackageTracker.EodService.Interfaces.IEvsFileProcessor;
using IExpenseProcessor = PackageTracker.EodService.Interfaces.IExpenseProcessor;
using IInvoiceProcessor = PackageTracker.EodService.Interfaces.IInvoiceProcessor;
using IPackageDetailProcessor = PackageTracker.EodService.Interfaces.IPackageDetailProcessor;
using IReturnAsnProcessor = PackageTracker.EodService.Interfaces.IReturnAsnProcessor;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using System.Data;
using System.Data.SqlClient;

namespace PackageTracker.EodService
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

                // Add DbContext for Entity Framework and configure context to connect to the Eod db
                n.AddDbContext<EodDbContext>();
                n.AddSingleton<IEodDbContextFactory, EodDbContextFactory>();

                // repositories
                n.AddSingleton<IActiveGroupRepository, ActiveGroupRepository>();
                n.AddSingleton<IBinRepository, BinRepository>();
                n.AddSingleton<IBinMapRepository, BinMapRepository>();
                n.AddSingleton<IContainerRepository, ContainerRepository>();
                n.AddSingleton<IClientRepository, ClientRepository>();
                n.AddSingleton<IEodPackageRepository, Repositories.EodPackageRepository>();
                n.AddSingleton<IEodContainerRepository, Repositories.EodContainerRepository>();
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
                n.AddSingleton<IClientProcessor, ClientProcessor>();
                n.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>();
                n.AddSingleton<IInvoiceProcessor, Processors.InvoiceProcessor>();
                n.AddSingleton<IPackagePostProcessor, PackagePostProcessor>();
                n.AddSingleton<IRateProcessor, RateProcessor>();
                n.AddSingleton<ISiteProcessor, SiteProcessor>();
                n.AddSingleton<IServiceRuleProcessor, ServiceRuleProcessor>();
                n.AddSingleton<ISequenceProcessor, SequenceProcessor>();
                n.AddSingleton<ISubClientProcessor, SubClientProcessor>();
                n.AddSingleton<IUspsShippingProcessor, UspsShippingProcessor>();
                n.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>();
                n.AddSingleton<IZipOverrideProcessor, ZipOverrideProcessor>();
                n.AddSingleton<IZoneProcessor, ZoneProcessor>();
                
                n.AddSingleton<IContainerDetailProcessor, Processors.ContainerDetailProcessor>();
                n.AddSingleton<IEodProcessor, Processors.EodProcessor>();
                n.AddSingleton<IEvsFileProcessor, Processors.EvsFileProcessor>();
                n.AddSingleton<IExpenseProcessor, Processors.ExpenseProcessor>();
                n.AddSingleton<IPackageDetailProcessor, Processors.PackageDetailProcessor>();
                n.AddSingleton<IReturnAsnProcessor, Processors.ReturnAsnProcessor>();

                // services
                n.AddSingleton<ICleanupService, CleanupService>();
                n.AddSingleton<IContainerDetailService, ContainerDetailService>();
                n.AddSingleton<IEodService, Services.EodService>();
                n.AddSingleton<IEvsFileService, EvsFileService>();
                n.AddSingleton<IInvoiceExpenseService, InvoiceExpenseService>();
                n.AddSingleton<IPackageDetailService, PackageDetailService>();
                n.AddSingleton<IReturnAsnService, ReturnAsnService>();

                // misc services
                n.AddSingleton<IQueueManager, QueueManager>();
                n.AddSingleton<ISemaphoreManager, SemaphoreManager>();

                // external 
                n.AddTransient<IEmailService, EmailService>();
                n.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
                n.AddSingleton<IBlobHelper, BlobHelper>();
                n.AddSingleton<IFileShareHelper, FileShareHelper>();
                n.AddTransient<IDbConnection>((sp) => new SqlConnection(configuration.GetConnectionString("MmsEodDb")));

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
                logger.LogInformation($"EOD Service Starting: Is 64 bit: {is64bit}");
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
