using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PackageTracker.Domain.Models.FileProcessing;
using Microsoft.Extensions.Configuration;
using PackageTracker.Domain.Utilities;
using System.Data;

namespace ParcelPrepGov.Reports.Repositories
{
    public class PostalAreaAndDistrictRepository : DatasetRepository, IPostalAreaAndDistrictRepository
    {
        private readonly ILogger<PostalAreaAndDistrictRepository> logger;
        private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;

        public PostalAreaAndDistrictRepository(
            ILogger<PostalAreaAndDistrictRepository> logger,
            IBlobHelper blobHelper,
            IConfiguration config,
            IDbConnection connection,
            IPpgReportsDbContextFactory factory) : base(config, connection, factory)
        {
            this.logger = logger;
            this.blobHelper = blobHelper;
            this.config = config;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="fileDetails"></param>
        /// <param name="ws"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<FileImportResponse> UploadExcelAsync(IFormFile fileDetails, ExcelWorkSheet ws, string userName)
        {
            var importResponse = new FileImportResponse();
            var fileArchivePath = config.GetSection("UspsFileImportArchive").Value;
            var archiveFileName = blobHelper.GenerateArchiveFileName(fileDetails.FileName);
            try
            {
                await blobHelper.UploadExcelToBlobAsync(ws, fileArchivePath, archiveFileName);
                importResponse.IsSuccessful = true;
                importResponse.Name = archiveFileName;
            }
            catch (Exception ex)
            {
                importResponse.IsSuccessful = false;
                importResponse.Message = $"Exception while archiving blob {archiveFileName}";
                logger.LogError($"Username: {userName} : {ex.Message}");
                return importResponse;
            }
            try
            {
                DateTime now = DateTime.Now;
                var postalAreasAndDistricts = new List<PostalAreaAndDistrict>();
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    postalAreasAndDistricts.Add(new PostalAreaAndDistrict
                    {
                        CreateDate = now,
                        ZipCode3Zip = ws.GetFormattedIntValue(row, "Zip Code 3 Zip", 3),
                        Scf = ws.GetIntValue(row, "SCF"),
                        PostalDistrict = ws.GetStringValue(row, "Postal District"),
                        PostalArea = ws.GetStringValue(row, "Postal Area")
                    });
                }

                await ExecuteBulkInsertAsync(postalAreasAndDistricts);
                await EraseOldDataAsync(now);
            }
            catch (Exception ex)
            {
                importResponse.IsSuccessful = false;
                importResponse.Message = $"Exception updating reports DB";
                logger.LogError($"Username: {userName} : {ex.Message}");
            }
            return importResponse;
        }

        /// <summary>
        /// sql delete routine
        /// </summary>
        /// <param name="cutoff"></param>
        /// <returns></returns>
        public async Task EraseOldDataAsync(DateTime cutoff)
        {
            await EraseOldDataAsync<PostalAreaAndDistrict>(cutoff);
        }

        /// <summary>
        /// sql insert routine
        /// </summary>
        /// <param name="postalAreasAndDistricts"></param>
        /// <returns></returns>
        public async Task ExecuteBulkInsertAsync(List<PostalAreaAndDistrict> postalAreasAndDistricts)
        {
            await ExecuteBulkInsertAsync<PostalAreaAndDistrict>(postalAreasAndDistricts);
        }

        /// <summary>
        /// download blob from archive
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            return await blobHelper.DownloadBlobAsByteArray(config.GetSection("UspsFileImportArchive").Value, fileName); 
        }
    }
}
