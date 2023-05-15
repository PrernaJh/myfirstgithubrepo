using EFCore.BulkExtensions;
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
	public class VisnSiteRepository : DatasetRepository, IVisnSiteRepository
	{
		private readonly ILogger<VisnSiteRepository> logger;
		private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;
        private readonly IPpgReportsDbContextFactory factory;

		public VisnSiteRepository(
			ILogger<VisnSiteRepository> logger,
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
                var visnSites = new List<VisnSite>();
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    visnSites.Add(new VisnSite
                    {
                        CreateDate = now,
                        Visn = ws.GetStringValue(row, "VISN"),
                        SiteParent = ws.GetStringValue(row, "SiteParent"),
                        SiteNumber = ws.GetStringValue(row, "SiteNumber"),
                        SiteType = ws.GetStringValue(row, "SiteType"),
                        SiteName = ws.GetStringValue(row, "SiteName"),
                        SiteAddress1 = ws.GetStringValue(row, "SiteAddress1"),
                        SiteAddress2 = ws.GetStringValue(row, "SiteAddress2"),
                        SiteCity = ws.GetStringValue(row, "SiteCity"),
                        SiteState = ws.GetStringValue(row, "SiteState"),
                        SiteZipCode = ws.GetStringValue(row, "SiteZipCode"),
                        SitePhone = ws.GetStringValue(row, "SitePhone"),
                        SiteShippingContact = ws.GetStringValue(row, "SiteShippingContact")
                    });
                }

                await ExecuteBulkInsertAsync(visnSites);
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
            await EraseOldDataAsync<VisnSite>(cutoff);
        }

        /// <summary>
        /// sql insert routine
        /// </summary>
        /// <param name="VisnSites"></param>
        /// <returns></returns>
        public async Task ExecuteBulkInsertAsync(List<VisnSite> VisnSites)
        {
            await ExecuteBulkInsertAsync<VisnSite>(VisnSites);
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


        public async Task<VisnSite> GetVisnSiteForPackageDatasetAsync(PackageDataset packageDataset)
        {
			VisnSite result = null;
			using (var context = factory.CreateDbContext())
			{
                var visnSiteParent = PackageIdUtility.GetVisnSiteParentId(packageDataset.ClientName, packageDataset.PackageId);
				result = await context.VisnSites.AsNoTracking().
					FirstOrDefaultAsync(v => v.SiteParent == visnSiteParent.ToString());
			}
			return result ?? new VisnSite();
		}
    }
}

