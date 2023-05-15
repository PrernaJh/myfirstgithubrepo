using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Reports.Utility;
using ParcelPrepGov.Web.Features.ContainerSearch.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;

using WorkbookExtensions = ParcelPrepGov.Reports.Utility.WorkbookExtensions;


namespace ParcelPrepGov.Web.Features.ContainerSearch
{
    [Authorize(PPGClaim.WebPortal.ContainerManagement.ContainerSearch)]
    public class ContainerSearchController : Controller
    {
        private readonly ILogger<ContainerSearchController> logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShippingContainerDatasetRepository _shippingContainerRepository;
        private readonly IConfiguration _config;
        public ContainerSearchController(ILogger<ContainerSearchController> logger,
            IShippingContainerDatasetRepository shippingContainerDatasetRepository,
            UserManager<ApplicationUser> userManager, 
            IConfiguration config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _shippingContainerRepository = shippingContainerDatasetRepository;
            _userManager = userManager;
            _config = config;
        }

        public IActionResult Index(string containerId)
        {
            var vm = new ContainerSearchViewModel()
            {                
                ContainerId = containerId
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Export(string barcode)
        {
            try
            {
                var role = User.GetRoles().FirstOrDefault();
                var workbook = WorkbookExtensions.CreateWorkbook();
                var worksheetIndex = 0;
                var workSheetsProcessed = 0;
                ContainerSearchResultViewModel result = await _shippingContainerRepository.GetContainerByBarcode(barcode, User.GetSite());

                ContainerSearchResultViewModel.HOST = Request.Host.Value;
                workbook.ImportDataToWorkSheets<ContainerSearchResultViewModel>(ref worksheetIndex, new List<ContainerSearchResultViewModel>() { result });
                workbook.Worksheets[workSheetsProcessed].Name = "Container Information";
                workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<ContainerSearchResultViewModel>(Request.Host.Value);

                workbook.ImportDataToWorkSheets<ContainerEventsViewModel>(ref worksheetIndex, result.EVENTS);
                workbook.Worksheets[workSheetsProcessed].Name = "Container Events";
                workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<ContainerEventsViewModel>(Request.Host.Value, role);

                ContainerSearchPacakgeViewModel.HOST = Request.Host.Value;
                workbook.ImportDataToWorkSheets<ContainerSearchPacakgeViewModel>(ref worksheetIndex, result.PACKAGES);
                workbook.Worksheets[workSheetsProcessed].Name = "Container Packages";
                workbook.Worksheets[workSheetsProcessed++].FixupReportWorkSheet<ContainerSearchPacakgeViewModel>(Request.Host.Value);

                workbook.Calculate();

                byte[] doc = await workbook.SaveDocumentAsync(DocumentFormat.Xlsx);

                var now = DateTime.Now;
                string fileName = $"{barcode}_{now.Date:yyyyMMddHHmmss}.xlsx";

                return base.File(doc, "application/ms-excel", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in ContainerSearch export is : {ex.Message.Substring(0, 100)}");
                throw;
            }
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(SingleSearch))]
        public async Task<IActionResult> SingleSearch([FromBody] ContainerSearchViewModel model)
        {
            ContainerSearchResultViewModel result = await _shippingContainerRepository.GetContainerByBarcode(model.Barcode, User.GetSite());           

            return Json(new
            {
                success = true,
                data = result
            });
        }
    }
}
