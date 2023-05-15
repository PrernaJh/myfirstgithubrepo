using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class EodPostProcessor : IEodPostProcessor
	{
		private readonly ISiteProcessor siteProcessor;
		private readonly IWebJobRunProcessor webJobRunProcessor;

		public EodPostProcessor(ISiteProcessor siteProcessor,
			IWebJobRunProcessor webJobRunProcessor
			)
		{
			this.siteProcessor = siteProcessor;
			this.webJobRunProcessor = webJobRunProcessor;
		}

        public async Task<bool> ShouldLockPackage(Package package)
		{
			var locked = false;
			if (package.PackageStatus == EventConstants.Processed)
			{
				// Package is locked if manifest date is not today, or end of day has been started for today.
				var today = TimeZoneUtility.GetLocalTime(package.TimeZone).Date;
				locked = package.LocalProcessedDate.Date < today;
				if (! locked)
				{
					var mostRecentJobRun =
						await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(package.SiteName, null, today, WebJobConstants.RunEodJobType);
					locked = mostRecentJobRun.IsSuccessful; // EOD was started for the manifest date.
				}
			}
			return locked;
		}
        public async Task<bool> ShouldLockContainer(ShippingContainer container)
        {
			var locked = false;
			if (container.Status == ContainerEventConstants.Closed)
			{
				// Container is locked if manifest date is not today, or end of day has been started for today.
				var site = await siteProcessor.GetSiteBySiteNameAsync(container.SiteName);
				var today = TimeZoneUtility.GetLocalTime(site.TimeZone).Date;
				locked = container.LocalProcessedDate.Date < today;
				if (!locked)
				{
					var mostRecentJobRun =
						await webJobRunProcessor.GetMostRecentJobRunByProcessedDate(container.SiteName, null, today, WebJobConstants.RunEodJobType);
					locked = mostRecentJobRun.IsSuccessful; // EOD was started for the manifest date.
				}
			}
			return locked;
		}

	}
}
