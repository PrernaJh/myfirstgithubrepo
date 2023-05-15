using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileManagement;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.ServiceManagement.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Globals;
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
        [Authorize(PPGClaim.WebPortal.ServiceManagement.ManageBinRules)]
		public IActionResult ManageBinMappings()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetBinMappingActiveGroups))]
		public async Task<JsonResult> GetBinMappingActiveGroups(string subClient)
		{
			if (!string.IsNullOrEmpty(subClient))
			{
				var result = await activeGroupProcessor.GetBinMapsActiveGroupsAsync(subClient);

				var binMapActiveGroups = result.Select(x => new ActiveGroupViewModel
				{
					Id = x.Id,
					Filename = x.Filename,
					Name = x.Name,
					StartDate = x.StartDate,
					UploadedBy = x.AddedBy,
					UploadDate = x.CreateDate
				});

				return new JsonResult(binMapActiveGroups);
			}
			else
			{
				return new JsonResult(new { });
			}

		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetBinMapsByActiveGroupId))]
		public async Task<JsonResult> GetBinMapsByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var response = await binWebProcessor.GetActiveBinMapsByActiveGroupIdAsync(model.Id);

			var data = response.BinMaps.Select(x => new
			{
				x.ZipCode,
				x.BinCode
			}).ToList();

			return new JsonResult(data);
		}

		private string BinMappingsFileName(string filename1, string filename2)
		{
			string basename1 = Path.GetFileNameWithoutExtension(filename1);
			string basename2 = Path.GetFileNameWithoutExtension(filename2);
			string ext = Path.GetExtension(filename1);
			return $"{basename1}+{basename2}{ext}";
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(ImportBinMappings))]
		public async Task<IActionResult> ImportBinMappings(ManageBinSchemasViewModel model)
		{
			try
			{
				#region validation
				string[] imageExtensions = { ".xls", ".xlsx" };
				var isValidExtenstion = imageExtensions.Any(ext =>
				{
					return model.FiveDigitFile.FileName.LastIndexOf(ext) > -1
					|| model.ThreeDigitFile.FileName.LastIndexOf(ext) > -1;
				});

				if (!isValidExtenstion)
				{
					return Ok(new
					{
						success = false,
						message = "Invalid file extenstion. Only .xls and .xlsx extensions allowed."
					});
				}
				#endregion

				#region Five Digit
				var binMaps = new List<BinMap>();
				using (var ms = new MemoryStream())
				{
					await model.FiveDigitFile.CopyToAsync(ms);
					using var ep = new ExcelPackage(ms);
					var ws = ep.Workbook.Worksheets[0];

					if (ws.Dimension == null)
						return Ok(new
						{
							success = false,
							message = $"Import aborted because of file: {model.FiveDigitFile.FileName}"
						});

					for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
					{
						var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
						if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
						{
							binMaps.Add(new BinMap
							{
								ZipCode = zipCode,
								BinCode = NullUtility.NullExists(ws.Cells[row, 2].Value)
							});
						}
					}
				}
				#endregion

				#region Three Digit
				using (var ms = new MemoryStream())
				{
					await model.ThreeDigitFile.CopyToAsync(ms);
					using var ep = new ExcelPackage(ms);
					var ws = ep.Workbook.Worksheets[0];

					if (ws.Dimension == null)
						return Ok(new
						{
							success = false,
							message = $"Import aborted because of file: {model.ThreeDigitFile.FileName}"
						});

					for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
					{
						var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
						if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
						{
							binMaps.Add(new BinMap
							{
								ZipCode = zipCode,
								BinCode = NullUtility.NullExists(ws.Cells[row, 2].Value)
							});
						}
					}
				}

				#endregion

				var request = new ImportBinsAndBinMapsRequest
				{
					BinMaps = binMaps,
					SubClientName = model.SelectedSubClient,
					Filename = BinMappingsFileName(model.FiveDigitFile.FileName, model.ThreeDigitFile.FileName),
					UserName = User.GetUsername(),
					StartDate = model.UploadStartDate
				};

				var response = await binWebProcessor.ImportBinsAndBinMaps(request);
				if (response.IsSuccessful)
				{
					TempData["Toast"] = $"Bin Mappings imported successfully, {Toast.Success}";
					return Ok(new
					{
						success = true
					});
				}
				else
				{
					return Ok(new
					{
						success = false,
						message = $"Bin Mappings import failed: {response.Message}"
					});
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while importing Bin Mappings: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing Bin Mappings: {ex.Message}."
				});
			}
		}
	}
}
