using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Threading.Tasks;
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;


namespace MMS.API.Domain.Processors
{
    public class ServiceRuleExtensionScanProcessor : IServiceRuleExtensionScanProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<ServiceRuleExtensionScanProcessor> logger;
        private readonly IServiceRuleExtensionRepository serviceRuleExtensionRepository;

        public ServiceRuleExtensionScanProcessor(IActiveGroupProcessor activeGroupProcessor,
                ILogger<ServiceRuleExtensionScanProcessor> logger,
                IServiceRuleExtensionRepository serviceRuleExtensionRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.serviceRuleExtensionRepository = serviceRuleExtensionRepository;
        }

        public async Task<ServiceOutput> UseFortyEightStatesServiceRuleExtension(Package package, ServiceRule serviceRule)
        {
            var serviceOutput = new ServiceOutput();
            var fortyEightStatesExtension = new ServiceRuleExtension();
            var fortyEightStatesGroupId = await activeGroupProcessor.GetFortyEightStatesActiveGroupIdAsync(package.SubClientName);

            if (StringHelper.Exists(fortyEightStatesGroupId))
            {
                package.ServiceRuleExtensionGroupId = fortyEightStatesGroupId;
                fortyEightStatesExtension = await serviceRuleExtensionRepository.GetFortyEightStatesRuleAsync(package);

                if (StringHelper.DoesNotExist(fortyEightStatesExtension.Id))
                {
                    fortyEightStatesExtension = await serviceRuleExtensionRepository.GetDefaultFortyEightStatesRuleAsync(package);

                    if (StringHelper.DoesNotExist(fortyEightStatesExtension.Id))
                    {
                        fortyEightStatesExtension.ShippingCarrier = package.ClientName;
                        fortyEightStatesExtension.ShippingMethod = ReturnToCustomer;
                    }
                }
                package.ServiceRuleExtensionId = fortyEightStatesExtension.Id ?? string.Empty;
            }
            else
            {
                logger.LogError($"No forty eight states group id found for package.Id: {package.Id}");
            }

            serviceOutput = new ServiceOutput
            {
                ServiceRuleId = serviceRule.Id,
                IsQCRequired = serviceRule.IsQCRequired,
                ServiceLevel = fortyEightStatesExtension.ServiceLevel,
                ShippingCarrier = fortyEightStatesExtension.ShippingCarrier,
                ShippingMethod = fortyEightStatesExtension.ShippingMethod,
                ServiceRuleExtensionId = fortyEightStatesExtension.Id
            };

            return serviceOutput;
        }
    }
}
