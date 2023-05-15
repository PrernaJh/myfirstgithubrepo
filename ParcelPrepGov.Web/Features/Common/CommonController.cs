using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Interfaces;
using MMS.Web.Domain.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Infrastructure;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Globals;
using ParcelPrepGov.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.Common
{
	public class CommonController : BaseApiController
	{
		private readonly ILogger<CommonController> logger;
		private readonly IMemoryCache _cache;
		private readonly ISiteProcessor _siteProcessor;
		private readonly IClientProcessor _clientProcessor;
		private readonly ISubClientProcessor _subClientProcessor;
		private readonly ISiteWebProcessor _siteWebProcessor;
		private readonly ISubClientWebProcessor _subClientWebProcessor;
		private readonly IClientWebProcessor _clientWebProcessor;

        public CommonController(ILogger<CommonController> logger, IMemoryCache memoryCache, ISiteProcessor siteProcessor, IClientProcessor clientProcessor, ISubClientProcessor subClientProcessor, ISiteWebProcessor siteWebProcessor, ISubClientWebProcessor subClientWebProcessor, IClientWebProcessor clientWebProcessor)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _siteProcessor = siteProcessor ?? throw new ArgumentNullException(nameof(siteProcessor));
            _clientProcessor = clientProcessor ?? throw new ArgumentNullException(nameof(clientProcessor));
            _subClientProcessor = subClientProcessor ?? throw new ArgumentNullException(nameof(subClientProcessor));
            _siteWebProcessor = siteWebProcessor;
            _subClientWebProcessor = subClientWebProcessor;
            _clientWebProcessor = clientWebProcessor;
        }

        [HttpGet(Name = nameof(GetSiteSelectBoxData))]
		public async Task<JsonResult> GetSiteSelectBoxData([FromQuery] DataSourceLoadOptions loadOptions)
		{

			var siteCacheKey = $"{User.GetUsername()}_{CacheKeys.Sites}_SelectBoxData";

			// Look for cache key.
			if (!_cache.TryGetValue(siteCacheKey, out List<SiteSelectBoxItemModel> sites))
			{
				// Key not in cache, so get data.
				var request = new GetSitesRequest
				{
					Name = User.GetSite()
				};

				var getSitesResponse = await _siteWebProcessor.GetSitesAsync(request);


				sites = getSitesResponse.GetSites.Select(x => new SiteSelectBoxItemModel()
				{
					Id = x.Id,
					Name = x.Name,
					Description = x.Description
				}).OrderBy(y => y.Description).ToList();

				// Set cache options.
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					// Keep in cache for this time, reset time if accessed.
					.SetSlidingExpiration(TimeSpan.FromHours(1));

				// Save data in cache.
				_cache.Set(siteCacheKey, sites, cacheEntryOptions);
			}

			return new JsonResult(sites);
		}

		[HttpGet(Name = nameof(GetClientSelectBoxData))]
		public async Task<JsonResult> GetClientSelectBoxData([FromQuery] DataSourceLoadOptions loadOptions)
		{

			var clientsCacheKey = $"{User.GetUsername()}_{CacheKeys.Clients}_SelectBoxData";

			// Look for cache key.
			if (!_cache.TryGetValue(clientsCacheKey, out List<ClientSelectBoxItemModel> clients))
			{
				// Key not in cache, so get data.
				var request = new GetClientsRequest
				{
					Name = User.GetClient()
				};

				var getClientsResponse = await _clientWebProcessor.GetClientsAsync(request);

				clients = getClientsResponse.GetClients.Select(x => new ClientSelectBoxItemModel()
				{
					Id = x.Id,
					Name = x.Name,
					Description = x.Description
				}).OrderBy(y => y.Description).ToList();

				// Set cache options.
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					// Keep in cache for this time, reset time if accessed.
					.SetSlidingExpiration(TimeSpan.FromHours(1));

				// Save data in cache.
				_cache.Set(clientsCacheKey, clients, cacheEntryOptions);
			}

			return new JsonResult(clients);
		}

		[HttpGet(Name = nameof(GetSubClientSelectBoxData))]
		public async Task<JsonResult> GetSubClientSelectBoxData()
		{
			try
			{
				var subClientsCacheKey = $"{User.GetUsername()}_{CacheKeys.SubClients}_SelectBoxData";

				// Look for cache key.
				if (!_cache.TryGetValue(subClientsCacheKey, out List<SubClientSelectBoxItemModel> subClients))
				{
					// Key not in cache, so get data.
					var request = new GetSubClientsRequest { 
						SiteName = User.GetSite(),
						ClientName = User.GetClient(),
						SubClientName = User.GetSubClient()
					};
					
					var getSubClientsResponse = await _subClientWebProcessor.GetSubclientsAsync(request);

					subClients = getSubClientsResponse.GetSubClients.Select(x => new SubClientSelectBoxItemModel()
					{
						Id = x.Id,
						Name = x.Name,
						Description = x.Description,
						ClientName = x.ClientName,
						SiteName = x.SiteName
					}).OrderBy(y => y.Description).ToList();

					// Set cache options.
					var cacheEntryOptions = new MemoryCacheEntryOptions()
						// Keep in cache for this time, reset time if accessed.
						.SetSlidingExpiration(TimeSpan.FromHours(1));

					// Save data in cache.
					_cache.Set(subClientsCacheKey, subClients, cacheEntryOptions);
				}				

				return new JsonResult(subClients);
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to load SubClients. Exception: {ex}");
				throw;
			}
		}
	}
}
