using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackageTracker.Data;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PackageTracker.CosmosDbExtensions
{
	public static class DbSetup
	{
		public static IServiceCollection AddCosmosDbToApi(this IServiceCollection services, IConfiguration configuration, Uri serviceEndpoint, string authKey, string databaseName)
		{
			var options = BuildClientOptions(configuration);
			var endpointUrl = serviceEndpoint.ToString();
			var client = new CosmosClient(endpointUrl, authKey, options);
			var factory = new CosmosDbContainerFactory(client, databaseName, GetContainerInfo());
			services.AddSingleton<ICosmosDbContainerFactory>(factory);

			factory.EnsureDbSetupAsync().Wait();
			return services;
		}

		private static CosmosClientOptions BuildClientOptions(IConfiguration configuration)
        {
			return new CosmosClientOptions
			{
				ApplicationRegion = configuration.GetValue<string>("ApplicationRegion"),
				AllowBulkExecution = configuration.GetValue<bool>("CosmosDB:AllowBulkExecution", false),
				RequestTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("CosmosDB:RequestTimeoutMinutes", 1)),
				MaxRetryAttemptsOnRateLimitedRequests = configuration.GetValue<int>("CosmosDB:MaxRetries", 9),
				MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(configuration.GetValue<int>("CosmosDB:MaxRetrySeconds", 30)),
				SerializerOptions = new CosmosSerializationOptions
				{
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
					IgnoreNullValues = true,
				}
			};
		}

		public static IServiceCollection AddCosmosDbToService(this IServiceCollection services, ref IConfigurationRoot configuration, string appSettings)
		{
			// CosmosDb client configs
			var cosmosDbConnectionStringOptions = configuration.GetSection("ConnectionStrings").Get<ConnectionStringOptions>();
			var (serviceEndpoint, authKey) = cosmosDbConnectionStringOptions;
			var databaseName = configuration.GetSection("DatabaseName").Value;
			var documentClient = new DocumentClient(serviceEndpoint, authKey,
				new ConnectionPolicy
				{
					ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Direct,
					ConnectionProtocol = Protocol.Tcp,
					MaxConnectionLimit = 1000,
				});
			documentClient.OpenAsync().Wait();

			var options = BuildClientOptions(configuration);
			var endpointUrl = serviceEndpoint.ToString();
			var client = new CosmosClient(endpointUrl, authKey, options);
			var factory = new CosmosDbContainerFactory(client, databaseName, GetContainerInfo());
			services.AddSingleton<ICosmosDbContainerFactory>(factory);

			factory.EnsureDbSetupAsync().Wait();
			configuration = OverrideAppSettingsFromDatabase(ApplicationTypeConstants.Service, services, configuration, appSettings, databaseName, documentClient);
			return services;
		}

		public static IServiceCollection AddCosmosDbToDataUtility(this IServiceCollection services, ref IConfigurationRoot configuration, string appSettings)
		{
			// CosmosDb client configs
			var cosmosDbConnectionStringOptions = configuration.GetSection("ConnectionStrings").Get<ConnectionStringOptions>();
			var (serviceEndpoint, authKey) = cosmosDbConnectionStringOptions;
			var databaseName = configuration.GetSection("DatabaseName").Value;
			var documentClient = new DocumentClient(serviceEndpoint, authKey,
				new ConnectionPolicy
				{
					ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Direct,
					ConnectionProtocol = Protocol.Tcp,
					MaxConnectionLimit = 1000,
				});
			documentClient.OpenAsync().Wait();

			var options = BuildClientOptions(configuration);
			var endpointUrl = serviceEndpoint.ToString();
			var client = new CosmosClient(endpointUrl, authKey, options);
			var factory = new CosmosDbContainerFactory(client, databaseName, GetContainerInfo());
			var bulkDbClientFactory = new BulkDbClientFactory(databaseName, GetCollectionNames(), documentClient);
			services.AddSingleton<ICosmosDbContainerFactory>(factory);
			services.AddSingleton<IBulkDbClientFactory>(bulkDbClientFactory);

			factory.EnsureDbSetupAsync().Wait();
			configuration = OverrideAppSettingsFromDatabase(ApplicationTypeConstants.Service, services, configuration, appSettings, databaseName, documentClient);
			return services;
		}

		private static IConfigurationRoot OverrideAppSettingsFromDatabase(string applicationType, IServiceCollection services, IConfigurationRoot configuration, string appSettings, string databaseName, DocumentClient documentClient)
		{
			// Override appsettings from "settings" collection.
			var settings = documentClient.MergeDocuments(databaseName, CollectionNameConstants.Settings, applicationType);
			if (Domain.Utilities.StringHelper.Exists(settings))
			{
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(settings)))
				{
					var configBuilder = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonStream(stream)
					   .AddJsonFile(appSettings, optional: false, reloadOnChange: true)
					   .AddEnvironmentVariables();

					configuration = configBuilder.Build();
				}
				services.AddSingleton<IConfiguration>(configuration);
			}
			return configuration;
		}

		public static IServiceCollection AddCosmosDbToWeb(this IServiceCollection services, IConfiguration configuration, Uri serviceEndpoint, string authKey, string databaseName)
		{
			var options = BuildClientOptions(configuration);
			var endpointUrl = serviceEndpoint.ToString();
			var client = new CosmosClient(endpointUrl, authKey, options);
			var factory = new CosmosDbContainerFactory(client, databaseName, GetContainerInfo());
			services.AddSingleton<ICosmosDbContainerFactory>(factory);

			factory.EnsureDbSetupAsync().Wait();
			return services;
		}

		private static List<string> GetCollectionNames()
		{
			return CollectionNamesList.CollectionNames;
		}

		private static List<ContainerInfo> GetContainerInfo()
		{
			var containerInfo = new List<ContainerInfo>();
			CollectionNamesList.CollectionNames.ForEach(n => containerInfo.Add(new ContainerInfo() { Name = n, PartitionKey = "/partitionKey" }));
			return containerInfo;
		}
	}
}
