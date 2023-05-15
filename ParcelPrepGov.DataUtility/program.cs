using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using MMS.Web.Domain.Interfaces;
using MMS.Web.Domain.Processors;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications;
using PackageTracker.Communications.Interfaces;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Data;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Models.JobOptions;
using PackageTracker.Data.Models.ReturnOptions;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Models.TrackPackages.Ups;
using PackageTracker.Domain.Processors;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using PackageTracker.EodService.Repositories;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

using IContainerDetailProcessor = PackageTracker.EodService.Interfaces.IContainerDetailProcessor;
using IEodProcessor = PackageTracker.EodService.Interfaces.IEodProcessor;
using IEvsFileProcessor = PackageTracker.EodService.Interfaces.IEvsFileProcessor;
using IExpenseProcessor = PackageTracker.EodService.Interfaces.IExpenseProcessor;
using IInvoiceProcessor = PackageTracker.EodService.Interfaces.IInvoiceProcessor;
using IPackageDetailProcessor = PackageTracker.EodService.Interfaces.IPackageDetailProcessor;
using IReturnAsnProcessor = PackageTracker.EodService.Interfaces.IReturnAsnProcessor;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

using EodContainerRepository = PackageTracker.EodService.Repositories.EodContainerRepository;
using EodPackageRepository = PackageTracker.EodService.Repositories.EodPackageRepository;
using ContainerDetailProcessor = PackageTracker.EodService.Processors.ContainerDetailProcessor;
using EodProcessor = PackageTracker.EodService.Processors.EodProcessor;
using EvsFileProcessor = PackageTracker.EodService.Processors.EvsFileProcessor;
using ExpenseProcessor = PackageTracker.EodService.Processors.ExpenseProcessor;
using InvoiceProcessor = PackageTracker.EodService.Processors.InvoiceProcessor;
using PackageDetailProcessor = PackageTracker.EodService.Processors.PackageDetailProcessor;
using ReturnAsnProcessor = PackageTracker.EodService.Processors.ReturnAsnProcessor;

namespace ParcelPrepGov.DataUtility
{
    class Program
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        public static IConfigurationRoot Configuration;

        // The Azure Cosmos DB endpoint
        private static Uri EndpointUri { get; set; }

        // The primary key for the Azure Cosmos account
        private static string PrimaryKey { get; set; }

        // The name of the database
        private static string DatabaseId { get; set; }
        private static bool Production { get; set; }

        public static (IConfigurationRoot, string) BuildConfiguration()
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

            return (configBuilder.Build(), appSettings);
        }

        public static ServiceProvider ConfigureServices(IServiceCollection services, string appSettings)
        {
            services
                .AddLogging(logging => logging.AddConsole())

                // extensions
                .AddCosmosDbToDataUtility(ref Configuration, appSettings)
                .AddBlobStorageAccount(Configuration.GetSection("AzureWebJobsStorage").Value)
                .AddFileShareStorageAccount(Configuration.GetSection("AzureWebJobsStorage").Value)

                // ppgreports
                .AddSingleton<IPpgReportsDbContextFactory, PpgReportsDbContextFactory>()
                .AddSingleton<ICarrierEventCodeRepository, CarrierEventCodeRepository>()
                .AddSingleton<IEvsCodeRepository, EvsCodeRepository>()
                .AddSingleton<IPostalAreaAndDistrictRepository, PostalAreaAndDistrictRepository>()
                .AddSingleton<IPostalDaysRepository, PostalDaysRepository>()
                .AddSingleton<IVisnSiteRepository, VisnSiteRepository>()

                // repositories
                .AddSingleton<IActiveGroupRepository, ActiveGroupRepository>()
                .AddSingleton<IBinDatasetRepository, BinDatasetRepository>()
                .AddSingleton<IBinRepository, BinRepository>()
                .AddSingleton<IBinMapRepository, BinMapRepository>()
                .AddSingleton<IBulkRepository, BulkRepository>()
                .AddSingleton<IClientRepository, ClientRepository>()
                .AddSingleton<IContainerRepository, ContainerRepository>()
                .AddSingleton<IOperationalContainerRepository, OperationalContainerRepository>()
                .AddSingleton<IFileConfigurationRepository, FileConfigurationRepository>()
                .AddSingleton<IJobRepository, JobRepository>()
                .AddSingleton<IPackageRepository, PackageRepository>()
                .AddSingleton<IRateRepository, RateRepository>()
                .AddSingleton<IRecallStatusRepository, RecallStatusRepository>()
                .AddSingleton<ISiteRepository, SiteRepository>()
                .AddSingleton<IServiceRuleRepository, ServiceRuleRepository>()
                .AddSingleton<IServiceRuleExtensionRepository, ServiceRuleExtensionRepository>()
                .AddSingleton<ISequenceRepository, SequenceRepository>()
                .AddSingleton<ISubClientRepository, SubClientRepository>()
                .AddSingleton<IWebJobRunRepository, WebJobRunRepository>()
                .AddSingleton<IZipMapRepository, ZipMapRepository>()
                .AddSingleton<IZipOverrideRepository, ZipOverrideRepository>()
                .AddSingleton<IZoneMapRepository, ZoneMapRepository>()

				// processors
				.AddSingleton<IActiveGroupProcessor, ActiveGroupProcessor>()
				.AddSingleton<IAsnFileProcessor, AsnFileProcessor>()
				.AddSingleton<IBinDatasetProcessor, BinDatasetProcessor>()
				.AddSingleton<IBinProcessor, BinProcessor>()
				.AddSingleton<IBinFileProcessor, BinFileProcessor>()
				.AddSingleton<IBulkProcessor, BulkProcessor>()
				.AddSingleton<IConsumerDetailFileProcessor, ConsumerDetailFileProcessor>()
				.AddSingleton<IClientProcessor, ClientProcessor>()
				.AddSingleton<IDatasetProcessor, DatasetProcessor>()
                .AddSingleton<IContainerRateFileProcessor, ContainerRateFileProcessor>()
				.AddSingleton<IFileConfigurationProcessor, FileConfigurationProcessor>()
				.AddSingleton<IJobUpdateProcessor, JobUpdateProcessor>()
				.AddSingleton<IPackageDatasetProcessor, PackageDatasetProcessor>()
				.AddSingleton<IPackagePostProcessor, PackagePostProcessor>()
				.AddSingleton<IRateProcessor, RateProcessor>()
				.AddSingleton<IRateFileProcessor, RateFileProcessor>()
				.AddSingleton<IReturnAsnProcessor, ReturnAsnProcessor>()
				.AddSingleton<ISiteProcessor, SiteProcessor>()
				.AddSingleton<IServiceRuleProcessor, ServiceRuleProcessor>()
				.AddSingleton<IServiceRuleExtensionFileProcessor, ServiceRuleExtensionFileProcessor>()
                .AddSingleton<IUpsGeoDescFileProcessor, UpsGeoDescFileProcessor>()
                .AddSingleton<ISequenceProcessor, SequenceProcessor>()
				.AddSingleton<ISubClientProcessor, SubClientProcessor>()
				.AddSingleton<IUpsDasFileProcessor, UpsDasFileProcessor>()
				.AddSingleton<IUspsEvsProcessor, UspsEvsProcessor>()
				.AddSingleton<IWebJobRunProcessor, WebJobRunProcessor>()
				.AddSingleton<IZipMapFileProcessor, ZipMapFileProcessor>()
				.AddSingleton<IZipOverrideProcessor, ZipOverrideProcessor>()
				.AddSingleton<IZipOverrideWebProcessor, ZipOverrideWebProcessor>()
				.AddSingleton<IZipMapProcessor, ZipMapProcessor>()
				.AddSingleton<IZoneFileProcessor, ZoneFileProcessor>()
				.AddSingleton<IZoneProcessor, ZoneProcessor>();

            services
                .AddSingleton<IAsnFileProcessor, AsnFileProcessor>()
                .AddSingleton<IBlobHelper, BlobHelper>()
                .AddSingleton<IPackageDatasetRepository, PackageDatasetRepository>()
                .AddSingleton<IPackageEventDatasetRepository, PackageEventDatasetRepository>()
                .AddSingleton<ISemaphoreManager, SemaphoreManager>()
                .AddSingleton<IShippingContainerDatasetRepository, ShippingContainerDatasetRepository>()
                .AddSingleton<ITrackPackageDatasetProcessor, TrackPackageDatasetProcessor>()
                .AddSingleton<ITrackPackageDatasetRepository, TrackPackageDatasetRepository>()
                .AddHttpClient<ITrackPackageProcessor, TrackPackageProcessor>();

            // external 
            services
                .AddTransient<IEmailService, EmailService>()
                .AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

            // Eod
            services
                .AddTransient<IDbConnection>((sp) => new SqlConnection(Configuration.GetConnectionString("MmsEodDb")))
                .AddSingleton<IEodDbContextFactory, EodDbContextFactory>()
                .AddDbContext<EodDbContext>()
                .AddSingleton<IEodContainerRepository, EodContainerRepository>()
                .AddSingleton<IEodPackageRepository, EodPackageRepository>()
                .AddSingleton<IEodProcessor, EodProcessor>()
                .AddSingleton<IContainerDetailProcessor, ContainerDetailProcessor>()
                .AddSingleton<IEvsFileProcessor, EvsFileProcessor>()
                .AddSingleton<IExpenseProcessor, ExpenseProcessor>()
                .AddSingleton<IInvoiceProcessor, InvoiceProcessor>()
                .AddSingleton<IPackageDetailProcessor, PackageDetailProcessor>()
                .AddSingleton<IReturnAsnProcessor, ReturnAsnProcessor>();

            return services.BuildServiceProvider();
        }

		static void Usage()
		{
			Console.WriteLine("Usage:");
			if (!Production)
			{
				Console.WriteLine("\tdelete\t\t\t\t\t\t-- Delete existing database");
				Console.WriteLine("\tcreate\t\t\t\t\t\t-- Create new database");
                Console.WriteLine("\tinitialize\t\t\t\t\t-- Load database with sites, subClients, etc.");
                Console.WriteLine("\tmap\t\t\t\t\t\t-- Load database with bins, bin maps, etc.");
                Console.WriteLine("\tbackup <Backup Database Id>\t\t\t-- backup database to backup to");
				Console.WriteLine("\trestore <Backup Database Id>\t\t\t-- backup database restore from");
				Console.WriteLine("\tempty <Container Id>\t\t\t\t-- remove all items from container");
				Console.WriteLine("\tcleanup\t\t\t\t\t\t-- cleanup containers (bins, binMaps, etc.)");
				Console.WriteLine("\tcleanup <Container Id> <query>\t\t\t-- cleanup selected container");
				Console.WriteLine("\timport <Container Id>\t\t\t\t-- import Cosmos items to container");
				Console.WriteLine("\timport exports\t\t\t\t\t-- import exported Cosmos items");
			}
			Console.WriteLine("\timport reports\t\t\t\t\t-- import tables to reports DB");
			Console.WriteLine("\texport <container Id> <query>\t\t\t-- export Cosmos items to folder");
			Console.WriteLine("\ttouch packages <site> <date>\t\t\t-- touch packages so they get pushed to eod/reports DB");
			Console.WriteLine("\tsearch <BLOB Container> <Search String> <FileName Match> -- search selected BLOB container");
			Console.WriteLine("\tprocess <BLOB Container> <FileName Match>\t-- re-process archived scandata");
			Console.WriteLine("\tdownload <BLOB Container> <FileName Match>\t-- download files from BLOB container");
			Console.WriteLine("\tcheck <Container Id>\t\t\t\t-- check if items have been imported to reports DB");
			Console.WriteLine("\tconsumerDetailFile <subClient> <date>\t\t-- create consumer detail file for date");
			Console.WriteLine("\tpackageDetailFile <site> <date>\t\t\t-- create package detail file for date");
			Console.WriteLine("\tcontainerDetailFile <site> <date>\t\t-- create container detail file for date");
			Console.WriteLine("\tpmodContainerDetailFile <site> <date>\t\t-- create PMOD container detail file for date");
			Console.WriteLine("\treturnAsnFile <subClient> <date>\t\t-- create asn return file for date");
			Console.WriteLine("\texpenseFile <subClient> <date> [WEEKLY] [MONTHLY] -- create expense file file for date");
			Console.WriteLine("\tinvoiceFile <subClient> <date> [WEEKLY] [MONTHLY] -- create invoice file file for date");
			Console.WriteLine("\tevsFile <site> <date>\t\t\t\t-- create evs file for site and date using packages collection");
			Console.WriteLine("\tcheckEod <site> <date>\t\t\t\t-- check Eod status for site and date");
			Console.WriteLine("\tcheckEvs <site> <date> [<check file>]\t\t-- create evs file for site and date using EODPackages collection");
			Console.WriteLine("\tevsPmodFile <site> <date> [<check file>]\t-- create evs PMOD file for site and date using containers collection");
			Console.WriteLine("\tcheckEvsPmod <site> <date> [<check file>]\t-- create evs PMOD file for site and date using EODContainers collection");
			Console.WriteLine("\trebuildEod <site> <date>\t\t\t-- rebuild Eod SQL collections for site and date");
			Console.WriteLine("\tpopulateSqlEod <site> <date>\t\t\t-- copy Cosmos Eod collections to SQL for site and date");
			Console.WriteLine("\tcheckSqlEod <site> <date>\t\t\t-- compare Eod SQL vs Cosmos collections for site and date");
			Console.WriteLine("\tupdateBins <subClient> <date>\t\t\t-- re-bin processed packages for date");
			Console.WriteLine("\tupdateBinDatasets <site> <date>\t\t\t-- update SQL bin datasets from Comsos for site which are newer than this date");
			Console.WriteLine("\tlistFiles <file share path>\t\t\t-- list files in file share");
			Console.WriteLine("\tdeleteObsoleteContainers <date>\t\t\t-- delete active containers before this date");
			Console.WriteLine("\timportAsnFile <subClient> <file>\t\t-- import ASN file");
			Console.WriteLine("\tupdateUspsTrackingData <site> <lookback min> <lookback max> -- lookup tracking data for packages");
			Console.WriteLine("\tratePackages <subClient> <date>\t\t\t-- re-rate processed packages for date");

			Console.WriteLine("\tcheckContainerDetail2 <old file> <new file>\t-- compare two Container Detail files");
			Console.WriteLine("\tcheckPmodContainerDetail2 <old file> <new file>\t-- compare two PMOD Container Detail files");
			Console.WriteLine("\tcheckPackageDetail2 <old file> <new file>\t-- compare two Package Detail files");
			Console.WriteLine("\tcheckReturnAsn2 <old file> <new file>\t\t-- compare two Return ASN files");
			Console.WriteLine("\tcheckInvoice2 <old file> <new file>\t\t-- compare two Invoice files");
			Console.WriteLine("\tcheckExpense2 <old file> <new file>\t\t-- compare two Expense files");
			Console.WriteLine("\tcheckEvs2 <old file> <new file>\t\t\t-- compare two evs files");
        }

        // <Main>
        public static async Task Main(string[] args)
        {
            try
            {
                var appSettings = string.Empty;
                (Configuration, appSettings) = BuildConfiguration();

                var cosmosDbConnectionStringOptions = Configuration.GetSection("ConnectionStrings").Get<ConnectionStringOptions>();
                (EndpointUri, PrimaryKey) = cosmosDbConnectionStringOptions;
                DatabaseId = Configuration.GetSection("DatabaseName").Value;
                Production = DatabaseId.Contains("ppgpro", StringComparison.InvariantCultureIgnoreCase);

				Program p = new Program();
				if (args.Length == 0)
				{
					Usage();
					return;
				}
				bool createDB = args.FirstOrDefault(x => x == "create") != null;
				bool deleteDB = args.FirstOrDefault(x => x == "delete") != null;
				bool initializeDB = args.FirstOrDefault(x => x == "initialize") != null;
                bool mapDB = args.FirstOrDefault(x => x == "map") != null;
                bool backupDB = args.FirstOrDefault(x => x == "backup") != null;
				bool restoreDB = args.FirstOrDefault(x => x == "restore") != null;
				bool empty = args.FirstOrDefault(x => x == "empty") != null;
				bool import = args.FirstOrDefault(x => x == "import") != null;
				bool cleanup = args.FirstOrDefault(x => x == "cleanup") != null;
				bool search = args.FirstOrDefault(x => x == "search") != null;
				bool process = args.FirstOrDefault(x => x == "process") != null;
				bool download = args.FirstOrDefault(x => x == "download") != null;
				bool check = args.FirstOrDefault(x => x == "check") != null;
				bool consumerDetailFile = args.FirstOrDefault(x => x == "consumerDetailFile") != null;
				bool packageDetailFile = args.FirstOrDefault(x => x == "packageDetailFile") != null;
				bool containerDetailFile = args.FirstOrDefault(x => x == "containerDetailFile") != null;
				bool pmodContainerDetailFile = args.FirstOrDefault(x => x == "pmodContainerDetailFile") != null;
				bool returnAsnFile = args.FirstOrDefault(x => x == "returnAsnFile") != null;
				bool expenseFile = args.FirstOrDefault(x => x == "expenseFile") != null;
				bool invoiceFile = args.FirstOrDefault(x => x == "invoiceFile") != null;
				bool evsFile = args.FirstOrDefault(x => x == "evsFile") != null;
				bool checkEvs = args.FirstOrDefault(x => x == "checkEvs") != null;
				bool evsPmodFile = args.FirstOrDefault(x => x == "evsPmodFile") != null;
				bool checkEvsPmod = args.FirstOrDefault(x => x == "checkEvsPmod") != null;
				bool touch = args.FirstOrDefault(x => x == "touch") != null;
				bool checkEod = args.FirstOrDefault(x => x == "checkEod") != null;
				bool checkEod2 = args.FirstOrDefault(x => x == "checkEod2") != null;
				bool rebuildEod = args.FirstOrDefault(x => x == "rebuildEod") != null;
				bool populateSqlEod = args.FirstOrDefault(x => x == "populateSqlEod") != null;
				bool checkSqlEod = args.FirstOrDefault(x => x == "checkSqlEod") != null;
				bool export = args.FirstOrDefault(x => x == "export") != null;
				bool updateBins = args.FirstOrDefault(x => x == "updateBins") != null;
				bool updateBinDatasets = args.FirstOrDefault(x => x == "updateBinDatasets") != null;
				bool listFiles = args.FirstOrDefault(x => x == "listFiles") != null;
				bool deleteObsoleteContainers = args.FirstOrDefault(x => x == "deleteObsoleteContainers") != null;
				bool importAsnFile = args.FirstOrDefault(x => x == "importAsnFile") != null;
				bool updateUspsTrackingData = args.FirstOrDefault(x => x == "updateUspsTrackingData") != null;
				bool ratePackages = args.FirstOrDefault(x => x == "ratePackages") != null;

				bool checkContainerDetail2 = args.FirstOrDefault(x => x == "checkContainerDetail2") != null;
				bool checkPmodContainerDetail2 = args.FirstOrDefault(x => x == "checkPmodContainerDetail2") != null;
				bool checkPackageDetail2 = args.FirstOrDefault(x => x == "checkPackageDetail2") != null;
				bool checkReturnAsn2 = args.FirstOrDefault(x => x == "checkReturnAsn2") != null;
				bool checkInvoice2 = args.FirstOrDefault(x => x == "checkInvoice2") != null;
				bool checkExpense2 = args.FirstOrDefault(x => x == "checkExpense2") != null;
				bool checkEvs2 = args.FirstOrDefault(x => x == "checkEvs2") != null;

				bool fix = args.FirstOrDefault(x => x == "fix") != null;

                if (Production &&
                    (deleteDB || createDB || backupDB || restoreDB || initializeDB || mapDB || empty || cleanup))
                {
                    Usage();
                    return;
                }

                if (deleteDB)
                {
                    await p.DeleteDBAsync(DatabaseId);
                }
                if (createDB)
                {
                    await p.CreateDBAsync(DatabaseId);
                }
                if (backupDB)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "backup");
                    if (++arginx >= args.Length) { Usage(); return; }
                    await p.CopyDBAsync(DatabaseId, args[arginx]);
                    return;
                }
                if (restoreDB)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "restore");
                    if (++arginx >= args.Length) { Usage(); return; }
                    await p.CopyDBAsync(args[arginx], DatabaseId);
                    return;
                }


                ServiceProvider = ConfigureServices(new ServiceCollection(), appSettings);
                if (initializeDB)
                {
                    await p.PopulateDBAsync();
                }
                if (mapDB)
                {
                    await p.MapDBAsync();
                }
                if (empty)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "empty");
                    if (++arginx >= args.Length) { Usage(); return; }
                    await p.EmptyContainerAsync(DatabaseId, args[arginx++]);
                }
                if (import)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "import");
                    if (++arginx >= args.Length) { Usage(); return; }
                    var containerId = args[arginx++];
                    if (containerId == "reports")
                        await p.ImportToReportsTablesAsync();
                    else if (Production)
                        Usage();
                    if (containerId == "exports")
                        await p.ImportExportsAsync();
                    else
                        await p.ImportToContainerAsync(containerId);
                }
                if (export)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "export");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var containerId = args[arginx++];
                    var query = args[arginx++];
                    await p.ExportFromContainerAsync(containerId, query);
                }
                if (cleanup)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "cleanup");
                    if (++arginx < args.Length)
                    {
                        var containerId = args[arginx++];
                        var query = arginx < args.Length ? args[arginx++] : null;
                        await p.CleanupContainerAsync(DatabaseId, containerId, query);
                    }
                    else
                    {
                        await p.CleanupContainersAsync(DatabaseId);
                    }
                }
                if (touch)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "touch");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var site = args[arginx++];
                    var date = args[arginx++];
                    var count = arginx < args.Length ? args[arginx++] : null;
                    await p.TouchPackagesAsync(site, date, count);
                }
                if (search)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "search");
                    if (++arginx >= args.Length - 2) { Usage(); return; }
                    var containerName = args[arginx++];
                    var searchString = args[arginx++];
                    var fileNameMatch = args[arginx++];
                    await p.SearchBlobContainerAsync(DatabaseId, containerName, searchString, fileNameMatch);
                }
                if (process)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "process");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var containerName = args[arginx++];
                    var fileNameMatch = args[arginx++];
                    await p.ProcessBlobContainerAsync(DatabaseId, containerName, fileNameMatch);
                }
                if (download)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "download");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var containerName = args[arginx++];
                    var fileNameMatch = args[arginx++];
                    var extraArg = arginx < args.Length ? args[arginx++] : string.Empty;
                    await p.DownloadBlobContainerAsync(DatabaseId, containerName, fileNameMatch, extraArg);
                }
                if (check)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "check");
                    if (++arginx >= args.Length) { Usage(); return; }
                    var containerId = args[arginx++];
                    await p.CheckAsync(containerId);
                }
				if (consumerDetailFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "consumerDetailFile");
					if (++arginx >= args.Length -1 ) { Usage(); return; }
					var subClient = args[arginx++];
					var date = args[arginx++];
					await p.ConsumerDetailFileAsync(subClient, date);
				}
				if (packageDetailFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "packageDetailFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					await p.PackageDetailFileAsync(site, date);
				}
                if (containerDetailFile)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "containerDetailFile");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var site = args[arginx++];
                    var date = args[arginx++];
                    await p.ContainerDetailFileAsync(site, date);
                }
                if (pmodContainerDetailFile)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "pmodContainerDetailFile");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var site = args[arginx++];
                    var date = args[arginx++];
                    await p.PmodContainerDetailFileAsync(site, date);
                }
                if (returnAsnFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "returnAsnFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var subClient = args[arginx++];
					var date = args[arginx++];
					await p.ReturnAsnFileAsync(subClient, date);
				}
				if (expenseFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "expenseFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var subClient = args[arginx++];
					var date = args[arginx++];
					var extraArg = arginx < args.Length ? args[arginx++] : string.Empty;
					await p.ExpenseFileAsync(subClient, date, extraArg);
				}
				if (invoiceFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "invoiceFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var subClient = args[arginx++];
					var date = args[arginx++];
					var extraArg = arginx < args.Length ? args[arginx++] : string.Empty;
					await p.InvoiceFileAsync(subClient, date, extraArg);
				}
				if (evsFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "evsFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					await p.EvsFileAsync(site, date);
				}
				if (checkEvs)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkEvs");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					var checkFilePath = arginx < args.Length ? args[arginx++] : null;
					await p.CheckEvsAsync(site, date, checkFilePath);
				}

				if (evsPmodFile)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "evsPmodFile");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					await p.EvsPmodFileAsync(site, date);
				}
				if (checkEvsPmod)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkEvsPmod");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					var checkFilePath = arginx < args.Length ? args[arginx++] : null;
					await p.CheckEvsPmodAsync(site, date, checkFilePath);
				}
                if (updateBins)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "updateBins");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var subClient = args[arginx++];
                    var date = args[arginx++];
                    await p.UpdateBinsAsync(subClient, date);
                }
                if (updateBinDatasets)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "updateBinDatasets");
                    if (++arginx >= args.Length -1) { Usage(); return; }
                    var site = args[arginx++];
                    var date = args[arginx++];
                    await p.UpdateBinDatasetsAsync(site, date);
                }
                if (listFiles)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "listFiles");
                    if (++arginx >= args.Length) { Usage(); return; }
                    var path = args[arginx++];
                    await p.ListFilesAsync(path);
                }
                if (deleteObsoleteContainers)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "deleteObsoleteContainers");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var siteName = args[arginx++];
                    var date = args[arginx++];
                    await p.DeleteObsoleteContainersAsync(siteName, date);
                }
                if (importAsnFile)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "importAsnFile");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var subClientName = args[arginx++];
                    var fileName = args[arginx++];
                    await p.ImportAsnFileAsync(subClientName, fileName);
                }
                if (updateUspsTrackingData)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "updateUspsTrackingData");
                    if (++arginx >= args.Length - 2) { Usage(); return; }
                    var siteName = args[arginx++];
                    var minString = args[arginx++];
                    var maxString = args[arginx++];
                    await p.UpdateUspsTrackingDataAsync(siteName, minString, maxString);
                }
                if (ratePackages)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "ratePackages");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var subClient = args[arginx++];
                    var date = args[arginx++];
                    await p.RatePackagesAsync(subClient, date);
                }

                if (checkEod)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkEod");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var site = args[arginx++];
					var date = args[arginx++];
					await p.CheckEodAsync(site, date);
				}
				if (checkEod2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkEod2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var name = args[arginx++];
					var date = args[arginx++];
					await p.CheckEod2Async(name, date);
				}
				if (rebuildEod)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "rebuildEod");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var name = args[arginx++];
					var date = args[arginx++];
					await p.RebuildEodAsync(name, date);
				}		
				if (populateSqlEod)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "populateSqlEod");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var name = args[arginx++];
					var date = args[arginx++];
					await p.PopulateSqlEodAsync(name, date);
				}
                if (checkSqlEod)
                {
                    var arginx = args.ToList().FindIndex(arg => arg == "checkSqlEod");
                    if (++arginx >= args.Length - 1) { Usage(); return; }
                    var name = args[arginx++];
                    var date = args[arginx++];
                    await p.CheckSqlEodAsync(name, date);
                }				
                
                if (checkContainerDetail2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkContainerDetail2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckContainerDetail2Async(checkFilePath, newFilePath);
				}
                if (checkPmodContainerDetail2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkPmodContainerDetail2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckPmodContainerDetail2Async(checkFilePath, newFilePath);
				}                
                if (checkPackageDetail2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkPackageDetail2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckPackageDetail2Async(checkFilePath, newFilePath);
				}
                if (checkReturnAsn2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkReturnAsn2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckReturnAsn2Async(checkFilePath, newFilePath);
				}
                if (checkInvoice2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkInvoice2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckInvoice2Async(checkFilePath, newFilePath);
				}
                if (checkExpense2)
				{
					var arginx = args.ToList().FindIndex(arg => arg == "checkExpense2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckExpense2Async(checkFilePath, newFilePath);
				}
                if (checkEvs2)
                {
					var arginx = args.ToList().FindIndex(arg => arg == "checkEvs2");
					if (++arginx >= args.Length - 1) { Usage(); return; }
					var checkFilePath = args[arginx++];
					var newFilePath = args[arginx++];
					await p.CheckEvs2Async(checkFilePath, newFilePath);
				}

                if (fix)
                {
					var arginx = args.ToList().FindIndex(arg => arg == "fix");
					if (++arginx >= args.Length -1 ) { Usage(); return; }
					var arg1 = args[arginx++];
					var arg2 = args[arginx++];
					await p.FixAsync(arg1, arg2);
				}            
            }
			catch (CosmosException de)
			{
				Exception baseException = de.GetBaseException();
				Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e);
			}
		}

        // </Main>

        // <CreateDBAsync>
        /// <summary>
        /// Create DB
        /// </summary>
        public async Task CreateDBAsync(string databaseId)
        {
            using (var cosmosClient = new CosmosClient(EndpointUri.ToString(), PrimaryKey, new CosmosClientOptions() { ApplicationName = "ParcelPrepGov.DataUtility" }))
            {
                var database = await CreateDatabaseAsync(cosmosClient, databaseId);
                CollectionNamesList.CollectionNames.ForEach(x => CreateContainerAsync(cosmosClient, database, x).Wait());
            }
            using (var documentClient = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await AddStoredProcedureAsync(documentClient, CollectionNameConstants.Sequences, "getSequenceNumber.txt");
            }
        }
        // </CreateDBAsync>

        // <DeleteDBAsync>
        /// <summary>
        /// Delete DB
        /// </summary>
        public async Task DeleteDBAsync(string databaseId)
        {
            using (var cosmosClient = new CosmosClient(EndpointUri.ToString(), PrimaryKey, new CosmosClientOptions() { ApplicationName = "ParcelPrepGov.DataUtility" }))
            {
                try
                {
                    var database = cosmosClient.GetDatabase(databaseId);
                    await DeleteDatabaseAsync(database);
                }
                catch
                {
                }
            }
        }
        // </DeleteDBAsync>


        // <PopulateDBAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task PopulateDBAsync(bool deleteIfExists = true)
        {
            // TODO: In the future we should investigate how to generate our indexes in this function
            using var documentClient = new DocumentClient(EndpointUri, PrimaryKey);
            await AddSettingsAsync(documentClient);

            await AddItemsToContainerAsync<Client>(CollectionNameConstants.Clients);
            await AddItemsToContainerAsync<SubClient>(CollectionNameConstants.SubClients);
            await AddItemsToContainerAsync<ClientFacility>(CollectionNameConstants.ClientFacilities);
            await AddItemsToContainerAsync<FileConfiguration>(CollectionNameConstants.FileConfigurations);
            await AddItemsToContainerAsync<JobOption>(CollectionNameConstants.JobOptions);
            await AddItemsToContainerAsync<ReturnOption>(CollectionNameConstants.ReturnOptions);
            await AddItemsToContainerAsync<Sequence>(CollectionNameConstants.Sequences);
            await AddItemsToContainerAsync<Site>(CollectionNameConstants.Sites);
        }
        // </PopulateDBAsync>
        public async Task MapDBAsync(bool deleteIfExists = true)
        {
            using var documentClient = new DocumentClient(EndpointUri, PrimaryKey);

            //await AddUpsGeoDescAsync(); // Only import these on request. 40k rows, rarely used
            await AddBinsAsync();
            await AddBinMapsAsync();
            await AddRatesAsync();
            await AddContainerRatesAsync();
            await AddServiceRulesAsync();
            await AddServiceRuleExtensionsAsync();
            await AddZipMapsAsync();
            await AddZipOverridesAsync();
            await AddZoneMapsAsync();
        }
        // <DeleteDBAsync>
        /// <summary>
        /// Delete DB
        /// </summary>
        public async Task DeleteDBAsync(string databaseId, string containterId)
        {
            using (var cosmosClient = new CosmosClient(EndpointUri.ToString(), PrimaryKey, new CosmosClientOptions() { ApplicationName = "ParcelPrepGov.DataUtility" }))
            {
                try
                {
                    var database = cosmosClient.GetDatabase(databaseId);
                    await DeleteDatabaseAsync(database);
                }
                catch
                {
                }
            }
        }

        // </DeleteDBAsync>


        // <EmptyContainerAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        private async Task EmptyContainerAsync(string databaseId, string containerId)
        {
            using (var cosmosClient = new CosmosClient(EndpointUri.ToString(), PrimaryKey, new CosmosClientOptions() { ApplicationName = "ParcelPrepGov.DataUtility" }))
            {
                var database = cosmosClient.GetDatabase(databaseId);
                await DeleteContainerAsync(cosmosClient, database, containerId);
                await CreateContainerAsync(cosmosClient, database, containerId);
            }
        }
        // </EmptyContainerAsync>

        // <ImportToContainerAsync>
        private async Task ImportToContainerAsync(string containerId)
        {
            if (containerId == CollectionNameConstants.Clients)
                await AddItemsToContainerAsync<Client>(CollectionNameConstants.Clients);
            else if (containerId == CollectionNameConstants.SubClients)
                await AddItemsToContainerAsync<SubClient>(CollectionNameConstants.SubClients);

            else if (containerId == CollectionNameConstants.FileConfigurations)
                await AddItemsToContainerAsync<FileConfiguration>(CollectionNameConstants.FileConfigurations);
            else if (containerId == CollectionNameConstants.JobOptions)
                await AddItemsToContainerAsync<JobOption>(CollectionNameConstants.JobOptions);
            else if (containerId == CollectionNameConstants.ReturnOptions)
                await AddItemsToContainerAsync<ReturnOption>(CollectionNameConstants.ReturnOptions);
            else if (containerId == CollectionNameConstants.Sequences)
                await AddItemsToContainerAsync<Sequence>(CollectionNameConstants.Sequences);
            else if (containerId == CollectionNameConstants.Sites)
                await AddItemsToContainerAsync<Site>(CollectionNameConstants.Sites);

            else if (containerId == CollectionNameConstants.Bins)
                await AddBinsAsync();
            else if (containerId == CollectionNameConstants.BinMaps)
                await AddBinMapsAsync();
            else if (containerId == CollectionNameConstants.Rates)
                await AddRatesAsync();
            else if (containerId == CollectionNameConstants.ServiceRules)
                await AddServiceRulesAsync();
            //else if (containerId == CollectionNameConstants.ServiceRuleExtensions)
            //	await AddServiceRuleExtensionsAsync();
            else if (containerId == CollectionNameConstants.ZipMaps)
                await AddZipMapsAsync();
            else if (containerId == CollectionNameConstants.ZipOverrides)
                await AddZipOverridesAsync();
            else if (containerId == CollectionNameConstants.ZoneMaps)
                await AddZoneMapsAsync();
        }
        // </ImportToContainerAsync>

        // <ImportExportsAsync>
        private async Task ImportExportsAsync()
        {
            using (var client = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await client.OpenAsync();
                foreach (var containerId in CollectionNamesList.CollectionNames)
                {
                    await ImportExportedItemsToContainerAsync(client, DatabaseId, containerId);
                }
            }
        }
        // </ImportExportsAsync>

        // <ExportFromContainerAsync>
        private async Task ExportFromContainerAsync(string containerId, string query)
        {
            using (var client = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await client.OpenAsync();
                if (containerId == CollectionNameConstants.ActiveGroups)
                    await ExportItemsFromContainerAsync<ActiveGroup>(client, DatabaseId, CollectionNameConstants.ActiveGroups, query);
                else if (containerId == CollectionNameConstants.Bins)
                    await ExportItemsFromContainerAsync<Bin>(client, DatabaseId, CollectionNameConstants.Bins, query);
                else if (containerId == CollectionNameConstants.BinMaps)
                    await ExportItemsFromContainerAsync<BinMap>(client, DatabaseId, CollectionNameConstants.BinMaps, query);
                else if (containerId == CollectionNameConstants.Jobs)
                    await ExportItemsFromContainerAsync<Job>(client, DatabaseId, CollectionNameConstants.Jobs, query);
                else if (containerId == CollectionNameConstants.Containers)
                    await ExportItemsFromContainerAsync<ShippingContainer>(client, DatabaseId, CollectionNameConstants.Containers, query);
                else if (containerId == CollectionNameConstants.OperationalContainers)
                    await ExportItemsFromContainerAsync<OperationalContainer>(client, DatabaseId, CollectionNameConstants.OperationalContainers, query);
                else if (containerId == CollectionNameConstants.Packages)
                    await ExportItemsFromContainerAsync<PackageTracker.Data.Models.Package>(client, DatabaseId, CollectionNameConstants.Packages, query);
                else if (containerId == CollectionNameConstants.Rates)
                    await ExportItemsFromContainerAsync<Rate>(client, DatabaseId, CollectionNameConstants.Rates, query);
                else if (containerId == CollectionNameConstants.ServiceRuleExtensions)
                    await ExportItemsFromContainerAsync<ServiceRule>(client, DatabaseId, CollectionNameConstants.ServiceRuleExtensions, query);
                else if (containerId == CollectionNameConstants.ServiceRules)
                    await ExportItemsFromContainerAsync<ServiceRule>(client, DatabaseId, CollectionNameConstants.ServiceRules, query);
                else if (containerId == CollectionNameConstants.Sequences)
                    await ExportItemsFromContainerAsync<Sequence>(client, DatabaseId, CollectionNameConstants.Sequences, query);
                else if (containerId == CollectionNameConstants.Sites)
                    await ExportItemsFromContainerAsync<Site>(client, DatabaseId, CollectionNameConstants.Sites, query);
                else if (containerId == CollectionNameConstants.SubClients)
                    await ExportItemsFromContainerAsync<SubClient>(client, DatabaseId, CollectionNameConstants.SubClients, query);
                else if (containerId == CollectionNameConstants.WebJobRuns)
                    await ExportItemsFromContainerAsync<WebJobRun>(client, DatabaseId, CollectionNameConstants.WebJobRuns, query);
                else if (containerId == CollectionNameConstants.ZipMaps)
                    await ExportItemsFromContainerAsync<ZipMap>(client, DatabaseId, CollectionNameConstants.ZipMaps, query);
                else if (containerId == CollectionNameConstants.ZoneMaps)
                    await ExportItemsFromContainerAsync<ZoneMap>(client, DatabaseId, CollectionNameConstants.ZoneMaps, query);
                else if (containerId == CollectionNameConstants.ZipOverrides)
                    await ExportItemsFromContainerAsync<ZipOverride>(client, DatabaseId, CollectionNameConstants.ZipOverrides, query);
            }
        }
        // </ExportFromContainerAsync>

        // <ExportItemsFromContainerAsync>
        private async Task ExportItemsFromContainerAsync<T>(DocumentClient client, string databaseId, string containerId, string query) where T : Entity
        {
            var exportDirectoryPath = $"./data export/{containerId}";
            Directory.CreateDirectory(exportDirectoryPath);
            Console.WriteLine($"Exporting items from container: {containerId}, to path: {exportDirectoryPath}, using query: {query}");
            try
            {
                var documents = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, containerId),
                    new SqlQuerySpec(query),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .ToList<T>();
                foreach (var document in documents)
                {
                    var fileName = document.Id.ToString();
                    var json = JsonUtility<T>.Serialize(document);
                    using (var fileStream = new FileStream($"{exportDirectoryPath}/{fileName}.json", FileMode.Create))
                    {
                        await fileStream.WriteAsync(System.Text.Encoding.ASCII.GetBytes(json));
                        await fileStream.FlushAsync();
                    }
                }
                Console.WriteLine($"Successfully exported {documents.Count} items from container: {containerId}");
            }
            catch (Exception)
            {
            }
        }
        // </ExportItemsFromContainerAsync>

        // <ImportExportedItemsToContainerAsync>
        private async Task ImportExportedItemsToContainerAsync(DocumentClient client, string databaseId, string containerId)
        {
            try
            {
                var importDirectoryPath = $"./data export/{containerId}";
                if (Directory.Exists(importDirectoryPath))
                {
                    Console.WriteLine($"Importing items to container: {containerId}, from path: {importDirectoryPath}");
                    var documents = new List<string>();
                    var bulkRepository = ServiceProvider.GetRequiredService<IBulkRepository>();
                    foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                    {
                        var fileName = Path.GetFileName(path);
                        if (fileName.EndsWith(".json"))
                        {
                            var json = string.Empty;
                            using (var stream = File.OpenRead(path))
                            {
                                using var reader = new StreamReader(stream);
                                json = reader.ReadToEnd().Trim();
                            }
                            if (json.StartsWith("{"))
                                documents.Add(json);
                        }
                        if (documents.Count() == 1000)
                        {
                            var response1 = await bulkRepository.BulkImportAsync(documents, containerId);
                            Console.WriteLine($"Successfully imported {response1.NumberOfDocumentsImported} items to container: {containerId}");
                            documents = new List<string>();
                        }
                    }
                    var response2 = await bulkRepository.BulkImportAsync(documents, containerId);
                    Console.WriteLine($"Successfully imported {response2.NumberOfDocumentsImported} items to container: {containerId}");
                }
            }
            catch (Exception)
            {
            }
        }
        // </ImportExportedItemsToContainerAsync>

        // <CleanupContainersAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        private async Task CleanupContainersAsync(string databaseId)
        {
            using (var client = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await client.OpenAsync();
                await CleanupContainerAsync<Bin>(client, databaseId, CollectionNameConstants.Bins, "binCode");
                await CleanupContainerAsync<BinMap>(client, databaseId, CollectionNameConstants.BinMaps, "binCode");
                await CleanupContainerAsync<Rate>(client, databaseId, CollectionNameConstants.Rates, "carrier");
                await CleanupContainerAsync<ServiceRule>(client, databaseId, CollectionNameConstants.ServiceRules, "mailCode");
                await CleanupContainerAsync<ServiceRuleExtension>(client, databaseId, CollectionNameConstants.ServiceRuleExtensions, "shippingCarrier");
                await CleanupContainerAsync<ZipMap>(client, databaseId, CollectionNameConstants.ZipMaps, "zipCode");
                await CleanupContainerAsync<ZipOverride>(client, databaseId, CollectionNameConstants.ZipOverrides, "zipCode");
                await CleanupContainerAsync<ZoneMap>(client, databaseId, CollectionNameConstants.ZoneMaps, "zipFirstThree");
            }
        }
        // </CleanupContainersAsync>

        // <CleanupContainerAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        private async Task CleanupContainerAsync(string databaseId, string containerId, string query)
        {
            using (var client = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await client.OpenAsync();
                if (containerId == CollectionNameConstants.Bins)
                    await CleanupContainerAsync<Bin>(client, databaseId, containerId, "binCode", query);
                else if (containerId == CollectionNameConstants.BinMaps)
                    await CleanupContainerAsync<BinMap>(client, databaseId, containerId, "binCode", query);
                else if (containerId == CollectionNameConstants.Packages)
                    await CleanupContainerAsync<PackageTracker.Data.Models.Package>(client, databaseId, containerId, "packageId", query);
                else if (containerId == CollectionNameConstants.Containers)
                    await CleanupContainerAsync<PackageTracker.Data.Models.ShippingContainer>(client, databaseId, containerId, "containerId", query);
                else if (containerId == CollectionNameConstants.OperationalContainers)
                    await CleanupContainerAsync<OperationalContainer>(client, databaseId, containerId, "containerId", query);
                else if (containerId == CollectionNameConstants.Rates)
                    await CleanupContainerAsync<Rate>(client, databaseId, containerId, "carrier", query);
                else if (containerId == CollectionNameConstants.ServiceRules)
                    await CleanupContainerAsync<ServiceRule>(client, databaseId, containerId, "mailCode", query);
                else if (containerId == CollectionNameConstants.ServiceRuleExtensions)
                    await CleanupContainerAsync<ServiceRuleExtension>(client, databaseId, containerId, "shippingCarrier", query);
                else if (containerId == CollectionNameConstants.ZipMaps)
                    await CleanupContainerAsync<ZipMap>(client, databaseId, containerId, "zipCode", query);
                else if (containerId == CollectionNameConstants.ZipOverrides)
                    await CleanupContainerAsync<ZipOverride>(client, databaseId, containerId, "zipCode", query);
                else if (containerId == CollectionNameConstants.ZoneMaps)
                    await CleanupContainerAsync<ZoneMap>(client, databaseId, containerId, "zipFirstThree", query);
                else if (containerId == CollectionNameConstants.WebJobRuns)
                    await CleanupContainerAsync<WebJobRun>(client, databaseId, containerId, "siteName", query);
            }
        }
        // </CleanupContainerAsync>


		private async Task CleanupContainerAsync<T>(DocumentClient client, string databaseId, string containerId, string column, string query = null) where T : Entity
		{
			string columnName = column.Substring(0, 1).ToUpper() + column.Substring(1);
			try
			{
				var parameters = new Microsoft.Azure.Documents.SqlParameterCollection
                {
					new Microsoft.Azure.Documents.SqlParameter("@columnName", columnName),
					new Microsoft.Azure.Documents.SqlParameter("@columnLower", column.ToLower()),
					new Microsoft.Azure.Documents.SqlParameter("@columnUpper", column.ToUpper())
				};
				if (query == null)
					query = $"SELECT * FROM c WHERE c.{column} = \"\" " +
								$"OR CONTAINS(c.{column}, @columnName)" +
								$"OR CONTAINS(c.{column}, @columnLower)" +
								$"OR CONTAINS(c.{column}, @columnUpper)";
				Console.WriteLine($"Removing items from {containerId} using query: {query}");
				var documents = client.CreateDocumentQuery<T>(
					UriFactory.CreateDocumentCollectionUri(databaseId, containerId),
						new SqlQuerySpec(query, parameters),
						new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
					.ToList<T>();
				if (documents.Any())
				{
					var tuples = new List<Tuple<string, string>>();
					foreach (var document in documents)
					{
						tuples.Add(Tuple.Create(document.PartitionKey, document.Id));
					}
					var bulkDbClient = new BulkDbClient(databaseId, containerId, client);
					var response = await bulkDbClient.BulkDeleteAsync(tuples);
					Console.WriteLine($"Deleted {response.NumberOfDocumentsDeleted} documents from {databaseId}/{containerId}");
				}
			}
			catch (Exception)
			{
			}
		}

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task<Microsoft.Azure.Cosmos.Database> CreateDatabaseAsync(CosmosClient cosmosClient, string databaseId)
        {
            var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId, 2200);
            var database = response.Database;
            Console.WriteLine($"Created Database: {database.Id}");
            return database;
        }
        // </CreateDatabaseAsync>

        // <DeleteContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteContainerAsync(CosmosClient cosmosClient, Microsoft.Azure.Cosmos.Database database, string containerId)
        {
            try
            {
                var container = database.GetContainer(containerId);
                await container.DeleteContainerAsync();
                Console.WriteLine($"Deleted Container: {container.Id}");
            }
            catch
            {
            }
        }
        // </DeleteContainerAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync(CosmosClient cosmosClient, Microsoft.Azure.Cosmos.Database database, string containerId)
        {
            // Create a new container
            var response = await database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            var container = response.Container;
            Console.WriteLine($"Created Container: {container.Id}");
        }
        // </CreateContainerAsync>
        // <AddItemsToContainerAsync>
        /// <summary>
        /// Add items to a container
        /// </summary>
        private async Task AddItemsToContainerAsync<T>(string containerId) where T : Entity
        {
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                var items = new List<string>();
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".json"))
                    {
                        var fileName = Path.GetFileName(path);
                        var json = await File.ReadAllTextAsync(path);
                        var entity = JsonUtility<T>.Deserialize(json);
                        if (entity.CreateDate == null || entity.CreateDate.Year <= 1)
                            entity.CreateDate = DateTime.Now;
                        items.Add(JsonUtility<T>.Serialize(entity));
                    }
                }
                var bulkProcessor = ServiceProvider.GetRequiredService<IBulkProcessor>();
                Console.WriteLine($"Attempt to import items to container: {containerId}");
                await bulkProcessor.BulkImportDocumentsToDbAsync(items, containerId);
                Console.WriteLine($"Successfully imported items to container: {containerId}");
            }
        }
        // </AddItemsToContainerAsync>

        // <AddStoredProcedureAsync>
        /// <summary>
        /// Add stored procedure to a container
        /// </summary>
        private async Task AddStoredProcedureAsync(DocumentClient documentClient, string containerId, string fileName)
        {
            var path = $"./stored procedures/{fileName}";
            if (File.Exists(path))
            {
                Console.WriteLine($"Importing stored procedure into container: {containerId}, from path: {path}");
                var text = await File.ReadAllTextAsync(path);
                var id = Path.GetFileNameWithoutExtension(path);
                var containerUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, containerId);
                var link = $"{containerUri.ToString()}/sprocs/{id}";
                try
                {
                    await documentClient.ReadStoredProcedureAsync(link);
                    await documentClient.DeleteStoredProcedureAsync(link);
                }
                catch
                {
                }
                await documentClient.CreateStoredProcedureAsync(
                    containerUri,
                    new Microsoft.Azure.Documents.StoredProcedure()
                    {
                        Id = id,
                        Body = text
                    }
                    );
                Console.WriteLine($"Created stored procedure in container: {containerId}, from: {Path.GetFileName(path)}");
            }
        }
        // </AddStoredProcedureAsync>

        // <AddSettingsAsync>
        /// <summary>
        /// Add settings to container
        /// </summary>
        private async Task AddSettingsAsync(DocumentClient client)
        {
            var containerId = CollectionNameConstants.Settings;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".json"))
                    {
                        var fileName = Path.GetFileName(path);
                        var json = await File.ReadAllTextAsync(path);
                        Console.WriteLine($"Attempt to import settings file: {fileName}");
                        var document = JObject.Parse(json);
                        await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, containerId), document);
                        Console.WriteLine($"Successfully imported settings file: {fileName}");
                    }
                }
            }
        }

        // <AddBinsAsync>
        /// <summary>
        /// Add bins to container
        /// </summary>
        private async Task AddBinsAsync()
        {
            Dictionary<string, ActiveGroup> activeGroups = new Dictionary<string, ActiveGroup>(); // indexed by site name
            var containerId = CollectionNameConstants.Bins;
            var importDirectoryPath = $"./data import/{containerId}";
            var startDate = DateTime.Now.AddDays(-1);
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var siteName = string.Empty;
                        var nameArray = fileName.Split("_");
                        if (nameArray[1] == "HISTORICAL")
                        {
                            // import bin file name format SITENAME_HISTORICAL_yyyy-MM-dd.txt
                            startDate = DateTime.Parse(nameArray[2].Substring(0, 10));
                            siteName = nameArray[0];
                        }
                        else
                        {
                            // import bin file name format SITENAME_bins.txt
                            siteName = nameArray[0];
                        }
                        var fileProcessor = ServiceProvider.GetRequiredService<IBinFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import bins file: {fileName}");
                        await fileProcessor.ProcessBinFileStream(stream, siteName, startDate);
                        Console.WriteLine($"Successfully imported bins file: {fileName}");
                    }
                    else if (path.EndsWith(".xlsx"))
                    {
                        await ImportExcelBinFile(path);
                    }
                }
            }
        }
        // </AddBinsAsync>

        private async Task ImportExcelBinFile(string path)
        {
            var fileName = Path.GetFileName(path);
            Console.WriteLine($"Attempt to import bins file: {fileName}");

            var parts = fileName.Split(new char[] { '_', '.' });
            var siteName = Regex.Match(parts[1], "[a-zA-Z]+").Value.ToUpper();
            var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
            var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
            if (StringHelper.DoesNotExist(site.Id))
                throw new ArgumentException($"Can't get site name from: {fileName}");

            var bins = new List<Bin>();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using var ep = new ExcelPackage(stream);
                var ws = ep.Workbook.Worksheets[0];

                if (ws.Dimension == null)
                    throw new ArgumentException($"Bad input file: {fileName}");

                DateTime createDate = DateTime.Now;
                var activeGroupId = Guid.NewGuid().ToString();
                var binActiveGroup = new ActiveGroup
                {
                    Id = activeGroupId,
                    Name = site.SiteName,
                    AddedBy = "System",
                    ActiveGroupType = ActiveGroupTypeConstants.Bins,
                    StartDate = TimeZoneUtility.GetLocalTime(site.TimeZone).AddDays(-1),
                    CreateDate = DateTime.Now,
                    IsEnabled = true
                };
                for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
                {
                    //If there is no bincode - skip it. Most likely just empty lines at the end of an excel file
                    if (NullUtility.NullExists(ws.Cells[row, 1].Value) == string.Empty)
                    {
                        continue;
                    }
                    bins.Add(new Bin
                    {
                        ActiveGroupId = activeGroupId,
                        PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(activeGroupId),
                        CreateDate = createDate,
                        BinCode = NullUtility.NullExists(ws.Cells[row, 1].Value),
                        LabelListSiteKey = NullUtility.NullExists(ws.Cells[row, 2].Value),
                        LabelListDescription = NullUtility.NullExists(ws.Cells[row, 3].Value),
                        LabelListZip = NullUtility.NullExists(ws.Cells[row, 4].Value),
                        OriginPointSiteKey = NullUtility.NullExists(ws.Cells[row, 5].Value),
                        OriginPointDescription = NullUtility.NullExists(ws.Cells[row, 6].Value),
                        DropShipSiteKeyPrimary = NullUtility.NullExists(ws.Cells[row, 7].Value),
                        DropShipSiteDescriptionPrimary = NullUtility.NullExists(ws.Cells[row, 8].Value),
                        DropShipSiteAddressPrimary = NullUtility.NullExists(ws.Cells[row, 9].Value),
                        DropShipSiteCszPrimary = NullUtility.NullExists(ws.Cells[row, 10].Value),
                        ShippingCarrierPrimary = NullUtility.NullExists(ws.Cells[row, 11].Value),
                        ShippingMethodPrimary = NullUtility.NullExists(ws.Cells[row, 12].Value),
                        ContainerTypePrimary = NullUtility.NullExists(ws.Cells[row, 13].Value),
                        LabelTypePrimary = NullUtility.NullExists(ws.Cells[row, 14].Value),
                        RegionalCarrierHubPrimary = NullUtility.NullExists(ws.Cells[row, 15].Value),
                        DaysOfTheWeekPrimary = NullUtility.NullExists(ws.Cells[row, 16].Value),
                        ScacPrimary = NullUtility.NullExists(ws.Cells[row, 17].Value),
                        AccountIdPrimary = NullUtility.NullExists(ws.Cells[row, 18].Value),
                        BinCodeSecondary = NullUtility.NullExists(ws.Cells[row, 19].Value),
                        DropShipSiteKeySecondary = NullUtility.NullExists(ws.Cells[row, 20].Value),
                        DropShipSiteDescriptionSecondary = NullUtility.NullExists(ws.Cells[row, 21].Value),
                        DropShipSiteAddressSecondary = NullUtility.NullExists(ws.Cells[row, 22].Value),
                        DropShipSiteCszSecondary = NullUtility.NullExists(ws.Cells[row, 23].Value),
                        ShippingMethodSecondary = NullUtility.NullExists(ws.Cells[row, 24].Value),
                        ContainerTypeSecondary = NullUtility.NullExists(ws.Cells[row, 25].Value),
                        LabelTypeSecondary = NullUtility.NullExists(ws.Cells[row, 26].Value),
                        RegionalCarrierHubSecondary = NullUtility.NullExists(ws.Cells[row, 27].Value),
                        DaysOfTheWeekSecondary = NullUtility.NullExists(ws.Cells[row, 28].Value),
                        ScacSecondary = NullUtility.NullExists(ws.Cells[row, 29].Value),
                        AccountIdSecondary = NullUtility.NullExists(ws.Cells[row, 30].Value),
                    });
                }
                if (bins.Any())
                {
                    var serializedBins = JsonUtility<Bin>.SerializeList(bins);
                    var activeGroupProcessor = ServiceProvider.GetRequiredService<IActiveGroupProcessor>();
                    await activeGroupProcessor.AddActiveGroupAsync(binActiveGroup);

                    var bulkProcessor = ServiceProvider.GetRequiredService<IBulkProcessor>();
                    var bulkResponse = await bulkProcessor.BulkImportDocumentsToDbAsync(serializedBins, CollectionNameConstants.Bins);
                    if (bulkResponse.BadInputDocuments.Any())
                        throw new ArgumentException($"Bad bins count {bulkResponse.BadInputDocuments.Count}: for {fileName}");

                    Console.WriteLine($"Bin file stream rows read: {bins.Count}");
                    Console.WriteLine($"Successfully imported bins file: {fileName}");
                }
            }
        }

        // <AddBinMapsAsync>
        /// <summary>
        /// Add bin maps to container
        /// </summary>
        private async Task AddBinMapsAsync()
        {
            Dictionary<string, ActiveGroup> activeGroups = new Dictionary<string, ActiveGroup>(); // indexed by site name

            var containerId = CollectionNameConstants.BinMaps;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        if (path.EndsWith(".txt"))
                        {
                            var fileName = Path.GetFileName(path);
                            var stream = new FileStream(path, FileMode.Open);
                            Console.WriteLine($"Attempt to import bin maps file: {fileName}");
                            var nameArray = fileName.Split("_");
                            var subClientName = nameArray[0];
                            var fileProcessor = ServiceProvider.GetRequiredService<IBinFileProcessor>();
                            await fileProcessor.ProcessBinMapFileStream(stream, subClientName);
                            Console.WriteLine($"Successfully imported bin maps file: {fileName}");
                        }
                    }
                }
            }
        }
        // </AddBinMapsAsync>

        // <AddServiceRulesAsync>
        /// <summary>
        /// Add service rules to container
        /// </summary>
        private async Task AddServiceRulesAsync()
        {
            var containerId = CollectionNameConstants.ServiceRules;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var nameArray = fileName.Split("_");
                        var subClientName = nameArray[0];
                        var fileProcessor = ServiceProvider.GetRequiredService<IServiceRuleProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import service rules file: {fileName}");
                        Console.WriteLine($"subclientname from file: {subClientName}");
                        await fileProcessor.ProcessServiceRuleFileStream(stream, subClientName);
                        Console.WriteLine($"Successfully imported service rules file: {fileName}");
                    }
                }
            }
        }
        // </AddServiceRulesAsync>


        // <AddServiceRuleExtensionsAsync>
        /// <summary>
        /// Add service rule extensions to container
        /// </summary>
        private async Task AddServiceRuleExtensionsAsync()
        {
            var containerId = CollectionNameConstants.ServiceRuleExtensions;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {

                        var fileName = Path.GetFileName(path);

                        var nameArray = fileName.Split("_");
                        var subClientName = nameArray[0];
                        var fileProcessor = ServiceProvider.GetRequiredService<IServiceRuleExtensionFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import service rule extensions file: {fileName}");

                        await fileProcessor.ImportFortyEightStatesFileToDatabase(stream, subClientName);

                        Console.WriteLine($"Successfully imported service rule extensions file: {fileName}");
                    }
                }
            }
        }
        //</AddServiceRuleExtensionsAsync>


        //<AddRatesAsync>
        /// <summary>
        /// Add rates to container
        /// </summary>
        private async Task AddRatesAsync()
        {
            var containerId = CollectionNameConstants.Rates;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var nameArray = fileName.Split("_");
                        var subClientName = nameArray[0];
                        var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                        var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                        var fileProcessor = ServiceProvider.GetRequiredService<IRateFileProcessor>();
                        var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import rates file: {fileName}");
                        var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                        Console.WriteLine($"SubclientName: {subClient}");
                        Console.WriteLine($"Site name:{site.SiteName}  site timezone: {site.TimeZone}");
                        await fileProcessor.ImportRatesFileToDatabase(stream, subClient);
                        Console.WriteLine($"Successfully imported rates file: {fileName}");
                    }
                }
            }
        }

        private async Task AddContainerRatesAsync()
        {
            var containerId = CollectionNameConstants.Rates;
            var importDirectoryPath = $"./data import/containerRates";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var nameArray = fileName.Split("_");
                        var siteName = nameArray[0];
                        Console.WriteLine($"site name before db lookup: {siteName}");
                        var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                        var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                        Console.WriteLine($"site name after db lookup: {site.SiteName} TimeZone: {site.TimeZone}");
                        var fileProcessor = ServiceProvider.GetRequiredService<IContainerRateFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import container rates file: {fileName}");
                        await fileProcessor.ImportContainerRatesFileToDatabase(stream, site);
                        Console.WriteLine($"Successfully imported container rates file: {fileName}");
                    }
                }
            }
        }

        private async Task AddUpsGeoDescAsync()
        {
            var containerId = CollectionNameConstants.ZipMaps;
            var importDirectoryPath = $"./data import/geoDesc";
            Console.WriteLine($"Starting UPS GeoDesc import.Directory path: {importDirectoryPath}");
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var nameArray = fileName.Split("_");
                        var siteName = nameArray[0];
                        var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                        var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                        var fileProcessor = ServiceProvider.GetRequiredService<IUpsGeoDescFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import UPS geo desc file: {fileName}");
                        await fileProcessor.ImportUpsGeoDescFileToDatabase(stream, site);
                        Console.WriteLine($"Successfully imported UPS geo desc file: {fileName}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Directory does not exist: {importDirectoryPath}");
            }
        }
        // </AddRatesAsync>

        // <AddZipMapsAsync>
        /// <summary>
        /// Add zip maps to container
        /// </summary>
        private async Task AddZipMapsAsync()
        {
            var containerId = CollectionNameConstants.ZipMaps;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var fileProcessor = ServiceProvider.GetRequiredService<IZipMapFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import zip maps file: {fileName}");
                        await fileProcessor.ImportZipMaps(stream, fileName);
                        Console.WriteLine($"Successfully imported zip maps file: {fileName}");
                    }
                }
            }
        }
        // </AddZipMapsAsync>

        // <AddZipOverridesAsync>
        /// <summary>
        /// Add zip overrides to container
        /// </summary>
        private async Task AddZipOverridesAsync()
        {
            var containerId = CollectionNameConstants.ZipOverrides;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    var fileName = Path.GetFileName(path);
                    if (fileName.EndsWith(".xlsx") && !fileName.Contains("~"))
                    {
                        var fileProcessor = ServiceProvider.GetRequiredService<IZipOverrideWebProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import zip overrides file: {fileName}");
                        var zipOverrides = new List<ZipOverride>();
                        using var ep = new ExcelPackage(stream);
                        var ws = ep.Workbook.Worksheets[0];

                        if (ws.Dimension == null)
                            throw new Exception($"Import aborted for file: {fileName}");

                        string activeGroupType;
                        if (fileName.Contains("FedEx Hawaii"))
                            activeGroupType = ActiveGroupTypeConstants.ZipsFedExHawaii;
                        else if (fileName.Contains("UPS_NDA"))
                            activeGroupType = ActiveGroupTypeConstants.ZipsUpsSat48;
                        else if (fileName.Contains("UPS_DAS"))
                            activeGroupType = ActiveGroupTypeConstants.ZipsUpsDas;
                        else
                            throw new Exception($"Import aborted for file: {fileName}");
                        for (int row = 1; row <= ws.Dimension.End.Row; row++)
                        {
                            var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
                            if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
                            {
                                zipOverrides.Add(new ZipOverride
                                {
                                    ZipCode = zipCode,
                                    ActiveGroupType = activeGroupType,
                                    CreateDate = DateTime.Now
                                });
                            }
                        }
                        var response = await fileProcessor.ImportZipOverrides(zipOverrides, "System", SiteConstants.AllSites, DateTime.Now.ToString());
                        if (response.IsSuccessful)
                            Console.WriteLine($"Successfully imported zip maps file: {fileName}");
                        else
                            Console.WriteLine($"Failed to imported zip maps file: {fileName}: {response.Message}");
                    }
                }
            }
        }
        // </AddZipOverridesAsync>

        // <AddZoneMapsAsync>
        /// <summary>
        /// Add zone maps to container
        /// </summary>
        private async Task AddZoneMapsAsync()
        {
            var containerId = CollectionNameConstants.ZoneMaps;
            var importDirectoryPath = $"./data import/{containerId}";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing items into container: {containerId}, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    if (path.EndsWith(".txt"))
                    {
                        var fileName = Path.GetFileName(path);
                        var fileProcessor = ServiceProvider.GetRequiredService<IZoneFileProcessor>();
                        var stream = new FileStream(path, FileMode.Open);
                        Console.WriteLine($"Attempt to import zone maps file: {fileName}");
                        await fileProcessor.ImportZoneFileToDatabase(stream);
                        Console.WriteLine($"Successfully imported zone maps file: {fileName}");
                    }
                }
            }
        }
        // </AddZoneMapsAsync>

        // <DeleteDatabaseAsync>
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAsync(Microsoft.Azure.Cosmos.Database database)
        {
            await database.DeleteAsync();
            Console.WriteLine($"Deleted Database: {database.Id}");
        }
        // </DeleteDatabaseAsync>

        // <CopyDBAsync>
        /// <summary>
        /// Copy DB
        /// </summary>
        private async Task CopyDBAsync(string sourceId, string destinationId)
        {
            await DeleteDBAsync(destinationId);
            await CreateDBAsync(destinationId);

            Console.WriteLine($"Copying: {sourceId} to {destinationId}");
            using (var client = new DocumentClient(EndpointUri, PrimaryKey))
            {
                await client.OpenAsync();
                CopyContainerAsync<ActiveGroup>(client, sourceId, destinationId, CollectionNameConstants.ActiveGroups).Wait();
                CopyContainerAsync<BinMap>(client, sourceId, destinationId, CollectionNameConstants.BinMaps).Wait();
                CopyContainerAsync<Bin>(client, sourceId, destinationId, CollectionNameConstants.Bins).Wait();
                CopyContainerAsync<Client>(client, sourceId, destinationId, CollectionNameConstants.Clients).Wait();
                CopyContainerAsync<ShippingContainer>(client, sourceId, destinationId, CollectionNameConstants.Containers).Wait();
                CopyContainerAsync<FileConfiguration>(client, sourceId, destinationId, CollectionNameConstants.FileConfigurations).Wait();
                CopyContainerAsync<Job>(client, sourceId, destinationId, CollectionNameConstants.Jobs).Wait();
                CopyContainerAsync<JobOption>(client, sourceId, destinationId, CollectionNameConstants.JobOptions).Wait();
                CopyContainerAsync<PackageTracker.Data.Models.Package>(client, sourceId, destinationId, CollectionNameConstants.Packages).Wait();
                CopyContainerAsync<Rate>(client, sourceId, destinationId, CollectionNameConstants.Rates).Wait();
                CopyContainerAsync<ReturnOption>(client, sourceId, destinationId, CollectionNameConstants.ReturnOptions).Wait();
                CopyContainerAsync<ServiceRule>(client, sourceId, destinationId, CollectionNameConstants.ServiceRules).Wait();
                CopyContainerAsync<ServiceRuleExtension>(client, sourceId, destinationId, CollectionNameConstants.ServiceRuleExtensions).Wait();
                CopyContainerAsync<Sequence>(client, sourceId, destinationId, CollectionNameConstants.Sequences).Wait();
                CopyContainerAsync<Site>(client, sourceId, destinationId, CollectionNameConstants.Sites).Wait();
                CopyContainerAsync<SubClient>(client, sourceId, destinationId, CollectionNameConstants.SubClients).Wait();
                CopyContainerAsync<WebJobRun>(client, sourceId, destinationId, CollectionNameConstants.WebJobRuns).Wait();
                CopyContainerAsync<ZipMap>(client, sourceId, destinationId, CollectionNameConstants.ZipMaps).Wait();
                CopyContainerAsync<ZipOverride>(client, sourceId, destinationId, CollectionNameConstants.ZipOverrides).Wait();
                CopyContainerAsync<ZoneMap>(client, sourceId, destinationId, CollectionNameConstants.ZoneMaps).Wait();
            }
            Console.WriteLine($"Copy succeeded: {sourceId} to {destinationId}");
        }
        // </CopyDBAsync>

        private async Task CopyContainerAsync<T>(DocumentClient client, string sourceId, string destinationId, string containerId) where T : Entity
        {
            Console.WriteLine($"Attempt to copy: {sourceId}/{containerId}");
            try
            {
                var documents = client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(sourceId, containerId),
                    new SqlQuerySpec("SELECT * FROM c"),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .ToList<T>();
                if (documents.Any())
                {
                    var serializedDocuments = JsonUtility<T>.SerializeList(documents);
                    var bulkDbClient = new BulkDbClient(destinationId, containerId, client);
                    var response = await bulkDbClient.BulkImportAsync(serializedDocuments);
                    Console.WriteLine($"Copied {response.NumberOfDocumentsImported} documents to {destinationId}/{containerId}");
                }
            }
            catch (Exception)
            {
            }
        }

        // <ImportToReportsTablesAsync>
        /// <summary>
        /// Import tables to reportsDB
        /// </summary>
        private async Task ImportToReportsTablesAsync()
        {
            var importDirectoryPath = $"./data import/reports";
            if (Directory.Exists(importDirectoryPath))
            {
                Console.WriteLine($"Importing tables into reports DB, from path: {importDirectoryPath}");
                foreach (var path in Directory.EnumerateFiles(importDirectoryPath))
                {
                    var fileName = Path.GetFileName(path);
                    if (fileName.EndsWith(".xlsx") && !fileName.Contains("~"))
                    {
                        if (fileName.Contains("Holidays"))
                        {
                            DateTime now = DateTime.Now;
                            var postalDays = new List<PostalDays>();
                            Console.WriteLine($"Attempt to import file: {fileName}");
                            var ws = new ExcelWorkSheet(path);
                            var nextDate = new DateTime();
                            var holiday = new DateTime();
                            int ordinal = 0;
                            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                            {
                                holiday = ws.GetDateValue(row, "Date");
                                if (nextDate.Year == 1)
                                    nextDate = new DateTime(holiday.Year, 1, 1);
                                while (nextDate < holiday)
                                {
                                    postalDays.Add(new PostalDays
                                    {
                                        PostalDate = nextDate,
                                        Ordinal = nextDate.DayOfWeek == DayOfWeek.Sunday ? ordinal : ordinal++,
                                        IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                                        CreateDate = now
                                    });
                                    nextDate = nextDate.AddDays(1);
                                }
                                postalDays.Add(new PostalDays
                                {
                                    PostalDate = nextDate,
                                    Ordinal = ordinal,
                                    Description = ws.GetStringValue(row, "Description"),
                                    IsHoliday = 1,
                                    IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                                    CreateDate = now
                                }); ;
                                nextDate = nextDate.AddDays(1);
                            }
                            while (nextDate.Year == holiday.Year)
                            {
                                postalDays.Add(new PostalDays
                                {
                                    PostalDate = nextDate,
                                    Ordinal = nextDate.DayOfWeek == DayOfWeek.Sunday ? ordinal : ordinal++,
                                    IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                                    CreateDate = now
                                });
                                nextDate = nextDate.AddDays(1);
                            }

                            var repository = ServiceProvider.GetRequiredService<IPostalDaysRepository>();
                            await repository.ExecuteBulkInsertAsync(postalDays); // Insert new data
                            await repository.EraseOldDataAsync(now);      // Erase old data		
                            Console.WriteLine($"Successfully imported Postal Holidays file: {fileName}");
                        }
                        else if (fileName.Contains("Postal"))
                        {
                            DateTime now = DateTime.Now;
                            var postalAreasAndDistricts = new List<PostalAreaAndDistrict>();
                            Console.WriteLine($"Attempt to import file: {fileName}");
                            var ws = new ExcelWorkSheet(path);
                            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                            {
                                postalAreasAndDistricts.Add(new PostalAreaAndDistrict
                                {
                                    CreateDate = now,
                                    ZipCode3Zip = ws.GetFormattedIntValue(row, "Zip Code 3 Zip", 3),
                                    Scf = ws.GetIntValue(row, "SCF"),
                                    PostalDistrict = ws.GetStringValue(row, "Postal District"),
                                    PostalArea = ws.GetStringValue(row, "Postal Area")
                                });
                            }
                            var repository = ServiceProvider.GetRequiredService<IPostalAreaAndDistrictRepository>();
                            await repository.ExecuteBulkInsertAsync(postalAreasAndDistricts); // Insert new data
                            await repository.EraseOldDataAsync(now);      // Erase old data		
                            Console.WriteLine($"Successfully imported Postal Areas and Districts file: {fileName}");
                        }
                        else if (fileName.Contains("VISN"))
                        {
                            DateTime now = DateTime.Now;
                            var visnSites = new List<VisnSite>();
                            Console.WriteLine($"Attempt to import file: {fileName}");
                            var ws = new ExcelWorkSheet(path);
                            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                            {
                                visnSites.Add(new VisnSite
                                {
                                    CreateDate = now,
                                    Visn = ws.GetStringValue(row, "VISN"),
                                    SiteParent = ws.GetStringValue(row, "SiteParent"),
                                    SiteNumber = ws.GetStringValue(row, "SiteNumber"),
                                    SiteType = ws.GetStringValue(row, "SiteType"),
                                    SiteName = ws.GetStringValue(row, "SiteName"),
                                    SiteAddress1 = ws.GetStringValue(row, "SiteAddress1"),
                                    SiteAddress2 = ws.GetStringValue(row, "SiteAddress2"),
                                    SiteCity = ws.GetStringValue(row, "SiteCity"),
                                    SiteState = ws.GetStringValue(row, "SiteState"),
                                    SiteZipCode = ws.GetStringValue(row, "SiteZipCode"),
                                    SitePhone = ws.GetStringValue(row, "SitePhone"),
                                    SiteShippingContact = ws.GetStringValue(row, "SiteShippingContact"),
                                });
                            }
                            var repository = ServiceProvider.GetRequiredService<IVisnSiteRepository>();
                            await repository.ExecuteBulkInsertAsync(visnSites); // Insert new data
                            await repository.EraseOldDataAsync(now); // Erase old data
                            Console.WriteLine($"Successfully imported VISN sites file: {fileName}");
                        }
                        else if (fileName.Contains("Evs"))
                        {
                            DateTime now = DateTime.Now;
                            var evsCodes = new List<EvsCode>();
                            Console.WriteLine($"Attempt to import file: {fileName}");
                            var ws = new ExcelWorkSheet(path);
                            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                            {
                                evsCodes.Add(new EvsCode
                                {
                                    CreateDate = now,
                                    Code = ws.GetStringValue(row, "Code"),
                                    Description = ws.GetStringValue(row, "Description"),
                                    IsStopTheClock = ws.GetIntValue(row, "IsStopTheClock"),
                                    IsUndeliverable = ws.GetIntValue(row, "IsUndeliverable")
                                });
                            }
                            var repository = ServiceProvider.GetRequiredService<IEvsCodeRepository>();
                            await repository.ExecuteBulkInsertAsync(evsCodes); // Insert new data
                            await repository.EraseOldDataAsync(now); // Erase old data
                            Console.WriteLine($"Successfully imported Evs Codes file: {fileName}");
                        }
                        else if (fileName.Contains("Carrier"))
                        {
                            DateTime now = DateTime.Now;
                            var carrierEventCodes = new List<CarrierEventCode>();
                            Console.WriteLine($"Attempt to import file: {fileName}");
                            var ws = new ExcelWorkSheet(path);
                            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                            {
                                carrierEventCodes.Add(new CarrierEventCode
                                {
                                    CreateDate = now,
                                    ShippingCarrier =  ws.GetStringValue(row, "ShippingCarrier"),
                                    Code = ws.GetStringValue(row, "Code"),
                                    Description = ws.GetStringValue(row, "Description"),
                                    IsStopTheClock = ws.GetIntValue(row, "IsStopTheClock"),
                                    IsUndeliverable = ws.GetIntValue(row, "IsUndeliverable")
                                });
                            }
                            var repository = ServiceProvider.GetRequiredService<ICarrierEventCodeRepository>();
                            await repository.ExecuteBulkInsertAsync(carrierEventCodes); // Insert new data
                            await repository.EraseOldDataAsync(now); // Erase old data
                            Console.WriteLine($"Successfully imported Carrier Event Codes file: {fileName}");
                        }
                    }
                }
            }
        }
        // </ImportToReportsTablesAsync>

        // <SearchBlobContainerAsync>
        private async Task SearchBlobContainerAsync(string databaseId, string containerName, string searchString, string fileNameMatch)
        {
            var blobHepler = ServiceProvider.GetRequiredService<IBlobHelper>();
            var parseContainerName = containerName.Split('/', 2);
            var directoryName = string.Empty;
            if (parseContainerName.Length > 1)
            {
                containerName = parseContainerName[0];
                directoryName = parseContainerName[1];
            }
            var container = blobHepler.GetBlobContainerReference(containerName);
            var directory = container.GetDirectoryReference(directoryName);
            BlobContinuationToken continuationToken = null;
            for (; ; )
            {
                var segment = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = segment.ContinuationToken;
                var blobs = segment.Results;
                foreach (var blob in blobs)
                {
                    if (blob.GetType() == typeof(CloudBlobDirectory))
                    {
                        await SearchBlobContainerAsync(DatabaseId, blob.Uri.AbsolutePath.Trim('/'), searchString, fileNameMatch);
                    }
                    else
                    {
                       var fileName = Path.GetFileName(blob.Uri.AbsolutePath).Replace("%20", " ");
                       if (Regex.IsMatch(fileName, fileNameMatch))
                        {
                            try
                            {
                                var text = string.Empty;
                                if (blob.GetType() == typeof(CloudBlockBlob))
                                {
                                    var blobRef = directory.GetBlockBlobReference(fileName);
                                    text = await blobRef.DownloadTextAsync();
                                }
                                else if (blob.GetType() == typeof(CloudAppendBlob))
                                {
                                    var blobRef = directory.GetAppendBlobReference(fileName);
                                    text = await blobRef.DownloadTextAsync();
                                }
                                var lines = text.Split("\n");
                                int lineNumber = 1;
                                int matches = 0;
                                foreach (var line in lines)
                                {
                                    if (line.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        System.Console.WriteLine($"{directoryName}/{fileName}[{lineNumber}]: {line.Trim()}");
                                        matches++;
                                    }
                                    lineNumber++;
                                }
                                if (matches == 0)
                                    System.Console.WriteLine($"{directoryName}/{fileName}[{lineNumber}]: *** No Matches ***");
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine($"{directoryName}/{fileName}: *** File inaccessible ***");
                            }
                        }
                    }
                }
                if (continuationToken == null)
                    break;
            }
        }
        // <SearchBlobContainerAsync>

        // </ProcessBlobContainerAsync>
        private async Task ProcessBlobContainerAsync(string databaseId, string containerName, string fileNameMatch)
        {
            var blobHepler = ServiceProvider.GetRequiredService<IBlobHelper>();
            var parseContainerName = containerName.Split('/', 2);
            var directoryName = string.Empty;
            if (parseContainerName.Length > 1)
            {
                containerName = parseContainerName[0];
                directoryName = parseContainerName[1];
            }
            var container = blobHepler.GetBlobContainerReference(containerName);
            var directory = container.GetDirectoryReference(directoryName);
            BlobContinuationToken continuationToken = null;
            for (; ; )
            {
                var segment = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = segment.ContinuationToken;
                var blobs = segment.Results;
                foreach (var blob in blobs)
                {
                    var fileName = Path.GetFileName(blob.Uri.AbsolutePath).Replace("%20", " ");
                    System.Console.WriteLine($"Processing: {fileName}");
                    try
                    {
                        if (Regex.IsMatch(fileName, fileNameMatch))
                        {
                            var blobRef = directory.GetBlockBlobReference(fileName);
                            var text = await blobRef.DownloadTextAsync();
                            var trackPackageProcessor = ServiceProvider.GetRequiredService<ITrackPackageProcessor>();
                            var trackPackages = new List<TrackPackage>();
                            var shippingCarrier = string.Empty;
                            if (containerName.Contains("UPS", StringComparison.InvariantCultureIgnoreCase))
                            {
                                shippingCarrier = ShippingCarrierConstants.Ups;
                                var upsPackages = XmlUtility<UpsTrackPackageResponse>.Deserialize(text);
                                trackPackageProcessor.ParseUpsTrackingDataAsync(false, trackPackages, upsPackages);
                            }
                            else if (containerName.Contains("USPS", StringComparison.InvariantCultureIgnoreCase))
                            {
                                shippingCarrier = ShippingCarrierConstants.Usps;
                                trackPackages = await trackPackageProcessor.ReadUspsTrackingFileStreamAsync(false, new MemoryStream(Encoding.UTF8.GetBytes(text)));
                            }
                            else if (containerName.Contains("FedEx", StringComparison.InvariantCultureIgnoreCase))
                            {
                                shippingCarrier = ShippingCarrierConstants.FedEx;
                                trackPackages = await trackPackageProcessor.ReadFedExTrackingFileStreamAsync(false, new MemoryStream(Encoding.UTF8.GetBytes(text)));
                            }
                            System.Console.WriteLine($"Total records read: { trackPackages.Count }");
                            var trackPackageDatasetProcessor = ServiceProvider.GetRequiredService<ITrackPackageDatasetProcessor>();
                            var response = await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(shippingCarrier, trackPackages);
                            System.Console.WriteLine($"Total records processed: { response.NumberOfDocuments }");
                        }
                    }
                    catch (Exception)
                    {
                        System.Console.WriteLine($"{directoryName}/{fileName}: *** File inaccessible ***");
                    }
                }
                if (continuationToken == null)
                    break;
            }
        }
        // </ProcessBlobContainerAsync>

        // </DownloadBlobContainerAsync>
        private async Task DownloadBlobContainerAsync(string databaseId, string containerName, string fileNameMatch, string extraArg)
        {
            var blobHepler = ServiceProvider.GetRequiredService<IBlobHelper>();
            var parseContainerName = containerName.Split('/', 2);
            var directoryName = string.Empty;
            if (parseContainerName.Length > 1)
            {
                containerName = parseContainerName[0];
                directoryName = parseContainerName[1];
            }
            var container = blobHepler.GetBlobContainerReference(containerName);
            var directory = container.GetDirectoryReference(directoryName);
            bool consolidate = extraArg.Contains("consolidate", StringComparison.InvariantCultureIgnoreCase);
            if (consolidate)
                Directory.CreateDirectory($"downloads/{containerName}");
            else
                Directory.CreateDirectory($"downloads/{containerName}/{directoryName}");
            BlobContinuationToken continuationToken = null;
            for (; ; )
            {
                var segment = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = segment.ContinuationToken;
                var blobs = segment.Results;
                foreach (var blob in blobs)
                {
                    var fileName = Path.GetFileName(blob.Uri.AbsolutePath).Replace("%20", " ");
                    try
                    {
                        if (Regex.IsMatch(fileName, fileNameMatch))
                        {
                            var blobRef = directory.GetBlockBlobReference(fileName);
                            if (consolidate)
                            {
                                if (StringHelper.Exists(directoryName))
                                {
                                    System.Console.WriteLine($"Downloading: {fileName} to downloads/{containerName}/{directoryName.Replace("/", "_")}_{fileName}");
                                    await blobRef.DownloadToFileAsync($"downloads/{containerName}/{directoryName.Replace("/", "_")}_{fileName}", FileMode.Create);
                                }
                                else
                                {
                                    System.Console.WriteLine($"Downloading: {fileName} to downloads/{containerName}/{fileName}");
                                    await blobRef.DownloadToFileAsync($"downloads/{containerName}/{fileName}", FileMode.Create);
                                }
                            }
                            else
                            {
                                var rest = Regex.IsMatch(fileName, @"^[0-9-]+_") ? fileName.Split("_", 2)[1] : fileName;
                                System.Console.WriteLine($"Downloading: {fileName} to downloads/{containerName}/{directoryName}/{rest}");
                                await blobRef.DownloadToFileAsync($"downloads/{containerName}/{directoryName}/{rest}", FileMode.Create);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        System.Console.WriteLine($"{directoryName}/{fileName}: *** File inaccessible ***");
                    }
                }
                if (continuationToken == null)
                    break;
            }
        }
        // </DownloadBlobContainerAsync>

        // <CheckAsync>
        private async Task CheckAsync(string containerId)
        {
            if (containerId == CollectionNameConstants.Bins)
                await CheckBinsAsync();
        }
        // </CheckAsync>	

        // <CheckBinsAsync>	
        private async Task CheckBinsAsync()
        {
            var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
            var activeGroupRepository = ServiceProvider.GetRequiredService<IActiveGroupRepository>();
            var binProcessor = ServiceProvider.GetRequiredService<IBinProcessor>();
            var binDatasetRepository = ServiceProvider.GetRequiredService<IBinDatasetRepository>();
            var binDatasetProcessor = ServiceProvider.GetRequiredService<IBinDatasetProcessor>();

            var sites = (await siteProcessor.GetAllSitesAsync()).ToList();
            foreach (var site in sites)
            {
                var activeGroups = await activeGroupRepository.GetActiveGroupsByTypeAsync(ActiveGroupTypeConstants.Bins, site.SiteName);
                foreach (var group in activeGroups)
                {
                    var importedBins = await binDatasetRepository.GetBinDatasetsAsync(group.Id);
                    if (importedBins.Count > 0)
                    {
                        var bins = await binProcessor.GetBinsByActiveGroupIdAsync(group.Id);
                        if (bins.Count > importedBins.Count)
                        {
                            var binsToImport = new List<BinDataset>();
                            foreach (var bin in bins.Where(b => importedBins.FirstOrDefault(i => i.BinCode == b.BinCode) == null))
                            {
                                binDatasetProcessor.CreateDataset(binsToImport, bin, group);
                            }
                            if (binsToImport.Count > 0)
                            {
                                System.Console.WriteLine($"Bin Active Group ID: {group.Id}, Total: {bins.Count}, Imported: {importedBins.Count}, To Import: {binsToImport.Count}");
                                await binDatasetRepository.ExecuteBulkInsertAsync(binsToImport, site.SiteName);
                            }
                        }
                    }
                }
            }
        }
        // </CheckBinsAsync>

        private async Task<(BulkDeleteResponse, int)> BulkDeleteByQueryAsync<T>(DocumentClient client, string databaseId, string containerId, string query) where T : Entity
        {
            Console.WriteLine($"Removing items from {containerId} using query: {query}.");
            try
            {
                var documents = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, containerId),
                    new SqlQuerySpec(query),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .ToList<T>();
                if (documents.Any())
                {
                    var tuples = new List<Tuple<string, string>>();
                    foreach (var document in documents)
                    {
                        tuples.Add(Tuple.Create(document.PartitionKey, document.Id));
                    }
                    var bulkDbClient = new BulkDbClient(databaseId, containerId, client);
                    var response = await bulkDbClient.BulkDeleteAsync(tuples);
                    Console.WriteLine($"Deleted {response.NumberOfDocumentsDeleted} documents of {documents.Count()} from {databaseId}/{containerId}");
                    return (response, documents.Count());
                }
            }
            catch (Exception)
            {
            }
            Console.WriteLine($"Deleted 0 documents of 0 from {databaseId}/{containerId}");
            return (new BulkDeleteResponse(), 0);
        }

        public class EodJob
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public bool BySubClient { get; set; }
        }

        private static IDictionary<string, EodJob> eodJobTypes = new Dictionary<string, EodJob>
        {
            { WebJobConstants.RunEodJobType, new EodJob {
                Type = WebJobConstants.RunEodJobType, Description = "Eod Started", BySubClient = false } },
            { WebJobConstants.ContainerDetailExportJobType, new EodJob {
                Type = WebJobConstants.ContainerDetailExportJobType, Description = "Container Detail File Export", BySubClient = false } },
            { WebJobConstants.PmodContainerDetailExportJobType, new EodJob {
                Type = WebJobConstants.PmodContainerDetailExportJobType, Description = "PMOD Container Detail File Export", BySubClient = false } },
            { WebJobConstants.PackageDetailExportJobType, new EodJob {
                Type = WebJobConstants.PackageDetailExportJobType, Description = "Package Detail File Export", BySubClient = false } },
            { WebJobConstants.ReturnAsnExportJobType, new EodJob {
                Type = WebJobConstants.ReturnAsnExportJobType, Description = "Return ASN File Export", BySubClient = true } },
            { WebJobConstants.UspsEvsExportJobType, new EodJob {
                Type = WebJobConstants.UspsEvsExportJobType, Description = "USPS eVs File Export", BySubClient = false } },
            { WebJobConstants.UspsEvsPmodExportJobType, new EodJob {
                Type = WebJobConstants.UspsEvsPmodExportJobType, Description = "USPS eVs PMOD File Export", BySubClient = false } },
            { WebJobConstants.InvoiceExportJobType, new EodJob {
                Type = WebJobConstants.InvoiceExportJobType, Description = "Invoice File Export", BySubClient = true } },
            { WebJobConstants.ExpenseExportJobType, new EodJob {
                Type = WebJobConstants.ExpenseExportJobType, Description = "Expense File Export", BySubClient = true } },
            { WebJobConstants.EodPackageMonitorJobType, new EodJob {
                Type = WebJobConstants.EodPackageMonitorJobType, Description = "Generate Shipped Events", BySubClient = false } },
        };

        private async Task<bool> IsEodStarted(Site site, DateTime siteLocalTime)
        {
            var result = false;
            var webJobRunProcessor = ServiceProvider.GetRequiredService<IWebJobRunProcessor>();
            foreach (var jobType in eodJobTypes.Keys)
            {
                var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, siteLocalTime.Date, jobType);
                if (mostRecentJobRun.IsSuccessful || mostRecentJobRun.InProgress)
                {
                    result = true;
                    break;
                }
            }
            System.Console.WriteLine($"EOD started for: {site.SiteName}: {result}");
            return result;
        }

        private async Task<(bool, IDictionary<string, IList<WebJobRunResponse>>)> IsEodCompleted(Site site, DateTime siteLocalTime)
        {
            var result = true;
            var webJobRuns = new Dictionary<string, IList<WebJobRunResponse>>();
            var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
            var subClients = await subClientProcessor.GetSubClientsAsync();
            var webJobRunProcessor = ServiceProvider.GetRequiredService<IWebJobRunProcessor>();
            foreach (var jobType in eodJobTypes.Keys)
            {
                webJobRuns[jobType] = new List<WebJobRunResponse>();
                if (eodJobTypes[jobType].BySubClient)
                {
                    foreach (var subClient in subClients.Where(s => s.SiteName == site.SiteName))
                    {
                        var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, subClient.Name, siteLocalTime.Date, jobType);
                        if (!mostRecentJobRun.IsSuccessful)
                            result = false;
                        webJobRuns[jobType].Add(mostRecentJobRun);
                    }
                }
                else
                {
                    var mostRecentJobRun = await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(site.SiteName, null, siteLocalTime.Date, jobType);
                    if (!mostRecentJobRun.IsSuccessful)
                        result = false;
                    webJobRuns[jobType].Add(mostRecentJobRun);
                }
            }
            System.Console.WriteLine($"EOD completed for: {site.SiteName}: {result}");
            var jobsList = new List<WebJobRunResponse>();
            foreach (var response in webJobRuns)
            {
                jobsList.AddRange(response.Value);
            }
            foreach (var webJobRun in jobsList.OrderBy(j => j.CreateDate))
            {
                if (StringHelper.Exists(webJobRun.SubClientName))
                    System.Console.WriteLine($"EOD job: {webJobRun.JobType} for: {webJobRun.SubClientName}: {webJobRun.IsSuccessful}: {webJobRun.LocalCreateDate}");
                else
                    System.Console.WriteLine($"EOD job: {webJobRun.JobType} for: {webJobRun.SiteName}: {webJobRun.IsSuccessful}: {webJobRun.LocalCreateDate}");
            }
            return (result, webJobRuns);
        }


        // <CheckEodAsync>	
        private async Task CheckEodAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                await IsEodStarted(site, date);
                await IsEodCompleted(site, date);
            }
        }
        // </CheckEodAsync>


        private static decimal RoundRate(decimal rate)
        {
            return decimal.Round(rate * 100) / (decimal)100;
        }

        // <FixRatesAsync>	
        private async Task FixRatesAsync(string subClientName)
        {
            var rateProcessor = ServiceProvider.GetRequiredService<IRateProcessor>();
            var rateRepository = ServiceProvider.GetRequiredService<IRateRepository>();
            var rates = await rateProcessor.GetCurrentRatesAsync(subClientName);
            foreach (var rate in rates)
            {
                rate.CostZone1 = RoundRate(rate.CostZone1);
                rate.CostZone2 = RoundRate(rate.CostZone2);
                rate.CostZone3 = RoundRate(rate.CostZone3);
                rate.CostZone4 = RoundRate(rate.CostZone4);
                rate.CostZone5 = RoundRate(rate.CostZone5);
                rate.CostZone6 = RoundRate(rate.CostZone6);
                rate.CostZone7 = RoundRate(rate.CostZone7);
                rate.CostZone8 = RoundRate(rate.CostZone8);
                rate.CostZone9 = RoundRate(rate.CostZone9);
                rate.CostZoneDdu = RoundRate(rate.CostZoneDdu);
                rate.CostZoneScf = RoundRate(rate.CostZoneScf);
                rate.CostZoneNdc = RoundRate(rate.CostZoneNdc);
                rate.CostZoneDduOut48 = RoundRate(rate.CostZoneDduOut48);
                rate.CostZoneScfOut48 = RoundRate(rate.CostZoneScfOut48);
                rate.CostZoneNdcOut48 = RoundRate(rate.CostZoneNdcOut48);

                rate.ChargeZone1 = RoundRate(rate.ChargeZone1);
                rate.ChargeZone2 = RoundRate(rate.ChargeZone2);
                rate.ChargeZone3 = RoundRate(rate.ChargeZone3);
                rate.ChargeZone4 = RoundRate(rate.ChargeZone4);
                rate.ChargeZone5 = RoundRate(rate.ChargeZone5);
                rate.ChargeZone6 = RoundRate(rate.ChargeZone6);
                rate.ChargeZone7 = RoundRate(rate.ChargeZone7);
                rate.ChargeZone8 = RoundRate(rate.ChargeZone8);
                rate.ChargeZone9 = RoundRate(rate.ChargeZone9);
                rate.ChargeZoneDdu = RoundRate(rate.ChargeZoneDdu);
                rate.ChargeZoneScf = RoundRate(rate.ChargeZoneScf);
                rate.ChargeZoneNdc = RoundRate(rate.ChargeZoneNdc);
                rate.ChargeZoneDduOut48 = RoundRate(rate.ChargeZoneDduOut48);
                rate.ChargeZoneScfOut48 = RoundRate(rate.ChargeZoneScfOut48);
                rate.ChargeZoneNdcOut48 = RoundRate(rate.ChargeZoneNdcOut48);
                await rateRepository.UpdateItemAsync(rate);
            }
            System.Console.WriteLine($"Updated {rates.Count()} for sub client: {subClientName}");
        }
        // </FixRatesAsync>	


        // <ConsumerDetailFileAsync>	
        private async Task ConsumerDetailFileAsync(string subClientName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, subClientName: subClientName);
                System.Console.WriteLine($"{packages.Count()} packages for subClient {subClient.Name}.");

                var consumerDetailFileProcessor = ServiceProvider.GetRequiredService<IConsumerDetailFileProcessor>();
                var bulkProcessor = ServiceProvider.GetRequiredService<IBulkProcessor>();
                if (packages.Any())
                {
                    int chunk = 10000;
                    int count = 0;
                    for (int offset = 0; offset < packages.Count(); offset += chunk, count++)
                    {
                        var packagesChunk = packages.Skip(offset).Take(chunk);
                        var response = new FileExportResponse();
                        var webJobId = Guid.NewGuid().ToString();
                        consumerDetailFileProcessor.CreateConsumerDetailRecords(response, packagesChunk, subClient, webJobId);

                        var byteArray = response.FileContents.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                        var formattedTime = date.AddHours(20).ToString("MMddyyyhh24") + count.ToString("00");
                        var fileName = $"400012_RTN1_{subClient.Key}_{formattedTime}.txt"; // this is a legacy fileName from BestWay that we have not been given a replacement schema for

                        System.Console.WriteLine($"Exporting {response.FileContents.Count} records to file: data export/consumerDetailFiles/{fileName}");
                        Directory.CreateDirectory($"data export/consumerDetailFiles");
                        using (var fileStream = new FileStream($"data export/consumerDetailFiles/{fileName}", FileMode.Create))
                        {
                            await fileStream.WriteAsync(byteArray);
                            await fileStream.FlushAsync();
                        }
                    }
                }
            }
        }
        // </ConsumerDetailFileAsync>

        // <PackageDetailFileAsync>	
        private async Task PackageDetailFileAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var packages = await GetPackageDetails(siteName, date);
                if (packages.Any())
                {
                    var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                    var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                    var webJobId = Guid.NewGuid().ToString();
                    var packageDetailProcessor = ServiceProvider.GetRequiredService<IPackageDetailProcessor>();
                    var items = packages.Select(p => PackageDetailProcessor.BuildRecordString(p.PackageDetailRecord, true)).ToList();
                    var byteArray = items.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    var fileName = $"600014_RPT_D301_{site.SiteName}_{date:MMddyyyy}0000[{items.Count}].txt";
                    System.Console.WriteLine($"Exporting {items.Count()} records to file: data export/packageDetailFiles/{fileName}");
                    Directory.CreateDirectory($"data export/packageDetailFiles");
                    using (var fileStream = new FileStream($"data export/packageDetailFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(Encoding.ASCII.GetBytes($"{String.Join(",", PackageDetailValueNames)}\n"));
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                }
            }
        }
        // </PackageDetailFileAsync>

        // <ContainerDetailFileAsync>	
        private async Task ContainerDetailFileAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var containers = await GetContainerDetails(siteName, date);
                if (containers.Any())
                {
                    var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                    var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                    var webJobId = Guid.NewGuid().ToString();
                    var containerDetailProcessor = ServiceProvider.GetRequiredService<IContainerDetailProcessor>();
                    var items = containers.Select(c => ContainerDetailProcessor.BuildRecordString(c.ContainerDetailRecord)).ToList();
                    var byteArray = items.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    var fileName = $"ASN_{site.SiteName}_{date:yyyyMMdd}0000[{items.Count}].txt";
                    System.Console.WriteLine($"Exporting {items.Count()} records to file: data export/containerDetailFiles/{fileName}");
                    Directory.CreateDirectory($"data export/containerDetailFiles");
                    using (var fileStream = new FileStream($"data export/containerDetailFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                }
            }
        }
        // </ContainerDetailFileAsync>


        // <PmodContainerDetailFileAsync>	
        private async Task PmodContainerDetailFileAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var containers = await GetPmodContainerDetails(siteName, date);
                if (containers.Any())
                {
                    var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                    var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                    var webJobId = Guid.NewGuid().ToString();
                    var containerDetailProcessor = ServiceProvider.GetRequiredService<IContainerDetailProcessor>();
                    var items = containers.Select(c => ContainerDetailProcessor.BuildPmodRecordString(c.PmodContainerDetailRecord)).ToList();
                    var byteArray = items.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    var fileName = $"600014_RPT_D301_{site.SiteName}_{date:yyyyMMdd}0000[{items.Count}].txt";
                    System.Console.WriteLine($"Exporting {items.Count()} records to file: data export/pmodContainerDetailFiles/{fileName}");
                    Directory.CreateDirectory($"data export/pmodContainerDetailFiles");
                    using (var fileStream = new FileStream($"data export/pmodContainerDetailFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                }
            }
        }
        // </PmodContainerDetailFileAsync>

        // <ReturnAsnFileAsync>	
        private async Task ReturnAsnFileAsync(string subClientName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var packages = await GetReturnAsns(subClient.SiteName, date, subClientName);
                if (packages.Any())
                {
                    var webJobId = Guid.NewGuid().ToString();
                    var returnAsnProcessor = ServiceProvider.GetRequiredService<IReturnAsnProcessor>();
                    var items = new List<string>();
                    if (subClient.ClientName == ClientSubClientConstants.DalcClientName)
                    {
                        items = packages.Select(p => ReturnAsnProcessor.BuildSimplifiedRecordString(p.ReturnAsnRecord)).ToList();
                    }
                    else
                    {
                        items = packages.Select(p => ReturnAsnProcessor.BuildRecordString(p.ReturnAsnRecord, true)).ToList();
                    }
                    var byteArray = items.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    var formattedTime = date.ToString("MMddyyyy");
                    var fileName = AsnFileUtility.FormatExportFileName(subClient, subClient.AsnExportFileNameFormat, date.Date) + $"[{items.Count}].txt";
                    System.Console.WriteLine($"Exporting {items.Count} records to file: data export/returnAsnFiles/{fileName}");
                    Directory.CreateDirectory($"data export/returnAsnFiles");
                    using (var fileStream = new FileStream($"data export/returnAsnFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                }
            }
        }
        // </ReturnAsnFileAsync>

        private async Task<FileExportResponse> GenerateExpenseFile(SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId)
        {
            var response = new FileExportResponse();
            var totalWatch = Stopwatch.StartNew();
            var dbWriteWatch = new Stopwatch();

            var dbReadWatch = Stopwatch.StartNew();
            var eodPackageExpenses = new Dictionary<DateTime, List<EodPackage>>();
            for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
            {
                eodPackageExpenses[dateToProcess] = new List<EodPackage>();
                eodPackageExpenses[dateToProcess].AddRange(await GetExpenseRecords(subClient.SiteName, dateToProcess, subClient.Name));
            }

            var eodContainerExpenses = new Dictionary<DateTime, List<EodContainer>>();
            if (subClient.ClientName == ClientSubClientConstants.CmopClientName)
            {
                for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
                {
                    eodContainerExpenses[dateToProcess] = new List<EodContainer>();
                    eodContainerExpenses[dateToProcess].AddRange(await GetExpenseRecords(subClient.SiteName, dateToProcess));
                }
            }
            dbReadWatch.Stop();

            if (eodPackageExpenses.Any() || eodContainerExpenses.Any())
            {
                for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
                {
                    if (eodPackageExpenses.ContainsKey(dateToProcess))
                    {
                        Console.WriteLine($"Processing {eodPackageExpenses[dateToProcess].Count()} Package Expense records for subClient: {subClient.Name}, date: {dateToProcess}");
                        ExpenseProcessor.ProcessSinglePackagesIntoGroupedRecords(eodPackageExpenses[dateToProcess], dateToProcess, subClient, webJobId, response);
                    }
                    if (eodContainerExpenses.ContainsKey(dateToProcess))
                    {
                        Console.WriteLine($"Processing {eodContainerExpenses[dateToProcess].Count()} Container Expense records for site: {subClient.SiteName}, date: {dateToProcess}");
                        ExpenseProcessor.ProcessSingleContainersIntoGroupedRecords(eodContainerExpenses[dateToProcess], dateToProcess, subClient.SiteName, webJobId, response);
                    }
                }
                response.IsSuccessful = true;
            }
            else
            {
                Console.WriteLine($"No Expense records found for subClient: {subClient.Name}");
                response.IsSuccessful = true;
            }

            response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
            response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
            response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

            return response;
        }

        // <ExpenseFileAsync>	
        private async Task ExpenseFileAsync(string subClientName, string dateString, string extraArg)
        {
            if (DateTime.TryParse(dateString, out var dateToProcess))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var firstDateToProcess = dateToProcess;
                var lastDateToProcess = dateToProcess;
                var weekly = extraArg == "WEEKLY" && dateToProcess.DayOfWeek == DayOfWeek.Sunday;
                var monthly = extraArg == "MONTHLY" && dateToProcess.Day == 1;
                if (weekly)
                {
                    firstDateToProcess = dateToProcess.AddDays(-7);
                    lastDateToProcess = dateToProcess.AddDays(-1);
                }
                else if (monthly)
                {
                    firstDateToProcess = dateToProcess.AddMonths(-1);
                    lastDateToProcess = dateToProcess.AddDays(-1);
                }
                var expenseProcessor = ServiceProvider.GetRequiredService<IExpenseProcessor>();
                var webJobId = Guid.NewGuid().ToString();
                var response = await GenerateExpenseFile(subClient, firstDateToProcess, lastDateToProcess, webJobId);
                if (response.NumberOfRecords > 0)
                {
                    var headers = expenseProcessor.BuildExpenseHeader();
                    var dataTypes = expenseProcessor.GetExcelDataTypes();
                    var fileArray = response.FileContents.ToArray();
                    var fileName = $"{subClient.Name}_EXPENSE_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    if (weekly)
                        fileName = $"{subClient.Name}_EXPENSE_WEEKLY_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    else if (monthly)
                        fileName = $"{subClient.Name}_EXPENSE_MONTHLY_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    var maxRows = 1048576;  // Maximum number of rows allowed in an Excel worksheet.
                    if (response.NumberOfRecords > maxRows - 1)
                    {
                        System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/expenseFiles/{fileName}.csv");
                        Directory.CreateDirectory($"data export/expenseFiles");
                        using (var fileStream = new FileStream($"data export/expenseFiles/{fileName}.csv", FileMode.Create))
                        {
                            await fileStream.WriteAsync(Encoding.ASCII.GetBytes($"{String.Join(",", headers)}\n"));
                            foreach (var record in fileArray)
                            {
                                await fileStream.WriteAsync(Encoding.ASCII.GetBytes(record.Replace("|", ",")));
                            }
                            await fileStream.FlushAsync();
                        }
                    }
                    else
                    {
                        var excel = ExcelUtility.GenerateExcel(headers, fileArray, dataTypes, fileName);
                        System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/expenseFiles/{fileName}.xlsx");
                        Directory.CreateDirectory($"data export/expenseFiles");
                        using (var fileStream = new FileStream($"data export/expenseFiles/{fileName}.xlsx", FileMode.Create))
                        {
                            await excel.WriteAsync(fileStream);
                            await fileStream.FlushAsync();
                        }
                    }
                }
            }
        }
        // </ExpenseFileAsync>

        private async Task<FileExportResponse> GenerateInvoiceFile(SubClient subClient, DateTime firstDateToProcess, DateTime lastDateToProcess, string webJobId)
        {
            var response = new FileExportResponse();
            var totalWatch = Stopwatch.StartNew();
            var dbWriteWatch = new Stopwatch();

            var dbReadWatch = Stopwatch.StartNew();
            var eodInvoices = new List<EodPackage>();
            for (var dateToProcess = firstDateToProcess; dateToProcess.Date <= lastDateToProcess.Date; dateToProcess = dateToProcess.AddDays(1))
            {
                var eodInvoicesForDate = await GetInvoiceRecords(subClient.SiteName, dateToProcess, subClient.Name);
                Console.WriteLine($"Processing {eodInvoicesForDate.Count()} Invoice records for subClient: {subClient.Name}, date: {dateToProcess}");
                eodInvoices.AddRange(eodInvoicesForDate);
            }
            dbReadWatch.Stop();
            Console.WriteLine($"Processing {eodInvoices.Count()} Total Invoice records for subClient: {subClient.Name}");

            if (eodInvoices.Any())
            {
                foreach (var eodPackage in eodInvoices)
                {
                    eodPackage.InvoiceRecord.SubClientName = subClient.Name; // This was missing in old data.
                    response.FileContents.Add(InvoiceProcessor.BuildRecordString(eodPackage.InvoiceRecord, true));
                    response.NumberOfRecords += 1;
                }
                response.IsSuccessful = true;
            }
            else
            {
                Console.WriteLine($"No Invoice records found for subClient: {subClient.Name}, date range: {firstDateToProcess}-{lastDateToProcess}");
                response.IsSuccessful = true;
            }

            response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
            response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
            response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

            return response;
        }

        // <InvoiceFileAsync>	
        private async Task InvoiceFileAsync(string subClientName, string dateString, string extraArg)
        {
            if (DateTime.TryParse(dateString, out var dateToProcess))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var firstDateToProcess = dateToProcess;
                var lastDateToProcess = dateToProcess;
                var weekly = extraArg == "WEEKLY" && dateToProcess.DayOfWeek == DayOfWeek.Sunday;
                var monthly = extraArg == "MONTHLY" && dateToProcess.Day == 1;
                if (weekly)
                {
                    firstDateToProcess = dateToProcess.AddDays(-7);
                    lastDateToProcess = dateToProcess.AddDays(-1);
                }
                else if (monthly)
                {
                    firstDateToProcess = dateToProcess.AddMonths(-1);
                    lastDateToProcess = dateToProcess.AddDays(-1);
                }
                var invoiceProcessor = ServiceProvider.GetRequiredService<IInvoiceProcessor>();
                var webJobId = Guid.NewGuid().ToString();
                var response = await GenerateInvoiceFile(subClient, firstDateToProcess, lastDateToProcess, webJobId);
                if (response.NumberOfRecords > 0)
                {
                    var headers = invoiceProcessor.BuildInvoiceHeader(true);
                    var dataTypes = invoiceProcessor.GetExcelDataTypes(true);
                    var fileArray = response.FileContents.ToArray();
                    var fileName = $"{subClient.Name}_INVOICE_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    if (weekly)
                        fileName = $"{subClient.Name}_INVOICE_WEEKLY_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    else if (monthly)
                        fileName = $"{subClient.Name}_INVOICE_MONTHLY_{dateToProcess:yyyyMMddhhmm}[{fileArray.Count()}]";
                    var maxRows = 1048576;  // Maximum number of rows allowed in an Excel worksheet.
                    if (response.NumberOfRecords > maxRows - 1)
                    {
                        System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/invoiceFiles/{fileName}.csv");
                        Directory.CreateDirectory($"data export/invoiceFiles");
                        using (var fileStream = new FileStream($"data export/invoiceFiles/{fileName}.csv", FileMode.Create))
                        {
                            await fileStream.WriteAsync(Encoding.ASCII.GetBytes($"{String.Join(",", headers)}\n"));
                            foreach (var record in fileArray)
                            {
                                await fileStream.WriteAsync(Encoding.ASCII.GetBytes(record.Replace("|", ",")));
                            }
                            await fileStream.FlushAsync();
                        }
                    }
                    else
                    {
                        var excel = ExcelUtility.GenerateExcel(headers, fileArray, dataTypes, fileName);
                        System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/invoiceFiles/{fileName}.xlsx");
                        Directory.CreateDirectory($"data export/invoiceFiles");
                        using (var fileStream = new FileStream($"data export/invoiceFiles/{fileName}.xlsx", FileMode.Create))
                        {
                            await excel.WriteAsync(fileStream);
                            await fileStream.FlushAsync();
                        }
                    }
                }
            }
        }
        // </InvoiceFileAsync>

        private class EvsFileChunk
        {
            public ManifestBuilder.UspsEvsRecord Header { get; set; }
            public IList<ManifestBuilder.UspsEvsRecord> Details { get; set; } = new List<ManifestBuilder.UspsEvsRecord>();
            public IList<ManifestBuilder.UspsEvsRecord> Containers { get; set; } = new List<ManifestBuilder.UspsEvsRecord>();
        };

        private static ManifestBuilder.UspsEvsRecord ParseContainer(string line)
        {
            var parts = line.Trim().Split("|");
            var index = 0;
            return new ManifestBuilder.UspsEvsRecord
            {
                C1ContainerRecordID = parts[index++],
                C1ContainerID = parts[index++],
                C1ContainerType = parts[index++],
                C1ElectronicFileNumber = parts[index++],
                C1DestinationZIPCode = parts[index++],
            };
        }

        private static ManifestBuilder.UspsEvsRecord ParseHeader(string line)
        {
            var parts = line.Trim().Split("|");
            var index = 0;
            return new ManifestBuilder.UspsEvsRecord
            {
                H1HeaderRecordID = parts[index++],
                H1ElectronicFileNumber = parts[index++],
                H1ElectronicFileType = parts[index++],
                H1DateofMailing = parts[index++],
                H1TimeofMailing = parts[index++],
                H1EntryFacilityType = parts[index++],
                H1EntryFacilityZIPCode = parts[index++],
                H1EntryFacilityZIPplus4Code = parts[index++],
                H1DirectEntryOriginCountryCode = parts[index++],
                H1ShipmentFeeCode = parts[index++],
                H1ExtraFeeforShipment = parts[index++],
                H1ContainerizationIndicator = parts[index++],
                H1USPSElectronicFileVersionNumber = parts[index++],
                H1TransactionID = parts[index++],
                H1SoftwareVendorCode = parts[index++],
                H1SoftwareVendorProductVersionNumber = parts[index++],
                H1FileRecordCount = parts[index++],
                H1MailerID = parts[index++],
            };
        }

        private static ManifestBuilder.UspsEvsRecord ParseDetail(string line)
        {
            var parts = line.Trim().Split("|");
            var index = 0;
            return new ManifestBuilder.UspsEvsRecord
            {
                D1DetailRecordID = parts[index++],
                D1TrackingNumber = parts[index++],
                D1ClassOfMail = parts[index++],
                D1ServiceTypeCode = parts[index++],
                D1BarcodeConstructCode = parts[index++],
                D1DestinationZIPCode = parts[index++],
                D1DestinationZIPplus4 = parts[index++],
                D1DestinationFacilityType = parts[index++],
                D1DestinationCountryCode = parts[index++],
                D1ForeignPostalCode = parts[index++],
                D1CarrierRoute = parts[index++],
                D1LogisticsManagerMailerID = parts[index++],
                D1MailOwnerMailerID = parts[index++],
                D1ContainerID1 = parts[index++],
                D1ContainerType1 = parts[index++],
                D1ContainerID2 = parts[index++],
                D1ContainerType2 = parts[index++],
                D1ContainerID3 = parts[index++],
                D1ContainerType3 = parts[index++],
                D1CRID = parts[index++],
                D1CustomerReferenceNumber1 = parts[index++],
                D1FASTReservationNumber = parts[index++],
                D1FASTScheduledInductionDate = parts[index++],
                D1FASTScheduledInductionTime = parts[index++],
                D1PaymentAccountNumber = parts[index++],
                D1MethodofPayment = parts[index++],
                D1PostOfficeofAccountZIPCode = parts[index++],
                D1MeterSerialNumber = parts[index++],
                D1ChargebackCode = parts[index++],
                D1Postage = parts[index++],
                D1PostageType = parts[index++],
                D1CustomizedShippingServicesContractsNumber = parts[index++],
                D1CustomizedShippingServicesContractsProductID = parts[index++],
                D1UnitofMeasureCode = parts[index++],
                D1Weight = parts[index++],
                D1ProcessingCategory = parts[index++],
                D1RateIndicator = parts[index++],
                D1DestinationRateIndicator = parts[index++],
                D1DomesticZone = parts[index++],
                D1Length = parts[index++],
                D1Width = parts[index++],
                D1Height = parts[index++],
                D1DimensionalWeight = parts[index++],
                D1ExtraServiceCode1stService = parts[index++],
                D1ExtraServiceFee1stService = parts[index++],
                D1ExtraServiceCode2ndService = parts[index++],
                D1ExtraServiceFee2ndService = parts[index++],
                D1ExtraServiceCode3rdService = parts[index++],
                D1ExtraServiceFee3rdService = parts[index++],
                D1ExtraServiceCode4thService = parts[index++],
                D1ExtraServiceFee4thService = parts[index++],
                D1ExtraServiceCode5thService = parts[index++],
                D1ExtraServiceFee5thService = parts[index++],
                D1ValueofArticle = parts[index++],
                D1CODAmountDueSender = parts[index++],
                D1HandlingCharge = parts[index++],
                D1SurchargeType = parts[index++],
                D1SurchargeAmount = parts[index++],
                D1DiscountType = parts[index++],
                D1DiscountAmount = parts[index++],
                D1NonIncidentalEnclosureRateIndicator = parts[index++],
                D1NonIncidentalEnclosureClass = parts[index++],
                D1NonIncidentalEnclosurePostage = parts[index++],
                D1NonIncidentalEnclosureWeight = parts[index++],
                D1NonIncidentalEnclosureProcessingCategory = parts[index++],
                D1PostalRoutingBarcode = parts[index++],
                D1OpenandDistributeContentsIndicator = parts[index++],
                D1POBoxIndicator = parts[index++],
                D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference = parts[index++],
                D1DeliveryOptionIndicator = parts[index++],
                D1DestinationDeliveryPoint = parts[index++],
                D1RemovalIndicator = parts[index++],
                D1TrackingIndicator = parts[index++],
                D1OriginalLabelTrackingNumberBarcodeConstructCode = parts[index++],
                D1OriginalTrackingNumber = parts[index++],
                D1CustomerReferenceNumber2 = parts[index++],
                D1RecipientNameDestination = parts[index++],
                D1DeliveryAddress = parts[index++],
                D1AncillaryServiceEndorsement = parts[index++],
                D1AddressServiceParticipantCod = parts[index++],
                D1KeyLine = parts[index++],
                D1ReturnAddress = parts[index++],
                D1ReturnAddressCity = parts[index++],
                D1ReturnAddressState = parts[index++],
                D1ReturnAddressZIPCode = parts[index++],
                D1LogisticMailingFacilityCRID = parts[index++],
            };
        }

        private IList<EvsFileChunk> ParseEvsFile(IList<string> lines)
        {
            var result = new List<EvsFileChunk>();
            EvsFileChunk chunk = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("H1"))
                {
                    if (chunk != null)
                        result.Add(chunk);
                    chunk = new EvsFileChunk
                    {
                        Header = ParseHeader(line)
                    };
                }
                else if (line.StartsWith("D1") && chunk != null)
                {
                    chunk.Details.Add(ParseDetail(line));
                }
                else if (line.StartsWith("C1") && chunk != null)
                {
                    chunk.Containers.Add(ParseContainer(line));
                }
            }
            if (chunk != null)
                result.Add(chunk);
            return result;
        }

        private async Task<IList<EvsFileChunk>> ReadEvsFile(string path)
        {
            var lines = new List<string>();
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(await reader.ReadLineAsync());
                    }
                }
            }
            return ParseEvsFile(lines);
        }

        static string[] detailValueNames =
        {
            "D1DetailRecordID",
            "D1TrackingNumber",
            "D1ClassOfMail",
            "D1ServiceTypeCode",
            "D1BarcodeConstructCode",
            "D1DestinationZIPCode",
            "D1DestinationZIPplus4",
            "D1DestinationFacilityType",
            "D1DestinationCountryCode",
            "D1ForeignPostalCode",
            "D1CarrierRoute",
            "D1LogisticsManagerMailerID",
            "D1MailOwnerMailerID",
            "D1ContainerID1",
            "D1ContainerType1",
            "D1ContainerID2",
            "D1ContainerType2",
            "D1ContainerID3",
            "D1ContainerType3",
            "D1CRID",
            "D1CustomerReferenceNumber1",
            "D1FASTReservationNumber",
            "D1FASTScheduledInductionDate",
            "D1FASTScheduledInductionTime",
            "D1PaymentAccountNumber",
            "D1MethodofPayment",
            "D1PostOfficeofAccountZIPCode",
            "D1MeterSerialNumber",
            "D1ChargebackCode",
            "D1Postage",
            "D1PostageType",
            "D1CustomizedShippingServicesContractsNumber",
            "D1CustomizedShippingServicesContractsProductID",
            "D1UnitofMeasureCode",
            "D1Weight",
            "D1ProcessingCategory",
            "D1RateIndicator",
            "D1DestinationRateIndicator",
            "D1DomesticZone",
            "D1Length",
            "D1Width",
            "D1Height",
            "D1DimensionalWeight",
            "D1ExtraServiceCode1stService",
            "D1ExtraServiceFee1stService",
            "D1ExtraServiceCode2ndService",
            "D1ExtraServiceFee2ndService",
            "D1ExtraServiceCode3rdService",
            "D1ExtraServiceFee3rdService",
            "D1ExtraServiceCode4thService",
            "D1ExtraServiceFee4thService",
            "D1ExtraServiceCode5thService",
            "D1ExtraServiceFee5thService",
            "D1ValueofArticle",
            "D1CODAmountDueSender",
            "D1HandlingCharge",
            "D1SurchargeType",
            "D1SurchargeAmount",
            "D1DiscountType",
            "D1DiscountAmount",
            "D1NonIncidentalEnclosureRateIndicator",
            "D1NonIncidentalEnclosureClass",
            "D1NonIncidentalEnclosurePostage",
            "D1NonIncidentalEnclosureWeight",
            "D1NonIncidentalEnclosureProcessingCategory",
            "D1PostalRoutingBarcode",
            "D1OpenandDistributeContentsIndicator",
            "D1POBoxIndicator",
            "D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference",
            "D1DeliveryOptionIndicator",
            "D1DestinationDeliveryPoint",
            "D1RemovalIndicator",
            "D1TrackingIndicator",
            "D1OriginalLabelTrackingNumberBarcodeConstructCode",
            "D1OriginalTrackingNumber",
            "D1CustomerReferenceNumber2",
            "D1RecipientNameDestination",
            "D1DeliveryAddress",
            "D1AncillaryServiceEndorsement",
            "D1AddressServiceParticipantCod",
            "D1KeyLine",
            "D1ReturnAddress",
            "D1ReturnAddressCity",
            "D1ReturnAddressState",
            "D1ReturnAddressZIPCode",
            "D1LogisticMailingFacilityCRID"
        };

        static void CompareDetailItem(string trackingNumber, string valueName, ManifestBuilder.UspsEvsRecord checkFileDetail, ManifestBuilder.UspsEvsRecord newFileDetail)
        {
            var checkValue = checkFileDetail.GetType().GetProperty(valueName).GetValue(checkFileDetail, null);
            var newValue = newFileDetail.GetType().GetProperty(valueName).GetValue(newFileDetail, null);
            if (checkValue.ToString() != newValue.ToString())
                System.Console.WriteLine($"'{valueName}' mismatch for TrackingNumber: {trackingNumber}, check file: {checkValue}, new file: {newValue}");
        }

        static void CompareDetailRecords(ManifestBuilder.UspsEvsRecord checkFileDetail, ManifestBuilder.UspsEvsRecord newFileDetail)
        {
            foreach (var valueName in detailValueNames)
            {
                CompareDetailItem(checkFileDetail.D1TrackingNumber, valueName, checkFileDetail, newFileDetail);
            }
        }

        static void CheckDetails(EvsFileChunk checkFileChunk, EvsFileChunk newFileChunk)
        {
            if (checkFileChunk.Details.Count != newFileChunk.Details.Count)
            {
                System.Console.WriteLine($"D1 count mismatch, check file: {checkFileChunk.Details.Count}, new file: {newFileChunk.Details.Count}");
            }
            foreach (var checkFileDetail in checkFileChunk.Details)
            {
                var newFileDetail = newFileChunk.Details.FirstOrDefault(d => d.D1TrackingNumber == checkFileDetail.D1TrackingNumber);
                if (newFileDetail == null)
                    System.Console.WriteLine($"Missing Tracking Number in new file: {checkFileDetail.D1TrackingNumber}");
            }
            foreach (var newFileDetail in newFileChunk.Details)
            {
                var checkFileDetail = checkFileChunk.Details.FirstOrDefault(d => d.D1TrackingNumber == newFileDetail.D1TrackingNumber);
                if (checkFileDetail == null)
                    System.Console.WriteLine($"Extra Tracking Number in new file: {newFileDetail.D1TrackingNumber}");
                else
                    CompareDetailRecords(checkFileDetail, newFileDetail);
            }
        }

        static void CheckContainers(EvsFileChunk checkFileChunk, EvsFileChunk newFileChunk)
        {
            if (checkFileChunk.Containers.Count != newFileChunk.Containers.Count)
            {
                System.Console.WriteLine($"C1 count mismatch, check file: {checkFileChunk.Containers.Count}, new file: {newFileChunk.Containers.Count}");
            }
            foreach (var checkFileContainer in checkFileChunk.Containers)
            {
                var newFileContainer = newFileChunk.Containers.FirstOrDefault(d => d.C1ContainerID == checkFileContainer.C1ContainerID);
                if (newFileContainer == null)
                    System.Console.WriteLine($"Missing Container ID in new file: {checkFileContainer.C1ContainerID}");
            }
            foreach (var newFileContainer in newFileChunk.Containers)
            {
                var checkFileContainer = checkFileChunk.Containers.FirstOrDefault(d => d.C1ContainerID == newFileContainer.C1ContainerID);
                if (checkFileContainer == null)
                    System.Console.WriteLine($"Extra Container ID in new file: {newFileContainer.C1ContainerID}");
                else
                    CompareContainerRecords(checkFileContainer, newFileContainer);
            }
        }
        static string[] headerValueNames =
        {
            "H1HeaderRecordID",
			//"H1ElectronicFileNumber",					// This won't match
			"H1ElectronicFileType",
			//"H1DateofMailing",						// This won't match
			//"H1TimeofMailing",						// This won't match
			"H1EntryFacilityType",
            "H1EntryFacilityZIPCode",
            "H1EntryFacilityZIPplus4Code",
            "H1DirectEntryOriginCountryCode",
            "H1ShipmentFeeCode",
            "H1ExtraFeeforShipment",
            "H1ContainerizationIndicator",
            "H1USPSElectronicFileVersionNumber",
			//"H1TransactionID",						// This won't match
			//"H1SoftwareVendorCode",					// This wasn't filled in in old code
			//"H1SoftwareVendorProductVersionNumber",	// This wasn't filled in in old code
			"H1FileRecordCount",
            "H1MailerID"
        };

        static void CompareHeaderItem(string zipCode, string zipPlus4Code, string valueName, ManifestBuilder.UspsEvsRecord checkFileHeader, ManifestBuilder.UspsEvsRecord newFileHeader)
        {
            var checkValue = checkFileHeader.GetType().GetProperty(valueName).GetValue(checkFileHeader, null);
            var newValue = newFileHeader.GetType().GetProperty(valueName).GetValue(newFileHeader, null);
            if (checkValue.ToString() != newValue.ToString())
                System.Console.WriteLine($"'{valueName}' mismatch for Zip: {zipCode}|{zipPlus4Code}, check file: {checkValue}, new file: {newValue}");
        }

        static void CompareHeaderRecords(ManifestBuilder.UspsEvsRecord checkFileHeader, ManifestBuilder.UspsEvsRecord newFileHeader)
        {
            foreach (var valueName in headerValueNames)
            {
                CompareHeaderItem(checkFileHeader.H1EntryFacilityZIPCode, checkFileHeader.H1EntryFacilityZIPplus4Code, valueName, checkFileHeader, newFileHeader);
            }
        }

        static string[] containerValueNames =
        {
            "C1ContainerRecordID",
            "C1ContainerID",
            "C1ContainerType",
			//"C1ElectronicFileNumber",						// This won't match
			"C1DestinationZIPCode"
        };

        static void CompareContainerItem(string containerId, string valueName, ManifestBuilder.UspsEvsRecord checkFileHeader, ManifestBuilder.UspsEvsRecord newFileHeader)
        {
            var checkValue = checkFileHeader.GetType().GetProperty(valueName).GetValue(checkFileHeader, null);
            var newValue = newFileHeader.GetType().GetProperty(valueName).GetValue(newFileHeader, null);
            if (checkValue.ToString() != newValue.ToString())
                System.Console.WriteLine($"'{valueName}' mismatch for Container ID: {containerId}, check file: {checkValue}, new file: {newValue}");
        }

        static void CompareContainerRecords(ManifestBuilder.UspsEvsRecord checkFileDetail, ManifestBuilder.UspsEvsRecord newFileDetail)
        {
            foreach (var valueName in containerValueNames)
            {
                CompareContainerItem(checkFileDetail.D1TrackingNumber, valueName, checkFileDetail, newFileDetail);
            }
        }

        static void CheckContainers(IList<ManifestBuilder.UspsEvsRecord> checkFile, IList<ManifestBuilder.UspsEvsRecord> newFile)
        {
            if (checkFile.Count != newFile.Count)
            {
                System.Console.WriteLine($"C1 count mismatch, check file: {checkFile.Count}, new file: {newFile.Count}");
            }
            foreach (var checkFileContainer in checkFile)
            {
                var newFileContainer = newFile.FirstOrDefault(c => c.C1ContainerID == checkFileContainer.C1ContainerID);
                if (newFileContainer == null)
                    System.Console.WriteLine($"Missing Container in new file: {checkFileContainer.C1ContainerID}");
            }
            foreach (var newFileContainer in newFile)
            {
                var checkFileContainer = checkFile.FirstOrDefault(c => c.C1ContainerID == newFileContainer.C1ContainerID);
                if (checkFileContainer == null)
                    System.Console.WriteLine($"Extra Container in new file: {newFileContainer.C1ContainerID}");
                else
                    CompareDetailRecords(checkFileContainer, newFileContainer);
            }
        }

        // <EvsFileAsync>	
        private async Task EvsFileAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, siteName: siteName, shippingCarrier: ShippingCarrierConstants.Usps);
                var containerRepository = ServiceProvider.GetRequiredService<IContainerRepository>();
                var containers = await containerRepository.GetClosedContainersForPackageEvsFile(siteName, date, date.AddDays(1));
                if (packages.Any())
                {
                    var now = DateTime.Now;
                    var fileName = $"USPS_eVs_{date:yyyMMdd}_{now:hhmmss}.ssf.manifest";
                    var uspsEvsProcessor = ServiceProvider.GetRequiredService<IUspsEvsProcessor>();
                    var response = await uspsEvsProcessor.CreateUspsRecords(site, packages, containers);
                    var byteArray = response.FileContents.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/evsFiles/{fileName}");
                    Directory.CreateDirectory($"data export/evsFiles");
                    using (var fileStream = new FileStream($"data export/evsFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                    await CheckEvsAsync(siteName, dateString, $"data export/evsFiles/{fileName}");
                }
            }
        }
        // </EvsFileAsync>

        private async Task<FileExportResponse> GenerateEvsEodFile(Site site, DateTime manifestDate)
        {
            var response = new FileExportResponse();
            var eodPackages = await GetEvsEodPackages(site.SiteName, manifestDate);
            var eodContainers = await GetEvsEodContainers(site.SiteName, manifestDate);
            var manifestPackages = new List<ManifestBuilder.Package>();
            var manifestContainers = new List<ManifestBuilder.ShippingContainer>();
            var sequenceProcessor = ServiceProvider.GetRequiredService<ISequenceProcessor>();
            var mailerIdSequenceMaps = await EvsFileProcessor.GenerateMailerIdSequenceMap(sequenceProcessor, site);
            var fileNameSequence = await sequenceProcessor.GetSequenceAsync(site.SiteName, SequenceTypeConstants.EvsFileName);

            foreach (var eodPackage in eodPackages.Where(p => p.EvsPackage != null))
            {
                manifestPackages.Add(EvsPackage.GetManifestBuilderPackage(eodPackage.EvsPackage));
            }
            foreach (var eodContainer in eodContainers.Where(c => c.EvsContainerRecord != null))
            {
                manifestContainers.Add(EvsContainer.GetManifestBuilderShippingContainer(eodContainer.EvsContainerRecord));
            }
            var request = new ManifestBuilder.CreateManifestRequest
            {
                Packages = manifestPackages,
                Containers = manifestContainers,
                EFNStartSequenceByMID = mailerIdSequenceMaps,
                MailDate = manifestDate,
                MailProducerMid = site.MailProducerMid,
                Site = new ManifestBuilder.Models.Site
                {
                    SiteName = site.SiteName,
                    EvsId = site.EvsId,
                    Zip = site.Zip
                }
            };

            var evsFileName = $"USPS_eVs_{manifestDate:yyyMMdd}{fileNameSequence.Number.ToString().PadLeft(4, '0')}.ssf.manifest";
            var createManifestResponse = ManifestBuilder.ManifestBuilder.CreateManifestFile(request);
            if (createManifestResponse.IsSuccessful)
            {

                response.FileName = evsFileName;
                response.FileContents.AddRange(createManifestResponse.EvsRecords);
                response.IsSuccessful = createManifestResponse.IsSuccessful;
                response.NumberOfRecords = response.FileContents.Count;
           }
            return response;
        }

        // <CheckEvsAsync>	
        private async Task CheckEvsAsync(string siteName, string dateString, string checkFilePath)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var now = DateTime.Now;
                var evsFileProcessor = ServiceProvider.GetRequiredService<IEvsFileProcessor>();
                var response = await GenerateEvsEodFile(site, date);
                var fileName = $"USPS_eVs_{date:yyyMMdd}_0000.ssf.manifest.{site.SiteName}[{response.FileContents.Count}].txt";
                System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/evsFiles/{fileName}");
                Directory.CreateDirectory($"data export/evsFiles");
                var byteArray = response.FileContents.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                using (var fileStream = new FileStream($"data export/evsFiles/{fileName}", FileMode.Create))
                {
                    await fileStream.WriteAsync(byteArray);
                    await fileStream.FlushAsync();
                }
                if (checkFilePath != null)
                {
                    await CheckEvs2Async(checkFilePath, $"data export/evsFiles/{fileName}");
                }
            }
        }
        // </CheckEvsAsync>

        private static string AddLeadingZeros(string packagesCount)
        {
            int length = packagesCount.Length + 1;
            if (length < 9)
            {
                packagesCount = packagesCount.PadLeft(9, '0');
            }
            return packagesCount.ToString();
        }

        private static void MergeEvsFileChunks(IList<EvsFileChunk> evsFile)
        {
            // Merge chunks from evs file if they have the same header record info, but had different bin codes.
            for (var index = 0; index < evsFile.Count; index++)
            {
                var checkFileChunk = evsFile[index];
                if (checkFileChunk.Header == null)
                    continue; // Chunk was merged.
                var matchingFileChunks = evsFile.Where(c => c.Header != null &&
                    c.Header.H1EntryFacilityZIPCode == checkFileChunk.Header.H1EntryFacilityZIPCode &&
                    c.Header.H1EntryFacilityZIPplus4Code == checkFileChunk.Header.H1EntryFacilityZIPplus4Code &&
                    c.Header.H1EntryFacilityType == checkFileChunk.Header.H1EntryFacilityType &&
                    c.Header.H1MailerID == checkFileChunk.Header.H1MailerID
                );
                var recordCount = int.Parse(checkFileChunk.Header.H1FileRecordCount);
                foreach (var matchingFileChunk in matchingFileChunks.Where(c => c != checkFileChunk))
                {
                    recordCount += int.Parse(matchingFileChunk.Header.H1FileRecordCount) - 1;
                    foreach (var detail in matchingFileChunk.Details)
                    {
                        checkFileChunk.Details.Add(detail);
                    }
                    foreach (var container in matchingFileChunk.Containers)
                    {
                        checkFileChunk.Containers.Add(container);
                    }
                    matchingFileChunk.Header = null;
                }
                checkFileChunk.Header.H1FileRecordCount = AddLeadingZeros(recordCount.ToString());
            }
        }

        // <EvsPmodFileAsync>	
        private async Task EvsPmodFileAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var containerRepository = ServiceProvider.GetRequiredService<IContainerRepository>();
                var containers = await containerRepository.GetContainersForUspsEvsFileAsync(site.SiteName, date.Date, date.AddDays(1).Date);
                if (containers.Any())
                {
                    var now = DateTime.Now;
                    var fileName = $"USPS_eVs_{date:yyyMMdd}_{now:hhmmss}.ssf.manifest";
                    var uspsEvsProcessor = ServiceProvider.GetRequiredService<IUspsEvsProcessor>();
                    var response = await uspsEvsProcessor.CreateUspsRecordsForContainers(site, containers);
                    var byteArray = response.FileContents.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                    System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/evsPmodFiles/{fileName}");
                    Directory.CreateDirectory($"data export/evsPmodFiles");
                    using (var fileStream = new FileStream($"data export/evsPmodFiles/{fileName}", FileMode.Create))
                    {
                        await fileStream.WriteAsync(byteArray);
                        await fileStream.FlushAsync();
                    }
                    await CheckEvsPmodAsync(siteName, dateString, $"data export/evsPmodFiles/{fileName}");
                }
            }
        }
        // </EvsPmodFileAsync>

        public async Task<FileExportResponse> GeneratePmodEvsEodFile(Site site, DateTime manifestDate)
        {
            var response = new FileExportResponse();
            var eodContainers = await GetEvsEodContainers(site.SiteName, manifestDate);
            var manifestPackages = new List<ManifestBuilder.Package>();
            var manifestContainers = new List<ManifestBuilder.ShippingContainer>();
            var sequenceProcessor = ServiceProvider.GetRequiredService<ISequenceProcessor>();
            var mailerIdSequenceMaps = await EvsFileProcessor.GenerateMailerIdSequenceMap(sequenceProcessor, site);
            var fileNameSequence = await sequenceProcessor.GetSequenceAsync(site.SiteName, SequenceTypeConstants.EvsFileName);

            foreach (var eodContainer in eodContainers.Where(c => c.EvsPackageRecord != null))
            {
                manifestPackages.Add(EvsPackage.GetManifestBuilderPackage(eodContainer.EvsPackageRecord));
            }
            var request = new ManifestBuilder.CreateManifestRequest
            {
                Packages = manifestPackages,
                Containers = manifestContainers,
                EFNStartSequenceByMID = mailerIdSequenceMaps,
                MailDate = manifestDate,
                MailProducerMid = site.MailProducerMid,
                IsForPmodContainers = true,
                Site = new ManifestBuilder.Models.Site
                {
                    SiteName = site.SiteName,
                    EvsId = site.EvsId,
                    Zip = site.Zip
                }
            };

            var evsFileName = $"USPS_eVs_{manifestDate:yyyMMdd}{fileNameSequence.Number.ToString().PadLeft(4, '0')}.ssf.manifest";
            var createManifestResponse = ManifestBuilder.ManifestBuilder.CreateManifestFile(request);
            if (createManifestResponse.IsSuccessful)
            {

                response.FileName = evsFileName;
                response.FileContents.AddRange(createManifestResponse.EvsRecords);
                response.IsSuccessful = createManifestResponse.IsSuccessful;
                response.NumberOfRecords = response.FileContents.Count;
            }
            return response;
        }

        // <CheckEvsPmodAsync>	
        private async Task CheckEvsPmodAsync(string siteName, string dateString, string checkFileName)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var now = DateTime.Now;
                var evsFileProcessor = ServiceProvider.GetRequiredService<IEvsFileProcessor>();
                var response = await GeneratePmodEvsEodFile(site, date);
                var fileName = $"USPS_eVs_{date:yyyMMdd}_0000.ssf.manifest.{site.SiteName}[{response.FileContents.Count}].txt";
                System.Console.WriteLine($"Exporting {response.NumberOfRecords} records to file: data export/evsPmodFiles/{fileName}");
                Directory.CreateDirectory($"data export/evsPmodFiles");
                var byteArray = response.FileContents.SelectMany(x => Encoding.ASCII.GetBytes(x)).ToArray();
                using (var fileStream = new FileStream($"data export/evsPmodFiles/{fileName}", FileMode.Create))
                {
                    await fileStream.WriteAsync(byteArray);
                    await fileStream.FlushAsync();
                }
                if (checkFileName != null)
                {
                    var checkFile = await ReadEvsFile(checkFileName);
                    var newFile = ParseEvsFile(response.FileContents);
                    if (checkFile.Count != newFile.Count)
                    {
                        System.Console.WriteLine($"H1 count mismatch, check file: {checkFile.Count(c => c.Header != null)}, new file: {newFile.Count}");
                    }
                    for (var index = 0; index < checkFile.Count; index++)
                    {
                        var checkFileChunk = checkFile[index];
                        var newFileChunk = newFile.FirstOrDefault(c => c.Header != null &&
                            c.Header.H1EntryFacilityZIPCode == checkFileChunk.Header.H1EntryFacilityZIPCode &&
                            c.Header.H1EntryFacilityZIPplus4Code == checkFileChunk.Header.H1EntryFacilityZIPplus4Code &&
                            c.Header.H1EntryFacilityType == checkFileChunk.Header.H1EntryFacilityType &&
                            c.Header.H1MailerID == checkFileChunk.Header.H1MailerID
                        );
                        if (newFileChunk == null)
                        {
                            System.Console.WriteLine("Missing chunk in new file: " +
                                $"H1EntryFacilityZIPCode: {checkFileChunk.Header.H1EntryFacilityZIPCode}, " +
                                $"H1EntryFacilityZIPplus4Code: {checkFileChunk.Header.H1EntryFacilityZIPplus4Code}, " +
                                $"H1EntryFacilityType: {checkFileChunk.Header.H1EntryFacilityType}, " +
                                $"H1MailerID: {checkFileChunk.Header.H1MailerID}"
                            );
                        }
                        else
                        {
                            CompareHeaderRecords(checkFileChunk.Header, newFileChunk.Header);
                            CheckDetails(checkFileChunk, newFileChunk);
                        }
                    }
                }
            }
        }
        // </CheckEvsPmodAsync>

        // <TouchPackagesAsync>	
        private async Task TouchPackagesAsync(string siteName, string dateString, string countString = null)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                int.TryParse(countString, out var count);
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, siteName: siteName, count: count);
                packages.ToList().ForEach(p => { p.EodUpdateCounter = Math.Max(p.SqlEodProcessCounter, p.EodProcessCounter) + 1; });
                if (packages.Any())
                {
                    System.Console.WriteLine($"Updating {packages.Count()} packages for: {dateString}");
                    var response = await packageRepository.UpdateItemsAsync(packages.ToList());
                    if (response.IsSuccessful)
                        System.Console.WriteLine($"Updated {response.Count} packages for: {dateString}");
                    else
                        System.Console.WriteLine($"Updated failed: {response.FailedCount} packages for: {dateString}");
                }
            }
        }
        // </TouchPackagesAsync>

        // <CheckEod2Async>
        private async Task CheckEod2Async(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                var eodPackages = await eodPackageRepository.GetEodOverview(siteName, date);
                System.Console.WriteLine($"Retrieved {eodPackages.Count()} EOD processed packages for site: {site.SiteName}, date: {date}.");
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, siteName: site.SiteName);
                System.Console.WriteLine($"Retrieved {packages.Count()} processed packages for site: {site.SiteName}, date: {date}.");
                foreach (var eodPackage in eodPackages)
                {
                    var package = packages.FirstOrDefault(p => p.Id == eodPackage.CosmosId);
                    if (package == null)
                        System.Console.WriteLine($"Extra EOD package: {eodPackage.PackageId}");
                }
                foreach (var package in packages)
                {
                    var eodPackage = eodPackages.FirstOrDefault(p => p.CosmosId == package.Id);
                    if (eodPackage == null)
                        System.Console.WriteLine($"Missing EOD package: {package.PackageId}");
                }
            }
        }
        // </CheckEod2Async>

        // <RebuildEodAsync>	
        private async Task RebuildEodAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var eodProcessor = ServiceProvider.GetRequiredService<IEodProcessor>();
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);

                await eodProcessor.ResetContainerEod(site, date, false, false);
                await eodProcessor.ResetPackageEod(site, date, false, false);
            }
        }
        // </RebuildEodAsync>	

        static (object, object) GetRecordValues(string valueName, object newRecord, object checkRecord)
        {
            var checkValue = checkRecord.GetType().GetProperty(valueName).GetValue(checkRecord) ?? "";
            var newValue = newRecord.GetType().GetProperty(valueName).GetValue(newRecord) ?? "";
            return (newValue, checkValue);
        }

        static (bool, object, object) CompareRecordValues(string valueName, object newRecord, object checkRecord)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, newRecord, checkRecord);
            bool diff = newValue.ToString() != checkValue.ToString();
            if (diff && (valueName.Contains("Weight") || valueName.Contains("Cost")
                || valueName.Contains("Charge") || valueName.Contains("Surcharge")
                || valueName.Contains("SiteCode") || valueName.Contains("Pieces")))
            {
                decimal.TryParse(checkValue.ToString(), out var checkNumber);
                decimal.TryParse(newValue.ToString(), out var newNumber);
                diff = newNumber != checkNumber;
                return (diff, newNumber, checkNumber);
            }
            return (diff, newValue, checkValue);
        }

        static void CompareRecordItem(string recordType, string id, string cosmosId, string recordName, string valueName, object newRecord, object checkRecord)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, newRecord, checkRecord);
            var diff = checkValue.ToString() != newValue.ToString();
            if (diff && (valueName.Contains("Weight") || valueName.Contains("Cost")
                    || valueName.Contains("Charge") || valueName.Contains("Surcharge") || valueName.Contains("SiteCode")))
            {
                decimal.TryParse(checkValue.ToString(), out var checkNumber);
                decimal.TryParse(newValue.ToString(), out var newNumber);
                diff = checkNumber != newNumber;
            }
            if (diff)
                System.Console.WriteLine($"'{recordName}.{valueName}' mismatch for {recordType}: {id} {cosmosId}, check file: {checkValue}, new file: {newValue}");
        }

        static string[] PackageParentValueNames =
        {
            "PackageId",
            "ContainerId",
            "Barcode",
            "SiteName",
            "SubClientName",
            "IsPackageProcessed",
        };

        static void ComparePackageParentItem(string packageId, string cosmosId, string valueName,
            EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, sqlPackage, cosmosPackage);
            if (checkValue.ToString() != newValue.ToString())
                System.Console.WriteLine($"'EodPackage.{valueName}' mismatch for package: {packageId} {cosmosId}, check file: {checkValue}, new file: {newValue}");
        }

        static void ComparePackageParentRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in PackageParentValueNames)
            {
                ComparePackageParentItem(sqlPackage.PackageId, sqlPackage.CosmosId, valueName, sqlPackage, cosmosPackage);
            }
        }

        static void CopyPackageParentItem(string valueName,
            EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            var value = cosmosPackage.GetType().GetProperty(valueName).GetValue(cosmosPackage);
            sqlPackage.GetType().GetProperty(valueName).SetValue(sqlPackage, value);
        }

        static void CopyPackageParentRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in PackageParentValueNames)
            {
                CopyPackageParentItem(valueName, sqlPackage, cosmosPackage);
            }
        }

        static string[] ContainerParentValueNames =
        {
            "ContainerId",
            "SiteName",
            "IsContainerClosed",
       };

        static void CompareContainerParentItem(string containerId, string cosmosId, string valueName,
            EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, sqlContainer, cosmosContainer);
            if (checkValue.ToString() != newValue.ToString())
                System.Console.WriteLine($"'EodContainer.{valueName}' mismatch for container: {containerId} {cosmosId}, check file: {checkValue}, new file: {newValue}");
        }

        static void CompareContainerParentRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ContainerParentValueNames)
            {
                CompareContainerParentItem(sqlContainer.ContainerId, sqlContainer.CosmosId, valueName, sqlContainer, cosmosContainer);
            }
        }

        static void CopyContainerParentItem(string valueName,
            EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            var value = cosmosContainer.GetType().GetProperty(valueName).GetValue(cosmosContainer);
            sqlContainer.GetType().GetProperty(valueName).SetValue(sqlContainer, value);
        }

        static void CopyContainerParentRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ContainerParentValueNames)
            {
                CopyContainerParentItem(valueName, sqlContainer, cosmosContainer);
            }
        }

        static string[] PackageDetailValueNames =
        {
            "MmsLocation",
            "Customer",
            "ShipDate",
            "VamcId",
            "PackageId",
            "TrackingNumber",
            "ShipMethod",
            "BillMethod",
            "EntryUnitType",
            "ShipCost",
            "BillingCost",
            "SignatureCost",
            "ShipZone",
            "ZipCode",
            "Weight",
            "BillingWeight",
            "SortCode",
            "MarkupReason",
       };

        static void ComparePackageDetailRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in PackageDetailValueNames)
            {
                CompareRecordItem("package", sqlPackage.PackageId, sqlPackage.CosmosId,"PackageDetailRecord", valueName, sqlPackage.PackageDetailRecord, cosmosPackage.PackageDetailRecord);
            }
        }
        
        static void ComparePackageDetailRecord(PackageDetailRecord newRecord, PackageDetailRecord checkRecord)
        {
            foreach (var valueName in PackageDetailValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'PackageDetailRecord.{valueName}' mismatch for {checkRecord.PackageId}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyPackageDetailItem(string valueName,
            EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            var value = cosmosPackage.PackageDetailRecord.GetType().GetProperty(valueName).GetValue(cosmosPackage.PackageDetailRecord);
            sqlPackage.PackageDetailRecord.GetType().GetProperty(valueName).SetValue(sqlPackage.PackageDetailRecord, value);
        }

        static void CopyPackageDetailRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in PackageDetailValueNames)
            {
                CopyPackageDetailItem(valueName, sqlPackage, cosmosPackage);
            }
        }

        static string[] ContainerDetailValueNames =
        {
            "TrackingNumber",
            "ShipmentType",
            "PickupDate",
            "ShipReferenceNumber",
            "ShipperAccount",
            "DestinationName",
            "DestinationAddress1",
            "DestinationAddress2",
            "DestinationCity",
            "DestinationState",
            "DestinationZip",
            "DropSiteKey",
            "OriginName",
            "OriginAddress1",
            "OriginAddress2",
            "OriginCity",
            "OriginState",
            "OriginZip",
            "Reference1",
            "Reference2",
            "Reference3",
            "CarrierRoute1",
            "CarrierRoute2",
            "CarrierRoute3",
            "Weight",
            "DeliveryDate",
            "ExtraSvcs1",
            "ExtraSvcs2",
            "ExtraSvcs3",
        };

        static void CompareContainerDetailRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ContainerDetailValueNames)
            {
                CompareRecordItem("container", sqlContainer.ContainerId, sqlContainer.CosmosId, "ContainerDetailRecord", valueName, sqlContainer.ContainerDetailRecord, cosmosContainer.ContainerDetailRecord);
            }
        }

        static void CompareContainerDetailRecord(ContainerDetailRecord newRecord, ContainerDetailRecord checkRecord)
        {
            foreach (var valueName in ContainerDetailValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'ContainerDetailRecord.{valueName}' mismatch for {checkRecord.TrackingNumber}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyContainerDetailItem(string valueName,
           EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            var value = cosmosContainer.ContainerDetailRecord.GetType().GetProperty(valueName).GetValue(cosmosContainer.ContainerDetailRecord);
            sqlContainer.ContainerDetailRecord.GetType().GetProperty(valueName).SetValue(sqlContainer.ContainerDetailRecord, value);
        }

        static void CopyContainerDetailRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ContainerDetailValueNames)
            {
                CopyContainerDetailItem(valueName, sqlContainer, cosmosContainer);
            }
        }

        static string[] PmodContainerDetailValueNames =
        {
            "Site",
            "PdCust",
            "PdShipDate",
            "PdVamcId",
            "ContainerId",
            "PdTrackingNum",
            "PdShipMethod",
            "PdBillMethod",
            "PdEntryUnitType",
            "PdShipCost",
            "PdBillingCost",
            "PdSigCost",
            "PdShipZone",
            "PdZip5",
            "PdWeight",
            "PdBillingWeight",
            "PdSortCode",
            "PdMarkupReason",
        };

        static void ComparePmodContainerDetailRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in PmodContainerDetailValueNames)
            {
                CompareRecordItem("container", sqlContainer.ContainerId, sqlContainer.CosmosId, "PmodContainerDetail", valueName, sqlContainer.PmodContainerDetailRecord, cosmosContainer.PmodContainerDetailRecord);
            }
        }

        static void ComparePmodContainerDetailRecord(PmodContainerDetailRecord newRecord, PmodContainerDetailRecord checkRecord)
        {
            foreach (var valueName in PmodContainerDetailValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'ContainerDetailRecord.{valueName}' mismatch for {checkRecord.ContainerId}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyPmodContainerDetailItem(string valueName,
             EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            var value = cosmosContainer.PmodContainerDetailRecord.GetType().GetProperty(valueName).GetValue(cosmosContainer.PmodContainerDetailRecord);
            sqlContainer.PmodContainerDetailRecord.GetType().GetProperty(valueName).SetValue(sqlContainer.PmodContainerDetailRecord, value);
        }

        static void CopyPmodContainerDetailRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in PmodContainerDetailValueNames)
            {
                CopyPmodContainerDetailItem(valueName, sqlContainer, cosmosContainer);
            }
        }

        static string[] ReturnAsnValueNames =
        {
            "ParcelId",
            "SiteCode",
            "PackageWeight",
            "ProductCode",
            "Over84Flag",
            "Over108Flag",
            "NonMachinableFlag",
            "DelCon",
            "Signature",
            "CustomerNumber",
            "BolNumber",
            "PackageCreateDateDayMonthYear",
            "PackageCreateDateHourMinuteSecond",
            "ZipDestination",
            "PackageBarcode",
            "Zone",
            "TotalShippingCharge",
            "ConfirmationSurcharge",
            "NonMachinableSurcharge",
        };

        static void CompareReturnAsnRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in ReturnAsnValueNames)
            {
                CompareRecordItem("container", sqlPackage.PackageId, sqlPackage.CosmosId, "ReturnAsnRecord", valueName, sqlPackage.ReturnAsnRecord, cosmosPackage.ReturnAsnRecord);
            }
        }

        static void CompareReturnAsnRecord(ReturnAsnRecord newRecord, ReturnAsnRecord checkRecord)
        {
            foreach (var valueName in ReturnAsnValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'ReturnAsnRecord.{valueName}' mismatch for {newRecord.ParcelId}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyReturnAsnItem(string valueName,
            EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            var value = cosmosPackage.ReturnAsnRecord.GetType().GetProperty(valueName).GetValue(cosmosPackage.ReturnAsnRecord);
            sqlPackage.ReturnAsnRecord.GetType().GetProperty(valueName).SetValue(sqlPackage.ReturnAsnRecord, value);
        }

        static void CopyReturnAsnRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in ReturnAsnValueNames)
            {
                CopyReturnAsnItem(valueName, sqlPackage, cosmosPackage);
            }
        }

        static string[] EvsPackageValueNames =
        {
            "ContainerId",
            "TrackingNumber",
            "ServiceType",
            "ProcessingCategory",
            "Zone",
            "Weight",
            "MailerId",
            "Cost",
            "IsPoBox",
            "RecipientName",
            "AddressLine1",
            "Zip",
            "ReturnAddressLine1",
            "ReturnCity",
            "ReturnState",
            "ReturnZip",
            "EntryZip",
            "DestinationRateIndicator",
            "EntryFacilityType",
            "MailProducerCrid",
            "ParentMailOwnerMid",
            "UspsMailOwnerMid",
            "ParentMailOwnerCrid",
            "UspsMailOwnerCrid",
            "UspsPermitNo",
            "UspsPermitNoZip",
            "UspsPaymentMethod",
            "UspsPostageType",
            "UspsCsscNo",
            "UspsCsscProductNo",
        };

        static void CompareEvsPackageItem(string recordType, string recordId, string cosmosId, string valueName, 
            EvsPackage newRecord, EvsPackage checkRecord)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, newRecord, checkRecord);
            var diff = checkValue.ToString() != newValue.ToString();
            if (diff && (valueName.Contains("Weight") || valueName.Contains("Cost")))
            {
                decimal.TryParse(checkValue.ToString(), out var checkNumber);
                decimal.TryParse(newValue.ToString(), out var newNumber);
                diff = checkNumber != newNumber;
            }
            else if (diff && valueName == "ServiceType")
            {
                int.TryParse(newValue.ToString(), out var newNumber);
                diff = checkValue.ToString() != ((ManifestBuilder.ServiceType) newNumber).ToString();
            }
            else if (diff && valueName == "ProcessingCategory")
            {
                int.TryParse(newValue.ToString(), out var newNumber);
                diff = checkValue.ToString() != ((ManifestBuilder.ProcessingCategory) newNumber).ToString();
            }            
            else if (diff && valueName == "EntryFacilityType")
            {
                int.TryParse(newValue.ToString(), out var newNumber);
                diff = checkValue.ToString() != ((ManifestBuilder.EntryFacilityType) newNumber).ToString();
            }               
            if (diff)
                System.Console.WriteLine($"'EvsPackage.{valueName}' mismatch for {recordType}: {recordId} {cosmosId}, check file: {checkValue}, new file: {newValue}");
        }

        static void ComparePackageEvsPackage(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in EvsPackageValueNames)
            {
                CompareEvsPackageItem("package", sqlPackage.PackageId, sqlPackage.CosmosId, valueName, sqlPackage.EvsPackage, cosmosPackage.EvsPackage);
            }
        }

        static void CompareContainerEvsPackage(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in EvsPackageValueNames)
            {
                CompareEvsPackageItem("container", sqlContainer.ContainerId, sqlContainer.CosmosId, valueName, sqlContainer.EvsPackageRecord, cosmosContainer.EvsPackageRecord);
            }
        }

        static void CopyEvsPackageItem(string valueName,
            EvsPackage sqlPackage, EvsPackage cosmosPackage)
        {
            var value = cosmosPackage.GetType().GetProperty(valueName).GetValue(cosmosPackage);

            sqlPackage.GetType().GetProperty(valueName).SetValue(sqlPackage, value);
        }

        static void CopyContainerEvsPackage(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in EvsPackageValueNames)
            {
                CopyEvsPackageItem(valueName, sqlContainer.EvsPackageRecord, cosmosContainer.EvsPackageRecord);
            }
        }

        static void CopyPackageEvsPackage(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in EvsPackageValueNames)
            {
                CopyEvsPackageItem(valueName, sqlPackage.EvsPackage, cosmosPackage.EvsPackage);
            }
        }

        static string[] EvsContainerValueNames =
        {
            "ContainerId",
            "ShippingCarrier",
            "ShippingMethod",
            "ContainerType",
            "CarrierBarcode",
            "EntryZip",
            "EntryFacilityType",
        };

        static void CompareEvsContainerItem(string containerId, string cosmosId, string valueName, 
            EvsContainer newRecord, EvsContainer checkRecord)
        {
            (var newValue, var checkValue) = GetRecordValues(valueName, newRecord, checkRecord);
            var diff = checkValue.ToString() != newValue.ToString();
            if (diff)
                System.Console.WriteLine($"'EvsContainer.{valueName}' mismatch for container: {containerId} {cosmosId}, check file: {checkValue}, new file: {newValue}");
        }

        static void CompareEvsContainer(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in EvsContainerValueNames)
            {
                CompareEvsContainerItem(sqlContainer.ContainerId, sqlContainer.CosmosId, valueName, sqlContainer.EvsContainerRecord, cosmosContainer.EvsContainerRecord);
            }
        }

        static void CopyEvsContainerItem(string valueName, 
            EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            var value = cosmosContainer.EvsContainerRecord.GetType().GetProperty(valueName).GetValue(cosmosContainer.EvsContainerRecord);
            sqlContainer.EvsContainerRecord.GetType().GetProperty(valueName).SetValue(sqlContainer.EvsContainerRecord, value);
        }

        static void CopyEvsContainer(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in EvsContainerValueNames)
            {
                CopyEvsContainerItem(valueName, sqlContainer, cosmosContainer);
            }
        }

        static string[] InvoiceRecordValueNames =
        {
            "SubClientName",
            "BillingDate",
            "PackageId",
            "TrackingNumber",
            "BillingReference1",
            "BillingProduct",
            "BillingWeight",
            "Zone",
            "SigCost",
            "BillingCost",
            "Weight",
            "TotalCost",
        };

        static void CompareInvoiceRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in InvoiceRecordValueNames)
            {
                CompareRecordItem("package", sqlPackage.PackageId, sqlPackage.CosmosId, "InvoiceRecord", valueName, sqlPackage.InvoiceRecord, cosmosPackage.InvoiceRecord);
            }
        }

        static void CompareInvoiceRecord(InvoiceRecord newRecord, InvoiceRecord checkRecord)
        {
            foreach (var valueName in InvoiceRecordValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'InvoiceRecord.{valueName}' mismatch for {newRecord.PackageId} {newRecord.TrackingNumber}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyInvoiceItem(string valueName,
            EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            var value = cosmosPackage.InvoiceRecord.GetType().GetProperty(valueName).GetValue(cosmosPackage.InvoiceRecord);
            sqlPackage.InvoiceRecord.GetType().GetProperty(valueName).SetValue(sqlPackage.InvoiceRecord, value);
        }

        static void CopyInvoiceRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in InvoiceRecordValueNames)
            {
                CopyInvoiceItem(valueName, sqlPackage, cosmosPackage);
            }
        }


        static string[] ExpenseRecordValueNames =
        {
            "SubClientName",
            "ProcessingDate",
            "BillingReference1",
            "Product",
            "TrackingType",
            "Cost",
            "ExtraServiceCost",
            "Weight",
            "Zone",
        };

        static string[] GroupedExpenseRecordValueNames =
{
            "SubClientName",
            "ProcessingDate",
            "BillingReference1",
            "Product",
            "TrackingType",
            "Pieces",
            "SumWeight",
            "SumExtraServiceCost",
            "SumTotalCost",
            "AverageWeight",
            "AverageZone",
        };

        static void CompareContainerExpenseRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ExpenseRecordValueNames)
            {
                CompareRecordItem("container", sqlContainer.ContainerId, sqlContainer.CosmosId, "ExpenseRecord", valueName, sqlContainer.ExpenseRecord, cosmosContainer.ExpenseRecord);
            }
        }

        static void ComparePackageExpenseRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in ExpenseRecordValueNames)
            {
               CompareRecordItem("package", sqlPackage.PackageId, sqlPackage.CosmosId, "ExpenseRecord", valueName, sqlPackage.ExpenseRecord, cosmosPackage.ExpenseRecord);
            }
        }

        static void CompareExpenseRecord(GroupedExpenseRecord newRecord, GroupedExpenseRecord checkRecord)
        {
            foreach (var valueName in GroupedExpenseRecordValueNames)
            {
                (var diff, var newValue, var checkValue) = CompareRecordValues(valueName, newRecord, checkRecord);
                if (diff)
                    System.Console.WriteLine($"'ExpenseRecord.{valueName}' mismatch for {newRecord.BillingReference1}, check file: {checkValue}, new file: {newValue}");
            }
        }

        static void CopyExpenseItem(string valueName,
           ExpenseRecord sqlExpenseRecord, ExpenseRecord cosmosExpenseRecord)
        {
            var value = cosmosExpenseRecord.GetType().GetProperty(valueName).GetValue(cosmosExpenseRecord);
            sqlExpenseRecord.GetType().GetProperty(valueName).SetValue(sqlExpenseRecord, value);
        }

        static void CopyContainerExpenseRecord(EodContainer sqlContainer, EodContainer cosmosContainer)
        {
            foreach (var valueName in ExpenseRecordValueNames)
            {
                if (valueName == "SubClientName")
                    cosmosContainer.ExpenseRecord.SubClientName = cosmosContainer.SiteName; // was missing in old data
                CopyExpenseItem(valueName, sqlContainer.ExpenseRecord, cosmosContainer.ExpenseRecord);
            }
        }

        static void CopyPackageExpenseRecord(EodPackage sqlPackage, EodPackage cosmosPackage)
        {
            foreach (var valueName in ExpenseRecordValueNames)
            {
                if (valueName == "SubClientName")
                    cosmosPackage.ExpenseRecord.SubClientName = cosmosPackage.SubClientName; // was missing in old data
                CopyExpenseItem(valueName, sqlPackage.ExpenseRecord, cosmosPackage.ExpenseRecord);
            }
        }

        // <PopulateSqlEodAsync>	
        private async Task PopulateSqlEodAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var eodSqlContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                var bulkContainerResponse = await eodSqlContainerRepository.DeleteEodContainers(siteName, date);
                System.Console.WriteLine($"Time to delete Eod containers for site: {siteName}, Manifest Date: {date}: {bulkContainerResponse.ElapsedTime}");
                var cosmosContainers = await GetCosmosEodContainers(siteName, date);
                System.Console.WriteLine($"Total Cosmos Eod containers read: { cosmosContainers.Count() }");

                var sqlContainers = new List<EodContainer>();
                foreach (var cosmosContainer in cosmosContainers)
                {
                    var sqlContainer = new EodContainer() {
                        CosmosId = cosmosContainer.CosmosId,
                        CosmosCreateDate = cosmosContainer.CreateDate,
                        LocalProcessedDate = date.Date
                    };
                    sqlContainers.Add(sqlContainer);
                    CopyContainerParentRecord(sqlContainer, cosmosContainer);
                    if (cosmosContainer.ContainerDetailRecord != null)
                    {
                        sqlContainer.ContainerDetailRecord = new ContainerDetailRecord
                        {
                            CosmosId = cosmosContainer.CosmosId
                        };
                        CopyContainerDetailRecord(sqlContainer, cosmosContainer);
                    }
                    if (cosmosContainer.PmodContainerDetailRecord != null)
                    {
                        sqlContainer.PmodContainerDetailRecord = new PmodContainerDetailRecord
                        {
                            CosmosId = cosmosContainer.CosmosId
                        };
                        CopyPmodContainerDetailRecord(sqlContainer, cosmosContainer);
                    }
                    if (cosmosContainer.EvsContainerRecord != null)
                    {
                        sqlContainer.EvsContainerRecord = new EvsContainer
                        {
                            CosmosId = cosmosContainer.CosmosId
                        };
                        CopyEvsContainer(sqlContainer, cosmosContainer);
                    }
                    if (cosmosContainer.EvsPackageRecord != null)
                    {
                        sqlContainer.EvsPackageRecord = new EvsPackage
                        {
                            CosmosId = cosmosContainer.CosmosId
                        };
                        CopyContainerEvsPackage(sqlContainer, cosmosContainer);
                    }
                    if (cosmosContainer.ExpenseRecord != null)
                    {
                        sqlContainer.ExpenseRecord = new ExpenseRecord
                        {
                            CosmosId = cosmosContainer.CosmosId,
                            SubClientName = cosmosContainer.SiteName
                        };
                        CopyContainerExpenseRecord(sqlContainer, cosmosContainer);
                    }
                }
                bulkContainerResponse = await eodSqlContainerRepository.UpsertEodContainersAsync(sqlContainers);
                if (bulkContainerResponse.IsSuccessful)
                    System.Console.WriteLine($"Time to insert Eod containers for site: {siteName}, Manifest Date: {date}: {bulkContainerResponse.ElapsedTime}");
                else
                    System.Console.WriteLine($"Failed to insert Eod containers for site: {siteName}, Manifest Date: {date}: {bulkContainerResponse.Message}");


                var eodSqlPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                var bulkPackageResponse = await eodSqlPackageRepository.DeleteEodPackages(siteName, date);
                System.Console.WriteLine($"Time to delete Eod packages for site: {siteName}, Manifest Date: {date}: {bulkPackageResponse.ElapsedTime}");
                var cosmosPackages = await eodPackageRepository.GetEodPackages(siteName, date);
                System.Console.WriteLine($"Total Cosmos Eod packages read: { cosmosPackages.Count() }");

                var sqlPackages = new List<EodPackage>();
                foreach (var cosmosPackage in cosmosPackages)
                {
                    var sqlPackage = new EodPackage()
                    {
                        CosmosId = cosmosPackage.CosmosId,
                        CosmosCreateDate = cosmosPackage.CreateDate,
                        LocalProcessedDate = date.Date
                    };
                    sqlPackages.Add(sqlPackage);
                    CopyPackageParentRecord(sqlPackage, cosmosPackage);
                    if (cosmosPackage.PackageDetailRecord != null)
                    {
                        sqlPackage.PackageDetailRecord = new PackageDetailRecord
                        {
                            CosmosId = cosmosPackage.CosmosId
                        };
                        CopyPackageDetailRecord(sqlPackage, cosmosPackage);
                    }
                    if (cosmosPackage.ReturnAsnRecord != null)
                    {
                        sqlPackage.ReturnAsnRecord = new ReturnAsnRecord
                        {
                            CosmosId = cosmosPackage.CosmosId
                        };
                        CopyReturnAsnRecord(sqlPackage, cosmosPackage);
                    }
                    if (cosmosPackage.EvsPackage != null)
                    {
                        sqlPackage.EvsPackage = new EvsPackage
                        {
                            CosmosId = cosmosPackage.CosmosId
                        };
                        CopyPackageEvsPackage(sqlPackage, cosmosPackage);
                    }
                    if (cosmosPackage.InvoiceRecord != null)
                    {
                        sqlPackage.InvoiceRecord = new InvoiceRecord
                        {
                            CosmosId = cosmosPackage.CosmosId
                        };
                        CopyInvoiceRecord(sqlPackage, cosmosPackage);
                    }
                    if (cosmosPackage.ExpenseRecord != null)
                    {
                        sqlPackage.ExpenseRecord = new ExpenseRecord
                        {
                            CosmosId = cosmosPackage.CosmosId,
                            SubClientName = cosmosPackage.SubClientName
                        };
                        CopyPackageExpenseRecord(sqlPackage, cosmosPackage);
                    }
                }
                bulkPackageResponse = await eodSqlPackageRepository.UpsertEodPackagesAsync(sqlPackages);
                if (bulkPackageResponse.IsSuccessful)
                    System.Console.WriteLine($"Time to insert Eod packages for site: {siteName}, Manifest Date: {date}: {bulkPackageResponse.ElapsedTime}");
                else
                    System.Console.WriteLine($"Failed to insert Eod packages for site: {siteName}, Manifest Date: {date}: {bulkPackageResponse.Message}");
            }
        }
        // </PopulateSqlEodAsync>	

        // <CheckSqlEodAsync>	
        private async Task CheckSqlEodAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var eodSqlContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                var sqlContainers = await eodSqlContainerRepository.GetEodContainers(siteName, date);
                System.Console.WriteLine($"Total SQL Eod containers read: { sqlContainers.Count() }");
                var cosmosContainers = await GetCosmosEodContainers(siteName, date);
                System.Console.WriteLine($"Total Cosmos Eod containers read: { cosmosContainers.Count() }");

                foreach (var cosmosContainer in cosmosContainers)
                {
                    var sqlContainer = sqlContainers.FirstOrDefault(e => e.CosmosId == cosmosContainer.CosmosId);
                    if (sqlContainer == null)
                    {
                        System.Console.WriteLine($"Missing SQL container: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                    }
                    else
                    {
                        CompareContainerParentRecord(sqlContainer, cosmosContainer);
                        if (cosmosContainer.ContainerDetailRecord != null)
                        {
                            if (sqlContainer.ContainerDetailRecord == null)
                                System.Console.WriteLine($"Missing SQL ContainerDetailRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                            else
                                CompareContainerDetailRecord(sqlContainer, cosmosContainer);
                        }
                        if (cosmosContainer.PmodContainerDetailRecord != null)
                        {
                            if (sqlContainer.PmodContainerDetailRecord == null)
                                System.Console.WriteLine($"Missing SQL PmodContainerDetailRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                            else
                                ComparePmodContainerDetailRecord(sqlContainer, cosmosContainer);
                        }
                        if (cosmosContainer.EvsContainerRecord != null)
                        {
                            if (sqlContainer.EvsContainerRecord == null)
                                System.Console.WriteLine($"Missing SQL EvsContainerRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                            else
                                CompareEvsContainer(sqlContainer, cosmosContainer);
                        }
                        if (cosmosContainer.EvsPackageRecord != null)
                        {
                            if (sqlContainer.EvsPackageRecord == null)
                                System.Console.WriteLine($"Missing SQL EvsPackageRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                            else
                                CompareContainerEvsPackage(sqlContainer, cosmosContainer);
                        }
                        if (cosmosContainer.ExpenseRecord != null)
                        {
                            if (sqlContainer.ExpenseRecord == null)
                                System.Console.WriteLine($"Missing SQL ExpenseRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                            else
                                CompareContainerExpenseRecord(sqlContainer, cosmosContainer);
                        }
                    }
                }

                foreach (var sqlContainer in sqlContainers)
                {
                    var cosmosContainer = cosmosContainers.FirstOrDefault(e => e.CosmosId == sqlContainer.CosmosId);
                    if (sqlContainer == null)
                    {
                        System.Console.WriteLine($"Extra SQL container: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                    }
                    else
                    {
                        if (sqlContainer.ContainerDetailRecord != null && cosmosContainer.ContainerDetailRecord == null)
                            System.Console.WriteLine($"Extra SQL ContainerDetailRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                        if (sqlContainer.PmodContainerDetailRecord != null && cosmosContainer.PmodContainerDetailRecord == null)
                            System.Console.WriteLine($"Extra SQL PmodContainerDetailRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                        if (sqlContainer.EvsContainerRecord != null && cosmosContainer.EvsContainerRecord == null)
                            System.Console.WriteLine($"Extra SQL EvsContainerRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                        if (sqlContainer.EvsPackageRecord != null && cosmosContainer.EvsPackageRecord == null)
                            System.Console.WriteLine($"Extra SQL EvsPackageRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                        if (sqlContainer.ExpenseRecord != null && cosmosContainer.ExpenseRecord == null)
                            System.Console.WriteLine($"Extra SQL ExpenseRecord: {cosmosContainer.ContainerId} {cosmosContainer.SiteName}");
                    }
                }

                var eodSqlPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                var sqlPackages = await eodSqlPackageRepository.GetEodPackages(siteName, date);
                System.Console.WriteLine($"Total SQL Eod packages read: { sqlPackages.Count() }");
                var cosmosPackages = await GetCosmosEodPackages(siteName, date);
                System.Console.WriteLine($"Total Cosmos Eod packages read: { cosmosPackages.Count() }");

                foreach (var cosmosPackage in cosmosPackages)
                {
                    var sqlPackage = sqlPackages.FirstOrDefault(e => e.CosmosId == cosmosPackage.CosmosId);
                    if (sqlPackage == null)
                    {
                        System.Console.WriteLine($"Missing SQL package: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                    }
                    else
                    {
                        ComparePackageParentRecord(sqlPackage, cosmosPackage);
                        if (cosmosPackage.PackageDetailRecord != null)
                        {
                            if (sqlPackage.PackageDetailRecord == null)
                                System.Console.WriteLine($"Missing SQL PackageDetailRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                            else
                                ComparePackageDetailRecord(sqlPackage, cosmosPackage);
                        }
                        if (cosmosPackage.ReturnAsnRecord != null)
                        {
                            if (sqlPackage.ReturnAsnRecord == null)
                                System.Console.WriteLine($"Missing SQL ReturnAsnRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                            else
                                CompareReturnAsnRecord(sqlPackage, cosmosPackage);
                        }
                        if (cosmosPackage.EvsPackage != null)
                        {
                            if (sqlPackage.EvsPackage == null)
                                System.Console.WriteLine($"Missing SQL EvsPackage: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                            else
                                ComparePackageEvsPackage(sqlPackage, cosmosPackage);
                        }
                        if (cosmosPackage.InvoiceRecord != null)
                        {
                            if (sqlPackage.InvoiceRecord == null)
                                System.Console.WriteLine($"Missing SQL InvoiceRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                            else
                                CompareInvoiceRecord(sqlPackage, cosmosPackage);
                        }
                        if (cosmosPackage.ExpenseRecord != null)
                        {
                            if (sqlPackage.PackageDetailRecord == null)
                                System.Console.WriteLine($"Missing SQL ExpenseRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                            else
                                ComparePackageExpenseRecord(sqlPackage, cosmosPackage);
                        }
                    }
                }

                foreach (var sqlPackage in sqlPackages)
                {
                    var cosmosPackage = cosmosPackages.FirstOrDefault(e => e.CosmosId == sqlPackage.CosmosId);
                    if (sqlPackage == null)
                    {
                        System.Console.WriteLine($"Extra SQL package: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                    }
                    else
                    {
                        if (sqlPackage.PackageDetailRecord != null && cosmosPackage.PackageDetailRecord == null)
                            System.Console.WriteLine($"Extra SQL PackageDetailRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                        if (sqlPackage.ReturnAsnRecord != null && cosmosPackage.ReturnAsnRecord == null)
                            System.Console.WriteLine($"Extra SQL ReturnAsnRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                        if (sqlPackage.EvsPackage != null && cosmosPackage.EvsPackage == null)
                            System.Console.WriteLine($"Extra SQL EvsPackage: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                        if (sqlPackage.InvoiceRecord != null && cosmosPackage.InvoiceRecord == null)
                            System.Console.WriteLine($"Extra SQL InvoiceRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                        if (sqlPackage.ExpenseRecord != null && cosmosPackage.ExpenseRecord == null)
                            System.Console.WriteLine($"Extra SQL ExpenseRecord: {cosmosPackage.PackageId} {cosmosPackage.SiteName}");
                    }
                }
            }
        }
        // </CheckSqlEodAsync>

        // <UpdateBinsAsync>	
        private async Task UpdateBinsAsync(string subClientName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var activeGroupRepository = ServiceProvider.GetRequiredService<IActiveGroupRepository>();
                var binGroupId = await activeGroupRepository.GetCurrentActiveGroupIdAsync(ActiveGroupTypeConstants.Bins, subClient.SiteName, date.AddHours(12));
                var binMapGroupId = await activeGroupRepository.GetCurrentActiveGroupIdAsync(ActiveGroupTypeConstants.BinMaps, subClient.Name, date.AddHours(12));
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, siteName: subClient.SiteName, subClientName: subClientName);
                if (packages.Any())
                {
                    var packagesToUpdate = new List<PackageTracker.Data.Models.Package>();
                    foreach (var package in packages)
                    {
                        if (StringHelper.DoesNotExist(package.BinCode) || package.BinGroupId != binGroupId || package.BinMapGroupId != binMapGroupId)
                            packagesToUpdate.Add(package);
                    }
                    System.Console.WriteLine($"Update package bins and bin maps: {packagesToUpdate.Count()} packages for subClient {subClient.Name}.");
                    if (packagesToUpdate.Any())
                    {
                        var binProcessor = ServiceProvider.GetRequiredService<IBinProcessor>();
                        var isUpdate = true;
                        await binProcessor.AssignBinsForListOfPackagesAsync(packagesToUpdate, binGroupId, binMapGroupId, isUpdate);
                        var bulkProcessor = ServiceProvider.GetRequiredService<IBulkProcessor>();
                        var bulkUpdateResponse = await packageRepository.UpdatePackagesSetBinData(packagesToUpdate);
                        System.Console.WriteLine($"Total Request Units Consumed by UpdatePackagesSetBinData: {bulkUpdateResponse.RequestCharge}, Total Time Taken: {bulkUpdateResponse.ElapsedTime}, Documents Updated: {bulkUpdateResponse.Count}");

                        if (packagesToUpdate.Count() != bulkUpdateResponse.Count)
                        {
                            System.Console.WriteLine($"Update package bins and bin maps failed to update {packagesToUpdate.Count() - bulkUpdateResponse.Count} packages for subClient {subClient.Name}.");
                        }
                        else
                        {
                            System.Console.WriteLine($"Update package bins and bin maps updated {packagesToUpdate.Count()} packages for subClient {subClient.Name}.");
                        }
                    }
                }
            }
        }
        // </UpdateBinsAsync>

        // <UpdateBinDatasetsAsync>	
        private async Task UpdateBinDatasetsAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                DateTime now = DateTime.Now;
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var activeGroupRepository = ServiceProvider.GetRequiredService<IActiveGroupRepository>();
                var binProcessor = ServiceProvider.GetRequiredService<IBinProcessor>();
                var binDatasetProcessor = ServiceProvider.GetRequiredService<IBinDatasetProcessor>();
                var binDatasetRepository = ServiceProvider.GetRequiredService<IBinDatasetRepository>();

                var activeGroups = await activeGroupRepository.GetActiveGroupsByTypeAsync(ActiveGroupTypeConstants.Bins, site.SiteName);
                foreach(var activeGroup in activeGroups.Where(g => g.CreateDate >= date && g.IsDatasetProcessed))
                {
                    var bins = await binProcessor.GetBinsByActiveGroupIdAsync(activeGroup.Id);
                    var binDatasets = new List<BinDataset>();
                    bins.ForEach(b => binDatasetProcessor.CreateDataset(binDatasets, b, activeGroup));
                    if (await binDatasetRepository.ExecuteBulkInsertAsync(binDatasets, site.SiteName))  // Insert new data
                    {
                        await binDatasetRepository.EraseOldDataAsync(activeGroup.Id, now);              // Erase old data
                        System.Console.WriteLine($"Updated {binDatasets.Count} binDatasets for active group: {activeGroup.Id} for site: {site.SiteName}");
                    }
                    else
                    {
                        System.Console.WriteLine($"Update failed for {binDatasets.Count} binDatasets for active group: {activeGroup.Id} for site: {site.SiteName}");
                        break;
                    }
                }
            }
        }
        // </UpdateBinDatasetsAsync>        

        // <ListFilesAsync>	
        private async Task ListFilesAsync(string path)
        {
            var cloudFileClientFactory = ServiceProvider.GetRequiredService<ICloudFileClientFactory>();
            var fileClient = cloudFileClientFactory.GetCloudFileClient();
            var storageCredentials = new StorageCredentials(Configuration.GetSection("AzureFileShareAccountName").Value,
                Configuration.GetSection("AzureFileShareKey").Value);
            var directory = fileClient.GetShareReference(path).GetRootDirectoryReference();
            var token = new FileContinuationToken();
            var files = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (var file in files.Results)
            {
                var cloudFile = new CloudFile(file.Uri, storageCredentials);
                System.Console.WriteLine($"{cloudFile.Name}\t{cloudFile.Properties.LastModified}");
            }
        }
        // </ListFilesAsync>

        // <DeleteObsoleteContainersAsync>	
        private async Task DeleteObsoleteContainersAsync(string siteName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var now = DateTime.UtcNow;
                var localTimeOffset = (int)(now - siteLocalTime).TotalMinutes;
                var operationalContainerRepository = ServiceProvider.GetRequiredService<IOperationalContainerRepository>();
                var operationalContainers = await operationalContainerRepository.GetOutOfDateOperationalContainersAsync(siteName, date, localTimeOffset);
                System.Console.WriteLine($"{operationalContainers.Count()} outdated operational containers found for for site {siteName}.");
                foreach (var container in operationalContainers)
                {
                    container.Status = ContainerEventConstants.Deleted;
                    await operationalContainerRepository.UpdateItemAsync(container);
                }
                var containerRepository = ServiceProvider.GetRequiredService<IContainerRepository>();
                var containers = await containerRepository.GetOutOfDateContainersAsync(siteName, date);
                System.Console.WriteLine($"{containers.Count()} outdated containers found for for site {siteName}.");
                foreach (var container in containers)
                {
                    container.Status = ContainerEventConstants.Deleted;
                    await containerRepository.UpdateItemAsync(container);
                }
            }
        }
        // </DeleteObsoleteContainersAsync>	

        // <ImportAsnFileAsync>	
        private async Task ImportAsnFileAsync(string subClientName, string fileName)
        {
            var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
            var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
            var asnFileProcessor = ServiceProvider.GetRequiredService<IAsnFileProcessor>();
            var packages = new List<PackageTracker.Data.Models.Package>();
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                if (subClient.AsnImportFormat == FileFormatConstants.CmopAsnImportFormat1)
                {
                    var fileReadResponse = await asnFileProcessor.ReadCmopFileStreamAsync(fileStream);
                    packages.AddRange(fileReadResponse.Packages);

				}
				else if (subClient.AsnImportFormat == FileFormatConstants.DalcAsnImportFormat1)
				{
					var fileReadResponse = await asnFileProcessor.ReadDalcFileStreamAsync(fileStream);
					packages.AddRange(fileReadResponse.Packages);
				}
				var webJobId = Guid.NewGuid();
				var importResponse = await asnFileProcessor.ImportPackages(packages, subClient, false, webJobId.ToString());
				System.Console.WriteLine($"{importResponse.NumberOfDocumentsImported} packages imported for for site {subClientName}.");
			}
		}
		// </ImportAsnFileAsync>

		// <UpdateUspsTrackingData>	
		private async Task UpdateUspsTrackingDataAsync(string siteName, string minString, string maxString)
		{
			var packageDatasetProcessor = ServiceProvider.GetRequiredService<IPackageDatasetProcessor>();
			var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
			var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
			var trackPackageProcessor = ServiceProvider.GetRequiredService<ITrackPackageProcessor>();
			var trackPackageDatasetProcessor = ServiceProvider.GetRequiredService<ITrackPackageDatasetProcessor>();
			var subClients = await subClientProcessor.GetSubClientsAsync();
			var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);

			int.TryParse(minString, out var lookbackMin);
			int.TryParse(maxString, out var lookbackMax);
			var packagesToUpdate = await packageDatasetProcessor.GetPackagesWithNoSTC(site, lookbackMin, lookbackMax);
            var shippingCarrier = ShippingCarrierConstants.Usps;
            System.Console.WriteLine($"USPS Packages without STC: {packagesToUpdate.Count()} for site: {site.SiteName}");
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
				System.Console.WriteLine($"Updating USPS Tracking data: {trackPackages.Count()} for site: {site.SiteName}");
				await trackPackageDatasetProcessor.UpdateTrackPackageDatasets(shippingCarrier, trackPackages);
			}
		}
        // </UpdateUspsTrackingData>

        // <RatePackagesAsync>	
        private async Task RatePackagesAsync(string subClientName, string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var activeGroupRepository = ServiceProvider.GetRequiredService<IActiveGroupRepository>();
                var ratesGroupId = await activeGroupRepository.GetCurrentActiveGroupIdAsync(ActiveGroupTypeConstants.Rates, subClientName, siteLocalTime);
                var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                var totalWatch = Stopwatch.StartNew();
                var packages = await packageRepository.GetProcessedPackagesByDate(date, siteName: subClient.SiteName, subClientName: subClientName);
                System.Console.WriteLine($"Update rates for: {packages.Count()} packages for subClient {subClient.Name}, Elapsed Time: {TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds)}.");
                if (packages.Any())
                {
                    System.Console.WriteLine($"Update package rates for subClient {subClient.Name}, ratesGroupId: {ratesGroupId}.");
                    var rateProcessor = ServiceProvider.GetRequiredService<IRateProcessor>();
                    var eodProcessor = ServiceProvider.GetRequiredService<IEodProcessor>();
                    var evsFileProcessor = ServiceProvider.GetRequiredService<IEvsFileProcessor>();
                    var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), false);
                    var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), true);
                    var webJobId = Guid.NewGuid().ToString();
                    int chunk = 500;
                    for (int offset = 0; offset < packages.Count(); offset += chunk)
                    {
                        var packagesChunk = packages.Skip(offset).Take(chunk);
                        var eodPackagesToUpdate = new List<EodPackage>();
                        await rateProcessor.AssignPackageRatesForEod(subClient, packagesChunk.ToList(), webJobId);
                        var bulkUpdateResponse = await packageRepository.UpdatePackagesEodProcessed(packagesChunk);
                        System.Console.WriteLine($"Total Request Units Consumed by UpdatePackagesEodProcessed: {bulkUpdateResponse.RequestCharge}, Total Time Taken: {bulkUpdateResponse.ElapsedTime}, Documents Updated: {bulkUpdateResponse.Count}");
                        if (! bulkUpdateResponse.IsSuccessful)
                        {
                            System.Console.WriteLine($"Update failed to update {packagesChunk.Count() - bulkUpdateResponse.Count} packages for subClient {subClient.Name}.");
                            break;
                        }
                        else
                        {
                            System.Console.WriteLine($"Updated {packagesChunk.Count()} packages for subClient {subClient.Name}.");
                        }
                        foreach (var package in packagesChunk)
                        {
                            var eodPackage = eodProcessor.GenerateEodPackage(site, null, subClient, package);
                            eodPackagesToUpdate.Add(eodPackage);
                            eodProcessor.GenerateEodPackageFileRecords(site, subClient, eodPackage, package,
                                primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
                        }
                        var response = await eodPackageRepository.UpsertEodPackagesAsync(eodPackagesToUpdate);
                        if (response.IsSuccessful)
                        {
                            Console.WriteLine($"Total time for Eod packages bulk insert or update: {response.ElapsedTime}");
                        }
                        else
                        {
                            Console.WriteLine($"Eod packages bulk insert or update failed: {response.Message}");
                            break;
                        }
                        System.Console.WriteLine($"Elapsed Time: {TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds)}.");
                    }
                }
            }
        }
        // </RatePackagesAsync>


        // <CheckEvs2Async>	
        private async Task CheckEvs2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadEvsFile(checkFilePath);
            var newFile = await ReadEvsFile(newFilePath);
            MergeEvsFileChunks(checkFile);
            MergeEvsFileChunks(newFile);
            if (checkFile.Count(c => c.Header != null) != newFile.Count(c => c.Header != null))
            {
                System.Console.WriteLine($"H1 count mismatch, check file: {checkFile.Count(c => c.Header != null)}, new file: {newFile.Count}");
            }
            for (var index = 0; index < checkFile.Count; index++)
            {
                var checkFileChunk = checkFile[index];
                if (checkFileChunk.Header == null)
                    continue; // Chunk was merged.
                var newFileChunk = newFile.FirstOrDefault(c => c.Header != null &&
                    c.Header.H1EntryFacilityZIPCode == checkFileChunk.Header.H1EntryFacilityZIPCode &&
                    c.Header.H1EntryFacilityZIPplus4Code == checkFileChunk.Header.H1EntryFacilityZIPplus4Code &&
                    c.Header.H1EntryFacilityType == checkFileChunk.Header.H1EntryFacilityType &&
                    c.Header.H1MailerID == checkFileChunk.Header.H1MailerID
                );
                if (newFileChunk == null)
                {
                    System.Console.WriteLine("Missing chunk in new file: " +
                        $"H1EntryFacilityZIPCode: {checkFileChunk.Header.H1EntryFacilityZIPCode}, " +
                        $"H1EntryFacilityZIPplus4Code: {checkFileChunk.Header.H1EntryFacilityZIPplus4Code}, " +
                        $"H1EntryFacilityType: {checkFileChunk.Header.H1EntryFacilityType}, " +
                        $"H1MailerID: {checkFileChunk.Header.H1MailerID}"
                    );
                }
                else
                {
                    CompareHeaderRecords(checkFileChunk.Header, newFileChunk.Header);
                    CheckDetails(checkFileChunk, newFileChunk);
                    CheckContainers(checkFileChunk, newFileChunk);
                }
            }
        }
        // </CheckEvs2Async>

        private static ContainerDetailRecord ParseContainerDetailLine(string line)
        {
            var record = new ContainerDetailRecord();
            var values = line.Split("|");
            for (int i = 0; i < Math.Min(ContainerDetailValueNames.Length, values.Length); i++)
            {
                var valueName = ContainerDetailValueNames[i];
                object value = values[i].Trim();
                if (valueName.Contains("Weight"))
                {
                    decimal.TryParse(value.ToString(), out var number);
                    value = number;
                }
                record.GetType().GetProperty(valueName).SetValue(record, value.ToString());
            }
            return record;
        }

        static async Task<IEnumerable<ContainerDetailRecord>> ReadContainerDetailFile(string path)
        {
            var records = new List<ContainerDetailRecord>();
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        records.Add(ParseContainerDetailLine(await reader.ReadLineAsync()));
                    }
                }
            }
            return records;
        }

        // <CheckContainerDetail2Async>	
        private async Task CheckContainerDetail2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadContainerDetailFile(checkFilePath);
            var newFile = await ReadContainerDetailFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.TrackingNumber == checkRecord.TrackingNumber);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.TrackingNumber}");
                }
                else
                {
                    CompareContainerDetailRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.TrackingNumber == newRecord.TrackingNumber);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.TrackingNumber}");
                }
            }
        }
        // </CheckContainerDetail2Async>

        private static PmodContainerDetailRecord ParsePmodContainerDetailLine(string line)
        {
            var record = new PmodContainerDetailRecord();
            var values = line.Split("|");
            for (int i = 0; i < Math.Min(PmodContainerDetailValueNames.Length, values.Length); i++)
            {
                var valueName = PmodContainerDetailValueNames[i];
                object value = values[i].Trim();
                if (valueName.Contains("Weight") || valueName.Contains("Cost"))
                {
                    decimal.TryParse(value.ToString(), out var number);
                    value = number;
                }
                record.GetType().GetProperty(valueName).SetValue(record, value.ToString());
            }
            return record;
        }

        static async Task<IEnumerable<PmodContainerDetailRecord>> ReadPmodContainerDetailFile(string path)
        {
            var records = new List<PmodContainerDetailRecord>();
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        records.Add(ParsePmodContainerDetailLine(await reader.ReadLineAsync()));
                    }
                }
            }
            return records;
        }

        // <CheckPmodContainerDetail2Async>	
        private async Task CheckPmodContainerDetail2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadPmodContainerDetailFile(checkFilePath);
            var newFile = await ReadPmodContainerDetailFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.ContainerId == checkRecord.ContainerId);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.ContainerId}");
                }
                else
                {
                    ComparePmodContainerDetailRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.ContainerId == newRecord.ContainerId);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.ContainerId}");
                }
            }
        }
        // </CheckPmodContainerDetail2Async>

        private static PackageDetailRecord ParsePackageDetailLine(string line)
        {
            var record = new PackageDetailRecord();
            var values = line.Split("|");
            for (int i = 0; i < Math.Min(PackageDetailValueNames.Length, values.Length); i++)
            {
                var valueName = PackageDetailValueNames[i];
                object value = values[i].Trim();
                if (valueName.Contains("Weight") || valueName.Contains("Cost"))
                {
                    decimal.TryParse(value.ToString(), out var number);
                    value = number;
                }                    
                record.GetType().GetProperty(valueName).SetValue(record, value.ToString());
            }
            return record;
        }

        static async Task<IEnumerable<PackageDetailRecord>> ReadPackageDetailFile(string path)
        {
            var records = new List<PackageDetailRecord>();
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    var first = true; // skip header line
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (! first)
                            records.Add(ParsePackageDetailLine(line));
                        first = false;
                    }
                }
            }
            return records;
        }

        // <CheckPackageDetail2Async>	
        private async Task CheckPackageDetail2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadPackageDetailFile(checkFilePath);
            var newFile = await ReadPackageDetailFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.PackageId == checkRecord.PackageId);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.PackageId}");
                }
                else
                {
                    ComparePackageDetailRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.PackageId == newRecord.PackageId);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.PackageId}");
                }
            }
        }
        // </CheckPackageDetail2Async>

        private static ReturnAsnRecord ParseAsnRecordLine(string line)
        {
            var record = new ReturnAsnRecord();
            var values = line.Split("|");
            for (int i = 0; i < Math.Min(ReturnAsnValueNames.Length, values.Length); i++)
            {
                object value = values[i].Trim();
                record.GetType().GetProperty(ReturnAsnValueNames[i]).SetValue(record, value);
            }
            return record;
        }

        static async Task<IEnumerable<ReturnAsnRecord>> ReadReturnAsnFile(string path)
        {
            var records = new List<ReturnAsnRecord>();
            using (var stream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        records.Add(ParseAsnRecordLine(await reader.ReadLineAsync()));
                    }
                }
            }
            return records;
        }

        // <CheckReturnAsn2Async>	
        private async Task CheckReturnAsn2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadReturnAsnFile(checkFilePath);
            var newFile = await ReadReturnAsnFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.ParcelId == checkRecord.ParcelId);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.ParcelId}");
                }
                else
                {
                    CompareReturnAsnRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.ParcelId == newRecord.ParcelId);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.ParcelId}");
                }
            }
        }
        // </CheckReturnAsn2Async>

        private static InvoiceRecord ParseInvoiceRecordLine(string line)
        {
            var record = new InvoiceRecord();
            var values = line.Split(",");
            for (int i = 0; i < Math.Min(InvoiceRecordValueNames.Length, values.Length); i++)
            {
                var valueName = InvoiceRecordValueNames[i];
                object value = values[i].Trim();
                record.GetType().GetProperty(valueName).SetValue(record, value);
            }
            return record;
        }

        static async Task<IEnumerable<InvoiceRecord>> ReadInvoiceRecordFile(string path)
        {
            var records = new List<InvoiceRecord>();
            if (path.EndsWith(".xlsx"))
            {
                var invoiceProcessor = ServiceProvider.GetRequiredService<IInvoiceProcessor>();
                var headers = invoiceProcessor.BuildInvoiceHeader(true);
                var ws = new ExcelWorkSheet(path);
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    var record = new InvoiceRecord();
                    for (int i = 0; i < Math.Max(InvoiceRecordValueNames.Length, headers.Length); i++)
                    {
                        var value = ws.GetStringValue(row, headers[i]);
                        record.GetType().GetProperty(InvoiceRecordValueNames[i]).SetValue(record, value);
                    }
                    if (StringHelper.Exists(record.PackageId))
                        records.Add(record);
                }
            }
            else
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var first = true; // skip header line
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!first && !line.StartsWith(','))
                                records.Add(ParseInvoiceRecordLine(line));
                            first = false;
                        }
                    }
                }
            }
            return records;
        }

        // <CheckInvoice2Async>	
        private async Task CheckInvoice2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadInvoiceRecordFile(checkFilePath);
            var newFile = await ReadInvoiceRecordFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.PackageId == checkRecord.PackageId);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.PackageId} {checkRecord.TrackingNumber}");
                }
                else
                {
                    CompareInvoiceRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.PackageId == newRecord.PackageId);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.PackageId} {newRecord.TrackingNumber}");
                }
            }
        }
        // </CheckInvoice2Async>

        private static GroupedExpenseRecord ParseExpenseRecordLine(string line)
        {
            var record = new GroupedExpenseRecord();
            var values = line.Split(",");
            for (int i = 0; i < Math.Min(GroupedExpenseRecordValueNames.Length, values.Length); i++)
            {
                var valueName = GroupedExpenseRecordValueNames[i];
                object value = values[i].Trim();
                if (valueName.Contains("Pieces"))

                {
                    int.TryParse(value.ToString(), out var number);
                    value = number;
                }
                record.GetType().GetProperty(valueName).SetValue(record, value);
            }
            return record;
        }

        static async Task<IEnumerable<GroupedExpenseRecord>> ReadExpenseRecordFile(string path)
        {
            var records = new List<GroupedExpenseRecord>();
            if (path.EndsWith(".xlsx"))
            {
                var expenseProcessor = ServiceProvider.GetRequiredService<IExpenseProcessor>();
                var headers = expenseProcessor.BuildExpenseHeader();
                var ws = new ExcelWorkSheet(path);
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    var record = new GroupedExpenseRecord();
                    for (int i = 0; i < Math.Max(GroupedExpenseRecordValueNames.Length, headers.Length); i++)
                    {
                        object value = ws.GetStringValue(row, headers[i]);
                        var valueName = GroupedExpenseRecordValueNames[i];
                        if (valueName.Contains("Pieces"))
                            value = ws.GetIntValue(row, headers[i]);
                        record.GetType().GetProperty(valueName).SetValue(record, value);
                    }
                    if (StringHelper.Exists(record.BillingReference1))
                        records.Add(record);
                }
            }
            else {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var first = true; // skip header line
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!first && !line.StartsWith(','))
                                records.Add(ParseExpenseRecordLine(line));
                            first = false;
                        }
                    }
                }
            }
            return records;
        }

        // <CheckExpense2Async>	
        private async Task CheckExpense2Async(string checkFilePath, string newFilePath)
        {
            var checkFile = await ReadExpenseRecordFile(checkFilePath);
            var newFile = await ReadExpenseRecordFile(newFilePath);
            foreach (var checkRecord in checkFile)
            {
                var newRecord = newFile.FirstOrDefault(r => r.BillingReference1 == checkRecord.BillingReference1 && r.Product == checkRecord.Product);
                if (newRecord == null)
                {
                    System.Console.WriteLine($"Missing Record in new file: {checkRecord.BillingReference1} {checkRecord.Product}");
                }
                else
                {
                    CompareExpenseRecord(newRecord, checkRecord);
                }
            }
            foreach (var newRecord in newFile)
            {
                var checkRecord = newFile.FirstOrDefault(r => r.BillingReference1 == newRecord.BillingReference1 && r.Product == newRecord.Product);
                if (checkRecord == null)
                {
                    System.Console.WriteLine($"Extra Record in new file: {newRecord.BillingReference1} { newRecord.Product}");
                }
            }
        }
        // </CheckExpense2Async>

        private async Task<IEnumerable<EodPackage>> GetEodPackages(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if( containerType == "Cosmos")
            {
                results.AddRange(await GetCosmosEodPackages(siteName, targetDate));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetEodPackages(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodPackage>> GetPackageDetails(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var packages = await GetCosmosEodPackages(siteName, targetDate);
                results.AddRange(packages.Where(p => p.PackageDetailRecord != null));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetPackageDetails(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodPackage>> GetReturnAsns(string siteName, DateTime targetDate, string subClientName)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var packages = await GetCosmosEodPackages(siteName, targetDate);
                results.AddRange(packages.Where(p => p.ReturnAsnRecord != null && p.SubClientName == subClientName));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetReturnAsns(siteName, targetDate, subClientName));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodPackage>> GetExpenseRecords(string siteName, DateTime targetDate, string subClientName)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var packages = await GetCosmosEodPackages(siteName, targetDate);
                results.AddRange(packages.Where(p => p.ExpenseRecord != null && p.SubClientName == subClientName));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetExpenseRecords(siteName, targetDate, subClientName));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodPackage>> GetInvoiceRecords(string siteName, DateTime targetDate, string subClientName)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var packages = await GetCosmosEodPackages(siteName, targetDate);
                results.AddRange(packages.Where(p => p.InvoiceRecord != null && p.SubClientName == subClientName));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetInvoiceRecords(siteName, targetDate, subClientName));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }
       
        private async Task<IEnumerable<EodPackage>> GetEvsEodPackages(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodPackage>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var packages = await GetCosmosEodPackages(siteName, targetDate);
                results.AddRange(packages.Where(p => p.EvsPackage != null));
            }
            else
            {
                var eodPackageRepository = ServiceProvider.GetRequiredService<IEodPackageRepository>();
                results.AddRange(await eodPackageRepository.GetEvsEodPackages(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Packages from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodContainer>> GetEodContainers(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodContainer>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                results.AddRange(await GetCosmosEodContainers(siteName, targetDate));
            }
            else
            {
                var eodContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                results.AddRange(await eodContainerRepository.GetEodContainers(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Containers from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodContainer>> GetContainerDetails(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodContainer>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var containers = await GetCosmosEodContainers(siteName, targetDate);
                results.AddRange(containers.Where(c => c.ContainerDetailRecord != null));
            }
            else
            {
                var eodContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                results.AddRange(await eodContainerRepository.GetContainerDetails(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Containers from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodContainer>> GetPmodContainerDetails(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodContainer>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var containers = await GetCosmosEodContainers(siteName, targetDate);
                results.AddRange(containers.Where(c => c.PmodContainerDetailRecord != null));
            }
            else
            {
                var eodContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                results.AddRange(await eodContainerRepository.GetPmodContainerDetails(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Containers from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodContainer>> GetExpenseRecords(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodContainer>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var containers = await GetCosmosEodContainers(siteName, targetDate);
                results.AddRange(containers.Where(c => c.ExpenseRecord != null));
            }
            else
            {
                var eodContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                results.AddRange(await eodContainerRepository.GetExpenseRecords(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Containers from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodContainer>> GetEvsEodContainers(string siteName, DateTime targetDate)
        {
            var stopWatch = Stopwatch.StartNew();
            var results = new List<EodContainer>();
            var containerType = Configuration.GetValue<string>("EodContainerType", "Cosmos");
            if (containerType == "Cosmos")
            {
                var containers = await GetCosmosEodContainers(siteName, targetDate);
                results.AddRange(containers.Where(c => c.EvsContainerRecord != null || c.EvsPackageRecord != null));
            }
            else
            {
                var eodContainerRepository = ServiceProvider.GetRequiredService<IEodContainerRepository>();
                results.AddRange(await eodContainerRepository.GetEvsEodContainers(siteName, targetDate));
            }
            Console.WriteLine($"Total time to Retrieved {results.Count} Eod Containers from {containerType}: {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds)}");
            return results;
        }

        private async Task<IEnumerable<EodPackage>> GetCosmosEodPackages(string siteName, DateTime targetDate)
        {
            var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
            var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
            var subClientProcessor = ServiceProvider.GetRequiredService<ISubClientProcessor>();
            var subClients = await subClientProcessor.GetSubClientsAsync();
            var packageRepository = ServiceProvider.GetRequiredService<IPackageRepository>();
            var packages = await packageRepository.GetProcessedPackagesByDate(targetDate, siteName: site.SiteName);
            var eodProcessor = ServiceProvider.GetRequiredService<IEodProcessor>();
            var evsFileProcessor = ServiceProvider.GetRequiredService<IEvsFileProcessor>();
            var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), false);
            var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, packages.ToList(), true);
            var eodPackages = new List<EodPackage>();
            foreach (var package in packages)
            {
                var subClient = subClients.FirstOrDefault(s => s.Name == package.SubClientName);
                var eodPackage = eodProcessor.GenerateEodPackage(site, null, subClient, package);
                eodPackages.Add(eodPackage);
                eodProcessor.GenerateEodPackageFileRecords(site, subClient, eodPackage, package,
                            primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
            }
            return eodPackages;
        }

        private async Task<IEnumerable<EodContainer>> GetCosmosEodContainers(string siteName, DateTime targetDate)
        {
            var siteProcessor = ServiceProvider.GetRequiredService<ISiteProcessor>();
            var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
            var containerRepository = ServiceProvider.GetRequiredService<IContainerRepository>();
            var containers = await containerRepository.GetClosedContainersByDate(targetDate, siteName: site.SiteName);
            var eodProcessor = ServiceProvider.GetRequiredService<IEodProcessor>();
            var activeBins = await eodProcessor.GetUniqueBinsByActiveGroups(containers);
            var evsFileProcessor = ServiceProvider.GetRequiredService<IEvsFileProcessor>();
            var primaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, containers.ToList(), false);
            var secondaryCarrierBinEntryZipMaps = await evsFileProcessor.GenerateBinEntryZipMap(site, containers.ToList(), true);
            var eodContainers = new List<EodContainer>();
            foreach (var container in containers)
            {
                var eodContainer = eodProcessor.GenerateEodContainer(site, null, container);
                eodContainers.Add(eodContainer);
                eodProcessor.GenerateEodContainerFileRecords(site, activeBins, eodContainer, container,
                    primaryCarrierBinEntryZipMaps, secondaryCarrierBinEntryZipMaps);
            }
            return eodContainers;
        }

        private readonly IDictionary<string, bool> carrierIsStopTheClock = new Dictionary<string, bool>(); // "ShippingCarrier:Code" => IsStopTheClock
        private readonly IDictionary<string, int> carrierIsUndeliverable = new Dictionary<string, int>(); // "ShippingCarrier:Code" => IsUndeliverable
        private readonly IDictionary<string, string> carrierEventDescriptions = new Dictionary<string, string>(); // "ShippingCarrier:Code" => Description

        private void LoadEventCodes()
        {
           var evsCodeRepository = ServiceProvider.GetRequiredService<IEvsCodeRepository>();
           if (carrierEventDescriptions.Count == 0)
            {
                var evsCodes = evsCodeRepository.GetEvsCodesAsync().GetAwaiter().GetResult().ToList();
                evsCodes.ForEach(c => carrierIsStopTheClock[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.IsStopTheClock == 1);
                evsCodes.ForEach(c => carrierIsUndeliverable[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.IsUndeliverable);
                evsCodes.ForEach(c => carrierEventDescriptions[$"{ShippingCarrierConstants.Usps}:{c.Code}"] = c.Description);
            }
        }

        public bool IsStopTheClock(string eventCode, string shippingCarrier)
        {
            LoadEventCodes();
            carrierIsStopTheClock.TryGetValue($"{shippingCarrier}:{eventCode}", out var isSTC);
            return isSTC;
        }

        public int IsUndeliverable(string eventCode, string shippingCarrier)
        {
            LoadEventCodes();
            carrierIsUndeliverable.TryGetValue($"{shippingCarrier}:{eventCode}", out var isUndeliverable);
            return isUndeliverable;
        }
        public string EventDescription(string eventCode, string shippingCarrier, string eventDescription)
        {
            LoadEventCodes();
            if (carrierEventDescriptions.TryGetValue($"{shippingCarrier}:{eventCode}", out var description))
                eventDescription = description;
            return eventDescription;
        }

        private async Task FixAsync(string siteName, string path)
        {
            LoadEventCodes();
            var packageDatasetRepository = ServiceProvider.GetRequiredService<IPackageDatasetRepository>();
            var postalDaysRepository = ServiceProvider.GetRequiredService<IPostalDaysRepository>();
 
            var ws = new ExcelWorkSheet(path);
            var packages = new List<PackageDataset>();
            for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
            {
                packages.Add(new PackageDataset
                {
                    PackageId = ws.GetStringValue(row, "PackageId"),
                    ShippingBarcode = ws.GetStringValue(row, "lastbar"),
                    StopTheClockEventDate = IsStopTheClock(ws.GetStringValue(row, "EventCode"), ShippingCarrierConstants.Usps) 
                        ?  ws.GetDateValue(row, "EventDate") : null,
                    LastKnownEventDate = ws.GetDateValue(row, "EventDate"),
                    LastKnownEventDescription = EventDescription(ws.GetStringValue(row, "EventCode"), ShippingCarrierConstants.Usps, ws.GetStringValue(row, "EventDescription")),
                    LastKnownEventLocation = ws.GetStringValue(row, "EventLocation"),
                    LastKnownEventZip = ws.GetStringValue(row, "EventZip"),
                    IsUndeliverable = IsUndeliverable(ws.GetStringValue(row, "EventCode"), ShippingCarrierConstants.Usps)
                });
            }
            Console.WriteLine($"Retrieved {packages.Count} packages from {path}");

            var chunk = 1000;
            for (int offset = 0; offset < packages.Count(); offset += chunk)
            {
                var packagesChunk = packages.Skip(offset).Take(chunk);
                var existingPackages = await packageDatasetRepository.GetProcessedPackagesBySiteAndPackageIdAsync(siteName, packagesChunk.ToList());
                Console.WriteLine($"Retrieved {existingPackages.Count()} existing packages");
                var packagesToUpdate = new List<PackageDataset>();
                foreach (var package in packagesChunk)
                {
                    var existingPackage = existingPackages.FirstOrDefault(p => p.ShippingBarcode == package.ShippingBarcode);
                    if (existingPackage != null)
                    {
                        packagesToUpdate.Add(existingPackage);
                        existingPackage.StopTheClockEventDate = package.StopTheClockEventDate;
                        existingPackage.LastKnownEventDate = package.LastKnownEventDate;
                        existingPackage.LastKnownEventDescription = package.LastKnownEventDescription;
                        existingPackage.LastKnownEventLocation = package.LastKnownEventLocation;
                        existingPackage.LastKnownEventZip = package.LastKnownEventZip;
                        existingPackage.ProcessedEventType = package.ProcessedEventType;
                        existingPackage.ProcessedMachineId = package.ProcessedMachineId;
                        existingPackage.ProcessedUsername = package.ProcessedUsername;

                        if (package.StopTheClockEventDate.HasValue)
                        {
                            existingPackage.IsUndeliverable = package.IsUndeliverable;
                            existingPackage.PostalDays =
                                postalDaysRepository.CalculatePostalDays(existingPackage.StopTheClockEventDate.Value, existingPackage.LocalProcessedDate, existingPackage.ShippingMethod);
                            existingPackage.CalendarDays =
                                postalDaysRepository.CalculateCalendarDays(existingPackage.StopTheClockEventDate.Value, existingPackage.LocalProcessedDate);
                        }
                    }
                }
                Console.WriteLine($"Updating {packagesToUpdate.Count} packages");
                await packageDatasetRepository.ExecuteBulkInsertOrUpdateAsync(packagesToUpdate);
            }
        }
    }
}
