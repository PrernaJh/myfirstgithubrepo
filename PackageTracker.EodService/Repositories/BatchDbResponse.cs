using System;

namespace PackageTracker.EodService.Repositories
{
    public class BatchDbResponse
    {
        public int Count { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public TimeSpan ElapsedTime { get; set; }
    }
}
