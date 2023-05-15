using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using System;

namespace PackageTracker.CosmosDbExtensions
{
	public static class DocumentMergerExtension
	{
		public static string MergeDocuments(this DocumentClient client, string databaseId, string containerId, string applicationType)
		{
			var settings = new JObject();
			try
			{
				var query = $@"SELECT * FROM s 
								WHERE CONTAINS(s.ApplicationType, @applicationType)";

				var parameters = new SqlParameterCollection
				{
					new SqlParameter("@applicationType", applicationType)
				};

				var documents = client.CreateDocumentQuery(
					UriFactory.CreateDocumentCollectionUri(databaseId, containerId),
					new SqlQuerySpec(query, parameters),
					new FeedOptions
					{
						MaxItemCount = -1,
						EnableCrossPartitionQuery = true
					});

				foreach (var document in documents)
				{
					var json = document.ToString();
					settings.Merge(JObject.Parse(json));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return settings.ToString();
		}
	}
}