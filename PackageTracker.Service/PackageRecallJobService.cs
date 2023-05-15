using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Service.Interfaces;
using PackageTracker.WebServices;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UpsVoidApi;

namespace PackageTracker.Service
{
    public class PackageRecallJobService : IPackageRecallJobService
    {
        private readonly ILogger<PackageRecallJobService> logger;

		private readonly IConfiguration configuration;
		private readonly IPackageRepository packageRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;
		private readonly IShippingProcessor shippingProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public PackageRecallJobService(ILogger<PackageRecallJobService> logger,
			IConfiguration configuration,
			IPackageRepository packageRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor,
			IShippingProcessor shippingProcessor,
			IWebJobRunProcessor webJobRunProcessor)
		{
			this.logger = logger;

			this.configuration = configuration;
			this.packageRepository = packageRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
			this.shippingProcessor = shippingProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

		public async Task ProcessRecalledPackages(string message)
        {
			var queueMessageArray = message.Split('\n');
			var subClientName = queueMessageArray[0];
			var userName = queueMessageArray[1];
			var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
			var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

			var recalledPackages = await packageRepository.GetRecalledPackages(subClientName);
			var releasedPackages = await packageRepository.GetReleasedPackages(subClientName);
			var totalPackages = 0;
			var upsPackages = 0;
			var packagesNotRecalled = 0;
			foreach (var packageId in queueMessageArray.Skip(2))
			{
				totalPackages++;
				var package = recalledPackages.Where(p => p.PackageId == packageId).FirstOrDefault() ??
					releasedPackages.Where(p => p.PackageId == packageId).FirstOrDefault();
				if (package != null && package.ShippingCarrier == ShippingCarrierConstants.Ups)
				{
					// For UPS we need to notify them if the package was processed, then recalled.
					upsPackages++;
					if (! await shippingProcessor.VoidUpsShipmentAsync(package, subClient))
						packagesNotRecalled++;
				}
			}
			await webJobRunProcessor.AddWebJobRunAsync(new WebJobRunRequest
			{
				Id = Guid.NewGuid().ToString(),
				SiteName = subClient.SiteName,
				ClientName = subClient.ClientName,
				SubClientName = subClient.Name,
				JobName = $"Package Recall: Total Packages: {totalPackages}, UPS packages: {upsPackages}",
				JobType = WebJobConstants.PackageRecallJobType,
				Username = userName,
				Message = packagesNotRecalled == 0 ? string.Empty : $"{packagesNotRecalled} package(s) not recalled",
				IsSuccessful = packagesNotRecalled == 0,
				LocalCreateDate = TimeZoneUtility.GetLocalTime(site.TimeZone)
			});
		}
	}
}
