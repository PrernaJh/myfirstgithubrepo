using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IUserLookupRepository
    {
        Task UpsertUser(UserLookup user);
    }
}
