using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.ServiceManagement.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement
{
	public partial class ServiceManagementController
	{
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageGeoDescriptors)]
		public IActionResult ManageGeoDescriptors()
		{
			return View(nameof(ManageGeoDescriptors));
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetGeoDescriptorsActiveGroups))]
		public async Task<JsonResult> GetGeoDescriptorsActiveGroups([FromQuery] string siteName)
		{
			var activeGroups = await activeGroupProcessor.GetAllActiveGroupsByType(ActiveGroupTypeConstants.UpsGeoDescriptors, false, siteName);

			var geoDescriptorActiveGroups = activeGroups.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				Filename = x.Filename,
				Name = x.Name,
				StartDate = x.StartDate,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			});

			return new JsonResult(geoDescriptorActiveGroups);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetGeoDescriptorsByActiveGroupId))]
		public async Task<JsonResult> GetGeoDescriptorsByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var data = await geoDescriptorsWebProcessor.GetZipMapsByActiveGroupIdAsync(model.Id);
			var zips = mapper.Map<List<ZipMapViewModel>>(data);
			return new JsonResult(zips.OrderBy(z => z.ZipCode));
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(ImportGeoDescriptors))]
		public async Task<IActionResult> ImportGeoDescriptors(ManageGeoDescriptorsViewModel model)
		{
			try
			{
				var returnMessages = new List<string>();

				var zipMaps = new List<ZipMap>();
				using (var ms = new MemoryStream())
				{
					await model.UploadFile.CopyToAsync(ms);
					using var ep = new ExcelPackage(ms);
					var ws = ep.Workbook.Worksheets[0];

					if (ws.Dimension == null)
						return Ok(new
						{
							success = false,
							message = $"Import aborted because of file: {model.UploadFile.FileName}"
						});


					for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
					{
						var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
						var value = NullUtility.NullExists(ws.Cells[row, 2].Value);
						if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
						{
							zipMaps.Add(new ZipMap
							{
								ZipCode = zipCode,
								Value = value,
								ActiveGroupType = ActiveGroupTypeConstants.UpsGeoDescriptors,
								CreateDate = DateTime.UtcNow
							});
						}
					}
				}

				var response = await geoDescriptorsWebProcessor.ImportZipMaps(zipMaps, User.GetUsername(), model.SelectedSite,
					model.UploadStartDate, model.UploadFile.FileName);

				if (!response.IsSuccessful)
				{
					return Ok(new
					{
						success = false,
						message = $"GEO descriptors file import failed: {response.Message}"
					});
				}
				else
				{
					return Ok(new
					{
						success = true,
						message = "GEO descriptors file has been successfully uploaded."
					});
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while importing GEO descriptors {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing GEO descriptors file: {ex.Message}."
				});
			}
		}

	}
}
