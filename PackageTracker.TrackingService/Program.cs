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
using PackageTracker.TrackingService.Interfaces;
using ParcelPrepGov.Reports;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Repositories;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace PackageTracker.TrackingService
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

                // reports db context
                n.AddSingleton<IPpgReportsDbContextFactory, PpgReportsDbContextFactory>();

                // reports repos
                n.AddSingleton<IBinDatasetProcessor, BinDatasetProcessor>();
                n.AddSingleton<IBinDatasetRepository, BinDatasetRepository>();
                n.AddSingleton<ICarrierEventCodeRepository, CarrierEventCodeRepository>();
                n.AddSingleton<IEvsCodeRepository, EvsCodeRepository>();
                n.AddSingleton<IJobDatasetProcessor, JobDatasetProcessor>();
                n.AddSingleton<IJobDatasetRepository, JobDatasetRepository>();
                n.AddSingleton<IPackageDatasetProcessor, PackageDatasetProcessor>();
                n.AddSingleton<IPackageDatasetRepository, PackageDatasetRepository>();
                n.AddSingleton<IPackageEventDatasetRepository, PackageEventDatasetRepository>();
                n.AddSingleton<IPackageSearchProcessor, PackageSearchProcessor>();
                n.AddSingleton<IRecallStatusRepository, RecallStatusRepository>();
                n.AddSingleton<IShippingContainerDatasetProcessor, ShippingContainerDatasetProcessor>();
                n.AddSingleton<IShippingContainerDatasetRepository, ShippingContainerDatasetRepository>();
                n.AddSingleton<ISubClientDatasetProcessor, SubClientDatasetProcessor>();
                n.AddSingleton<ISubClientDatasetRepository, SubClientDatasetRepository>();
                n.AddSingleton<ITrackPackageDatasetProcessor, TrackPackageDatasetProcessor>();
                n.AddSingleton<ITrackPackageDatasetRepository, TrackPackageDatasetRepository>();
                n.AddSingleton<IVisnSiteRepository, VisnSiteRepository>();
                n.AddSingleton<IPostalDaysRepository, PostalDaysRepository>();

                // repos
                n.AddSingleton<IActiveGroupRepository, ActiveGroupRepository>();
                n.AddSingleton<IBinRepository, BinRepository>();
                n.AddSingleton<IBinMapRepository, BinMapRepository>();
                n.AddSingleton<IContainerRepository, ContainerRepository>();
                n.AddSingleton<IFileConfigurationRepository, FileConfigurationRepository>();
                n.AddSingleton<IJobRepository, JobRepository>();
                n.AddSingleton<IJobOptionRepository, JobOptionRepository>();
                n.AddSingleton<IPackageRepository, PackageRepository>();
                n.AddSingleton<ISequenceRepository, SequenceRepository>();
                n.AddSingleton<ISiteRepository, SiteRepository>();
                n.AddSingleton<ISubClientRepository, SubClientRepository>();
                n.AddSingleton<IWebJobRunRepository, WebJobRunRepository>();

                // processors
                n.AddSingleton<IActiveGroupProcessor, ActiveGroupProcessor>();
                n.AddSingleton<IBinProcessor, BinProcessor>();
                n.AddSingleton<IDatasetProcessor, DatasetProcessor>();
                n.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>();
                n.AddSingleton<IJobUpdateProcessor, JobUpdateProcessor>();
                n.AddSingleton<ISiteProcessor, SiteProcessor>();
                n.AddSingleton<ISubClientProcessor, SubClientProcessor>();
                n.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>();

                // misc services
                n.AddSingleton<ISemaphoreManager, SemaphoreManager>();

                // services
                n.AddSingleton<IReportService, ReportService>();
                n.AddSingleton<ITrackPackageService, TrackPackageService>();


                // Necessary for web services.
                MethodInfo method = typeof(XmlSerializer).GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                method.Invoke(null, new object[] { 1 });

                // external 
                n.AddTransient<IEmailService, EmailService>();
                n.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
                n.AddSingleton<IBlobHelper, BlobHelper>();
                n.AddSingleton<IFileShareHelper, FileShareHelper>();
                n.AddHttpClient<ITrackPackageProcessor, TrackPackageProcessor>();
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
                    var logger = host.Services.GetService<ILoggerFactory>().CreateLogger("PackageTracker.TrackingService.Program");
                    logger.LogInformation($"Tracking Service Starting: Is 64 bit: {is64bit}");
                    logger.LogInformation($"DatabaseName: {configuration.GetSection("DatabaseName").Value}");

                    host.Run();
                }
            }
            catch (Exception ex)
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

