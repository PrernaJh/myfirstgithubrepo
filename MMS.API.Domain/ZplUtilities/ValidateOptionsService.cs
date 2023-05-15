using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace MMS.API.Domain.ZplUtilities
{
	public class ValidateOptionsService : IHostedService
	{
		private readonly ILogger<ValidateOptionsService> logger;
		private readonly IHostApplicationLifetime appLifetime;
		private readonly IOptions<ZPLConfiguration> zplConfiguration;

		public ValidateOptionsService(
			ILogger<ValidateOptionsService> logger,
			IHostApplicationLifetime appLifetime,
			IOptions<ZPLConfiguration> zplConfiguration)
		{
			this.logger = logger;
			this.appLifetime = appLifetime;
			this.zplConfiguration = zplConfiguration;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				_ = zplConfiguration.Value; // accessing this triggers validation
			}
			catch (OptionsValidationException ex)
			{
				logger.Log(LogLevel.Error, "One or more options validation checks failed.");

				foreach (var failure in ex.Failures)
				{
					logger.Log(LogLevel.Error, failure);
				}

				appLifetime.StopApplication(); // stop the app now
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask; // nothing to do
		}
	}
}