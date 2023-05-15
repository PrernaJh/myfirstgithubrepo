using AutoMapper;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Domain.Interfaces;
using ParcelPrepGov.Web.Features.RecallRelease.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PackageTracker.Identity.Data.Constants;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.AzureExtensions;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using DevExpress.Spreadsheet;
using PackageTracker.Data.Interfaces;
using ParcelPrepGov.Reports.Interfaces;
using PackageTracker.Identity.Data;
using Microsoft.EntityFrameworkCore;
using ParcelPrepGov.Reports.Utility;

namespace ParcelPrepGov.Web.Features.RecallRelease
{
    [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.ReadPackageRecall)]
    public class RecallReleaseController : Controller
    {
        private readonly ILogger<RecallReleaseController> logger;

        private readonly IConfiguration configuration;
        private readonly IEmailConfiguration emailConfiguration;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;
        private readonly IQueueClientFactory queueFactory;
        private readonly IRecallReleaseProcessor recallReleaseProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IPackageRepository packageRepository;
        private readonly IPackageDatasetRepository packageDatasetRepository;
        private readonly PackageTrackerIdentityDbContext identityDbContext;

        public RecallReleaseController(ILogger<RecallReleaseController> logger,
            IConfiguration configuration,
            IEmailConfiguration emailConfiguration,
            IEmailService emailService,
            IMapper mapper,
            IQueueClientFactory queueFactory,
            IRecallReleaseProcessor recallReleaseProcessor,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            IPackageRepository packageRepository,
            IPackageDatasetRepository packageDatasetRepository,
            PackageTrackerIdentityDbContext identityDbContext
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.queueFactory = queueFactory ?? throw new ArgumentNullException(nameof(queueFactory));
            this.recallReleaseProcessor = recallReleaseProcessor ?? throw new ArgumentNullException(nameof(recallReleaseProcessor));
            this.siteProcessor = siteProcessor ?? throw new ArgumentNullException(nameof(siteProcessor));
            this.subClientProcessor = subClientProcessor ?? throw new ArgumentNullException(nameof(subClientProcessor));
            this.packageRepository = packageRepository; // null happens when startup fails to bind repository found in web
            this.packageDatasetRepository = packageDatasetRepository;
            this.identityDbContext = identityDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>        
        /// export data to xlsx 
        /// </summary>   
        [HttpPost]
        public async Task<IActionResult> Export(string subClient)
        {
            try
            {
                var recallData = await recallReleaseProcessor.GetRecalledPackagesAsync(subClient);
                var releaseData = await recallReleaseProcessor.GetReleasedPackagesAsync(subClient);

                var workbook = ParcelPrepGov.Reports.Utility.WorkbookExtensions.CreateWorkbook();                
                var workSheetIndex = 0;

                IEnumerable<RecalledPackageViewModel> recalledData = ToDtoRecall(recallData);
                workbook.ImportDataToWorkSheets(ref workSheetIndex, recalledData);
                Worksheet recallSheet = workbook.Worksheets[0];
                recallSheet.Name = "Recalled Packages";
                recallSheet.DeleteIgnoredColumns<RecalledPackageViewModel>();

                IEnumerable<ReleasedPackageViewModel> releasedData = ToDtoReleased(releaseData);
                workbook.ImportDataToWorkSheets(ref workSheetIndex, releasedData);
                Worksheet releaseSheet = workbook.Worksheets[1];
                releaseSheet.Name = "Released Packages";
                releaseSheet.DeleteIgnoredColumns<ReleasedPackageViewModel>();

                workbook.Calculate();
                   
                byte[] doc = await workbook.SaveDocumentAsync(DocumentFormat.Xlsx);

                string fileName = $"{DateTime.Now.Date:yyyyMMddHHmmss}_{"RecallRelease"}_{DateTime.Now.Date:yyyyMMdd}";
                return File(doc, "application/ms-excel", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in Export Recall/Release is : {ex.Message.Substring(0, 100)}");
                throw;
            }
        }

        private static IEnumerable<RecalledPackageViewModel> ToDtoRecall(IEnumerable<Package> releasedData)
        {
            try
            {
                return (from r in releasedData.AsQueryable()
                        select new RecalledPackageViewModel
                        {
                            JobBarcode = r.JobBarcode,
                            LocalProcessedDate = (r.LocalProcessedDate == null || r.LocalProcessedDate.Year == 1 || r.LocalProcessedDate == DateTime.MinValue) ? string.Empty : r.LocalProcessedDate.ToString(),
                            MailCode = r.MailCode,
                            PackageId = r.PackageId,
                            PackageStatus = r.PackageStatus,
                            RecallStatus = r.RecallStatus,
                            ProcessedDate = (r.ProcessedDate == null || r.ProcessedDate.Year == 1 || r.ProcessedDate == DateTime.MinValue) ? string.Empty : r.ProcessedDate.ToString(),                            
                            SubClientName = r.SubClientName,
                            RecallDate = (r.RecallDate == null || r.RecallDate == DateTime.MinValue) ? string.Empty : r.RecallDate.Value.ToString(),
                            BinCode = r.BinCode,
                            Barcode = r.Barcode,
                            RecipientName = r.RecipientName,
                            AddressLine1 = r.AddressLine1,
                            AddressLine2 = r.AddressLine2,
                            AddressLine3 = r.AddressLine3,
                            City = r.City,
                            State = r.State,
                            Zip = r.Zip,
                            ClientName = r.ClientName,
                            ContainerId = r.ContainerId,
                            ShippingMethod = r.ShippingMethod,
                            ShippingCarrier = r.ShippingCarrier,
                            SiteName = r.SiteName
                        }).ToList();
            }
            catch (Exception)
            {
                throw;

            }
        }
        private static IEnumerable<ReleasedPackageViewModel> ToDtoReleased(IEnumerable<Package> releasedData)
        {
            try
            {
                return (from r in releasedData.AsQueryable()
                        select new ReleasedPackageViewModel
                        {
                            JobBarcode = r.JobBarcode,
                            LocalProcessedDate = (r.LocalProcessedDate == null || r.LocalProcessedDate.Year == 1 || r.LocalProcessedDate == DateTime.MinValue) ? string.Empty : r.LocalProcessedDate.ToString(),
                            MailCode = r.MailCode,
                            PackageId = r.PackageId,
                            PackageStatus = r.PackageStatus,
                            RecallStatus = r.RecallStatus,
                            ProcessedDate = (r.ProcessedDate == null || r.ProcessedDate.Year == 1 || r.ProcessedDate == DateTime.MinValue) ? string.Empty : r.ProcessedDate.ToString(),
                            SubClientName = r.SubClientName,
                            RecallDate = (r.RecallDate == null || r.RecallDate == DateTime.MinValue) ? string.Empty : r.RecallDate.Value.ToString(),
                            BinCode = r.BinCode,
                            Barcode = r.Barcode,
                            RecipientName = r.RecipientName,
                            AddressLine1 = r.AddressLine1,
                            AddressLine2 = r.AddressLine2,
                            AddressLine3 = r.AddressLine3,
                            City = r.City,
                            State = r.State,
                            Zip = r.Zip,
                            ClientName = r.ClientName,
                            ContainerId = r.ContainerId,
                            ShippingMethod = r.ShippingMethod,
                            ShippingCarrier = r.ShippingCarrier,
                            SiteName = r.SiteName
                        }).ToList();
            }
            catch (Exception)
            {
                throw;

            }
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetPackagesFromStatus))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.ReadPackageRelease)]
        public async Task<JsonResult> GetPackagesFromStatus(string subClient, string packageStatus, string startDate, string endDate)
        {
            var packagesFromStatus = await packageDatasetRepository.GetPackagesFromStatusAsync(subClient, packageStatus, startDate, endDate);
            packagesFromStatus.ToList().ForEach(p =>
            { 
                if (DateTime.TryParse(p.LocalProcessedDate, out var dateObj))
                {

                    if (dateObj.Year == 1)
                        p.LocalProcessedDate = string.Empty;
                }
            });
            return new JsonResult(packagesFromStatus);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetRecalledPackages))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.ReadPackageRelease)]
        public async Task<JsonResult> GetRecalledPackages(string subClient)
        {
            if (string.IsNullOrEmpty(subClient))
            {
                return new JsonResult(new { });
            }
            var recalledPackages = await recallReleaseProcessor.GetRecalledPackagesAsync(subClient);

            if (recalledPackages == null || !recalledPackages.Any())
            {
                return new JsonResult(new List<RecalledPackageViewModel>());
            }

            var recalledPackagesViewModel = mapper.Map<IEnumerable<RecalledPackageViewModel>>(recalledPackages);
            recalledPackagesViewModel.ToList().ForEach(p =>
            {
                //DateTime dateObj;
                if (DateTime.TryParse(p.LocalProcessedDate, out var dateObj))
                {

                    if (dateObj.Year == 1)
                        p.LocalProcessedDate = string.Empty;
                }
            });
            return new JsonResult(recalledPackagesViewModel);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetReleasedPackages))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.ReadPackageRelease)]
        public async Task<JsonResult> GetReleasedPackages(string subClient)
        {
            if (string.IsNullOrEmpty(subClient))
            {
                return new JsonResult(new { });
            }
            var releasedPackages = await recallReleaseProcessor.GetReleasedPackagesAsync(subClient);

            if (releasedPackages == null || !releasedPackages.Any())
            {
                return new JsonResult(new List<ReleasedPackageViewModel>());
            }

            var releasedPackagesViewModel = mapper.Map<IEnumerable<ReleasedPackageViewModel>>(releasedPackages);
            releasedPackagesViewModel.ToList().ForEach(p =>
            {
                //DateTime dateObj;
                if (DateTime.TryParse(p.LocalProcessedDate, out var dateObj)) {

                    if (dateObj.Year == 1)
                        p.LocalProcessedDate = string.Empty;
                } 
            });
            return new JsonResult(releasedPackagesViewModel);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(FindPackage))]
        public async Task<JsonResult> FindPackage([FromQuery] DataSourceLoadOptions loadOptions, string subClient)
        {
            JArray filterJArray = (JArray)loadOptions.Filter[0];
            JToken lastVal = filterJArray.Last();
            var partialPackageId = lastVal.ToString();

            var packages = await recallReleaseProcessor.FindPackagesToRecallByPartial(subClient, partialPackageId);

            var recalledPackagesViewModel = mapper.Map<IEnumerable<RecalledPackageViewModel>>(packages);
            recalledPackagesViewModel.ToList().ForEach(p =>
            {
                //DateTime dateObj;
                if (DateTime.TryParse(p.LocalProcessedDate, out var dateObj))
                {

                    if (dateObj.Year == 1)
                        p.LocalProcessedDate = string.Empty;
                }
            });
            return new JsonResult(recalledPackagesViewModel);
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(RecallPackage))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.PackageRecall)]
        public async Task<IActionResult> RecallPackage([FromBody] SubClientPackageModel request)
        {
            try
            {
                var response = await recallReleaseProcessor.ProcessRecallPackageForSubClient(request.PackageId.Trim(), request.SubClient, User.Identity.Name);
                var subClient = await subClientProcessor.GetSubClientByNameAsync(request.SubClient);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var model = new EmailRecallReleaseViewModel(Request.Host.Value, User.Identity.Name, siteLocalTime, 
                    response.PackageIds, response.LockedPackageIds, response.FailedPackageIds, 
                    response.Packages, recallFlag: true);
                await SendRecallPackageJobAsync(model, subClient);
                await SendRecallReleaseEmailAsync(model, site, subClient);

                if (response.IsSuccessful)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"{request.PackageId} Recall Successful"
                    });
                }
                else
                {
                    var message = $"{request.PackageId} Recall failed";
                    if (response.LockedPackageIds.Any())
                    {
                        message += ", package is processed and manifested";
                    }

                    return Json(new
                    {
                        success = false,
                        message = message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(ReleasePackage))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.PackageRelease)]
        public async Task<IActionResult> ReleasePackage([FromBody] SubClientPackageModel request)
        {
            try
            {
                var response = await recallReleaseProcessor.ProcessReleasePackageForSubClient(request.PackageId, request.SubClient, User.Identity.Name);
                var subClient = await subClientProcessor.GetSubClientByNameAsync(request.SubClient);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var model = new EmailRecallReleaseViewModel(Request.Host.Value, User.Identity.Name, siteLocalTime,
                    response.PackageIds, response.LockedPackageIds, response.FailedPackageIds,
                    response.Packages, recallFlag: false);
                await SendRecallReleaseEmailAsync(model, site, subClient);

                if (response.IsSuccessful)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"{request.PackageId} Release Successful"
                    });
                }
                else
                {
                    var message = $"{request.PackageId} Release failed";
                    if (response.LockedPackageIds.Any())
                    {
                        message += ", package is processed and manifested";
                    }
                    return Json(new
                    {
                        success = false,
                        message = message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(RecallFile))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.PackageRecall)]
        public async Task<IActionResult> RecallFile(IFormFile recallFile, string recallFileSubClient)
        {
            try
            {
                string subClientName = recallFileSubClient;
                string[] acceptedExtensions = { ".csv" };

                var fileName = recallFile.FileName.ToLower();
                var isValidExtenstion = acceptedExtensions.Any(ext =>
                {
                    return fileName.LastIndexOf(ext) > -1;
                });

                if (!isValidExtenstion)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Only .csv (comma separated file) allowed."
                    });
                }

                using var stream = new MemoryStream();
                await recallFile.CopyToAsync(stream);
                stream.Position = 0;
                var response = await recallReleaseProcessor.ImportListOfRecallPackagesForSubClient(stream, subClientName, User.Identity.Name);

                var successCount = response.PackageIds.Count();
                var failureCount = response.FailedPackageIds.Count();
                var lockedCount = response.LockedPackageIds.Count();

                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var model = new EmailRecallReleaseViewModel(Request.Host.Value, User.Identity.Name, siteLocalTime,
                    response.PackageIds, response.LockedPackageIds, response.FailedPackageIds,
                    response.Packages, recallFlag: true);
                await SendRecallPackageJobAsync(model, subClient);
                await SendRecallReleaseEmailAsync(model, site, subClient);

                return Json(new
                {
                    success = failureCount == 0 && lockedCount == 0,
                    message = $"Success count: {successCount} Failure count: {failureCount} Processed and manifested count: {lockedCount}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(ReleaseFile))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.PackageRelease)]
        public async Task<IActionResult> ReleaseFile(IFormFile releaseFile, string releaseFileSubClientValue)
        {
            try
            {
                string subClientName = releaseFileSubClientValue;
                string[] acceptedExtensions = { ".csv" };

                var fileName = releaseFile.FileName.ToLower();
                var isValidExtenstion = acceptedExtensions.Any(ext =>
                {
                    return fileName.LastIndexOf(ext) > -1;
                });

                if (!isValidExtenstion)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Only .csv (comma separated file) allowed."
                    });
                }

                using var stream = new MemoryStream();
                await releaseFile.CopyToAsync(stream);
                stream.Position = 0;
                var response = await recallReleaseProcessor.ImportListOfReleasePackagesForSubClient(stream, subClientName, User.Identity.Name);

                var successCount = response.PackageIds.Count();
                var failureCount = response.FailedPackageIds.Count();
                var lockedCount = response.LockedPackageIds.Count();

                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var model = new EmailRecallReleaseViewModel(Request.Host.Value, User.Identity.Name, siteLocalTime,
                    response.PackageIds, response.LockedPackageIds, response.FailedPackageIds,
                    response.Packages, recallFlag: false);

                await SendRecallReleaseEmailAsync(model, site, subClient);

                return Json(new
                {
                    success = failureCount == 0 && lockedCount == 0,
                    message = $"Success count: {successCount} Failure count: {failureCount} Processed and manifested count: {lockedCount}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private async Task SendRecallPackageJobAsync(EmailRecallReleaseViewModel model, SubClient subClient)
        {
            try
            {
                var queueClient = queueFactory.GetClient();
                var queue = queueClient.GetQueueReference(configuration["PackageRecallJobQueue"]);
                int chunk = 1000; // Queue messages are limited to 64K: chunk * 32 (per package ID) * 4/3 (BASE64 encode) < 64K

                for (int offset = 0; offset < model.PackageIds.Count; offset += chunk)
                {
                    var packageIds = model.PackageIds.Skip(offset).Take(chunk);
                    var queueMessage = $"{subClient.Name}\n{model.UserName}\n{string.Join("\n", packageIds)}";
                    await queue.AddMessageAsync(new CloudQueueMessage(queueMessage));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Can't enqueue package recall job: {ex}");
                // TODO: send Tec email, do not need to notify end user of this
            }
        }

        static void AddRecipient(IList<EmailAddress> toAddresses, string name, string address)
        {
            if (StringHelper.Exists(address) &&
                toAddresses.FirstOrDefault(a => string.Equals(a.Address, address, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                toAddresses.Add(new EmailAddress
                {
                    Name = StringHelper.Exists(name) ? name : address,
                    Address = address
                });
            }
        }

        private async Task SendRecallReleaseEmailAsync(EmailRecallReleaseViewModel model, Site site, SubClient subClient, bool recallDeleted = false)
        {
            try
            {
                model.Email = User.GetEmail();
                model.SiteGencoName = subClient.Description;

                string emailHtmlContent = 
                    await this.RenderViewToStringAsync(recallDeleted ? "_EmailRecallDeleted" : 
                            (model.RecallFlag ? "_EmailRecall" : "_EmailRelease"), model);

                if (emailHtmlContent == null)
                {
                    return;
                }

                var fromAddresses = new List<EmailAddress>();
                var fromAddress = new EmailAddress
                {
                    Name = emailConfiguration.SmtpUsername,
                    Address = emailConfiguration.SmtpUsername
                };
                fromAddresses.Add(fromAddress);

                var toAddresses = new List<EmailAddress>();
                foreach (var user in (await identityDbContext.Users
                        .AsNoTracking()
                        .ToListAsync())
                        .Where(u => u.SendRecallReleaseAlerts 
                            && (u.Client == subClient.ClientName || u.Client == IdentityDataConstants.Global)
                            && (u.Site == subClient.SiteName || u.Site == IdentityDataConstants.Global)
                            && (u.SubClient == subClient.Name || u.SubClient == IdentityDataConstants.Global)))
                {
                    AddRecipient(toAddresses, user.UserName, user.Email);
                }
                AddRecipient(toAddresses, model.UserName, model.Email);

                var action = recallDeleted ? "Deleted" : (model.RecallFlag ? "Recalled" : "Released");
                var message = model.PackageIds.Count != 1 ? $"{model.PackageIds.Count} Packages Have Been " : "A Package Has Been";
                EmailMessage msg = new EmailMessage
                {
                    Content = emailHtmlContent,
                    Subject = $"{message} {action}",
                    FromAddresses = fromAddresses,
                    ToAddresses = toAddresses
                };

                List<EmailAttachment> attachments = null;
                if (model.Packages.Count > 0)
                {
                    // Add spreadsheet attachment 
                    var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                    action = model.RecallFlag ? "Recall" : "Release";
                    var fileName = $"{siteLocalTime.ToString("yyyyMMdd_HHmmss")}_{subClient.Name}_Package_{action}.xlsx";
                    var ws = new ExcelWorkSheet($"{subClient.Name} Packages",
                        new string[] {
                            "Package Id",
                            "Status",
                            "Recall Status",
                            "Processed Date",
                            "Container Id",
                            "Bin Code",
                            "Shipping Carrier",
                            "Shipping Method",
                            "Name",
                            "Address",
                            "City",
                            "State",
                            "Zip"
                        }
                    );
                    var dataTypes = new eDataTypes[] {
                        eDataTypes.String, // PackageId
						eDataTypes.String, // PackageStatus
						eDataTypes.String, // RecallStatus
						eDataTypes.String, // LocalProcessedDate
						eDataTypes.String, // ContainerId
						eDataTypes.String, // BinCode
						eDataTypes.String, // ShippingCarrier
						eDataTypes.String, // ShippingMethod
						eDataTypes.String, // RecipientName
						eDataTypes.String, // AddressLine1
						eDataTypes.String, // City
						eDataTypes.String, // State
						eDataTypes.String, // Zip
					};
                    foreach (var package in model.Packages)
                    {
                        var row = ws.RowCount + 1;
                        ws.InsertRow(row,
                            new string[] {
                                package.PackageId,
                                package.PackageStatus,
                                package.RecallStatus,
                                package.LocalProcessedDate.ToString("g"),
                                package.ContainerId,
                                package.BinCode,
                                package.ShippingCarrier,
                                package.ShippingMethod,
                                package.RecipientName,
                                package.AddressLine1,
                                package.City,
                                package.State,
                                package.Zip
                            }
                            , dataTypes);
                        ws.InsertHyperlink($"A{row}", package.PackageId, $"https://{model.Host}/PackageSearch?packageId={package.PackageId}");
                    }
                    attachments = new List<EmailAttachment>();
                    attachments.Add(new EmailAttachment
                    {
                        MimeType = MimeTypeConstants.OPEN_OFFICE_SPREADSHEET,
                        FileName = fileName,
                        FileContents = await ws.GetContentsAsync()
                    });
                }
                await emailService.SendAsync(msg, true, attachments);
            }
            catch (Exception ex)
            {
                logger.LogError($"Can't send package recall/release email: {ex}");
                throw new Exception("Can't send package recall/recall email.", ex);
            }
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(DeleteRecallPackage))]
        [Authorize(Policy = PPGClaim.WebPortal.PackageManagement.DeleteRecallPackage)]
        public async Task<IActionResult> DeleteRecallPackage([FromBody] SubClientPackageModel request)
        {
            try
            {
                var response = await recallReleaseProcessor.ProcessDeleteRecallPackageForSubClient(request.PackageId, request.SubClient, User.Identity.Name);
                var subClient = await subClientProcessor.GetSubClientByNameAsync(request.SubClient);
                var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
                var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                var model = new EmailRecallReleaseViewModel(Request.Host.Value, User.Identity.Name, siteLocalTime,
                    response.PackageIds, response.LockedPackageIds, response.FailedPackageIds,
                    response.Packages, recallFlag: false);
                await SendRecallReleaseEmailAsync(model, site, subClient, true);

                if (response.IsSuccessful)
                { 
                    return Json(new
                    {
                        success = true,
                        message = $"{request.PackageId} Delete Successful"
                    });
                }
                else
                {
                    var message = $"{request.PackageId} Delete failed";
                    if (response.LockedPackageIds.Any())
                    {
                        message += ", package is processed and manifested";
                    }
                    return Json(new
                    {
                        success = false,
                        message = message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}