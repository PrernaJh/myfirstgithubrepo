using System.Threading.Tasks;

namespace PackageTracker.AzureExtensions
{
    public interface IServiceBusHelper
    {
        Task<string> SendTopicMessageAsync<ServiceBusPayload>(ServiceBusPayload payload);
    }
}
