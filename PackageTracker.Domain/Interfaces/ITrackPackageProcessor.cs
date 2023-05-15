using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Models.TrackPackages.Ups;
using PackageTracker.Domain.Models.TrackPackages.Usps;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
    public interface ITrackPackageProcessor
    {
        Task<(FileReadResponse, List<TrackPackage>)> ImportFedExTrackingFileAsync(WebJobSettings webJobSettings, Stream stream);       
        Task<(XmlImportResponse, List<TrackPackage>)> ImportUpsTrackingDataAsync(WebJobSettings webJobSettings);
        Task<(FileReadResponse, List<TrackPackage>)> ImportUspsTrackingFileAsync(WebJobSettings webJobSettings, Stream stream, int chunk = 0);
        List<TrackPackage> ImportUspsTrackPackageResponse(UspsTrackPackageResponse response, string shippingBarcode);

        Task<List<TrackPackage>> ReadFedExTrackingFileStreamAsync(bool logTrackPackages, Stream stream);
        void ParseUpsTrackingDataAsync(bool logTrackPackages, List<TrackPackage> trackPackages, UpsTrackPackageResponse upsPackages);
        Task<UspsTrackPackageResponse> GetUspsTrackingData(string trackingID, string userId, string sourceId);
        Task<List<TrackPackage>> ReadUspsTrackingFileStreamAsync(bool logTrackPackages, Stream stream, int chunk = 0);
    }
}
