using MMS.Web.Domain.Models;
using PackageTracker.Domain.Models.FileManagement;
using PackageTracker.Domain.Models.FileProcessing;
using System.Threading.Tasks;


namespace MMS.Web.Domain.Interfaces
{
    public interface IBinWebProcessor
    {
        Task<FileImportResponse> ImportBinsAndBinMaps(ImportBinsAndBinMapsRequest request);
        Task<GetBinsResponse> GetActiveBinsByActiveGroupIdAsync(string activeGroupId);
        Task<GetBinMapsResponse> GetActiveBinMapsByActiveGroupIdAsync(string activeGroupId);
    }
}
