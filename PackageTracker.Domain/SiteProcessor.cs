using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class SiteProcessor : ISiteProcessor
	{
		private readonly ISiteRepository siteRepository;

		public SiteProcessor(ISiteRepository siteRepository)
		{
			this.siteRepository = siteRepository;
		}

		public async Task<Site> GetSiteByIdAsync(string siteId)
		{
			return await siteRepository.GetSiteByIdAsync(siteId);
		}

		public async Task<string> GetSiteIdBySiteNameAsync(string siteName)
		{
			return await siteRepository.GetSiteIdBySiteNameAsync(siteName);
		}

		public async Task<Site> GetSiteBySiteNameAsync(string siteName)
		{
			return await siteRepository.GetSiteBySiteNameAsync(siteName);
		}

		public async Task<IEnumerable<Site>> GetAllSitesAsync()
		{
			return await siteRepository.GetAllSitesAsync();
		}

		public async Task<GetAllSiteNamesResponse> GetAllSiteNamesAsync()
		{
			var response = new GetAllSiteNamesResponse();
			var sites = await siteRepository.GetAllSitesAsync();

			foreach (var site in sites)
			{
				response.SiteNames.Add(site.SiteName);
			}

			return response;
		}

        public async Task<GenericResponse<string>> AddUserToCriticalEmailList(ModifyCritialEmailListRequest request)
        {
			var siteName = request.SiteName;
			var email = request.Email;
			try
            {
				var site = await siteRepository.GetSiteBySiteNameAsync(siteName);
				if (StringHelper.DoesNotExist(site.Id))
					return new GenericResponse<string>(email, false, $"Site {siteName} does not exist");
				if (site.CriticalAlertEmailList.Exists(x => string.Equals(x, email, StringComparison.InvariantCultureIgnoreCase)))
					return new GenericResponse<string>(email, true, $"Email {email} already in critial alert list for site {siteName}");
				site.CriticalAlertEmailList.Add(email);
				await siteRepository.UpdateItemAsync(site);
				return new GenericResponse<string>(email, true, $"Email {email} added to critical alert list for site {siteName}");
            } catch (Exception ex)
            {
				return new GenericResponse<string>(email, false, ex.Message);
			}
        }

        public async Task<GenericResponse<string>> RemoveUserFromCriticalEmailList(ModifyCritialEmailListRequest request)
		{
			var siteName = request.SiteName;
			var email = request.Email;
			try
			{
				var site = await siteRepository.GetSiteBySiteNameAsync(siteName);
				if (StringHelper.DoesNotExist(site.Id))
					return new GenericResponse<string>(email, false, $"Site {siteName} does not exist");
				if (! site.CriticalAlertEmailList.Exists(x => string.Equals(x, email, StringComparison.InvariantCultureIgnoreCase)))
					return new GenericResponse<string>(email, true, $"Email {email} not in critial alert list for site {siteName}");
				site.CriticalAlertEmailList.RemoveAll(x => string.Equals(x, email, StringComparison.InvariantCultureIgnoreCase));
				await siteRepository.UpdateItemAsync(site);
				return new GenericResponse<string>(email, true, $"Email {email} removed from critical alert list for site {siteName}");
			}
			catch (Exception ex)
			{
				return new GenericResponse<string>(email, false, ex.Message);
			}
		}

        public async Task<IEnumerable<string>> GetSiteCritialEmailListBySiteId(string id)
        {
			var site = await GetSiteByIdAsync(id);

			return site?.CriticalAlertEmailList ?? new List<string>();
        }

        public async Task<IEnumerable<string>> GetSiteCritialEmailListBySiteName(string siteName)
        {
			var site = await GetSiteBySiteNameAsync(siteName);

			return site?.CriticalAlertEmailList ?? new List<string>();
        }
    }
}
