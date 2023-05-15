using Microsoft.Azure.Documents;
using PackageTracker.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace PackageTracker.Data
{
	public class BulkDbClientFactory : IBulkDbClientFactory
	{
		private readonly string databaseName;
		private readonly List<string> collectionNames;
		private readonly IDocumentClient documentClient;

		public BulkDbClientFactory(string databaseName, List<string> collectionNames, IDocumentClient documentClient)
		{
			this.databaseName = databaseName;
			this.collectionNames = collectionNames;
			this.documentClient = documentClient;
		}

		public IBulkDbClient GetClient(string collectionName)
		{
			if (!collectionNames.Contains(collectionName))
			{
				throw new ArgumentException($"Unable to find collection: {collectionName}");
			}

			return new BulkDbClient(databaseName, collectionName, documentClient);
		}
	}
}
