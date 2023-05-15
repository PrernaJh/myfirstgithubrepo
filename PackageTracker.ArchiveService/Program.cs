using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications;
using PackageTracker.Communications.Interfaces;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.ArchiveService.Interfaces;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Repositories;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using PackageTracker.Domain;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data;
using Microsoft.Extensions.Caching.Distributed;

namespace PackageTracker.ArchiveService
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder();

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(dir);

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

            builder.ConfigureServices(n =>
            {
                // extensions
                n.AddCosmosDbToService(ref configuration, appSettings); // Make sure this is first since it may update 'configuration'
                var cloudStorageAccountSettings = configuration.GetSection("AzureWebJobsStorage").Value;
                n.AddBlobStorageAccount(cloudStorageAccountSettings);
                n.AddFileShareStorageAccount(cloudStorageAccountSettings);
                n.AddQueueStorageAccount(cloudStorageAccountSettings);

                // ppgreports
                n.AddSingleton<IPpgReportsDbContextFactory, PpgReportsDbContextFactory>();
                n.AddSingleton<IPackageDatasetRepository, PackageDatasetRepository>();
                n.AddSingleton<IRecallStatusRepository, RecallStatusRepository>();
                n.AddSingleton<IShippingContainerDatasetRepository, ShippingContainerDatasetRepository>();

                // repositories
                n.AddSingleton<IActiveGroupRepository, ActiveGroupRepository>();
                n.AddSingleton<IBinRepository, BinRepository>();
                n.AddSingleton<IBinMapRepository, BinMapRepository>();
                n.AddSingleton<IFileConfigurationRepository, FileConfigurationRepository>();
                n.AddSingleton<IPostalDaysRepository, PostalDaysRepository>();
                n.AddSingleton<IReportsRepository, ReportsRepository>();
                n.AddSingleton<ISiteRepository, SiteRepository>();
                n.AddSingleton<ISubClientRepository, SubClientRepository>();
                n.AddSingleton<IWebJobRunRepository, WebJobRunRepository>();

                // processors
                n.AddSingleton<IActiveGroupProcessor, ActiveGroupProcessor>();
                n.AddSingleton<IArchiveDataProcessor, ArchiveDataProcessor>();
                n.AddSingleton<IBinProcessor, BinProcessor>();
                n.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>();
                n.AddSingleton<IHistoricalDataProcessor, HistoricalDataProcessor>();
                n.AddSingleton<ISiteProcessor, SiteProcessor>();
                n.AddSingleton<ISubClientProcessor, SubClientProcessor>();
                n.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>();

                // misc services
                n.AddSingleton<IDistributedCache, DummyCache>();
                n.AddSingleton<ISemaphoreManager, SemaphoreManager>();

                // services
                n.AddSingleton<IArchiveDataService, ArchiveDataService>();
                n.AddSingleton<IHistoricalDataService, HistoricalDataService>();
                n.AddSingleton<IReportService, ReportService>();

                // Necessary for web services.
                MethodInfo method = typeof(XmlSerializer).GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                method.Invoke(null, new object[] { 1 });

                // external 
                n.AddTransient<IEmailService, EmailService>();
                n.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
                n.AddSingleton<IBlobHelper, BlobHelper>();
                n.AddSingleton<IFileShareHelper, FileShareHelper>();
                n.AddTransient<IDbConnection>((sp) => new SqlConnection(configuration.GetConnectionString("PpgReportsDb")));

                // Application Insights.
                n.AddApplicationInsightsTelemetryWorkerService();
            });

            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddTimers();
            });
            try
            {
                using (var host = builder.Build())
                {
                    var is64bit = 8 == IntPtr.Size;
                    var logger = host.Services.GetService<ILoggerFactory>().CreateLogger("PackageTracker.ArchiveService.Program");
                    logger.LogInformation($"Archive Service Starting: Is 64 bit: {is64bit}");
                    logger.LogInformation($"DatabaseName: {configuration.GetSection("DatabaseName").Value}");

                    host.Run();
                }
            }catch(Exception ex)
            {
                throw;
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
