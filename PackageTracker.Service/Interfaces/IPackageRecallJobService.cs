using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
    public interface IPackageRecallJobService
    {
        Task ProcessRecalledPackages(string message);
    }
}
