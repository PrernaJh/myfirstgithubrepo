using Microsoft.AspNetCore.Http;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IFileManager
    { 
        Task<FileImportResponse> UploadExcelAsync(IFormFile fileDetails, ExcelWorkSheet ws, string userName);
        Task<byte[]> DownloadFileAsync(string fileName);
    } 
}
