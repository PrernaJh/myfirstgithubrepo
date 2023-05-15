using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.ServiceManagement.Models;
using ParcelPrepGov.Web.Infrastructure;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement
{
	public partial class ServiceManagementController 
	{
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageCostsAndCharges)]
		public IActionResult ManagePackageRates()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetCurrentPackageRates))]
		public async Task<JsonResult> GetCurrentPackageRates([FromQuery] string customerName)
		{
			if (string.IsNullOrEmpty(customerName)) return new JsonResult(new List<ActiveGroupViewModel>());

			var activeGroups = await rateWebProcessor.GetAllRateActiveGroupsAsync(customerName);

			var viewModel = activeGroups.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				Filename = x.Filename,
				Name = x.Name,
				StartDate = x.StartDate,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			});

			return new JsonResult(viewModel);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(UploadPackageRates))]
		public async Task<IActionResult> UploadPackageRates(IFormFile uploadFile, DateTime startDate, string customerName)
		{
			try
			{
				#region validation
				string[] imageExtensions = { ".xls", ".xlsx" };
				var fileName = uploadFile.FileName.ToLower();
				var isValidExtenstion = imageExtensions.Any(ext =>
				{
					return fileName.LastIndexOf(ext) > -1;
				});

				if (!isValidExtenstion)
				{
					return Ok(new
					{
						success = false,
						message = "Invalid file extenstion. Only .xls and .xlsx extensions allowed."
					});
				}

				if (string.IsNullOrEmpty(customerName))
				{
					return Ok(new
					{
						success = false,
						message = "Customer can not be null."
					});
				}
				#endregion

				using var ms = new MemoryStream();
				await uploadFile.CopyToAsync(ms);
				using var ep = new ExcelPackage(ms);
				var ws = ep.Workbook.Worksheets[0];

				if (ws.Dimension == null)
					return Ok(new
					{
						success = false,
						message = "File appears to be empty"
					});

				var fileImportResponse = await UploadNewRates(ws, startDate, customerName, true, uploadFile.FileName);
				if (fileImportResponse.IsSuccessful)
				{
					return Ok(new
					{
						success = true,
						message = uploadFile.FileName
					});
				}

				return Ok(new
				{
					success = false,
					message = fileImportResponse.Message
				});

			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while uploading Package Rates: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while uploading Package Rates: {ex.Message}."
				});
			}
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetRatesByActiveGroupId))]
		public async Task<JsonResult> GetRatesByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var rates = await rateWebProcessor.GetRatesByActiveGroupIdAsync(model.Id);
			var rateModels = mapper.Map<List<RateViewModel>>(rates);
			// This can be removed later after this software has been released and new rates files have been uploaded ...
			foreach (var rate in rates.Where(r => !r.IsOutside48States && (
					r.CostZoneDduOut48 > 0 ||
					r.CostZoneScfOut48 > 0 ||
					r.CostZoneNdcOut48 > 0 ||
					r.ChargeZoneDduOut48 > 0 ||
					r.ChargeZoneScfOut48 > 0 ||
					r.ChargeZoneNdcOut48 > 0
				)))
            {
				rate.CostZoneDdu = rate.CostZoneDduOut48;
				rate.CostZoneScf = rate.CostZoneScfOut48;
				rate.CostZoneNdc = rate.CostZoneNdcOut48;
				rate.ChargeZoneDdu = rate.ChargeZoneDduOut48;
				rate.ChargeZoneScf = rate.ChargeZoneScfOut48;
				rate.ChargeZoneNdc = rate.ChargeZoneNdcOut48;
				var rateModel = mapper.Map<RateViewModel>(rate);
				rateModel.IsOutside48States = true;
				rateModels.Add(rateModel);
			}
			// ...
			return new JsonResult(rateModels
				.OrderBy(r => r.IsOutside48States)
				.ThenBy(r => r.IsRural)
				.ThenBy(r => r.Carrier)
				.ThenBy(r => r.Service)
				.ThenBy(r => r.WeightNotOverOz)
				);
		}
	}
}
