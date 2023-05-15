using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
    public interface ICreatedPackagePostProcessService
    {
        Task PostProcessCreatedPackages();
    }
}
