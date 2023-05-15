using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface ISiteProcessor
	{
		Task<Site> GetSiteByIdAsync(string id);
		Task<string> GetSiteIdBySiteNameAsync(string siteName);
		Task<Site> GetSiteBySiteNameAsync(string siteName);
		Task<IEnumerable<Site>> GetAllSitesAsync();
		Task<GetAllSiteNamesResponse> GetAllSiteNamesAsync();
		Task<IEnumerable<string>> GetSiteCritialEmailListBySiteId(string id);
		Task<IEnumerable<string>> GetSiteCritialEmailListBySiteName(string siteName);
		Task<GenericResponse<string>> AddUserToCriticalEmailList(ModifyCritialEmailListRequest request);
		Task<GenericResponse<string>> RemoveUserFromCriticalEmailList(ModifyCritialEmailListRequest request);
	}
}
