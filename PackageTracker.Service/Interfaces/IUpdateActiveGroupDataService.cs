using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{

    public interface IUpdateActiveGroupDataService
    {
        Task UpdatePackageBinsAndBinMapsAsync(WebJobSettings webJobSettings);
        Task UpdateServiceRuleGroupIds(string message);
    }
}