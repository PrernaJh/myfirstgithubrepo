using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface IGeoDescriptorsWebProcessor
    {
        Task<List<ZipMap>> GetZipMapsByActiveGroupIdAsync(string activeGroupId);

        Task<FileImportResponse> ImportZipMaps(List<ZipMap> zipMaps, string username, string name, string startDate, string filename = null);

    }
}