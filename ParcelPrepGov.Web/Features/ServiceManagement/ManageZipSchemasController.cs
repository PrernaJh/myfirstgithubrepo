using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Constants;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement
{
	public partial class ServiceManagementController
	{
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageZipSchemas)]
		public IActionResult ManageZipSchemas()
		{
			return View(nameof(ManageZipSchemas));
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetZipsByActiveGroupType))]
		public async Task<JsonResult> GetZipsByActiveGroupType([FromQuery] string activeGroupType)
		{
			if (activeGroupType == null)
				return new JsonResult(new string[] { });

			var activeGroups = await activeGroupProcessor.GetAllActiveGroupsByType(activeGroupType.ToUpper(), true);

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
		[HttpPost(Name = nameof(GetZipsByActiveGroupId))]
		public async Task<JsonResult> GetZipsByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var data = await zipOverrideWebProcessor.GetZipOverridesByActiveGroupIdAsync(model.Id);
			var zips = mapper.Map<List<ZipExcel>>(data);
			return new JsonResult(zips.OrderBy(z => z.ZipCode));
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(ImportZipSchemas))]
		public async Task<IActionResult> ImportZipSchemas(ManageZipSchemasViewModel model)
		{
			try
			{
				var returnMessages = new List<string>();

				if (model.FedExHawaiiUploadFile != null)
				{ 
					var zipOverrideFedExHawaii = new List<ZipOverride>();
					using (var ms = new MemoryStream())
					{
						await model.FedExHawaiiUploadFile.CopyToAsync(ms);
						using var ep = new ExcelPackage(ms);
						var ws = ep.Workbook.Worksheets[0];

						if (ws.Dimension == null)
							return Ok(new
							{
								success = false,
								message = $"Import aborted because of file: {model.FedExHawaiiUploadFile.FileName}"
							});


						for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
						{
							var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
							if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
							{
								zipOverrideFedExHawaii.Add(new ZipOverride
								{
									ZipCode = zipCode,
									ActiveGroupType = ActiveGroupTypeConstants.ZipsFedExHawaii,
									CreateDate = DateTime.UtcNow
								});
							}
						}
					}

					var response = await zipOverrideWebProcessor.ImportZipOverrides(zipOverrideFedExHawaii, User.GetUsername(), SiteConstants.AllSites, 
						model.FedExHawaiiStartDate, model.FedExHawaiiUploadFile.FileName);

					if (! response.IsSuccessful)
					{
						return Ok(new
						{
							success = false,
							message = $"FedEx Hawaii file import failed: {response.Message}"
						});
					}
					else
					{
						return Ok(new
						{
							success = true,
							message = "FedEx Hawaii file has been successfully uploaded."
						});
					}
				}

				else if (model.UpsNdaSat48File != null)
				{ 
					var zipOverrideNdaSat = new List<ZipOverride>();

					using (var ms = new MemoryStream())
					{
						await model.UpsNdaSat48File.CopyToAsync(ms);

						using var ep = new ExcelPackage(ms);
						var ws = ep.Workbook.Worksheets[0];

						if (ws.Dimension == null)
							return Ok(new
							{
								success = false,
								message = $"Import aborted because of file: {model.UpsNdaSat48File.FileName}"
							});

						for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
						{
							var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
							if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
							{
								zipOverrideNdaSat.Add(new ZipOverride
								{
									ZipCode = zipCode,
									ActiveGroupType = ActiveGroupTypeConstants.ZipsUpsSat48,
									CreateDate = DateTime.UtcNow
								});
							}
						}
					}

					var response = await zipOverrideWebProcessor.ImportZipOverrides(zipOverrideNdaSat, User.GetUsername(), SiteConstants.AllSites, 
						model.UpsNdaSat48StartDate, model.UpsNdaSat48File.FileName);

					if (!response.IsSuccessful)
					{
						return Ok(new
						{
							success = false,
							message = $"Ups NDA Sat file upload failed: {response.Message}"
						});
					}
					else
					{
						return Ok(new
						{
							success = true,
							message = "Ups NDA Sat file has been successfully uploaded."
						});
					}
				}

				else if (model.UpsDasFile != null)
				{
					var zipOverrideSat = new List<ZipOverride>();

					using (var ms = new MemoryStream())
					{
						await model.UpsDasFile.CopyToAsync(ms);

						using var ep = new ExcelPackage(ms);
						var ws = ep.Workbook.Worksheets[0];

						if (ws.Dimension == null)
							return Ok(new
							{
								success = false,
								message = $"Import aborted because of file: {model.UpsDasFile.FileName}"
							});

						for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
						{
							var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
							if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
							{
								zipOverrideSat.Add(new ZipOverride
								{
									ZipCode = zipCode,
									ActiveGroupType = ActiveGroupTypeConstants.ZipsUpsDas,
									CreateDate = DateTime.UtcNow
								});
							}
						}
					}

					var response = await zipOverrideWebProcessor.ImportZipOverrides(zipOverrideSat, User.GetUsername(), SiteConstants.AllSites,
						model.UpsDasStartDate, model.UpsDasFile.FileName);

					if (!response.IsSuccessful)
					{
						return Ok(new
						{
							success = false,
							message = $"Ups Das file import failed: {response.Message}"
						});
					}
					else
					{
						return Ok(new
						{
							success = true,
							message = "Ups Das file has been successfully uploaded."
						});
					}
				}

				else if (model.UspsRuralFile != null)
				{
					var zipOverrideSat = new List<ZipOverride>();

					using (var ms = new MemoryStream())
					{
						await model.UspsRuralFile.CopyToAsync(ms);

						using var ep = new ExcelPackage(ms);
						var ws = ep.Workbook.Worksheets[0];

						if (ws.Dimension == null)
							return Ok(new
							{
								success = false,
								message = $"Import aborted because of file: {model.UspsRuralFile.FileName}"
							});

						for (int row = ws.Dimension.Start.Row + 1; row <= ws.Dimension.End.Row; row++)
						{
							var zipCode = NullUtility.NullExists(ws.Cells[row, 1].Value);
							if (zipCode != string.Empty && Regex.IsMatch(zipCode, @"^[0-9-]+$"))
							{
								zipOverrideSat.Add(new ZipOverride
								{
									ZipCode = zipCode,
									ActiveGroupType = ActiveGroupTypeConstants.ZipsUspsRural,
									CreateDate = DateTime.UtcNow
								});
							}
						}
					}

					var response = await zipOverrideWebProcessor.ImportZipOverrides(zipOverrideSat, User.GetUsername(), SiteConstants.AllSites,
						model.UspsRuralStartDate, model.UspsRuralFile.FileName);

					if (!response.IsSuccessful)
					{
						return Ok(new
						{
							success = false,
							message = $"Usps Rural file import failed: {response.Message}"
						});
					}
					else
					{
						return Ok(new
						{
							success = true,
							message = "Usps Rural file has been successfully uploaded."
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
				logger.LogError($"Username: {User.Identity.Name} Exception while importing zip schemas: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing zip schemas: {ex.Message}."
				});
			}
		}

	}
}
