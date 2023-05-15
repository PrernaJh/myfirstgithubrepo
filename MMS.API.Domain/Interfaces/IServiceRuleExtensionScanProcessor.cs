using MMS.API.Domain.Models;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface IServiceRuleExtensionScanProcessor
    {
        Task<ServiceOutput> UseFortyEightStatesServiceRuleExtension(Package package, ServiceRule serviceRule);
    }
}
