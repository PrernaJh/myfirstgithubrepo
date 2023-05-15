using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
    public interface ICleanupService
    {
        Task CleanupEodCollectionsAsync(WebJobSettings webJobSettings);
    }
}
