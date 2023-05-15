using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Constants;
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
using System.Threading.Tasks;
namespace ParcelPrepGov.Web.Features.ServiceManagement
{
    public partial class ServiceManagementController 
	{
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageBinRules)]
		public IActionResult ManageBinSchemas()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetBinActiveGroups))]
		public async Task<JsonResult> GetBinActiveGroups(string site)
		{
			var result = await activeGroupProcessor.GetBinSchemaActiveGroupsAsync(site);

			var serviceRuleActiveGroups = result.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				StartDate = x.StartDate,
				Filename = x.Filename,
				Name = x.Name,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			}).ToList();

			return new JsonResult(serviceRuleActiveGroups);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetBinsByActiveGroupId))]
		public async Task<JsonResult> GetBinsByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var response = await binWebProcessor.GetActiveBinsByActiveGroupIdAsync(model.Id);

			foreach (var bin in response.Bins.Where(x=>StringHelper.Exists(x.BinCodeSecondary) && StringHelper.DoesNotExist(x.ShippingCarrierSecondary)))
			{
				bin.ShippingCarrierSecondary = ContainerConstants.UspsCarrier;
			}

			var data = response.Bins.Select(x => new
			{
				x.BinCode,
				x.LabelListSiteKey,
				x.LabelListDescription,
				x.LabelListZip,
				x.OriginPointSiteKey,
				x.OriginPointDescription,
				x.DropShipSiteKeyPrimary,
				x.DropShipSiteDescriptionPrimary,
				x.DropShipSiteAddressPrimary,
				x.DropShipSiteCszPrimary,
				x.DropShipSiteNotePrimary,
				x.ShippingCarrierPrimary,
				x.ShippingMethodPrimary,
				x.ContainerTypePrimary,
				x.LabelTypePrimary,
				x.RegionalCarrierHubPrimary,
				x.DaysOfTheWeekPrimary,
				x.ScacPrimary,
				x.AccountIdPrimary,
				x.BinCodeSecondary,
				x.DropShipSiteKeySecondary,
				x.DropShipSiteDescriptionSecondary,
				x.DropShipSiteAddressSecondary,
				x.DropShipSiteCszSecondary,
				x.DropShipSiteNoteSecondary,
				x.ShippingCarrierSecondary,
				x.ShippingMethodSecondary,
				x.ContainerTypeSecondary,
				x.LabelTypeSecondary,
				x.RegionalCarrierHubSecondary,
				x.DaysOfTheWeekSecondary,
				x.ScacSecondary,
				x.AccountIdSecondary,
				x.IsAptb,
				x.IsScsc
			}).ToList().OrderBy(x=>x.BinCode);

			return new JsonResult(data);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(ImportBinSchemas))]
		public async Task<IActionResult> ImportBinSchemas(ManageBinSchemasViewModel model)
		{
			try
			{
				#region validation
				string[] imageExtensions = { ".xls", ".xlsx" };
				var isValidExtenstion = imageExtensions.Any(ext =>
				{
					return model.BinUploadFile.FileName.LastIndexOf(ext) > -1;
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

				#region Bins
				var bins = new List<Bin>();
				using (var ms = new MemoryStream())
				{
					await model.BinUploadFile.CopyToAsync(ms);
					using var ep = new ExcelPackage(ms);
					var ws = ep.Workbook.Worksheets[0];

					if (ws.Dimension == null)
						return Ok(new
						{
							success = false,
							message = $"Import aborted because of file: {model.BinUploadFile.FileName}"
						});

					DateTime createDate = DateTime.Now;
					for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
					{
						//If there is no bincode - skip it. Most likely just empty lines at the end of an excel file
						if (NullUtility.NullExists(ws.Cells[row, 1].Value) == string.Empty)
						{
							continue;
						}
						bins.Add(new Bin
						{
							BinCode = NullUtility.NullExists(ws.Cells[row, 1].Value),
							LabelListSiteKey = NullUtility.NullExists(ws.Cells[row, 2].Value),
							LabelListDescription = NullUtility.NullExists(ws.Cells[row, 3].Value),
							LabelListZip = NullUtility.NullExists(ws.Cells[row, 4].Value),
							OriginPointSiteKey = NullUtility.NullExists(ws.Cells[row, 5].Value),
							OriginPointDescription = NullUtility.NullExists(ws.Cells[row, 6].Value),
							DropShipSiteKeyPrimary = NullUtility.NullExists(ws.Cells[row, 7].Value),
							DropShipSiteDescriptionPrimary = NullUtility.NullExists(ws.Cells[row, 8].Value),
							DropShipSiteAddressPrimary = NullUtility.NullExists(ws.Cells[row, 9].Value),
							DropShipSiteCszPrimary = NullUtility.NullExists(ws.Cells[row, 10].Value),
							DropShipSiteNotePrimary = NullUtility.NullExists(ws.Cells[row, 11].Value),
							ShippingCarrierPrimary = NullUtility.NullExists(ws.Cells[row, 12].Value),
							ShippingMethodPrimary = NullUtility.NullExists(ws.Cells[row, 13].Value),
							ContainerTypePrimary = NullUtility.NullExists(ws.Cells[row, 14].Value),
							LabelTypePrimary = NullUtility.NullExists(ws.Cells[row, 15].Value),
							RegionalCarrierHubPrimary = NullUtility.NullExists(ws.Cells[row, 16].Value),
							DaysOfTheWeekPrimary = NullUtility.NullExists(ws.Cells[row, 17].Value),
							ScacPrimary = NullUtility.NullExists(ws.Cells[row, 18].Value),
							AccountIdPrimary = NullUtility.NullExists(ws.Cells[row, 19].Value),
							BinCodeSecondary = NullUtility.NullExists(ws.Cells[row, 20].Value),
							DropShipSiteKeySecondary = NullUtility.NullExists(ws.Cells[row, 21].Value),
							DropShipSiteDescriptionSecondary = NullUtility.NullExists(ws.Cells[row, 22].Value),
							DropShipSiteAddressSecondary = NullUtility.NullExists(ws.Cells[row, 23].Value),
							DropShipSiteCszSecondary = NullUtility.NullExists(ws.Cells[row, 24].Value),
							DropShipSiteNoteSecondary = NullUtility.NullExists(ws.Cells[row, 25].Value),
							ShippingCarrierSecondary = NullUtility.NullExists(ws.Cells[row, 26].Value),
							ShippingMethodSecondary = NullUtility.NullExists(ws.Cells[row, 27].Value),
							ContainerTypeSecondary = NullUtility.NullExists(ws.Cells[row, 28].Value),
							LabelTypeSecondary = NullUtility.NullExists(ws.Cells[row, 29].Value),
							RegionalCarrierHubSecondary = NullUtility.NullExists(ws.Cells[row, 30].Value),
							DaysOfTheWeekSecondary = NullUtility.NullExists(ws.Cells[row, 31].Value),
							ScacSecondary = NullUtility.NullExists(ws.Cells[row, 32].Value),
							AccountIdSecondary = NullUtility.NullExists(ws.Cells[row, 33].Value),
							IsAptb = bool.Parse(NullUtility.NullExists(ws.Cells[row, 34].Value)),
							IsScsc = bool.Parse(NullUtility.NullExists(ws.Cells[row, 35].Value)),
							CreateDate = createDate
						});
					}
				}

				#endregion

				var request = new ImportBinsAndBinMapsRequest
				{
					Bins = bins,
					Filename = model.BinUploadFile.FileName,
					SiteName = model.SelectedSite,
					UserName = User.GetUsername(),
					StartDate = model.UploadStartDate
				};

				var response = await binWebProcessor.ImportBinsAndBinMaps(request);
				if (response.IsSuccessful)
				{
					TempData["Toast"] = $"Bin Schemas imported successfully, {Toast.Success}";
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
						message = $"Bin Schemas import failed: {response.Message}"
					});
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while importing Bin Schemas: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing Bin Schemas: {ex.Message}."
				});
			}
		}
	}
}
