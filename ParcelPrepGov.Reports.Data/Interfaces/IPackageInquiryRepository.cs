using ParcelPrepGov.Reports.Models;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IPackageInquiryRepository
    {
        Task<PackageInquiry> GetPackageInquiryAsync(int packageDatasetId);
    }
}
