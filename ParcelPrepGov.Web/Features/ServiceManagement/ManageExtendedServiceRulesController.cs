using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Data.Models;
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
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageExtendedServiceRules)]
		public IActionResult ManageExtendedServiceRules()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(Get))]
		public async Task<JsonResult> GetExtendedServiceRules([FromQuery] string subClientName)
		{
			if (string.IsNullOrEmpty(subClientName)) return new JsonResult(new List<ActiveGroupViewModel>());

			var result = await activeGroupProcessor.GetFortyEightStatesActiveGroupsAsync(subClientName);

			var ExtendedserviceRuleActiveGroups = result.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				Filename = x.Filename,
				Name = x.Name,
				StartDate = x.StartDate,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			});

			return new JsonResult(ExtendedserviceRuleActiveGroups);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetExtendedServiceRulesByActiveGroupId))]
		public async Task<JsonResult> GetExtendedServiceRulesByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var activeGroup = await activeGroupProcessor.GetActiveGroupByIdAsync(model.Id, model.Name);
			var extendedServiceRules = await serviceRuleExtensionProcessor.GetExtendedServiceRulesByActiveGroupIdAsync(model.Id);
			var groupedExtendedServiceRules = extendedServiceRules.GroupBy(x=>x.MailCode).OrderBy(x=>x.Key);
			var orderedExtendedServiceRules = new List<ServiceRuleExtension>();

			foreach (var mailCodeGroup in groupedExtendedServiceRules)
			{
				var orderedMailCodeGroup = mailCodeGroup
				.OrderBy(x => x.MinWeight)
				.OrderBy(x => x.MaxWeight)
				.OrderBy(x => x.ShippingCarrier)
				.OrderBy(x => x.ShippingMethod)
				.OrderBy(x => x.ServiceLevel);

				orderedExtendedServiceRules.AddRange(orderedMailCodeGroup);
			}

			var serviceRuleExtensionExcels = mapper.Map<List<ServiceRuleExtensionExcel>>(orderedExtendedServiceRules);
			return new JsonResult(serviceRuleExtensionExcels);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(UploadExtendedRules))]
		public async Task<IActionResult> UploadExtendedRules(UploadExtendedServiceRulesViewModel model)
		{
			try
			{
				var validExtensions = new List<string> { ".xls", ".xlsx" };
				var fileName = model.UploadFile.FileName.ToUpper();
				var extension = Path.GetExtension(fileName);
				var match = validExtensions.FirstOrDefault(x => string.Compare(x.ToUpper(), extension.ToUpper(), true) == 0);

				if (match == null)
				{
					return Ok(new
					{
						success = false,
						message = "Invalid file extenstion. Only .xls and .xlsx extensions allowed."
					});
				}

				if (string.IsNullOrEmpty(model.SubClientName) || model.SubClientName == "undefined")
					return Ok(new
					{
						success = false,
						message = "Invalid subclient."
					});


				var extendedServiceRules = new List<ServiceRuleExtension>();

				using var ms = new MemoryStream();
				await model.UploadFile.CopyToAsync(ms);
				using var ep = new ExcelPackage(ms);
				var ws = ep.Workbook.Worksheets[0];

				if (ws.Dimension == null)
					return Ok(new
					{
						success = false,
						message = "File appears to be empty"
					});

				for (int rw = ws.Dimension.Start.Row + 1; rw <= ws.Dimension.End.Row; rw++)
				{
					extendedServiceRules.Add(new ServiceRuleExtension
                    {
                        MailCode = GetStringValue(ws.Cells[rw, 1].Value),
						StateCode = GetStringValue(ws.Cells[rw, 2].Value),
						IsDefault = GetBoolValue(ws.Cells[rw, 3].Value),
						InFedExList = GetBoolValue(ws.Cells[rw, 4].Value),
						InUpsList = GetBoolValue(ws.Cells[rw, 5].Value),
						IsSaturdayDelivery = GetBoolValue(ws.Cells[rw, 6].Value),
						MinWeight = GetDecimalValue(ws.Cells[rw, 7].Value),
                        MaxWeight = GetDecimalValue(ws.Cells[rw, 8].Value),
                        ShippingCarrier = GetStringValue(ws.Cells[rw, 9].Value),
                        ShippingMethod = GetStringValue(ws.Cells[rw, 10].Value),
                        ServiceLevel = GetStringValue(ws.Cells[rw, 11].Value),
					});
                }

				var response = await serviceRuleExtensionProcessor.ImportListOfNewExtendedServiceRules(extendedServiceRules, model.UploadStartDate.ToShortDateString(), model.SubClientName, User.GetUsername(), model.UploadFile.FileName);

				if (!response.IsSuccessful)
				{
					return Ok(new
					{
						success = false,
						message = response.Message
					});
				}

				return Ok(new
				{
					success = true
				});

			}
			catch (Exception ex)
			{
				logger.LogError($"Username: {User.Identity.Name} Exception while importing Extended Service Rules file: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing Extended Service Rules file: {ex.Message}."
				});
			}

		}
	}
}
