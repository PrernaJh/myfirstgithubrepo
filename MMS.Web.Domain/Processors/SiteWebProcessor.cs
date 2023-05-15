using MMS.Web.Domain.Interfaces;
using MMS.Web.Domain.Models;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Processors
{
    public class SiteWebProcessor : ISiteWebProcessor
    {
        private readonly ISiteRepository siteRepository;       

        public SiteWebProcessor(ISiteRepository siteRepository)
        {
            this.siteRepository = siteRepository;
        }

        public async Task<GetSitesResponse> GetSitesAsync(GetSitesRequest request)
        {
            var response = new GetSitesResponse();
            var sites = await siteRepository.GetAllSitesAsync();
            var sitesResponse = new List<Site>();

            if (request.Name != IdentityDataConstants.Global)
            {
                sitesResponse.AddRange(sites.Where(x => x.SiteName == request.Name));
            }
            else
            {
                sitesResponse.AddRange(sites);
            }

            foreach (var site in sitesResponse)
            {
                response.GetSites.Add(new GetSite
                {
                    Id = site.Id,
                    Description = site.Description,
                    Name = site.SiteName
                });
            }

            return response;
        }
        
    }
}
