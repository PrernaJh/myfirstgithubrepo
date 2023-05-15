using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.SMS;
using System;

namespace PackageTracker.Communications
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection BootstrapCommunications(this IServiceCollection services, IConfiguration configuration)
		{

			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			services.AddTransient<IEmailService, EmailService>();
			services.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

			services.Configure<SmsConfiguration>(configuration.GetSection(SmsConfiguration.TwilioSmsSection));
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<SmsConfiguration>, SmsConfigurationValidation>());
			services.AddTransient<ISmsService, SmsService>();

			return services;
		}
	}
}
