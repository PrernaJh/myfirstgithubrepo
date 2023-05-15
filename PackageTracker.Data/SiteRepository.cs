using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
	public class SiteRepository : CosmosDbRepository<Site>, ISiteRepository
	{
		public SiteRepository(ILogger<SiteRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
			base(logger, configuration, factory)
		{ }

		public override string ContainerName { get; } = CollectionNameConstants.Sites;

		public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GenerateDefaultPartitionKeyString();

		public async Task<string> GetSiteIdBySiteNameAsync(string siteName)
		{
			var query = $@"SELECT TOP 1 s.id FROM {ContainerName} s WHERE s.siteName = @siteName";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results.Any() ? results.First().Id : string.Empty;
		}

		public async Task<Site> GetSiteBySiteNameAsync(string siteName)
		{
			var query = $@"SELECT TOP 1 * FROM {ContainerName} s WHERE s.siteName = @siteName";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteName", siteName);
			var results = await GetItemsAsync(queryDefinition, siteName);
			return results.FirstOrDefault() ?? new Site();
		}

		public async Task<IEnumerable<Site>> GetAllSitesAsync()
		{
			var query = $@"SELECT * FROM {ContainerName} s WHERE s.isEnabled = true";
			var queryDefinition = new QueryDefinition(query);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results;
		}

        public async Task<Site> GetSiteByIdAsync(string siteId)
        {
			var query = $@"SELECT TOP 1 * FROM {ContainerName} s WHERE s.id = @siteId";
			var queryDefinition = new QueryDefinition(query)
													.WithParameter("@siteId", siteId);
			var results = await GetItemsCrossPartitionAsync(queryDefinition);
			return results.FirstOrDefault() ?? new Site();
		}
    }
}
