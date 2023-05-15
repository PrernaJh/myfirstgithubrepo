using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface IServiceRuleExtensionWebProcessor
    {
        Task<List<ServiceRuleExtension>> GetExtendedServiceRulesByActiveGroupIdAsync(string activeGroupId);
        Task<FileImportResponse> ImportListOfNewExtendedServiceRules(List<ServiceRuleExtension> extendedServiceRules, string startDate, string clientName, string username, string filename = null);
    }
}
