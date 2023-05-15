using MMS.Web.Domain.Models;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface ISiteWebProcessor
    {
        Task<GetSitesResponse> GetSitesAsync(GetSitesRequest request);
    }
}
