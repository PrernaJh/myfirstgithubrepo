using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.RecallRelease;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IRecallReleaseProcessor
    {
        Task<RecallReleasePackageResponse> ImportListOfRecallPackagesForSubClient(Stream stream, string subClientName, string username);
        Task<RecallReleasePackageResponse> ImportListOfReleasePackagesForSubClient(Stream stream, string subClientName, string username);
        Task<RecallReleasePackageResponse> ProcessRecallPackageForSubClient(string packageId, string subClientName, string username);
        Task<RecallReleasePackageResponse> ProcessReleasePackageForSubClient(string packageId, string subClientName, string username);
        Task<RecallReleasePackageResponse> ProcessDeleteRecallPackageForSubClient(string packageId, string subClientName, string username);
        Task<IEnumerable<Package>> GetRecalledPackagesAsync(string subClientName);
        Task<IEnumerable<Package>> GetReleasedPackagesAsync(string subClientName);
        Task<IEnumerable<Package>> FindPackagesToRecallByPartial(string subClientName, string partialPackageId); 
    }
}
