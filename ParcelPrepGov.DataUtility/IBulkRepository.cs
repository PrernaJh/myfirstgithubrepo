using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public interface IBulkRepository
	{
		Task<BulkImportResponse> BulkImportAsync(IEnumerable<string> documents, string containerId);
		Task<BulkImportResponse> BulkImportAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity;
		Task<BulkDeleteResponse> BulkDeleteAsync<T>(IEnumerable<T> documents, string containerId) where T : Entity;
	}
}
