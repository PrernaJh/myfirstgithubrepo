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
using System;
using System.IO;

namespace PackageTracker.AsnService
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

			builder.ConfigureServices(n =>
			{
				// extensions
				n.AddCosmosDbToService(ref configuration, appSettings); // Make sure this is first since it may update 'configuration'
				n.AddBlobStorageAccount(configuration.GetSection("AzureWebJobsStorage").Value);
				n.AddFileShareStorageAccount(configuration.GetSection("AzureWebJobsStorage").Value);

				// repositories
				n.AddSingleton<IActiveGroupRepository, ActiveGroupRepository>();
				n.AddSingleton<IBinRepository, BinRepository>();
				n.AddSingleton<IBinMapRepository, BinMapRepository>();
				n.AddSingleton<IFileConfigurationRepository, FileConfigurationRepository>();
				n.AddSingleton<IPackageRepository, PackageRepository>();
				n.AddSingleton<ISiteRepository, SiteRepository>();				
				n.AddSingleton<ISequenceRepository, SequenceRepository>();
				n.AddSingleton<ISubClientRepository, SubClientRepository>();
				n.AddSingleton<IWebJobRunRepository, WebJobRunRepository>();
				n.AddSingleton<IZipMapRepository, ZipMapRepository>();
				n.AddSingleton<IZipOverrideRepository, ZipOverrideRepository>();
				n.AddSingleton<IZoneMapRepository, ZoneMapRepository>();

				// processors
				n.AddSingleton<IActiveGroupProcessor, ActiveGroupProcessor>();
				n.AddSingleton<IAsnFileProcessor, AsnFileProcessor>();
				n.AddSingleton<IBinProcessor, BinProcessor>();
				n.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>();
				n.AddSingleton<IPackageDuplicateProcessor, PackageDuplicateProcessor>();
				n.AddSingleton<ISiteProcessor, SiteProcessor>();
				n.AddSingleton<ISequenceProcessor, SequenceProcessor>();
				n.AddSingleton<ISubClientProcessor, SubClientProcessor>();
				n.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>();
				n.AddSingleton<IZipMapProcessor, ZipMapProcessor>();
				n.AddSingleton<IZipOverrideProcessor, ZipOverrideProcessor>();
				n.AddSingleton<IZoneProcessor, ZoneProcessor>();

				// services
				n.AddSingleton<IAsnFileService, AsnFileService>();

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
				logger.LogInformation($"ASN Service Starting: Is 64 bit: {is64bit}");
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
