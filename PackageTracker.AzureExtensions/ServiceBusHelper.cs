using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.AzureExtensions
{
	public class ServiceBusHelper : IServiceBusHelper
	{
		private readonly ITopicClient topicClient;

		public ServiceBusHelper(ITopicClient topicClient)
		{
			this.topicClient = topicClient;
		}

		public async Task<string> SendTopicMessageAsync<T>(T payload)
		{
			var message = JsonSerializer.Serialize(payload);
			var bytes = Encoding.UTF8.GetBytes(message);

			await topicClient.SendAsync(new Message()
			{
				Body = bytes,
				ContentType = "text/plain"
			});

			return message;
		}
	}
}
