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
		public IActionResult ManageContainerRates()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetCurrentContainerRates))]
		public async Task<JsonResult> GetCurrentContainerRates([FromQuery] string siteName)
		{
			if (string.IsNullOrEmpty(siteName)) return new JsonResult(new List<ActiveGroupViewModel>());

			var activeGroups = await activeGroupProcessor.GetContainerRatesActiveGroupsAsync(siteName);

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
		[HttpPost(Name = nameof(UploadContainerRates))]
		public async Task<IActionResult> UploadContainerRates(IFormFile uploadFile, DateTime startDate, string siteName)
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

				if (string.IsNullOrEmpty(siteName))
				{
					return Ok(new
					{
						success = false,
						message = "Customer can not be null."
					});
				}
				#endregion

				var rates = new List<Rate>();

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


				var fileImportResponse = await UploadNewRates(ws, startDate, siteName, false, uploadFile.FileName);
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
				logger.LogError($"Username: {User.Identity.Name} Exception while uploading Container Rates: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while uploading Container Rates: {ex.Message}."
				});
			}
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetContainerRatesByActiveGroupId))]
		public async Task<JsonResult> GetContainerRatesByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var rates = await rateWebProcessor.GetRatesByActiveGroupIdAsync(model.Id);
			var rateModels = mapper.Map<List<RateViewModel>>(rates);
			return new JsonResult(rateModels
				.OrderBy(r => r.IsOutside48States)
				.ThenBy(r => r.IsRural)
				.ThenBy(r => r.Carrier)
				.ThenBy(r => r.ContainerType)
				.ThenBy(r => r.WeightNotOverOz)
				);
		}
	}
}
