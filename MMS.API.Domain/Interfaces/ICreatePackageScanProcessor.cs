using MMS.API.Domain.Models.ProcessScanAndAuto;
using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
    public interface ICreatePackageScanProcessor
    {
        Task ProcessCreatedPackage(Package package, ProcessScanPackage processScan);
    }
}
