using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Web.Features.FileManagement.Models;
using ParcelPrepGov.Web.Features.ServiceManagement.Models;
using ParcelPrepGov.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ParcelPrepGov.Web.Features.ServiceManagement
{
    public partial class ServiceManagementController 
    {
        [Authorize(PPGClaim.WebPortal.ServiceManagement.ManageUspsHolidays)]
        public IActionResult ManageUspsEvsCodes()
        {
            return View(nameof(ManageUspsEvsCodes));
        }

        /// <summary>
        /// create history for grid
        /// upload file to cosmos
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Name = nameof(ImportUspsEvsCodes))]
        public async Task<IActionResult> ImportUspsEvsCodes(ManageUspsHolidaysViewModel model)
        {
            var response = new FileImportResponse();
            _bll = new ManageUspsEvsCodeLogic();
            var webJob = new WebJobRunDataset
            {
                SiteName = SiteConstants.AllSites,
                ClientName = string.Empty,
                SubClientName = string.Empty,
                JobName = "USPS Evs Code File Import",
                JobType = WebJobConstants.UspsEvsCodesImportJobType,
                Username = User.Identity.Name,
            };
            try
            {
                if (ModelState.IsValid)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.UploadFile.CopyToAsync(memoryStream);

                        var ws = new ExcelWorkSheet(memoryStream);
                        if (_bll.IsValid(ws))
                        {
                            response = await evsCodeRepository.UploadExcelAsync(model.UploadFile, ws, User.Identity.Name);
                            webJob.IsSuccessful = response.IsSuccessful;
                            webJob.Message = response.Message;
                            if (response.IsSuccessful)
                            {
                                webJob.FileName = model.UploadFile.FileName;
                                webJob.FileArchiveName = response.Name;
                                webJob.NumberOfRecords = ws.RowCount;
                            }
                        }
                        else
                        {
                            response.IsSuccessful = false;
                            webJob.FileName = model.UploadFile.FileName;
                            webJob.Message = response.Message = "Spreadsheet file is invalid.";
                        }
                    }
                }
                else
                {
                    response.IsSuccessful = false;
                    webJob.FileName = model.UploadFile.FileName;
                    webJob.Message = response.Message = "Model state is invalid.";
                }
                await webJobRunDatasetRepository.AddWebJobRunAsync(webJob);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = "There was a problem importing USPS Evs Codes.";
                logger.LogError($"Username: {User.Identity.Name} Exception while importing USPS Evs Codes: {ex.Message}");
            }
            return new JsonResult(response);
        }

        [Route("{controller}/{action}")]
        public async Task<IActionResult> DownloadUspsEvsCodesFile(DateTime? createDate = null)
        {
            var uspsEvsCodesJobs = await webJobRunDatasetRepository.GetWebJobRunsByJobTypeAsync(WebJobConstants.UspsEvsCodesImportJobType);
            var selectedJob = uspsEvsCodesJobs.FirstOrDefault(j => j.DatasetCreateDate == createDate);
            if (selectedJob == null)
                selectedJob = uspsEvsCodesJobs.FirstOrDefault();
            if (selectedJob != null)
            {
                try
                {
                    var value = await evsCodeRepository.DownloadFileAsync(selectedJob.FileArchiveName);
                    return File(value, "application/ms-excel", selectedJob.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception while downloading {selectedJob.FileName}: {ex.Message}");
                }
            }
            return null;
        }

        [HttpGet(Name = nameof(GetUspsEvsCodesJobHistory))]
        public async Task<JsonResult> GetUspsEvsCodesJobHistory()
        {
            var uspsEvsCodesJobList = new List<UspsEvsJobHistoryModel>();
            var uspsEvsCodesJobs = await webJobRunDatasetRepository.GetWebJobRunsByJobTypeAsync(WebJobConstants.UspsEvsCodesImportJobType);

            foreach (var webJob in uspsEvsCodesJobs)
            {
                uspsEvsCodesJobList.Add(
                    new UspsEvsJobHistoryModel
                    {
                        CreateDate = webJob.DatasetCreateDate,
                        FileName = webJob.FileName,
                        IsSuccessful = webJob.IsSuccessful,
                        JobName = webJob.JobName,
                        ErrorMessage = webJob.Message,
                        Username = webJob.Username,
                    });
            }
            return new JsonResult(uspsEvsCodesJobList);
        }
    }
}
