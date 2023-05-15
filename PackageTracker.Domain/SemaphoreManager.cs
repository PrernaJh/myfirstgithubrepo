using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;

namespace PackageTracker.Domain
{
    public class SemaphoreManager : ISemaphoreManager
    {
        private readonly ILogger<SemaphoreManager> logger;

        private static readonly IDictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>();
        public SemaphoreManager(ILogger<SemaphoreManager> logger)
        {
            this.logger = logger;
        }

        public SemaphoreSlim GetSemaphore(string key)
        {
            logger.LogInformation($"Get Semaphore: {key}");
            lock (semaphores)
            {
                if (!semaphores.TryGetValue(key, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    semaphores.Add(key, semaphore);
                }
                return semaphore;
            }
        }
    }
}
