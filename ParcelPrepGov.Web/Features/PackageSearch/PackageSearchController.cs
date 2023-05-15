using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Web.Features.PackageSearch.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PackageTracker.Data.Models;

namespace ParcelPrepGov.Web.Features.PackageSearch
{
    [Authorize(PPGClaim.WebPortal.PackageManagement.PackageSearch)]
    public class PackageSearchController : Controller
    {
        private readonly ILogger<PackageSearchController> logger;
        private readonly IPackageSearchProcessor packageSearchProcessor;        
        private readonly IPackageDatasetRepository packageDataRepository;
        private readonly IPackageInquiryRepository packageInquiryRepository;
        private readonly IBinDatasetRepository binDatasetRepository;
        private readonly IShippingContainerDatasetRepository shippingContainerDatasetRepository;
        private readonly IPackageEventDatasetRepository packageEventDatasetRepository;
        private readonly IJobDatasetRepository jobDatasetRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public PackageSearchController(ILogger<PackageSearchController> logger,
            IPackageSearchProcessor packageSearchProcessor,            
            IPackageDatasetRepository packageDataRepository,
            IPackageInquiryRepository packageInquiryRepository,
            IShippingContainerDatasetRepository shippingContainerDatasetRepository,
            IBinDatasetRepository binDatasetRepository, 
            IPackageEventDatasetRepository packageEventDatasetRepository,
            IJobDatasetRepository jobDatasetRepository,
            UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.packageSearchProcessor = packageSearchProcessor ?? throw new ArgumentNullException(nameof(packageSearchProcessor));            
            this.packageInquiryRepository = packageInquiryRepository;
            this.packageDataRepository = packageDataRepository;
            this.binDatasetRepository = binDatasetRepository;
            this.shippingContainerDatasetRepository = shippingContainerDatasetRepository;
            this.packageEventDatasetRepository = packageEventDatasetRepository;
            this.jobDatasetRepository = jobDatasetRepository;
            _userManager = userManager;
            _config = config;
        }

        public IActionResult Index(string packageId, string barcode)
        {
            var model = new PackageSearchViewModel
            {
                PackageId = packageId,
                Barcode = barcode
            };

            return View(model);
        }

        /// <summary>
        /// get packageid's from a file
        /// as to not overflow the querystring limitation
        /// </summary>
        /// <param name="searchFile"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> GetValues(IFormFile searchFile, string values)
        {
            if (!string.IsNullOrEmpty(values) && searchFile == null)
            {
                return values;
            }
            using var stream = new MemoryStream();
            await searchFile.CopyToAsync(stream);
            stream.Position = 0;
            var Ids = await ParseIdsFromStreamAsync(stream); // i could just read the line from the stream but i chose not to trust the file
            string search = string.Join(",", Ids);
            return search;
        }

        /// <summary>
        /// export data to xlsx 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Export(PackageSearchViewModel model)
        {
            var url = _config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value;
            var item = await packageDataRepository.ExportPackageDataToSpreadSheet(Request.Host.Value, url , model.Ids.Trim().Replace(" ", ","));
            return base.File(item, "application/ms-excel");
        }

        [HttpPost]
        public async Task<IActionResult> SearchBinDataSet(string activeGroupId, string binCode)
        {
            var result = await binDatasetRepository.GetBinDatasetsAsync(activeGroupId, binCode);
             
            return Json(new
            {
                success = true,
                data = result
            });
        }
        /// <summary>
        /// search by file
        /// </summary>
        /// <param name="searchFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IList<PackageSearchExportModel>> SearchByFile(IFormFile searchFile)
        {
            if (searchFile != null)
            {
                using var stream = new MemoryStream();
                await searchFile.CopyToAsync(stream);
                stream.Position = 0;
                var Ids = await ParseIdsFromStreamAsync(stream); // i could just read the line from the stream but i chose not to trust the file
                string search = string.Join(",", Ids);
                search = search.Replace(" ", string.Empty);

                var url = _config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value;
                var data = await packageDataRepository.GetPackageDataForExportAsync(search, url);
                await AddGridInquiryUrl(data);

                return data;
            }
            else
            {
                return null;
            }
        }

        [HttpPost(Name = nameof(Search))]
        public async Task<IList<PackageSearchExportModel>> Search([FromBody] PackageSearchViewModel model)
        {
            var url = _config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value;
            string fixedString = model.Ids.Trim().Replace(" ", ",");

            var data = await packageDataRepository.GetPackageDataForExportAsync(fixedString, url);

            await AddGridInquiryUrl(data);

            return data;
        }

        private async Task AddGridInquiryUrl(IList<PackageSearchExportModel> data)
        {
            if (User.IsClientWebPackageSearchUser())
            {
                //ClientWebPackageSearchUser role should not have access to inquiries.
                foreach (var d in data)
                {
                    d.INQUIRY_ID = string.Empty;
                    d.INQUIRY_ID_HYPERLINK_UIONLY = string.Empty;
                }
            }
            else
            {
                var helpDeskLoginUtil = new PackageTracker.Domain.Utilities.HelpDeskAutoLoginUtility(_userManager, _config);
                var user = _userManager.GetUserAsync(User).Result;

                var secret = _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value;
                foreach (var d in data)
                {
                    if (d.INQUIRY_ID != null)
                    {
                        d.INQUIRY_ID_HYPERLINK_UIONLY = await helpDeskLoginUtil.GenerateHelpDeskUrlAsync(user.UserName, user.Email, secret, d.INQUIRY_ID, null, null, null, null, d.CARRIER, false);
                    }
                    else
                    {
                        d.INQUIRY_ID_HYPERLINK_UIONLY = await helpDeskLoginUtil.GenerateHelpDeskUrlAsync(user.UserName, user.Email, secret, d.INQUIRY_ID, 
                            d.PACKAGE_ID, d.TRACKING_NUMBER, d.SiteName, d.ID, d.SHIPPING_CARRIER, false);
                    }
                }
            }
        }

        private static TrackPackageResultViewModel TransformPackageEvent(PackageSearchEvent packageEvent, PackageDataset packageDataset)
        {
            return new TrackPackageResultViewModel
            {
                ShippingCarrier = "FSC INTERNAL",
                TrackingNumber = packageEvent.TrackingNumber ?? string.Empty,
                EventDate = packageEvent.LocalEventDate,
                EventCode = packageEvent.EventType,
                EventDescription = $"{packageEvent.EventStatus}: {packageEvent.Description}",
                EventLocation = $"{packageEvent.SiteName}: {packageEvent.MachineId}",
                EventZip = packageDataset == null ? string.Empty : packageDataset.SiteZip,
                Username = packageEvent.Username,
                DisplayName = packageEvent.DisplayName ?? string.Empty
            };
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(SingleSearch))]
        public async Task<IActionResult> SingleSearch([FromBody] PackageSearchViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Ids))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Package Id"
                    });
                }

                var packageDatasets = await packageSearchProcessor.GetPackageDatasetsByIdOrBarcodeAsync(model.Ids.Trim());
                var trackPackageDatasets = await packageSearchProcessor.GetTrackingDataForPackageDatasetsAsync(packageDatasets);
                var internalEvents = await packageEventDatasetRepository.GetEventDataForPackageDatasetsAsync(packageDatasets);               

                if (packageDatasets.Any())
                {
                    var results = new List<PackageSearchResultViewModel>();
                    foreach (var packageDataset in packageDatasets)
                    {
                        var internalEventsForPackage = internalEvents.Where(x => x.PackageDatasetId == packageDataset.Id);
                        var visn = await packageSearchProcessor.GetVisnSiteForPackageDatasetAsync(packageDataset);
                        var externalEventsForPackage = trackPackageDatasets
                            .Where(t => t.PackageDatasetId == packageDataset.Id && t.SiteName == packageDataset.SiteName)
                            .Select(x => new TrackPackageResultViewModel()
                            {
                                EventCode = x.EventCode,
                                EventDate = x.EventDate,
                                EventDescription = x.EventDescription,
                                EventLocation = x.EventLocation,
                                EventZip = x.EventZip,
                                ShippingCarrier = x.ShippingCarrier,
                                ShippingContainerId = x.ShippingContainerId,
                                TrackingNumber = x.TrackingNumber,
                                Username = string.Empty,
                                DisplayName = string.Empty
                            }).ToList();

                        externalEventsForPackage.AddRange(internalEventsForPackage.Select(e => TransformPackageEvent(e, packageDataset)));
                        
                        results.Add(await PackageDatasetToModelAsync(packageDataset, visn, externalEventsForPackage));
                    }
                    return Json(new
                    {
                        success = true,
                        data = results
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Package Not Found"
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on PackageSearch for ID: {model.Ids}. Exception: {ex}");
                return Json(new
                {
                    success = false,
                    message = "Failed to retrieve package information"
                });
            }
        }


        /// <summary>
        /// hashset data of ids from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<HashSet<string>> ParseIdsFromStreamAsync(Stream stream)
        {
            var packageIds = new HashSet<string>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var parts = line.Split(',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        packageIds.Add(parts[i]);
                    }
                }
            }
            return packageIds;
        }

        /// <summary>
        /// utility method
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private static string GenerateDisplayAddress(PackageDataset package)
        {
            var res = $"{package.AddressLine1}";
            if (StringHelper.Exists(package.AddressLine2))
            {
                if (StringHelper.Exists(package.AddressLine3))
                {
                    res += $" {package.AddressLine2} {package.AddressLine3}";
                }

                res += $" {package.AddressLine2}";
            }
            return res += $" {package.City} {package.State} {package.Zip}";
        }

        /// <summary>
        /// dto candidate
        /// </summary>
        /// <param name="packageDataset"></param>
        /// <param name="visn"></param>
        /// <param name="trackPackages"></param>
        /// <returns></returns>
        private async Task<PackageSearchResultViewModel> PackageDatasetToModelAsync(PackageDataset packageDataset, VisnSite visn, List<TrackPackageResultViewModel> trackPackages)
        {
            var shippingContainer = (await shippingContainerDatasetRepository.GetDatasetsByContainerIdAsync(new List<TrackPackage>()
                {
                    new TrackPackage
                    {
                    ShippingCarrier = packageDataset.ShippingCarrier,
                    TrackingNumber = packageDataset.ContainerId,
                    SiteName = packageDataset.SiteName
                    }
                }))
                .ToList()
                .FirstOrDefault(
                    c => c.BinCode == packageDataset.BinCode 
                        && c.ContainerId == packageDataset.ContainerId 
                        && c.BinActiveGroupId == packageDataset.BinGroupId);

            var bindata = await binDatasetRepository.GetBinDatasetsAsync(packageDataset.BinGroupId, packageDataset.BinCode);
            var binCodeDescription = bindata == null ? string.Empty :
                (shippingContainer != null
                    ? (shippingContainer.IsSecondaryCarrier ? bindata.DropShipSiteDescriptionSecondary : bindata.DropShipSiteDescriptionPrimary) + " - " +
                    (shippingContainer.IsSecondaryCarrier ? bindata.DropShipSiteCszSecondary : bindata.DropShipSiteCszPrimary)
                    : bindata.DropShipSiteDescriptionPrimary + " - " + bindata.DropShipSiteCszPrimary
                );

            var helpDeskLoginUtil = new PackageTracker.Domain.Utilities.HelpDeskAutoLoginUtility(_userManager, _config);
            var user = await _userManager.GetUserAsync(User);
            var inquiry = await packageInquiryRepository.GetPackageInquiryAsync(packageDataset.Id);
            var jobBarcode = await jobDatasetRepository.GetJobBarcodeByCosmosId(packageDataset.JobId);
            var secret = _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value;

            var result = new PackageSearchResultViewModel
            {
                PackageId = packageDataset.PackageId,
                InquiryId = inquiry != null ? inquiry.InquiryId.ToString() : null,
                PackageDatasetId = packageDataset.Id,
                BinGroupId = packageDataset.BinGroupId,
                ServiceRequestNumber = inquiry != null ? inquiry.ServiceRequestNumber : null,
                FscJob = jobBarcode,
                Barcode = packageDataset.ShippingBarcode,
                RecipientName = packageDataset.RecipientName,
                RecipientAddress = GenerateDisplayAddress(packageDataset),
                Carrier = packageDataset.ShippingCarrier,
                Type = packageDataset.ShippingMethod,
                Weight = packageDataset.Weight,
                CreateDate = packageDataset.CosmosCreateDate.ToString("MM/dd/yyyy"),
                ShippingCarrier = packageDataset.ShippingCarrier,
                ShippingMethod = packageDataset.ShippingMethod,
                AddressLine1 = packageDataset.AddressLine1,
                AddressLine2 = packageDataset.AddressLine2,
                AddressLine3 = packageDataset.AddressLine3,
                City = packageDataset.City,
                Zip = packageDataset.Zip,
                ShippingBarcode = packageDataset.HumanReadableBarcode ?? packageDataset.ShippingBarcode,
                ShippingBarcodeHyperlink = PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(packageDataset.ShippingCarrier,
                    packageDataset.HumanReadableBarcode ?? packageDataset.ShippingBarcode),
                ProcessedDate = packageDataset.LocalProcessedDate.ToString("MM/dd/yyyy"),
                PackageStatus = packageDataset.PackageStatus,
                BinCode = packageDataset.BinCode,
                SiteName = packageDataset.SiteName,
                Zone = packageDataset.Zone,
                ServiceLevel = packageDataset.ServiceLevel,
                IsPoBox = packageDataset.IsPoBox,
                IsRural = packageDataset.IsRural,
                IsOrmd = packageDataset.IsOrmd,
                IsUpsDas = packageDataset.IsUpsDas,
                IsSaturday = packageDataset.IsSaturday,
                IsOutside48States = packageDataset.IsOutside48States,
                IsDduScfBin = packageDataset.IsDduScfBin,
                MailCode = packageDataset.MailCode,
                MarkupType = packageDataset.MarkUpType,
                CosmosCreateDate = packageDataset.CosmosCreateDate.ToString("MM/dd/yyyy"),
                State = packageDataset.State,
                ContainerId = packageDataset.ContainerId,
                MedicalCenterId = visn.SiteNumber,
                MedicalCenterName = visn.SiteName,
                MedicalCenterAddress1 = visn.SiteAddress1,
                MedicalCenterAddress2 = visn.SiteAddress2,
                MedicalCenterCsz = $"{visn.SiteCity}, {visn.SiteState} {visn.SiteZipCode}",
                BinCodeDescription = binCodeDescription,
                ClientName = packageDataset.ClientName,
                LastKnownDate = packageDataset.LastKnownEventDate.GetMonthDateYearWithAmPm(),
                LastKnownDescription = packageDataset.LastKnownEventDescription,
                LastKnownLocation = packageDataset.LastKnownEventLocation,
                LastKnownZip = packageDataset.LastKnownEventZip,
                StopTheClockDate = packageDataset.StopTheClockEventDate.GetMonthDateYearWithAmPm(),
                IsStopTheClock = packageDataset.IsStopTheClock.HasValue ? packageDataset.IsStopTheClock.Value == 1 : false,
                IsUndeliverable = packageDataset.IsUndeliverable.HasValue ? packageDataset.IsUndeliverable.Value == 1 : false
            };
          
            if (result.PackageStatus != EventConstants.Processed)
            {
                result.ShippingCarrier = string.Empty;
                result.ShippingMethod = string.Empty;
                result.ShippingBarcode = string.Empty;
                result.ServiceLevel = string.Empty;
                result.ProcessedDate = string.Empty;
                result.BinCode = string.Empty;
            }

            trackPackages.ForEach(x => result.PackageTracking.Add(new TrackPackageResultViewModel
            {                
                ShippingContainerId = x.ShippingContainerId ?? string.Empty,
                ShippingCarrier = x.ShippingCarrier ?? string.Empty,
                TrackingNumber = x.TrackingNumber ?? string.Empty,
                EventDate = x.EventDate,
                EventCode = x.EventCode ?? string.Empty,
                EventDescription = x.EventDescription ?? string.Empty,
                EventLocation = x.EventLocation ?? string.Empty,
                EventZip = x.EventZip ?? string.Empty,
                Username = x.Username ?? string.Empty,
                DisplayName = x.DisplayName ?? string.Empty
            }));


            if (result.InquiryId == null)
            {
                result.InquiryId = "Create new inquiry";
            }

            result.InquiryIdHyperLink = await helpDeskLoginUtil.GenerateHelpDeskUrlAsync(user.UserName, user.Email, secret, 
                    result.InquiryId, result.PackageId, result.ShippingBarcode, result.SiteName, result.PackageDatasetId, 
                    result.Carrier, false);
          

            return result;
        }

    }
}