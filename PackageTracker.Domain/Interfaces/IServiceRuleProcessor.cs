using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface IServiceRuleProcessor
    {
        Task<FileImportResponse> ProcessServiceRuleFileStream(Stream stream, string subClientName);
        Task<ServiceRule> GetServiceRuleByOverrideMailCode(Package package);
    }
}
