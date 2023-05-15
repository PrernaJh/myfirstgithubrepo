using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class RateProcessor : IRateProcessor
	{
		private readonly ILogger<RateProcessor> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IContainerRepository containerRepository;
		private readonly IPackageRepository packageRepository;
		private readonly IRateRepository rateRepository;
		private readonly IServiceRuleProcessor serviceRuleProcessor;
		private readonly ISubClientProcessor subClientProcessor;

		public RateProcessor(ILogger<RateProcessor> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IContainerRepository containerRepository,
			IPackageRepository packageRepository,
			IRateRepository rateRepository,
			IServiceRuleProcessor serviceRuleProcessor,
			ISubClientProcessor subClientProcessor)
		{
			this.logger = logger;
			this.activeGroupProcessor = activeGroupProcessor;
			this.containerRepository = containerRepository;
			this.packageRepository = packageRepository;
			this.rateRepository = rateRepository;
			this.serviceRuleProcessor = serviceRuleProcessor;
			this.subClientProcessor = subClientProcessor;
		}

		public async Task<List<Rate>> GetCurrentRatesAsync(string subClientName)
		{
			var activeGroupId = await activeGroupProcessor.GetRatesActiveGroupIdAsync(subClientName);
			var response = new List<Rate>();
			var rates = await rateRepository.GetRatesByActiveGroupId(activeGroupId);
			// This can be removed later after this software has been released and new rates files have been uploaded ...
			foreach (var rate in rates.Where(r => !r.IsOutside48States && (
						r.CostZoneDduOut48 > 0 ||
						r.CostZoneScfOut48 > 0 ||
						r.CostZoneNdcOut48 > 0 ||
						r.ChargeZoneDduOut48 > 0 ||
						r.ChargeZoneScfOut48 > 0 ||
						r.ChargeZoneNdcOut48 > 0
					)))
			{
				var outside48Rate = new Rate(rate);
				outside48Rate.IsOutside48States = true;
				outside48Rate.CostZoneDdu = rate.CostZoneDduOut48;
				outside48Rate.CostZoneScf = rate.CostZoneScfOut48;
				outside48Rate.CostZoneNdc = rate.CostZoneNdcOut48;
				outside48Rate.ChargeZoneDdu = rate.ChargeZoneDduOut48;
				outside48Rate.ChargeZoneScf = rate.ChargeZoneScfOut48;
				outside48Rate.ChargeZoneNdc = rate.ChargeZoneNdcOut48;
				response.Add(outside48Rate);
			}
			// ...
			response.AddRange(rates);
			return response;
		}		

		public async Task<List<Package>> AssignPackageRatesForEod(SubClient subClient, List<Package> packages, string webJobId)
		{
			try
			{
				logger.LogInformation($"assign package rates for Eod {subClient.Name} total count: {packages.Count()}");

				if (packages.Any(x => x.PackageStatus == EventConstants.Processed))
				{
					var rates = await GetCurrentRatesAsync(subClient.Name);
					var ratesActiveGroupId = rates.Any() ? rates.FirstOrDefault().ActiveGroupId : string.Empty;
					logger.LogInformation($"Rates activeGroupId { ratesActiveGroupId } for subclient {subClient.Name} total packages: {packages.Count()}");

					foreach (var package in packages.Where(x => x.PackageStatus == EventConstants.Processed))
					{
						var billingWeight = GenerateBillingWeight(package.Weight);
						// Use "rural" and/or "outside48" rates if present
						//	and package.IsRural and/or package.IsOutside48States are set ...
						// First try to match both flags, remember is most cases this will succeed because both flags are not set.
						var rate = rates.Where(x => x.Carrier == package.ShippingCarrier
												&& x.Service == package.ShippingMethod
												&& x.ContainerType == ContainerConstants.ContainerTypePackage
												&& x.WeightNotOverOz >= billingWeight
												&& x.IsRural == package.IsRural
												&& x.IsOutside48States == package.IsOutside48States)
												.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
						if (rate == null && package.IsOutside48States)
						{
							// Failed to match both flags, try to match IsOutside48States ...
							rate = rates.Where(x => x.Carrier == package.ShippingCarrier
												&& x.Service == package.ShippingMethod
												&& x.ContainerType == ContainerConstants.ContainerTypePackage
												&& x.WeightNotOverOz >= billingWeight
												&& x.IsOutside48States == package.IsOutside48States)
												.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
						}
						if (rate == null && package.IsRural)
						{
							// Failed to match both flags, try to match IsRural ...
							rate = rates.Where(x => x.Carrier == package.ShippingCarrier
												&& x.Service == package.ShippingMethod
												&& x.ContainerType == ContainerConstants.ContainerTypePackage
												&& x.WeightNotOverOz >= billingWeight
												&& x.IsRural == package.IsRural)
												.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
						}
						if (rate == null && (package.IsRural || package.IsOutside48States))
						{
							// Failed to match either flag, try for default rate.
							rate = rates.Where(x => x.Carrier == package.ShippingCarrier
												&& x.Service == package.ShippingMethod
												&& x.ContainerType == ContainerConstants.ContainerTypePackage
												&& x.WeightNotOverOz >= billingWeight)
												.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
						}
						if (rate != null)
						{
							package.RateGroupId = ratesActiveGroupId;
							package.RateId = rate.Id;
							package.WebJobIds.Add(webJobId);
							package.BillingWeight = billingWeight; // package weight converted to ounces

							var costAndCharge = await CalculatePackageCostAndCharge(package, rate, rates);
							var extraCostAndCharge = CalculatePackageExtraCostAndCharge(package, subClient);

							package.Cost = costAndCharge.Cost;
							package.Charge = costAndCharge.Charge;
							package.ExtraCost = extraCostAndCharge.ExtraCost;
							package.ExtraCharge = extraCostAndCharge.ExtraCharge;
							package.IsRateAssigned = true;
						}
						else
						{
							logger.LogInformation($"Rate not found for packageId: {package.PackageId}");
						}
					}
				}

				return packages;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to assign package rates for subclient: {subClient.Name} Exception: {ex}");
				// TODO: Send email
				return packages;
			}
		}

		public async Task<List<ShippingContainer>> AssignContainerRatesForEod(Site site, List<ShippingContainer> containers, string webJobId)
		{
			try
			{
				if (containers.Any(x => x.Status == ContainerEventConstants.Closed))
				{
					var rates = await GetCurrentContainerRatesAsync(site.SiteName);
					var ratesActiveGroupId = rates.Any() ? rates.FirstOrDefault().ActiveGroupId : string.Empty;
					logger.LogInformation($"Rates activeGroupId { ratesActiveGroupId } for site {site.SiteName} total containers: {containers.Count()}");

					foreach (var container in containers.Where(x => x.Status == ContainerEventConstants.Closed))
					{
						var costAndCharge = CalculateContainerCostAndCharge(container, rates, webJobId, false);

						if (StringHelper.Exists(container.RateId))
						{
							container.Cost = costAndCharge.Cost;
							container.Charge = costAndCharge.Charge;
							container.IsRateAssigned = true;
						}
					}
				}

				return containers;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to assign container rates for site: {site.SiteName} Exception: {ex}");
				return containers;
			}
		}

		private async Task<List<Rate>> GetCurrentContainerRatesAsync(string siteName)
		{
			var activeGroupId = await activeGroupProcessor.GetContainerRatesActiveGroupIdAsync(siteName);
			var response = await rateRepository.GetRatesByActiveGroupId(activeGroupId);
			return response.ToList();
		}

		private (decimal Cost, decimal Charge) CalculateContainerCostAndCharge(ShippingContainer container, List<Rate> rates, string webJobId, bool isUpdate)
		{
			var cost = 0m;
			var charge = 0m;

			decimal.TryParse(container.Weight, out var containerWeight);
			var billingWeight = GenerateBillingWeight(containerWeight);			

			// Use "rural" and/or "outside48" rates if present
			//	and container.IsRural and/or container.IsOutside48States are set ...
			// First try to match both flags, remember is most cases this will succeed because both flags are not set.
			var rate = rates.Where(x => x.Carrier == container.ShippingCarrier
									&& x.ContainerType == container.ContainerType
									&& x.WeightNotOverOz >= billingWeight
									&& x.IsRural == container.IsRural
									&& x.IsOutside48States == container.IsOutside48States)
									.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
			if (rate == null && container.IsOutside48States)
			{
				// Failed to match both flags, try to match IsOutside48States ...
				rate = rates.Where(x => x.Carrier == container.ShippingCarrier
									&& x.ContainerType == container.ContainerType
									&& x.WeightNotOverOz >= billingWeight
									&& x.IsOutside48States == container.IsOutside48States)
									.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
			}
			if (rate == null && container.IsRural)
			{
				// Failed to match both flags, try to match IsRural ...
				rate = rates.Where(x => x.Carrier == container.ShippingCarrier
									&& x.ContainerType == container.ContainerType
									&& x.WeightNotOverOz >= billingWeight
									&& x.IsRural == container.IsRural)
									.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
			}
			if (rate == null && (container.IsRural || container.IsOutside48States))
			{
				// Failed to match either flag, try for default rate.
				rate = rates.Where(x => x.Carrier == container.ShippingCarrier
									&& x.Service == container.ShippingMethod
									&& x.ContainerType == container.ContainerType
									&& x.WeightNotOverOz >= billingWeight)
									.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
			}
			if (rate != null)
			{
				if (isUpdate)
				{
					container.HistoricalRateIds.Add(container.RateId);
					container.HistoricalRateGroupIds.Add(container.RateGroupId);
				}

				container.RateId = rate.Id;
				container.RateGroupId = rate.ActiveGroupId;
				container.BillingWeight = billingWeight.ToString();

				container.WebJobIds.Add(webJobId);

				if (container.Zone == 1)
				{
					cost = rate.CostZone1;
					charge = rate.ChargeZone1;
				}
				else if (container.Zone == 2)
				{
					cost = rate.CostZone2;
					charge = rate.ChargeZone2;
				}
				else if (container.Zone == 3)
				{
					cost = rate.CostZone3;
					charge = rate.ChargeZone3;
				}
				else if (container.Zone == 4)
				{
					cost = rate.CostZone4;
					charge = rate.ChargeZone4;
				}
				else if (container.Zone == 5)
				{
					cost = rate.CostZone5;
					charge = rate.ChargeZone5;
				}
				else if (container.Zone == 6)
				{
					cost = rate.CostZone6;
					charge = rate.ChargeZone6;
				}
				else if (container.Zone == 7)
				{
					cost = rate.CostZone7;
					charge = rate.ChargeZone7;
				}
				else if (container.Zone == 8)
				{
					cost = rate.CostZone8;
					charge = rate.ChargeZone8;
				}
				else if (container.Zone == 9)
				{
					cost = rate.CostZone9;
					charge = rate.ChargeZone9;
				}
			}

			return (cost, charge);
		}

		private async Task<(decimal Cost, decimal Charge)> CalculatePackageCostAndCharge(Package package, Rate rateForPackage, List<Rate> ratesForSubClient)
		{
			var cost = 0m;
			var charge = 0m;
			var zoneCostAndCharge = GetZoneBasedCostAndCharge(package, rateForPackage);
			var usePsCostAndCharge = package.ShippingMethod == ShippingMethodConstants.UspsParcelSelectLightWeight
				|| package.ShippingMethod == ShippingMethodConstants.UspsParcelSelect;
			var markedUpToFirstClass = package.ShippingMethod == ShippingMethodConstants.UspsFirstClass && package.IsMarkUpTypeCompany;
			var isDduBin = StringHelper.Exists(package.BinCode) && package.BinCode.Substring(0, 1) == "D";
			var isScfBin = StringHelper.Exists(package.BinCode) && package.BinCode.Substring(0, 1) == "S";

			if (usePsCostAndCharge)
			{
				if (isDduBin)
				{
					cost = rateForPackage.CostZoneDdu;
					charge = rateForPackage.ChargeZoneDdu;
				}
				else if (isScfBin)
				{
					cost = rateForPackage.CostZoneScf;
					charge = rateForPackage.ChargeZoneScf;
				}
			}
			else if (markedUpToFirstClass)
			{
				// use original mailcode to query service rules to see if package was marked up from PSLW -> FIRST CLASS
				// if so use CHARGE from PSLW and COST from FIRST CLASS, else use normal rate

				var serviceRuleBeforeMarkup = await serviceRuleProcessor.GetServiceRuleByOverrideMailCode(package);
				if (serviceRuleBeforeMarkup.ShippingMethod == ShippingMethodConstants.UspsParcelSelectLightWeight ||
					serviceRuleBeforeMarkup.ShippingMethod == ShippingMethodConstants.UspsParcelSelect)
				{
					// Find the rate for PS ...
					var rateBeforeMarkup = ratesForSubClient.Where(x => x.Carrier == package.ShippingCarrier
												&& x.Service == serviceRuleBeforeMarkup.ShippingMethod
												&& x.ContainerType == ContainerConstants.ContainerTypePackage
												&& x.WeightNotOverOz >= package.BillingWeight)
												.OrderBy(y => y.WeightNotOverOz).FirstOrDefault();
					// Find original bin code ...
					var originalBincode = package.HistoricalBinCodes.LastOrDefault();
					if (rateBeforeMarkup != null && originalBincode != null)
					{
						isDduBin = originalBincode.Substring(0, 1) == "D";
						isScfBin = originalBincode.Substring(0, 1) == "S";
						if (isDduBin)
						{
							charge = package.IsOutside48States ? rateBeforeMarkup.ChargeZoneDduOut48 : rateBeforeMarkup.ChargeZoneDdu;
						}
						else if (isScfBin)
						{
							charge = package.IsOutside48States ? rateBeforeMarkup.ChargeZoneScfOut48 : rateBeforeMarkup.ChargeZoneScf;
						}
					}
					cost = zoneCostAndCharge.Cost;
				}
				else // if wasn't marked up from PS, use normal rate
				{
					cost = zoneCostAndCharge.Cost;
					charge = zoneCostAndCharge.Charge;
				}
			}
			else // if not PS or not marked up, use normal rate
			{
				cost = zoneCostAndCharge.Cost;
				charge = zoneCostAndCharge.Charge;
			}

			return (cost, charge);
		}

		private static (decimal ExtraCost, decimal ExtraCharge) CalculatePackageExtraCostAndCharge(Package package, SubClient subClient)
		{
			var extraCost = 0m;
			var extraCharge = 0m;
			var isUspsSignature = package.ShippingCarrier == ShippingCarrierConstants.Usps && package.ServiceLevel == ServiceLevelConstants.Signature;
			if (isUspsSignature)
			{
				extraCost = subClient.SignatureCost;
				extraCharge = subClient.SignatureCharge;
			}
			return (extraCost, extraCharge);
		}

		private static (decimal Cost, decimal Charge) GetZoneBasedCostAndCharge(Package package, Rate rate)
		{
			var cost = 0m;
			var charge = 0m;

			if (package.Zone == 1)
			{
				cost = rate.CostZone1;
				charge = rate.ChargeZone1;
			}
			else if (package.Zone == 2)
			{
				cost = rate.CostZone2;
				charge = rate.ChargeZone2;
			}
			else if (package.Zone == 3)
			{
				cost = rate.CostZone3;
				charge = rate.ChargeZone3;
			}
			else if (package.Zone == 4)
			{
				cost = rate.CostZone4;
				charge = rate.ChargeZone4;
			}
			else if (package.Zone == 5)
			{
				cost = rate.CostZone5;
				charge = rate.ChargeZone5;
			}
			else if (package.Zone == 6)
			{
				cost = rate.CostZone6;
				charge = rate.ChargeZone6;
			}
			else if (package.Zone == 7)
			{
				cost = rate.CostZone7;
				charge = rate.ChargeZone7;
			}
			else if (package.Zone == 8)
			{
				cost = rate.CostZone8;
				charge = rate.ChargeZone8;
			}
			else if (package.Zone == 9)
			{
				cost = rate.CostZone9;
				charge = rate.ChargeZone9;
			}

			return (cost, charge);
		}

		private static decimal GenerateBillingWeight(decimal weight)
		{
			//ceiling in ounces
			var weightInOunces = weight * 16;
			return Math.Ceiling(weightInOunces);
		}
    }
}
