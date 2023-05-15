using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class PostalDaysRepository : DatasetRepository, IPostalDaysRepository
    {
        private readonly ILogger<PostalDaysRepository> logger;
        private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;
        private readonly IPpgReportsDbContextFactory factory;

        private readonly IDictionary<DateTime, int> postalDays = new Dictionary<DateTime, int>(); // Date => Ordinal

        public PostalDaysRepository(
            ILogger<PostalDaysRepository> logger,
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

        private void LoadPostalDays()
        {
            if (postalDays.Count == 0)
            {
                var postalItems = GetPostalDaysAsync().GetAwaiter().GetResult().ToList();
                // build dictionary and set row
                postalItems.ForEach(p => postalDays[p.PostalDate] = p.Ordinal);
            }
        }

        public int CalculateCalendarDays(DateTime stopTheClockEventDate, DateTime manifestDate)
        {
            var diff = (stopTheClockEventDate.Date - manifestDate.Date).TotalDays;
            if (diff < 0)
                return 0;

            return (int)diff;
        }

        /// <summary>
        /// if either of these 2 dates are not in postal days table then default to calendar days
        /// </summary>
        public int CalculatePostalDays(DateTime stopTheClockEventDate, DateTime manifestDate, string shippingMethod)
        {
            // get all the days into a dictionary to use
            LoadPostalDays();
            var diff = -1;
            if (postalDays.TryGetValue(manifestDate.Date, out var start))
            {
                // we got it
                if (postalDays.TryGetValue(stopTheClockEventDate.Date, out var end))
                {
                    diff = end - start;
                    if (diff > 0 && (
                            shippingMethod == ShippingMethodConstants.UspsParcelSelectLightWeight ||
                            shippingMethod == ShippingMethodConstants.UspsParcelSelect
                            )
                        )
                    {
                        // if PS we count 1 day less
                        diff--;
                    }
                }
            }
            if (diff < 0) // means table needs to be updated 
            {
                // if either of these 2 dates are not in postal days table then default to calendar days
                diff = CalculateCalendarDays(stopTheClockEventDate, manifestDate);
            }
            return diff;
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
                var postalDays = new List<PostalDays>();
                var nextDate = new DateTime();
                var holiday = new DateTime();
                int ordinal = 0;
                for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                {
                    holiday = ws.GetDateValue(row, "Date");
                    if (nextDate.Year == 1)
                        nextDate = new DateTime(holiday.Year, 1, 1);
                    while (nextDate < holiday)
                    {
                        postalDays.Add(new PostalDays
                        {
                            PostalDate = nextDate,
                            Ordinal = nextDate.DayOfWeek == DayOfWeek.Sunday ? ordinal : ordinal++,
                            IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                            CreateDate = now
                        });
                        nextDate = nextDate.AddDays(1);
                    }
                    postalDays.Add(new PostalDays
                    {
                        PostalDate = nextDate,
                        Ordinal = ordinal,
                        Description = ws.GetStringValue(row, "Description"),
                        IsHoliday = 1,
                        IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                        CreateDate = now
                    }); ;
                    nextDate = nextDate.AddDays(1);
                }
                while (nextDate.Year == holiday.Year)
                {
                    postalDays.Add(new PostalDays
                    {
                        PostalDate = nextDate,
                        Ordinal = nextDate.DayOfWeek == DayOfWeek.Sunday ? ordinal : ordinal++,
                        IsSunday = nextDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                        CreateDate = now
                    });
                    nextDate = nextDate.AddDays(1);
                }

                await ExecuteBulkInsertAsync(postalDays);
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
            await EraseOldDataAsync<PostalDays>(cutoff);
        }

        /// <summary>
        /// sql insert routine
        /// </summary>
        /// <param name="postalDays"></param>
        /// <returns></returns>
        public async Task ExecuteBulkInsertAsync(List<PostalDays> postalDays)
        {
            await ExecuteBulkInsertAsync<PostalDays>(postalDays);
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

        public async Task<IList<PostalDays>> GetPostalDaysAsync()
        {
            try
            {
                using (var context = factory.CreateDbContext())
                {
                    return await context.PostalDays.AsNoTracking().ToListAsync();
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"There was an issue retreiving postal days ..  {ex.Message}");
                throw;
            }
        }
    }
}

