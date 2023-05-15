using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface ISiteRepository : IRepository<Site>
	{
		Task<string> GetSiteIdBySiteNameAsync(string siteName);
		Task<Site> GetSiteBySiteNameAsync(string siteName);
		Task<IEnumerable<Site>> GetAllSitesAsync();
        Task<Site> GetSiteByIdAsync(string siteId);
    }
}
