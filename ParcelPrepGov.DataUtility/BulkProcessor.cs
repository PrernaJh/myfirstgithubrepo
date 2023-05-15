using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public class BulkProcessor : IBulkProcessor
	{
		private readonly IBulkRepository bulkRepository;

		public BulkProcessor(IBulkRepository bulkRepository)
		{
			this.bulkRepository = bulkRepository;
		}

		public async Task<BulkImportResponse> BulkImportDocumentsToDbAsync(IEnumerable<string> documents, string containerId)
		{
			return await bulkRepository.BulkImportAsync(documents, containerId);
		}

		public async Task<BulkImportResponse> BulkImportDocumentsToDbAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity
		{
			return await bulkRepository.BulkImportAsync(documents, containerId);
		}

		public async Task<BulkDeleteResponse> BulkDeleteAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity
		{
			return await bulkRepository.BulkDeleteAsync(documents, containerId);
		}
	}
}
