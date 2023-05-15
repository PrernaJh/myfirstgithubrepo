using System;

namespace PackageTracker.CosmosDbExtensions
{
	public class ConnectionStringOptions
	{
		public Uri CosmosDbServiceEndpoint { get; set; }
		public string CosmosDbAuthKey { get; set; }

		public void Deconstruct(out Uri serviceEndpoint, out string authKey)
		{
			serviceEndpoint = CosmosDbServiceEndpoint;
			authKey = CosmosDbAuthKey;
		}
	}
}