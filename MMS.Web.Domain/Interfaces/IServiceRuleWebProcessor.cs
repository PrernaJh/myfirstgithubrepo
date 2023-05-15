using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MMS.Web.Domain.Interfaces
{
    public interface IServiceRuleWebProcessor
    {
        Task<List<ServiceRule>> GetServiceRulesByActiveGroupIdAsync(string activeGroupId);
        Task<FileImportResponse> ImportListOfNewServiceRules(List<ServiceRule> serviceRules, string startDate, string cutomerName, string username, string filename = null);
        Task<FileImportResponse> ImportListOfNewBusinessRules(List<ServiceRule> serviceRules, string startDate, string subClientName, string username, string filename = null);
    }
}
