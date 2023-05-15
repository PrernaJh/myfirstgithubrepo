using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Components.Toolbar
{
	public class SectionSelectorViewComponent : ViewComponent
	{
		protected readonly ISiteRepository siteRepository;
		protected readonly IClientRepository clientRepository;
		protected readonly ISubClientRepository subClientRepository;
		public SectionSelectorViewComponent(ISiteRepository siteRepository,
			IClientRepository clientRepository,
			ISubClientRepository subClientRepository)
		{
			this.siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
			this.clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
			this.subClientRepository = subClientRepository ?? throw new ArgumentNullException(nameof(subClientRepository));
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{


			IEnumerable<SelectListItem> sites = (await GetSelectableSites()).Select(x => new SelectListItem(x, x));
			IEnumerable<SelectListItem> clients = (await GetSelectableClients()).Select(x => new SelectListItem(x, x));
			IEnumerable<SelectListItem> subclients = (await GetSelectableSubClients()).Select(x => new SelectListItem(x, x));


			SectionSelectorModel model = new SectionSelectorModel
			{
				Sites = sites,
				Clients = clients,
				SubClients = subclients
			};

			return View(model);
		}

		public async Task<List<string>> GetSelectableSites()
		{
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSubClient = User.GetSubClient() == SiteConstants.AllSites;

			if (isGlobalSite)
			{
				if (isGlobalClient && isGlobalSubClient)
				{
					var sites = (await siteRepository.GetAllSitesAsync()).Select(x => x.SiteName).ToList();
					sites.Insert(0, SiteConstants.AllSites);
					return sites;
				}
				else
				{
					var sites = new List<string> { SiteConstants.AllSites };
					return sites;
				}
			}
			else
			{
				var sites = new List<string> { User.GetSite() };
				return sites;
			}
		}

		public async Task<List<string>> GetSelectableClients()
		{
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;

			if (!isGlobalSite)
			{
				var clients = new List<string> { SiteConstants.AllSites };
				return clients;
			}
			else if (isGlobalClient)
			{
				var clients = (await clientRepository.GetClientsAsync()).Select(x => x.Name).ToList();
				clients.Insert(0, SiteConstants.AllSites);
				return clients;
			}
			else
			{
				var clients = new List<string> { User.GetClient() };
				return clients;
			}
		}

		public async Task<List<string>> GetSelectableSubClients()
		{
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSubClient = User.GetSubClient() == SiteConstants.AllSites;

			if (!isGlobalSite)
			{
				var subclients = new List<string> { SiteConstants.AllSites };
				return subclients;
			}
			if (isGlobalClient)
			{
				var subclients = (await subClientRepository.GetSubClientsAsync()).Select(x => x.Name).ToList();
				subclients.Insert(0, SiteConstants.AllSites);
				return subclients;
			}
			else if (isGlobalSubClient)
			{
				var subclients = (await subClientRepository.GetSubClientsAsync())
					.Where(x => x.ClientName == User.GetClient())
					.Select(x => x.Name).ToList();
				subclients.Insert(0, SiteConstants.AllSites);
				return subclients;
			}
			else
			{
				var subclients = new List<string> { User.GetSubClient() };
				return subclients;
			}
		}
	}
}
