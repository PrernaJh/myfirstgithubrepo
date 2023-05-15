using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PackageTracker.Domain
{
    public class QueueMessage
    {
        public string MessageText { get; set; }
        public DateTime TimeStamp { get; set; }
    };

    public class QueueManager : IQueueManager
    {
        private readonly ILogger<QueueManager> logger;

        private static readonly IDictionary<string, QueueMessage> messages = new Dictionary<string, QueueMessage>();
        public QueueManager(ILogger<QueueManager> logger)
        {
            this.logger = logger;
        }

        public bool IsDuplicateQueueMessage(string queueName, string messageText, int lookbackMinutes)
        {
            lock (messages)
            {
                var duplicate = false;
                duplicate = messages.TryGetValue(queueName, out var queueMessage)
                    && queueMessage.MessageText == messageText
                    && queueMessage.TimeStamp > DateTime.Now.AddMinutes(-lookbackMinutes); // Too soon?
                messages[queueName] = new QueueMessage { MessageText = messageText, TimeStamp = DateTime.Now };
                if (duplicate)
                    logger.LogError($"Received duplicate queue message: {queueName}: {messageText}");
               return duplicate;
            }
        }
    }
}
