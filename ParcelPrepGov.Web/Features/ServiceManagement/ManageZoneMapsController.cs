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
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageZoneMaps)]
		public IActionResult ManageZoneMaps()
		{
			return View(nameof(ManageZoneMaps));
		}


		[AjaxOnly]
		[HttpPost(Name = nameof(GetZoneMapsByActiveGroupId))]
		public async Task<JsonResult> GetZoneMapsByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var data = await zoneMapsWebProcessor.GetZoneMapsByActiveGroupIdAsync(model.Id);
			var zoneMaps = mapper.Map<List<ZoneMapViewModel>>(data);
			return new JsonResult(zoneMaps.OrderBy(z => z.ZipFirstThree));
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetZoneMaps))]
		public async Task<JsonResult> GetZoneMaps()
		{
			var activeGroups = await activeGroupProcessor.GetAllActiveGroupsByType(ActiveGroupTypeConstants.ZoneMaps, true);

			var zipSchemaActiveGroups = activeGroups.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				Filename = x.Filename,
				Name = x.Name,
				StartDate = x.StartDate,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			});

			return new JsonResult(zipSchemaActiveGroups);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(ImportZoneMaps))]
		public async Task<IActionResult> ImportZoneMaps(ManageZoneMapsViewModel model)
		{
			try
			{
				var returnMessages = new List<string>();

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

					var zoneMaps = new List<ZoneMap>();
					for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
					{
						var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
						var zoneMatrix = NullUtility.NullExists(ws.Cells[row, 2].Value);
						if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
						{
							zoneMaps.Add(new ZoneMap
							{
								ZipFirstThree = zipCode,
								ZoneMatrix = zoneMatrix,
								CreateDate = DateTime.UtcNow
							});
						}
					}
					logger.Log(LogLevel.Information, $"Zone Maps file stream rows read: { zoneMaps.Count() }");

					var response = await zoneMapsWebProcessor.ImportZoneMaps(zoneMaps, User.GetUsername(), SiteConstants.AllSites, 
						model.StartDate, model.UploadFile.FileName);

					if (! response.IsSuccessful)
					{
						return Ok(new
						{
							success = false,
							message = $"Zone Maps file import failed: {response.Message}"
						});
					}
					else
					{
						return Ok(new
						{
							success = true,
							message = "Zone Maps file has been successfully uploaded."
						});
					}
				}

				return Ok(new
				{
					success = true
				});

			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while importing Zone Maps: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing Zone Maps: {ex.Message}."
				});
			}
		}

	}
}
