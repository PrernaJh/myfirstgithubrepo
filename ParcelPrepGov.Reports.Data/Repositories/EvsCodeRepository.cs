using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
	public class EvsCodeRepository : DatasetRepository, IEvsCodeRepository
	{
		private readonly ILogger<EvsCodeRepository> logger;
        private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;
        private readonly IPpgReportsDbContextFactory factory;

		public EvsCodeRepository(
			ILogger<EvsCodeRepository> logger,
			IBlobHelper blobHelper,
			IConfiguration config,
			IDbConnection connection,
			IPpgReportsDbContextFactory factory) : base(config, connection, factory)
		{
			this.logger = logger;
            this.blobHelper = blobHelper;
            this.config = config;
			this.factory = factory;
		}

		public async Task<IList<EvsCode>> GetEvsCodesAsync()
		{
			using (var context = factory.CreateDbContext())
			{
				return await context.EvsCodes.AsNoTracking().ToListAsync();
			}
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
                var evsCodes = new List<EvsCode>();
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    evsCodes.Add(new EvsCode
                    {
                        CreateDate = now,
                        Code = ws.GetStringValue(row, "Code"),
                        Description = ws.GetStringValue(row, "Description"),
                        IsStopTheClock = ws.GetIntValue(row, "IsStopTheClock"),
                        IsUndeliverable = ws.GetIntValue(row, "IsUndeliverable")
                    });
                }

                await ExecuteBulkInsertAsync(evsCodes);
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
            await EraseOldDataAsync<EvsCode>(cutoff);
        }

        /// <summary>
        /// sql insert routine
        /// </summary>
        /// <param name="evsCodes"></param>
        /// <returns></returns>
        public async Task ExecuteBulkInsertAsync(List<EvsCode> evsCodes)
        {
            await ExecuteBulkInsertAsync<EvsCode>(evsCodes);
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

