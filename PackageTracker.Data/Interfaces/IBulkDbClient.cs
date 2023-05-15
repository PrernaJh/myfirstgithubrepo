using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IBulkDbClient
	{
		Task<BulkImportResponse> BulkImportAsync(IEnumerable<string> documents);
		Task<BulkUpdateResponse> BulkUpdateAsync(IEnumerable<UpdateItem> documents);
		Task<BulkDeleteResponse> BulkDeleteAsync(List<Tuple<string, string>> tuples);
	}
}
