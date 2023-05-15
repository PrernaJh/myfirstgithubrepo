using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface IRateWebProcessor
    {
        Task<FileImportResponse> ImportListOfNewRates(List<Rate> rates, string startDate, string subClientName, string username, string fileName);
        Task<FileImportResponse> ImportListOfNewContainerRates(List<Rate> rates, string startDate, string siteName, string username, string fileName);
        Task<List<ActiveGroup>> GetAllRateActiveGroupsAsync(string subClientName);
        Task<List<Rate>> GetRatesByActiveGroupIdAsync(string activeGroupId);
    }
}
