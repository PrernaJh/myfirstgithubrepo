using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.HealthChecks;
using System;

namespace PackageTracker.CosmosDbExtensions
{
	public static class DbHealthCheck
	{
		public static HealthCheckBuilder AddCosmosDbCheck(this HealthCheckBuilder builder, Uri serviceEndpoint,
			string authKey)
		{
			return AddCosmosDbCheck(builder, serviceEndpoint, authKey, builder.DefaultCacheDuration);
		}

		public static HealthCheckBuilder AddCosmosDbCheck(this HealthCheckBuilder builder, Uri serviceEndpoint,
			string authKey, TimeSpan cacheDuration)
		{
			var checkName = $"CosmosDbCheck({serviceEndpoint})";

			builder.AddCheck(checkName, async () =>
			{
				try
				{
					using (var documentClient = new DocumentClient(serviceEndpoint, authKey))
					{
						await documentClient.OpenAsync();
						return HealthCheckResult.Healthy($"{checkName}: Healthy");
					}
				}
				catch (Exception ex)
				{
					// Failed to connect to CosmosDB.
					return HealthCheckResult.Unhealthy($"{checkName}: Exception during check: ${ex.Message}");
				}
			}, cacheDuration);

			return builder;
		}
	}
}