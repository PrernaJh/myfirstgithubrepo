using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
    public class CreatePackageServiceProcessor : ICreatePackageServiceProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<CreatePackageServiceProcessor> logger;
        private readonly IServiceRuleRepository serviceRuleRepository;
        private readonly IServiceRuleExtensionRepository serviceRuleExtensionRepository;

        public CreatePackageServiceProcessor(IActiveGroupProcessor activeGroupProcessor,
                ILogger<CreatePackageServiceProcessor> logger,
                IServiceRuleRepository serviceRuleRepository,
                IServiceRuleExtensionRepository serviceRuleExtensionRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.serviceRuleRepository = serviceRuleRepository;
            this.serviceRuleExtensionRepository = serviceRuleExtensionRepository;
        }

        public async Task<bool> GetCreatePackageServiceDataAsync(Package package)
        {
            try
            {
                var isServiced = false;
                if (package.BusinessRuleType == CreatePackageConstants.ClientRuleTypeConstant)
                {
                    // customer provided carrier and shipping method
                    isServiced = true;
                }
                else if (package.BusinessRuleType == CreatePackageConstants.SystemRuleTypeConstant)
                {
                    isServiced = await AssignCreatePackageServiceRules(package);
                }

                return isServiced;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error servicing CreatePackage package ID: { package.Id } Exception: {ex}");
                return false;
            }
        }

        private async Task<bool> AssignCreatePackageServiceRules(Package package)
        {
            var isServiced = false;
            var localDateTime = TimeZoneUtility.GetLocalTime(package.TimeZone);

            if (localDateTime.DayOfWeek == DayOfWeek.Friday)
            {
                package.IsSaturday = true;
            }
            var serviceRuleActiveGroupId = await activeGroupProcessor.GetServiceRuleActiveGroupIdByDateAsync(package.SubClientName, package.ShipDate, package.TimeZone);

            if (StringHelper.Exists(serviceRuleActiveGroupId))
            {
                package.ServiceRuleGroupId = serviceRuleActiveGroupId;
                var serviceRule = await serviceRuleRepository.GetServiceRuleAsync(package);

                if (StringHelper.Exists(serviceRule.Id))
                {
                    var serviceOutput = new ServiceOutput();
                    package.ServiceRuleId = serviceRule.Id;

                    if (serviceRule.ShippingMethod == ShippingMethodConstants.Outside48States)
                    {
                        package.OverrideServiceRuleId = serviceRule.Id;
                        //serviceOutput = await UseFortyEightStatesServiceRuleExtension(package, serviceRule);
                    }
                    else // standard servicing
                    {
                        serviceOutput = new ServiceOutput
                        {
                            ServiceRuleId = serviceRule.Id,
                            IsQCRequired = serviceRule.IsQCRequired,
                            ServiceLevel = serviceRule.ServiceLevel,
                            ShippingCarrier = serviceRule.ShippingCarrier,
                            ShippingMethod = serviceRule.ShippingMethod
                        };
                    }

                    package.ServiceRuleId = serviceOutput.ServiceRuleId;
                    package.IsQCRequired = serviceOutput.IsQCRequired;
                    package.ServiceLevel = serviceOutput.ServiceLevel;
                    package.ShippingCarrier = serviceOutput.ShippingCarrier;
                    package.ShippingMethod = serviceOutput.ShippingMethod;
                    package.ServiceRuleExtensionId = StringHelper.Exists(serviceOutput.ServiceRuleExtensionId) ? serviceOutput.ServiceRuleExtensionId : string.Empty;
                    package.OverrideServiceRuleGroupId = StringHelper.Exists(serviceOutput.OverrideGroupId) ? serviceOutput.OverrideGroupId : string.Empty;
                    //isServiced = ValidateServicing(package);
                }
            }

            return isServiced;
        }
    }
}
