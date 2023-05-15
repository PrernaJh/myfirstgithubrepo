using System.Threading;

namespace PackageTracker.Domain.Interfaces
{
    public interface IQueueManager
    {
        bool IsDuplicateQueueMessage(string queueName, string messageText, int lookbackMinutes);
    }
}
