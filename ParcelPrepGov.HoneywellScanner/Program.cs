using HoneywellScanner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using ParcelPrepGov.API.Client.Services;
using ParcelPrepGov.API.Client.Interfaces;
using System.IO;

namespace ParcelPrepGov.HoneywellScanner
{
	class Program
    {
		private static IConfigurationRoot Configuration;
		private static ScannerListener ScannerListener;

		public static IConfigurationRoot BuildConfiguration()
		{
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

			return configBuilder.Build();
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


		static void Main(string[] args)
        {
			var builder = new HostBuilder();

			var appSettings = string.Empty;
			Configuration = BuildConfiguration();
			builder.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddApplicationInsights();
				logging.AddConsole();
				logging.AddDebug();
				// For some unknown reason the ApplicationInsightsLogProvider doesn't correctly pick this up from appsettings,
				//  so we need to do this:
				logging.AddFilter<ApplicationInsightsLoggerProvider>("",
					LogLevelFromString(Configuration["Logging:ApplicationInsights:LogLevel:Default"]));
			});

			builder.ConfigureServices(services =>
			{
				// Application Insights.
				services.AddApplicationInsightsTelemetryWorkerService();

				services.AddSingleton<IConfiguration>(Configuration);
				services.AddSingleton<IErrorLogger, ErrorLogger>();
				services.AddSingleton<IAccountService, AccountService>();
				services.AddSingleton<IContainerService, ContainerService>();
			});

			using (var host = builder.Build())
			{
				host.Services.GetService<ILoggerFactory>().CreateLogger("ParcelPrepGov.HoneywellScanner")
					.LogInformation("Starting");
				string ip = Configuration.GetSection("HoneywellScanner").GetValue<string>("Ip");
				uint port = Configuration.GetSection("HoneywellScanner").GetValue<uint>("Port");
				uint activityTimeout = Configuration.GetSection("HoneywellScanner").GetValue<uint>("ActivityTimeout");
				if (port != 0)
				{
					ScannerListener = new ScannerListener(host.Services, ip, port, activityTimeout);

				}
				host.Run();
			}
		}
	}
}
