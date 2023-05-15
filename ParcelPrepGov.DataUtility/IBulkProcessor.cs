using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public interface IBulkProcessor
	{
		Task<BulkImportResponse> BulkImportDocumentsToDbAsync(IEnumerable<string> documents, string containerId);
		Task<BulkImportResponse> BulkImportDocumentsToDbAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity;
		Task<BulkDeleteResponse> BulkDeleteAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity;
	}
}
