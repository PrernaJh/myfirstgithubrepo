using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace PackageTracker.AzureExtensions
{
	public static class ServiceBusSetup
	{
		public static IServiceCollection AddServiceBusTopic(this IServiceCollection services, string serviceBusEndpoint, string serviceBusTopicName)
		{
			var serviceBusTopicClient = new TopicClient(serviceBusEndpoint, serviceBusTopicName);

			services.AddSingleton<ITopicClient>(serviceBusTopicClient);
			services.AddScoped<IServiceBusHelper, ServiceBusHelper>();

			return services;
		}
	}
}
