using Microsoft.Azure.Cosmos;
using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace PackageTracker.Data.CosmosDb
{
    public class BatchDbResponse<T> where T : Entity
    {
        public int Count { get; set; }
        public int FailedCount { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public double RequestCharge { get; set; }
        public TimeSpan ElapsedTime { get; set;  }
        public IList<T> FailedItems  { get; set;  } = new List<T>();
        public BatchDbResponse()
        {
        }

        public BatchDbResponse(TransactionalBatchResponse input, TimeSpan elapsedTime)
        {
            Count = input.Count;
            FailedCount = input.IsSuccessStatusCode ? 0 : input.Count;
            Message = input.ErrorMessage;
            StatusCode = input.StatusCode;
            IsSuccessful = input.IsSuccessStatusCode;
            RequestCharge = input.RequestCharge;
            ElapsedTime = elapsedTime;
        }    
    }
}
