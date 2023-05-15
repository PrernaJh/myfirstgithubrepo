using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using PackageTracker.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class BulkDbClient : IBulkDbClient
	{
		private readonly string databaseName;
		private readonly string collectionName;
		private readonly IDocumentClient documentClient;

		public BulkDbClient(string databaseName, string collectionName, IDocumentClient documentClient)
		{
			this.databaseName = databaseName;
			this.collectionName = collectionName;
			this.documentClient = documentClient;
		}

		public async Task<BulkImportResponse> BulkImportAsync(IEnumerable<string> documents)
		{
			//var importResponse = new ConcurrentBag<BulkImportResponse>();
			//var batchSize = BatchUtility.GetBatchSizeByTens(documents.Count());
			//var batchedDocuments = BatchUtility.BatchList(documents, batchSize);

			var collection = documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
				.Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();

			var bulkExecutor = new BulkExecutor(documentClient as DocumentClient, collection);

			await bulkExecutor.InitializeAsync();

			var importResponse = await bulkExecutor.BulkImportAsync(documents, false, false, null, null);

			//var tasks = batchedDocuments.Select(async batch =>
			//{
			//    var response = await bulkExecutor.BulkImportAsync(batch, false, false, null, null);
			//    importResponse.Add(response);
			//});

			//await Task.WhenAll(tasks);

			return importResponse;
		}

		public async Task<BulkUpdateResponse> BulkUpdateAsync(IEnumerable<UpdateItem> documents)
		{
			//var importResponse = new ConcurrentBag<BulkImportResponse>();
			//var batchSize = BatchUtility.GetBatchSizeByTens(documents.Count());
			//var batchedDocuments = BatchUtility.BatchList(documents, batchSize);

			var collection = documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
				.Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();

			var bulkExecutor = new BulkExecutor(documentClient as DocumentClient, collection);

			await bulkExecutor.InitializeAsync();

			try
			{
				var updateResponse = await bulkExecutor.BulkUpdateAsync(documents);
				return updateResponse;
			}
			catch (Exception ex)
			{

				throw ex;
			}

			//var tasks = batchedDocuments.Select(async batch =>
			//{
			//    var response = await bulkExecutor.BulkImportAsync(batch, false, false, null, null);
			//    importResponse.Add(response);
			//});

			//await Task.WhenAll(tasks);

            
        }

		public async Task<BulkDeleteResponse> BulkDeleteAsync(List<Tuple<string, string>> tuples)
		{
			var collection = documentClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
				.Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();

			var bulkExecutor = new BulkExecutor(documentClient as DocumentClient, collection);

			await bulkExecutor.InitializeAsync();

            var deleteResponse = await bulkExecutor.BulkDeleteAsync(tuples);
            return deleteResponse;
        }    
	}
}
