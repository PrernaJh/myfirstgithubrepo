using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Web.Features.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
    public class ManageUspsEvsCodeLogic : IBusinessLogic<ExcelWorkSheet>
    {
        static string[] requiredColumns =
        {
            "Code",
            "Description",
            "IsStopTheClock",
            "IsUndeliverable"
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
