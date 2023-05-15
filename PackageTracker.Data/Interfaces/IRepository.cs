using Microsoft.Azure.Cosmos;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        string ResolvePartitionKeyString(string unresolvedPartitionKey);

        Task<IEnumerable<T>> GetItemsAsync(QueryDefinition queryDefinition, string unresolvedPartitionKey, int maxRetries = -1);
        Task<IEnumerable<T>> GetItemsCrossPartitionAsync(QueryDefinition queryDefinition, int maxRetries = -1);
        Task<T> GetItemAsync(string id, string unresolvedPartitionKey);
        Task<T> AddItemAsync(T item, string unresolvedPartitionKey);
        Task<BatchDbResponse<T>> AddItemsAsync(ICollection<T> items, string unresolvedPartitionKey, int maxRetries = -1);
        Task<BatchDbResponse<T>> AddItemsAsync(ICollection<T> items, int maxRetries = -1);
        Task<T> UpdateItemAsync(T item);
        Task<BatchDbResponse<T>> UpdateItemsAsync(ICollection<T> items, int maxRetries = -1);
        Task DeleteItemAsync(string id, string unresolvedPartitionKey);
        Task<T> ExecuteStoredProcedure(string sprocName, string unresolvedPartitionKey, string[] inputParams);
        Task<IEnumerable<int>> GetTimestampsAsync(QueryDefinition queryDefinition, int maxRetries = -1);
        Task<BatchDbResponse<T>> DeleteItemsAsync(QueryDefinition queryDefinition, string unresolvedPartitionKey, int maxRetries = -1);
        Task<BatchDbResponse<T>> DeleteItemsAsync(ICollection<T> items, int maxRetries = -1);
        Task<BatchDbResponse<T>> PatchItemsAsync(IDictionary<T, ICollection<PatchOperation>> items, int maxRetries = -1);

        Task<BatchDbResponse<T>> UpdateSetDatasetProcessed(IEnumerable<T> items);
    }
}

