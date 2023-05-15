using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IPackageDuplicateProcessor
    {
        Task<IEnumerable<Package>> GetDuplicatePackages(Package package);
        Task<IEnumerable<Package>> GetPackagesForDuplicateAsnChecker(string siteName);
        Task<Package> EvaluateDuplicatePackagesOnScan(List<Package> packages, bool isRepeatScan = false);
        IEnumerable<Package> CheckDuplicatePackages(Package package, IEnumerable<Package> duplicatePackages);
    }
}
