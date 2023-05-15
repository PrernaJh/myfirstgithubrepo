using MMS.API.Domain.Models.AutoScan;
using MMS.API.Domain.Models.ProcessScanAndAuto;
using PackageTracker.Data.Models;

namespace MMS.API.Domain.Interfaces
{
    public interface IPackageErrorProcessor
    {
        string EvaluateScannedPackageStatus(Package package, ProcessScanPackage processScan);
        string EvaluateCreatedPackageStatus(Package package);
        void GenerateCreatedPackageError(Package package, ProcessScanPackage processScan);
        void GenerateScanPackageError(Package package, ProcessScanPackage processScan);
        void GenerateAutoScanCreatedPackageError(Package package, ProcessAutoScanPackage processScan, ParcelDataResponse response);
        void GenerateAutoScanPackageError(Package package, ProcessAutoScanPackage processScan, ParcelDataResponse response);
        void GenerateBinValidationZpl(Package package, string message);
        void GenerateBinValidationLabel(Package package);
    }
}
