using MMS.API.Domain.Models.Returns;
using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace MMS.API.Domain.Interfaces
{
    public interface IPackageLabelProcessor
    {
        (Package, bool) GetPackageLabelData(Package package);
        void AddUspsLabelFieldValues(Package package, List<LabelFieldValue> labelFieldValues);
        List<LabelFieldValue> GetLabelFieldsForReturnToCustomer(ReturnLabelRequest request);
        List<LabelFieldValue> GetLabelFieldsForReturnEodProcessed(string siteName, string packageId);
        void GetLabelDataForAutoScanReprint(Package package);
        List<LabelFieldValue> GetLabelDataForSortCodeChange(string binCode, string packageId);
    }
}
