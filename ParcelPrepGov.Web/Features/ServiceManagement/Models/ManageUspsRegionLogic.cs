﻿using PackageTracker.Domain.Utilities;

namespace ParcelPrepGov.Web.Features.Models
{
    public class ManageUspsRegionLogic : IBusinessLogic<ExcelWorkSheet>
    {
        static string[] requiredColumns =
        {
            "Zip Code 3 Zip",
            "SCF",
            "Postal District",
            "Postal Area"
        };

        public bool IsValid(ExcelWorkSheet t)
        {
            bool isValid = true;
            if (t.RowCount> 0)
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