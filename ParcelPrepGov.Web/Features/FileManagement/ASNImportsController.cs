using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.FileManagement.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.FileManagement
{

    public partial class FileManagementController
    {

        [Authorize(PPGClaim.WebPortal.FileManagement.AsnImports)]
        public IActionResult ASNImports()
        {
            return View();
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetASNImports))]
        public async Task<JsonResult> GetASNImports([FromQuery] string subClientName)
        {
            var asnImports = new List<ASNImportsModel>();

            try
            {
                if (subClientName == null) return new JsonResult(asnImports);

                var subclient = await subClientRepository.GetSubClientByNameAsync(subClientName);
                var siteName = subclient.SiteName;

                var asnImportWebJobs = await webJobRunProcessor.GetAsnImportWebJobRunsBySubClientAsync(siteName, subClientName);

                foreach (var webJob in asnImportWebJobs)
                {
                    foreach (var fileDetail in webJob.FileDetails)
                    {
                        asnImports.Add(
                            new ASNImportsModel
                            {
                                LocalCreateDate = webJob.LocalCreateDate,
                                CreateDate = webJob.CreateDate,
                                FileName = fileDetail.FileName,
                                IsSuccessful = webJob.IsSuccessful,
                                JobName = webJob.JobName,
                                ErrorMessage = webJob.Message,
                                Username = webJob.Username,
                                NumberOfRecords = fileDetail.NumberOfRecords
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on ANSI Import for ID: {subClientName}. Exception: {ex}");
            }

            return new JsonResult(asnImports);
        }
    }
}
