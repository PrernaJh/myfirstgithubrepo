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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PackageTracker.Data.Constants.ShippingCarrierConstants;
using static PackageTracker.Data.Constants.ShippingMethodConstants;

namespace MMS.API.Domain.Processors
{
    public class PackageServiceProcessor : IPackageServiceProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly ILogger<PackageServiceProcessor> logger;
        private readonly IServiceRuleRepository serviceRuleRepository;
        private readonly IServiceRuleExtensionScanProcessor serviceRuleExtensionScanProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly IZipOverrideRepository zipOverrideRepository;

        public PackageServiceProcessor(IActiveGroupProcessor activeGroupProcessor,
                ILogger<PackageServiceProcessor> logger,
                IServiceRuleRepository serviceRuleRepository,
                IServiceRuleExtensionScanProcessor serviceRuleExtensionScanProcessor,
                ISiteProcessor siteProcessor,
                IZipOverrideRepository zipOverrideRepository)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.logger = logger;
            this.serviceRuleRepository = serviceRuleRepository;
            this.serviceRuleExtensionScanProcessor = serviceRuleExtensionScanProcessor;
            this.siteProcessor = siteProcessor;
            this.zipOverrideRepository = zipOverrideRepository;
        }

        public async Task<bool> GetServiceDataAsync(Package package)
        {
            try
            {
                var isServiced = false;
                isServiced = await AssignServiceRules(package);

                return isServiced;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error servicing Package ID: { package.Id } Exception: {ex}");
                return false;
            }
        }

        private async Task<bool> AssignServiceRules(Package package)
        {
            var isServiced = false;
            var localDateTime = TimeZoneUtility.GetLocalTime(package.TimeZone);
            var overrideActiveGroups = await activeGroupProcessor.GetServiceOverrideActiveGroupsAsync(package.SubClientName, package.TimeZone);

            if (localDateTime.DayOfWeek == DayOfWeek.Friday)
            {
                package.IsSaturday = true;
            }

            var serviceRule = await serviceRuleRepository.GetServiceRuleAsync(package);

            if (StringHelper.Exists(serviceRule.Id))
            {
                var serviceOutput = new ServiceOutput();
                //If the package has a service rule shipping method for outside 48 states,
                //then use the service output to update the service rule before applying any overrides
                if (serviceRule.ShippingMethod == ShippingMethodConstants.Outside48States)
                {
                    package.OverrideServiceRuleId = serviceRule.Id;
                    serviceOutput = await serviceRuleExtensionScanProcessor.UseFortyEightStatesServiceRuleExtension(package, serviceRule);

                    serviceRule.Id = serviceOutput.ServiceRuleId;
                    serviceRule.IsQCRequired = serviceOutput.IsQCRequired;
                    serviceRule.ServiceLevel = serviceOutput.ServiceLevel;
                    serviceRule.ShippingCarrier = serviceOutput.ShippingCarrier;
                    serviceRule.ShippingMethod = serviceOutput.ShippingMethod;
                }

                var evaluateServiceOverrides = EvaluateServiceOverrides(serviceRule, overrideActiveGroups);

                if (evaluateServiceOverrides.ShouldOverride)
                {
                    package.OverrideServiceRuleId = serviceRule.Id;
                    package.OverrideServiceRuleGroupId = evaluateServiceOverrides.ActiveGroup.Id;
                    serviceOutput = UseServiceRuleOverride(evaluateServiceOverrides.ActiveGroup, serviceRule);
                }
                else if (package.ZipOverrides.Contains(ActiveGroupTypeConstants.ZipCarrierOverride))
                {
                    package.OverrideServiceRuleId = serviceRule.Id;
                    serviceOutput = await UseZipCarrierOverride(package, serviceRule);
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

                if (serviceOutput.ShippingCarrier == Ups
                    && serviceOutput.ShippingMethod == UpsNextDayAir
                    && package.IsSaturday
                    && !package.ZipOverrides.Contains(ActiveGroupTypeConstants.ZipsUpsSat48))
                {
                    serviceOutput.ShippingCarrier = package.ClientName;
                    serviceOutput.ShippingMethod = ReturnToCustomer;
                }

                AssignServiceOutputToPackage(package, serviceOutput);
                await CheckForBinOverride(package);
                isServiced = ValidateServicing(package);
            }

            return isServiced;
        }

        private async Task CheckForBinOverride(Package package)
        {
            var isUspsAndFirstClassOrPriority = package.ShippingCarrier == Usps && (package.ShippingMethod == UspsFirstClass || package.ShippingMethod == UspsPriority);
            var isUps = package.ShippingCarrier == Ups;
            var isFedEx = package.ShippingCarrier == FedEx;

            if (isUspsAndFirstClassOrPriority || isUps || isFedEx)
            {
                var site = await siteProcessor.GetSiteBySiteNameAsync(package.SiteName);
                var siteBinOverride = site.BinOverrides.FirstOrDefault(x => x.ShippingCarrier == package.ShippingCarrier && x.ShippingMethod == package.ShippingMethod);

                if (siteBinOverride != null)
                {
                    package.HistoricalBinCodes.Add(package.BinCode);
                    package.BinCode = siteBinOverride.BinCode;
                }
                else
                {
                    siteBinOverride = site.BinOverrides.FirstOrDefault(x => x.ShippingCarrier == package.ShippingCarrier);
                    if (siteBinOverride != null)
                    {
                        package.HistoricalBinCodes.Add(package.BinCode);
                        package.BinCode = siteBinOverride.BinCode;
                    }
                }
                logger.LogInformation($"Site bin override for packageId: {package.PackageId} Site {package.SiteName}");
            }
        }

        private async Task<ServiceOutput> UseZipCarrierOverride(Package package, ServiceRule serviceRule)
        {
            var serviceOutput = new ServiceOutput();
            var zipCarrierOverrideActiveGroupId = await activeGroupProcessor.GetZipCarrierOverrideActiveGroupIdAsync(package.SubClientName, package.TimeZone);
            var zipCarrierOverride = await zipOverrideRepository.GetZipCarrierOverrideAsync(package, serviceRule, zipCarrierOverrideActiveGroupId);
            var shouldOverride = false;

            if (StringHelper.Exists(zipCarrierOverride.Id))
            {
                var noPoBoxCarriers = new List<string>
                {
                    Ups,
                    FedEx
                };
                var noOrmdShippingMethods = new List<string>
                {
                    UpsNextDayAir,
                    UpsNextDayAirSaver,
                    UpsSecondDayAir,
                    UspsPriorityExpress,
                    FedExPriorityOvernight
                };

                var poBoxBadCarrier = package.IsPoBox == true && noPoBoxCarriers.Contains(zipCarrierOverride.ToShippingCarrier);
                var ormdBadShippingMethod = package.IsOrmd == true && noOrmdShippingMethods.Contains(zipCarrierOverride.ToShippingMethod);

                if (!poBoxBadCarrier && !ormdBadShippingMethod)
                {
                    shouldOverride = true;
                }

            }
            if (shouldOverride)
            {
                serviceOutput = new ServiceOutput
                {
                    ServiceRuleId = serviceRule.Id,
                    IsQCRequired = serviceRule.IsQCRequired,
                    ServiceLevel = serviceRule.ServiceLevel,
                    ShippingCarrier = zipCarrierOverride.ToShippingCarrier,
                    ShippingMethod = zipCarrierOverride.ToShippingMethod,
                    OverrideGroupId = zipCarrierOverrideActiveGroupId
                };
            }
            else
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

            return serviceOutput;
        }

        private static ServiceOutput UseServiceRuleOverride(ActiveGroup overrideActiveGroup, ServiceRule serviceRule)
        {
            return new ServiceOutput
            {
                ServiceRuleId = serviceRule.Id,
                IsQCRequired = serviceRule.IsQCRequired,
                ServiceLevel = serviceRule.ServiceLevel,
                ShippingCarrier = overrideActiveGroup.ServiceOverride.NewShippingCarrier,
                ShippingMethod = overrideActiveGroup.ServiceOverride.NewShippingMethod,
                OverrideGroupId = overrideActiveGroup.Id
            };
        }

        private static bool ValidateServicing(Package package)
        {
            return StringHelper.Exists(package.ServiceRuleId) && StringHelper.Exists(package.ShippingMethod) && StringHelper.Exists(package.ShippingCarrier);
        }

        private (bool ShouldOverride, ActiveGroup ActiveGroup) EvaluateServiceOverrides(ServiceRule serviceRule, List<ActiveGroup> overrideActiveGroups)
        {
            var response = false;
            var activeGroup = new ActiveGroup();

            if (overrideActiveGroups.Any())
            {
                var activeGroups = new List<ActiveGroup>();

                foreach (var overrideActiveGroup in overrideActiveGroups)
                {
                    if (serviceRule.ShippingCarrier == overrideActiveGroup.ServiceOverride.OldShippingCarrier && serviceRule.ShippingMethod == overrideActiveGroup.ServiceOverride.OldShippingMethod)
                    {
                        activeGroups.Add(overrideActiveGroup);
                    }
                }

                if (activeGroups.Any())
                {
                    activeGroup = activeGroups.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                    response = true;
                }
            }

            return (response, activeGroup);
        }

        private void AssignServiceOutputToPackage(Package package, ServiceOutput serviceOutput)
        {
            package.ServiceRuleId = serviceOutput.ServiceRuleId;
            package.IsQCRequired = serviceOutput.IsQCRequired;
            package.ServiceLevel = serviceOutput.ServiceLevel;
            package.ShippingCarrier = serviceOutput.ShippingCarrier;
            package.ShippingMethod = serviceOutput.ShippingMethod;
            package.ServiceRuleExtensionId = StringHelper.Exists(serviceOutput.ServiceRuleExtensionId) ? serviceOutput.ServiceRuleExtensionId : string.Empty;
            package.OverrideServiceRuleGroupId = StringHelper.Exists(serviceOutput.OverrideGroupId) ? serviceOutput.OverrideGroupId : string.Empty;
        }
    }
}
