using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PackageTracker.Domain.Interfaces;
using ParcelPrepGov.Web.Infrastructure.Globals;
using ParcelPrepGov.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Components.Toolbar
{
	public class ToolbarViewComponent : ViewComponent
	{
		private readonly IMemoryCache _cache;
		private readonly ISiteProcessor _siteProcessor;

		public ToolbarViewComponent(IMemoryCache memoryCache, ISiteProcessor siteProcessor)
		{
			_cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			_siteProcessor = siteProcessor ?? throw new ArgumentNullException(nameof(siteProcessor));
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			// Look for cache key.
			if (!_cache.TryGetValue(CacheKeys.Sites, out List<SiteViewModel> sites))
			{
				// Key not in cache, so get data.
				var siteList = await _siteProcessor.GetAllSitesAsync();

				sites = siteList.Select(x => new SiteViewModel()
				{
					Id = new Guid(x.Id),
					SiteName = x.SiteName

				}).ToList();

				// Set cache options.
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					// Keep in cache for this time, reset time if accessed.
					.SetSlidingExpiration(TimeSpan.FromHours(8));

				// Save data in cache.
				_cache.Set(CacheKeys.Sites, sites, cacheEntryOptions);
			}

			return View(sites);
		}
	}
}
