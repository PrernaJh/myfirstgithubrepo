using MMS.API.Domain.Models.Returns;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IReturnProcessor
    {
        Task<GetReturnOptionResponse> GetReturnOptionsAsync(string siteName);
        Task<ReturnPackageResponse> ReturnPackageAsync(ReturnPackageRequest request);
    }
}
