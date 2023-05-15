using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Interfaces;
using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement
{
    [Authorize(Roles = PPGRole.SystemAdministrator + "," + PPGRole.Administrator + "," + PPGRole.FSCWebFinancialUser + "," + PPGRole.TransportationUser)]
    public partial class ServiceManagementController : Controller
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IBinWebProcessor binWebProcessor;
        private readonly IEvsCodeRepository evsCodeRepository;
        private readonly IGeoDescriptorsWebProcessor geoDescriptorsWebProcessor;
        private readonly ILogger<ServiceManagementController> logger;
        private readonly IMapper mapper;
        private readonly IPostalAreaAndDistrictRepository postalAreaAndDistrictRepository;
        private readonly IPostalDaysRepository postalDaysRepository;
        private readonly IRateWebProcessor rateWebProcessor;
        private readonly IServiceRuleWebProcessor serviceRuleWebProcessor;
        private readonly IServiceRuleExtensionWebProcessor serviceRuleExtensionProcessor;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;
        private readonly IVisnSiteRepository visnSiteRepository;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IWebJobRunDatasetRepository webJobRunDatasetRepository;
        private readonly IZipOverrideWebProcessor zipOverrideWebProcessor;
        private readonly IZoneMapsWebProcessor zoneMapsWebProcessor;

        public ServiceManagementController(
            IActiveGroupProcessor activeGroupProcessor,
            IBinWebProcessor binWebProcessor,
            IEvsCodeRepository evsCodeRepository,
            IGeoDescriptorsWebProcessor geoDescriptorsWebProcessor,
            ILogger<ServiceManagementController> logger,
            IMapper mapper,
            IPostalAreaAndDistrictRepository postalAreaAndDistrictRepository,
            IPostalDaysRepository postalDaysRepository,
            IRateWebProcessor rateWebProcessor,
            IServiceRuleWebProcessor serviceRuleWebProcessor,
            IServiceRuleExtensionWebProcessor serviceRuleExtensionProcessor,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor,
            IVisnSiteRepository visnSiteRepository,
            IWebHostEnvironment webHostEnvironment,
            IWebJobRunDatasetRepository webJobRunDatasetRepository,
            IZipOverrideWebProcessor zipOverrideWebProcessor,
            IZoneMapsWebProcessor zoneMapsWebProcessor)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.binWebProcessor = binWebProcessor;
            this.evsCodeRepository = evsCodeRepository;
            this.geoDescriptorsWebProcessor = geoDescriptorsWebProcessor;
            this.logger = logger;
            this.mapper = mapper;
            this.postalAreaAndDistrictRepository = postalAreaAndDistrictRepository;
            this.postalDaysRepository = postalDaysRepository;
            this.rateWebProcessor = rateWebProcessor;
            this.serviceRuleWebProcessor = serviceRuleWebProcessor;
            this.serviceRuleExtensionProcessor = serviceRuleExtensionProcessor;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
            this.visnSiteRepository = visnSiteRepository;
            this.webHostEnvironment = webHostEnvironment;
            this.webJobRunDatasetRepository = webJobRunDatasetRepository;
            this.zipOverrideWebProcessor = zipOverrideWebProcessor;
            this.zoneMapsWebProcessor = zoneMapsWebProcessor;
        }


        [AjaxOnly]
        [HttpGet(Name = nameof(GetLocationLocalDate))]
        public async Task<JsonResult> GetLocationLocalDate([FromQuery] string subClientName)
        {
            var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
            var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
            var result = TimeZoneUtility.GetLocalTime(site.TimeZone).Date;
            return new JsonResult(result);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetSiteLocalDate))]
        public async Task<JsonResult> GetSiteLocalDate([FromQuery] string siteName)
        {
            var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
            var result = TimeZoneUtility.GetLocalTime(site.TimeZone).Date;
            return new JsonResult(result);
        }

        private static string GetStringValue(object value)
        {
            if (value != null)
                return value.ToString().Trim().ToUpper();
            return string.Empty;
        }

        private static int GetIntValue(object value)
        {
            if (value != null)
            {
                if (int.TryParse(value.ToString().Trim(), out int intValue))
                    return intValue;
            }
            return 0;
        }

        private static bool GetBoolValue(object value)
        {
            if (value != null)
            {
                var stringValue = value.ToString().Trim().ToUpper();
                if (bool.TryParse(stringValue, out bool boolValue))
                    return boolValue;
                else if (stringValue.Contains("T"))
                    return true;
            }
            return false;
        }

        private static decimal GetDecimalValue(object value)
        {
            if (value != null)
            {
                if (decimal.TryParse(value.ToString().Trim(), out decimal decimalValue))
                    return decimalValue;
            }
            return 0M;
        }

        private static decimal RoundRate(decimal rate)
        {
            return decimal.Round(rate * 100) / (decimal)100;
        }

        private static decimal GetRateValue(object value)
        {
            return RoundRate(GetDecimalValue(value));
        }

        private async Task<FileImportResponse> UploadNewRates(ExcelWorksheet ws, DateTime startDate, string customerName, bool arePackageRates, string fileName)
        {
            var rates = new List<Rate>();
            var now = DateTime.Now;
            var headerRow = ws.Dimension.Start.Row;
            for (int i = headerRow + 1; i <= ws.Dimension.End.Row; i++)
            {
                if (GetStringValue(ws.Cells[i, 1].Value) == string.Empty)
                {
                    continue;
                }
                var column = 1;
                var rate = new Rate
                {
                    CreateDate = now,
                    Carrier = GetStringValue(ws.Cells[i, column++].Value),
                    Service = RateUtility.AssignServiceTypeMapping(GetStringValue(ws.Cells[i, column++].Value), GetStringValue(ws.Cells[i, 1].Value)),
                    ContainerType = GetStringValue(ws.Cells[i, column++].Value),
                    WeightNotOverOz = GetDecimalValue(ws.Cells[i, column++].Value),
                };
                rate.CostZone1 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone2 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone3 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone4 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone5 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone6 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone7 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone8 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZone9 = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZoneDdu = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZoneScf = GetRateValue(ws.Cells[i, column++].Value);
                rate.CostZoneNdc = GetRateValue(ws.Cells[i, column++].Value);
                if (GetStringValue(ws.Cells[headerRow, column].Value).Contains("48"))
                {
                    rate.CostZoneDduOut48 = GetRateValue(ws.Cells[i, column++].Value);
                    rate.CostZoneScfOut48 = GetRateValue(ws.Cells[i, column++].Value);
                    rate.CostZoneNdcOut48 = GetRateValue(ws.Cells[i, column++].Value);
                }
                rate.ChargeZone1 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone2 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone3 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone4 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone5 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone6 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone7 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone8 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZone9 = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZoneDdu = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZoneScf = GetRateValue(ws.Cells[i, column++].Value);
                rate.ChargeZoneNdc = GetRateValue(ws.Cells[i, column++].Value);
                if (column + 2 <= ws.Dimension.End.Column && GetStringValue(ws.Cells[headerRow, column].Value).Contains("48"))
                {
                    rate.ChargeZoneDduOut48 = GetRateValue(ws.Cells[i, column++].Value);
                    rate.ChargeZoneScfOut48 = GetRateValue(ws.Cells[i, column++].Value);
                    rate.ChargeZoneNdcOut48 = GetRateValue(ws.Cells[i, column++].Value);
                }
                if (column <= ws.Dimension.End.Column)
                {
                    rate.IsRural = GetBoolValue(ws.Cells[i, column++].Value);
                }
                if (column <= ws.Dimension.End.Column)
                {
                    rate.IsOutside48States = GetBoolValue(ws.Cells[i, column++].Value);
                }
                rates.Add(rate);
            }
            FileImportResponse fileImportResponse;
            if (arePackageRates)
                fileImportResponse = await rateWebProcessor.ImportListOfNewRates(rates, startDate.ToShortDateString(), customerName, User.GetUsername(), fileName);
            else
                fileImportResponse = await rateWebProcessor.ImportListOfNewContainerRates(rates, startDate.ToShortDateString(), customerName, User.GetUsername(), fileName);
            return fileImportResponse;
        }
    }
}