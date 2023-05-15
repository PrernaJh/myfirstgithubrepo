using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface IZoneMapsWebProcessor
    {
        Task<List<ZoneMap>> GetZoneMapsByActiveGroupIdAsync(string activeGroupId);

        Task<FileImportResponse> ImportZoneMaps(List<ZoneMap> zoneMaps, string username, string name, string startDate, string filename = null);

    }
}
