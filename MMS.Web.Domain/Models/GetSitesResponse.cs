using System;
using System.Collections.Generic;
using System.Text;

namespace MMS.Web.Domain.Models
{
    public class GetSitesResponse : BaseResponse
    {
        public List<GetSite> GetSites { get; set; } = new List<GetSite>();
    }
}
