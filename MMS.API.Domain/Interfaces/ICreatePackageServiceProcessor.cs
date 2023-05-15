using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface ICreatePackageServiceProcessor
    {
        Task<bool> GetCreatePackageServiceDataAsync(Package package);
    }
}
