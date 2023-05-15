using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace PackageTracker.Domain.Utilities
{
    public static class BinUtility
    {        
        public static bool IsSCF(this string binCode)
        {            
            return binCode != null && binCode.StartsWith(BinConstants.SCFSortTypeIdentifier);
        }

        public static bool IsDDU(this string binCode)
        {
            return binCode != null && binCode.StartsWith(BinConstants.DDUSortTypeIdentifier);
        }
    }
}
