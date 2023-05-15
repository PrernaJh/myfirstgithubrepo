using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
    public abstract class BulkDbRepository : IBulkRepository
    {
        private readonly ILogger logger;
        private readonly IBulkDbClientFactory bulkDbClientFactory;

        protected BulkDbRepository(ILogger logger, IBulkDbClientFactory bulkDbClientFactory)
        {
            this.logger = logger;
            this.bulkDbClientFactory = bulkDbClientFactory;
        }

        public async Task<BulkImportResponse> BulkImportAsync(IEnumerable<string> documents, string containerId)
        {
            try
            {
                var bulkDbClient = bulkDbClientFactory.GetClient(containerId);
                return await bulkDbClient.BulkImportAsync(documents);
            }
            catch (DocumentClientException ex)
            {
                logger.LogError($"Exception during bulk import: {ex}");
                throw ex;
            }
        }

        public async Task<BulkImportResponse> BulkImportAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity
        {
            try
            {
                var bulkDbClient = bulkDbClientFactory.GetClient(containerId);
                return await bulkDbClient.BulkImportAsync(JsonUtility<T>.SerializeList(documents.ToList()));
            }
            catch (DocumentClientException ex)
            {
                logger.LogError($"Exception during bulk import: {ex}");
                throw ex;
            }
        }

        public async Task<BulkDeleteResponse> BulkDeleteAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity
        {
            BulkDeleteResponse response = new BulkDeleteResponse();
            try
            {
                if (documents.Any())
                {
                    var tuples = new List<Tuple<string, string>>();
                    foreach (var document in documents)
                    {
                        tuples.Add(Tuple.Create(document.PartitionKey, document.Id));
                    }
                    var bulkDbClient = bulkDbClientFactory.GetClient(containerId);
                    response = await bulkDbClient.BulkDeleteAsync(tuples);
                    logger.LogInformation($"Deleted {response.NumberOfDocumentsDeleted} documents from {containerId}");
                }
            }
            catch (DocumentClientException ex)
            {
                logger.LogError($"Can't delete {documents.Count()} documents from {containerId}, Exception: {ex}");
                throw ex;
            }
            return response;
        }
    }
}