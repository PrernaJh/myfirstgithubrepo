﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Web.Features.FileManagement.Models;
using ParcelPrepGov.Web.Features.Models;
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
        private IBusinessLogic<ExcelWorkSheet> _bll;

        [Authorize(PPGClaim.WebPortal.ServiceManagement.ManageUspsRegions)]
        public IActionResult ManageUspsRegions()
        {
            return View(nameof(ManageUspsRegions));
        }
        /// <summary>
        /// create history for grid
        /// upload file to cosmos
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Name = nameof(ImportUspsRegions))]
        public async Task<IActionResult> ImportUspsRegions(ManageUspsRegionsViewModel model)
        {
            var response = new FileImportResponse();
            _bll = new ManageUspsRegionLogic();
            var webJob = new WebJobRunDataset
            {
                SiteName = SiteConstants.AllSites,
                ClientName = string.Empty,
                SubClientName = string.Empty,
                JobName = "USPS Regions File Import",
                JobType = WebJobConstants.UspsRegionsImportJobType,
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
                            response = await postalAreaAndDistrictRepository.UploadExcelAsync(model.UploadFile, ws, User.Identity.Name);
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
                response.Message = "There was a problem importing USPS Regions.";
                logger.LogError($"Username: {User.Identity.Name} Exception while importing USPS Regions: {ex.Message}");
            }
            return new JsonResult(response);
        }

        [Route("{controller}/{action}")]
        public async Task<IActionResult> DownloadFile(DateTime? createDate = null)
        {
            var uspsRegionJobs = await webJobRunDatasetRepository.GetWebJobRunsByJobTypeAsync(WebJobConstants.UspsRegionsImportJobType);
            var selectedJob = uspsRegionJobs.FirstOrDefault(j => j.DatasetCreateDate == createDate);
            if (selectedJob == null)
                selectedJob = uspsRegionJobs.FirstOrDefault();
            if (selectedJob != null)
            {
                try
                {
                    var value = await postalAreaAndDistrictRepository.DownloadFileAsync(selectedJob.FileArchiveName);
                    return File(value, "application/ms-excel", selectedJob.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception while downloading {selectedJob.FileName}: {ex.Message}");
                }
            }
            return null;
        }

        [HttpGet(Name = nameof(GetUspsRegionJobHistory))]
        public async Task<JsonResult> GetUspsRegionJobHistory()
        {
            var uspsRegionJobList = new List<UspsRegionJobHistoryModel>();
            var uspsRegionJobs = await webJobRunDatasetRepository.GetWebJobRunsByJobTypeAsync(WebJobConstants.UspsRegionsImportJobType);

            foreach (var webJob in uspsRegionJobs)
            {
                uspsRegionJobList.Add(
                    new UspsRegionJobHistoryModel
                    {
                        CreateDate = webJob.DatasetCreateDate,
                        FileName = webJob.FileName,
                        IsSuccessful = webJob.IsSuccessful,
                        JobName = webJob.JobName,
                        ErrorMessage = webJob.Message,
                        Username = webJob.Username,
                    });
            }
            return new JsonResult(uspsRegionJobList);
        }
    }
}
