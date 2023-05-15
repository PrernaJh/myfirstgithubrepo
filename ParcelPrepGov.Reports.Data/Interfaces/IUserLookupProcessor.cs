using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IUserLookupProcessor
    {
        Task UpsertUser(ApplicationUser user);        
    }
}
