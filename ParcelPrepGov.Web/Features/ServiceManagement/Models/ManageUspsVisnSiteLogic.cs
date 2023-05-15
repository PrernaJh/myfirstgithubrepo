using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Web.Features.Models;
using System;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
    public class ManageUspsVisnSiteLogic : IBusinessLogic<ExcelWorkSheet>
    {
        static string[] requiredColumns =
        {
            "VISN",
            "SiteParent",
            "SiteNumber",
            "SiteType",
            "SiteName",
            "SiteAddress1",
            "SiteAddress2",
            "SiteCity",
            "SiteState",
            "SiteZipCode",
            "SitePhone",
            "SiteShippingContact"
        };

        public bool IsValid(ExcelWorkSheet t)
        {
            bool isValid = true;
            if (t.RowCount > 0)
            {
                foreach (var requiredColumn in requiredColumns)
                {
                    if (StringHelper.DoesNotExist(t.GetStringValue(t.HeaderRow, requiredColumn)))
                        isValid = false;
                }
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }
    }
}

