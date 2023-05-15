using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MMS.Web.Domain.Interfaces
{
    public interface IZipOverrideWebProcessor
    {
        Task<List<ZipOverride>> GetZipOverridesByActiveGroupIdAsync(string activeGroupId);

        Task<FileImportResponse> ImportZipOverrides(List<ZipOverride> zipOverrides, string username, string name, string startDate, string filename = null);
    }
}
