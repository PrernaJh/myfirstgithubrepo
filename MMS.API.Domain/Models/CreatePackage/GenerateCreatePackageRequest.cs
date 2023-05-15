using MMS.API.Domain.Utilities;
using PackageTracker.Data.Models;

namespace MMS.API.Domain.Models.CreatePackage
{
    public class GenerateCreatePackageRequest
    {
        public ClientFacility ClientFacility { get; set; }
        public PackageTimer Timer { get; set; } = new PackageTimer();
        public bool IsScanPackage { get; set; }
        public bool IsAutoScan { get; set; }
        public bool IsInitialCreate { get; set; }
    }
}