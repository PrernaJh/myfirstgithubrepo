using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Processors;
using System.Linq;

namespace MMS.API.Domain.ZplUtilities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection BootstrapZPLProcessor(this IServiceCollection services, IConfiguration configuration)
        {            
            services.Configure<ZPLConfiguration>(configuration.GetSection(ZPLConfiguration.ZPLSection));            
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ZPLConfiguration>, ZPLConfigurationValidation>());
            services.AddHostedService<ValidateOptionsService>();
            services.TryAddSingleton<IAutoScanZplProcessor, AutoScanZplProcessor>();

            return services;
        }
    }
}