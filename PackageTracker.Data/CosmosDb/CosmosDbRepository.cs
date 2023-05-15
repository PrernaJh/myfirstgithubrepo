using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PackageTracker.Data.CosmosDb
{
	public class TimeStamp
	{
		public int _ts { get; set; }
	}

	public abstract class CosmosDbRepository<T> : IRepository<T>, IContainerContext<T> where T : Entity
	{
		public abstract string ContainerName { get; }
		public string GenerateId(T entity) => Guid.NewGuid().ToString();
		public PartitionKey GetPartitionKeyFromString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyFromString(input);
		public abstract string ResolvePartitionKeyString(string unresolvedPartitionKey);

		private readonly ILogger logger;
		private readonly ICosmosDbContainerFactory cosmosDbContainerFactory;
		private readonly Container container;

		private readonly int maxApplicationRetries;
		private readonly int maxApplicationRetrySeconds;
		private readonly int maxBatchSize;

		private readonly Random randomNumberGenerator = new Random();
		private void WaitBeforeRetry()
		{
			if (maxApplicationRetries > 0)
            {
				Thread.Sleep(randomNumberGenerator.Next(1000, Math.Max(1000, maxApplicationRetrySeconds / maxApplicationRetries)));
            }
		}

		public CosmosDbRepository(ILogger logger, IConfiguration configuration, ICosmosDbContainerFactory cosmosDbContainerFactory)
		{
			this.logger = logger;
			this.cosmosDbContainerFactory = cosmosDbContainerFactory;
			container = cosmosDbContainerFactory.GetContainer(ContainerName).Container;
			
			maxApplicationRetries = configuration.GetValue<int>("CosmosDB:MaxApplicationRetries", 0);
			maxApplicationRetrySeconds = configuration.GetValue<int>("CosmosDB:MaxRetrySeconds", 0);
			maxBatchSize = configuration.GetValue<int>("CosmosDB:MaxBatchSize", 100);
		}

		public async Task<T> AddItemAsync(T item, string unresolvedPartitionKey)
		{
			try
			{
				if (StringUtility.HasNoValue(item.Id))
				{
					item.Id = GenerateId(item);
				}
				if (item.CreateDate == null || item.CreateDate.Year == 1)
				{
					item.CreateDate = DateTime.Now;
				}
				item.PartitionKey = ResolvePartitionKeyString(unresolvedPartitionKey);
				var partitionKey = GetPartitionKeyFromString(item.PartitionKey);
				var response = await container.CreateItemAsync(item, partitionKey);
				return response;
			}
			catch (Exception ex)
			{
				logger.LogError($"AddItemAsync Failed[{ContainerName}]: {item.Id}: {item.PartitionKey}: {ex}");
				throw ex;
			}
		}

		public async Task<BatchDbResponse<T>> AddItemsAsync(ICollection<T> items, string unresolvedPartitionKey, int maxRetries)
		{
			var stopWatch = Stopwatch.StartNew();
			var resolvedPartitionKey = ResolvePartitionKeyString(unresolvedPartitionKey);
			items.ToList().ForEach(i => i.PartitionKey = resolvedPartitionKey);
			var response = await AddItemsAsync(items.ToList(), maxRetries);
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<BatchDbResponse<T>> AddItemsAsync(ICollection<T> items, int maxRetries)
		{
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse<T>() { IsSuccessful = true };
			var options = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
 			var chunk = maxBatchSize;
			for (var offset = 0; offset < items.Count(); offset += chunk)
			{
				var itemsChunk = items.Skip(offset).Take(chunk);
				var concurrentTasks = new List<Task>(itemsChunk.Count());
				foreach (var item in itemsChunk)
				{
					if (StringUtility.HasNoValue(item.Id))
						item.Id = GenerateId(item);
					concurrentTasks.Add(container.CreateItemAsync(item, GetPartitionKeyFromString(item.PartitionKey), options)
						.ContinueWith(itemResponse => UpdateResponse(response, item, itemResponse)));
				}
				await Task.WhenAll(concurrentTasks);
			}
			if (response.FailedItems.Count > 0 && --maxRetries >= 0)
			{
				logger.LogWarning($"AddItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
				WaitBeforeRetry();
				var retryResponse = await AddItemsAsync(response.FailedItems, maxRetries);
				response.FailedItems = retryResponse.FailedItems;
				response.FailedCount = retryResponse.FailedCount;
				response.IsSuccessful = retryResponse.IsSuccessful;
				response.Message = retryResponse.Message;
				response.StatusCode = retryResponse.StatusCode;
				retryResponse.RequestCharge += retryResponse.RequestCharge;
			}
			if (!response.IsSuccessful)
				logger.LogError($"AddItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task DeleteItemAsync(string id, string unresolvedPartitionKey)
		{
			await container.DeleteItemAsync<T>(id, GetPartitionKeyFromString(ResolvePartitionKeyString(unresolvedPartitionKey)));
		}

		public async Task<T> GetItemAsync(string id, string unresolvedPartitionKey)
		{
			var partitionKey = ResolvePartitionKeyString(unresolvedPartitionKey);
			try
			{
				var response = await container.ReadItemAsync<T>(id, GetPartitionKeyFromString(partitionKey));
				return response.Resource;
			}
			catch (CosmosException ex)
			{
				if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
					return null;
				logger.LogError($"GetItemAsync Failed[{ContainerName}]: {id}: {partitionKey}: {ex}");
				throw ex;
			}
			catch (Exception ex)
			{
				logger.LogError($"GetItemAsync Failed[{ContainerName}]: {id}: {partitionKey}: {ex}");
				throw ex;
			}
		}

		private static string FormatQueryParameters(IReadOnlyList<(string Name, object Value)> parameters)
        {
			var result = new StringBuilder();
			parameters.ToList().ForEach(p => result.Append($"[{p.Name},{p.Value}],"));
			return result.ToString().Trim(',');
        }

		public async Task<IEnumerable<T>> GetItemsAsync(QueryDefinition queryDefinition, string unresolvedPartitionKey, int maxRetries = -1)
		{
			var results = new List<T>();
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			for (maxRetries++; --maxRetries >= 0;)
			{
				try
				{
					var query = container.GetItemQueryIterator<T>(queryDefinition, null,
						new QueryRequestOptions()
						{
							PartitionKey = GetPartitionKeyFromString(ResolvePartitionKeyString(unresolvedPartitionKey))
						});

					while (query.HasMoreResults)
					{
						var response = await query.ReadNextAsync();
						results.AddRange(response.ToList());
					}
					break;
				}
				catch (Exception ex)
				{
					if (maxRetries > 0)
					{
						logger.LogWarning($"GetItemsAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						WaitBeforeRetry();
						results.Clear();
					}
					else
					{
						logger.LogError($"GetItemsAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						throw ex;
					}
				}
			}
			return results;
		}

		public async Task<IEnumerable<T>> GetItemsCrossPartitionAsync(QueryDefinition queryDefinition, int maxRetries = -1)
		{
			var results = new List<T>();
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			for (maxRetries++; --maxRetries >= 0;)
			{
				try
				{
					var query = container.GetItemQueryIterator<T>(queryDefinition);

					while (query.HasMoreResults)
					{
						var response = await query.ReadNextAsync();
						results.AddRange(response.ToList());
					}
					break;
				}
				catch (Exception ex)
				{
					if (maxRetries > 0)
                    {
						logger.LogWarning($"GetItemsCrossPartitionAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						WaitBeforeRetry();
						results.Clear();
					}
					else
                    {
						logger.LogError($"GetItemsCrossPartitionAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						throw ex;
                    }
				}
			}
			return results;
		}

		public async Task<T> UpdateItemAsync(T item)
		{
			try
			{
				var response = await container.UpsertItemAsync(item, GetPartitionKeyFromString(item.PartitionKey));
				return response;
			}
			catch (Exception ex)
			{
				logger.LogError($"UpdateItemAsync Failed[{ContainerName}]: {item.Id}: {item.PartitionKey}: {ex}");
				throw ex;
			}
		}

		public async Task<BatchDbResponse<T>> UpdateItemsAsync(ICollection<T> items, int maxRetries = -1)
		{
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse<T>() { IsSuccessful = true };
			var options = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			var chunk = maxBatchSize;
			for (var offset = 0; offset < items.Count(); offset += chunk)
			{
				var itemsChunk = items.Skip(offset).Take(chunk);
				var concurrentTasks = new List<Task>(itemsChunk.Count());
				foreach (var item in itemsChunk)
				{
					concurrentTasks.Add(container.UpsertItemAsync(item, GetPartitionKeyFromString(item.PartitionKey), options)
						.ContinueWith(itemResponse => UpdateResponse(response, item, itemResponse)));
				}
				await Task.WhenAll(concurrentTasks);
			}
			if (response.FailedItems.Count > 0 && --maxRetries >= 0)
			{
				logger.LogWarning($"UpdateItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
				WaitBeforeRetry();
				var retryResponse = await UpdateItemsAsync(response.FailedItems, maxRetries);
				response.FailedItems = retryResponse.FailedItems;
				response.FailedCount = retryResponse.FailedCount;
				response.IsSuccessful = retryResponse.IsSuccessful;
				response.Message = retryResponse.Message;
				response.StatusCode = retryResponse.StatusCode;
				retryResponse.RequestCharge += retryResponse.RequestCharge;
			}
			if (!response.IsSuccessful)
				logger.LogError($"UpdateItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}


		public async Task<T> ExecuteStoredProcedure(string sprocName, string partitionKey, string[] inputParams)
		{
			try
			{
				var result = await container.Scripts.ExecuteStoredProcedureAsync<T>(sprocName, 
					GetPartitionKeyFromString(ResolvePartitionKeyString(partitionKey)), inputParams);
				return result.Resource;
			}
			catch (Exception ex)
			{
				logger.LogError($"ExecuteStoredProcedure Failed[{ContainerName}]: {sprocName}: {partitionKey}: [{string.Join(',', inputParams)}]: {ex}");
				throw ex;
			}
		}

        public async Task<IEnumerable<int>> GetTimestampsAsync(QueryDefinition queryDefinition, int maxRetries = -1)
        {
			var results = new List<int>();
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			for (maxRetries++; --maxRetries >= 0;) 
				try
				{
					var query = container.GetItemQueryIterator<TimeStamp>(queryDefinition);
					while (query.HasMoreResults)
					{
						var response = await query.ReadNextAsync();
						results.AddRange(response.Select(r => r._ts).ToList());
					}
					break;
				}
				catch (Exception ex)
				{
					if (maxRetries > 0)
					{
						logger.LogWarning($"GetTimestampsAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						WaitBeforeRetry();
						results.Clear();
					}
                    else
                    {
						logger.LogError($"GetTimestampsAsync Failed[{ContainerName}]: {queryDefinition.QueryText}: {FormatQueryParameters(queryDefinition.GetQueryParameters())}: {ex}");
						throw ex;
                    }
				}
			return results;
		}

        public async Task<BatchDbResponse<T>> DeleteItemsAsync(QueryDefinition queryDefinition, string unresolvedPartitionKey, int maxRetries = -1)
        {
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse<T>() { IsSuccessful = true };
			var query = container.GetItemQueryIterator<T>(queryDefinition, null,
				new QueryRequestOptions()
				{
					PartitionKey = GetPartitionKeyFromString(ResolvePartitionKeyString(unresolvedPartitionKey))
				});
			while (query.HasMoreResults)
			{
				var items = await query.ReadNextAsync();
				response.RequestCharge += items.RequestCharge;
				var batchResponse = await DeleteItemsAsync(items.ToList(), maxRetries);
				response.Count += batchResponse.Count;
				response.RequestCharge += batchResponse.RequestCharge;
				if (! batchResponse.IsSuccessful)
                {
					response.FailedItems.ToList().AddRange(batchResponse.FailedItems);
					response.FailedCount += batchResponse.FailedCount;
					response.IsSuccessful = batchResponse.IsSuccessful;
					response.Message = batchResponse.Message;
					response.StatusCode = batchResponse.StatusCode;
				}
			}
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<BatchDbResponse<T>> DeleteItemsAsync(ICollection<T> items, int maxRetries = -1)
		{
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse<T>() { IsSuccessful = true };
			var options = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			var chunk = maxBatchSize;
			for (var offset = 0; offset < items.Count(); offset += chunk)
			{
				var itemsChunk = items.Skip(offset).Take(chunk);
				var concurrentTasks = new List<Task>(itemsChunk.Count());
				foreach (var item in itemsChunk)
				{
					concurrentTasks.Add(container.DeleteItemAsync<T>(item.Id, GetPartitionKeyFromString(item.PartitionKey), options)
						.ContinueWith(itemResponse => UpdateResponse(response, item, itemResponse)));
				}
				await Task.WhenAll(concurrentTasks);
			}
			if (response.FailedItems.Count > 0 && --maxRetries >= 0)
			{
				logger.LogWarning($"DeleteItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
				WaitBeforeRetry();
				var retryResponse = await DeleteItemsAsync(response.FailedItems, maxRetries);
				response.FailedItems = retryResponse.FailedItems;
				response.FailedCount = retryResponse.FailedCount;
				response.IsSuccessful = retryResponse.IsSuccessful;
				response.Message = retryResponse.Message;
				response.StatusCode = retryResponse.StatusCode;
				retryResponse.RequestCharge += retryResponse.RequestCharge;
			}
			if (!response.IsSuccessful)
				logger.LogError($"DeleteItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<BatchDbResponse<T>> PatchItemsAsync(IDictionary<T, ICollection<PatchOperation>> items, int maxRetries = -1)
		{
			var stopWatch = Stopwatch.StartNew();
			var response = new BatchDbResponse<T>() { IsSuccessful = true };
			var options = new ItemRequestOptions { EnableContentResponseOnWrite = false };
			maxRetries = maxRetries < 0 ? maxApplicationRetries : maxRetries;
			var chunk = maxBatchSize;
			for (var offset = 0; offset < items.Count(); offset += chunk)
			{
				var itemsChunk = items.Skip(offset).Take(chunk);
				var concurrentTasks = new List<Task>(itemsChunk.Count());
				foreach (var item in itemsChunk)
				{
					concurrentTasks.Add(container.PatchItemAsync<T>(item.Key.Id, GetPartitionKeyFromString(item.Key.PartitionKey), item.Value.ToList())
						.ContinueWith(itemResponse => UpdateResponse(response, item.Key, itemResponse)));
				}
				await Task.WhenAll(concurrentTasks);
			}
			if (response.FailedItems.Count > 0 && --maxRetries >= 0)
			{
				logger.LogWarning($"PatchItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
				WaitBeforeRetry();
				var failedItems = new Dictionary<T, ICollection<PatchOperation>>();
				response.FailedItems.ToList().ForEach(i => failedItems[i] = items[i]);
				var retryResponse = await PatchItemsAsync(failedItems, maxRetries);
				response.FailedItems = retryResponse.FailedItems;
				response.FailedCount = retryResponse.FailedCount;
				response.IsSuccessful = retryResponse.IsSuccessful;
				response.Message = retryResponse.Message;
				response.StatusCode = retryResponse.StatusCode;
				retryResponse.RequestCharge += retryResponse.RequestCharge;
			}
			if (!response.IsSuccessful)
				logger.LogError($"PatchItemsAsync Failed[{ContainerName}]: Items: {response.FailedCount} of {response.Count}, Message: {response.Message}, Status Code: {response.StatusCode}");
			response.ElapsedTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<BatchDbResponse<T>> UpdateSetDatasetProcessed(IEnumerable<T> items)
		{
			var updateItems = new Dictionary<T, ICollection<PatchOperation>>();
			foreach (var item in items)
			{
				var updateOperations = new List<PatchOperation>
				{
					PatchOperation.Set("/isDatasetProcessed", item.IsDatasetProcessed)
				};
				updateItems[item] = updateOperations;
			}
			return await PatchItemsAsync(updateItems);
		}

		private static void UpdateResponse(BatchDbResponse<T> response, T item, Task<ItemResponse<T>> itemResponse)
        {
			lock (response)
			{
				response.Count++;
				if (itemResponse.IsCompletedSuccessfully)
				{
					response.RequestCharge += itemResponse.Result != null ? itemResponse.Result.RequestCharge : 0;
				}
				else
				{
					response.IsSuccessful = false;
					response.FailedItems.Add(item);
					response.FailedCount++;
					AggregateException innerExceptions = itemResponse.Exception.Flatten();
					if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
					{
						// Should we do something with cosmosException.RetryAfter?
						response.StatusCode = cosmosException.StatusCode;
						response.Message = cosmosException.Message;
					}
					else
					{
						response.StatusCode = HttpStatusCode.InternalServerError;
						response.Message = innerExceptions.InnerExceptions.FirstOrDefault()?.Message;
					}
				}
			}
		}
    }
}
