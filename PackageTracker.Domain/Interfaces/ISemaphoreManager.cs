using System.Threading;

namespace PackageTracker.Domain.Interfaces
{
    public interface ISemaphoreManager
    {
        SemaphoreSlim GetSemaphore(string key);
    }
}
