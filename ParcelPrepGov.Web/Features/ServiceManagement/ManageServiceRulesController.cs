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
		[Authorize(PPGClaim.WebPortal.ServiceManagement.ManageServiceRules)]
		public IActionResult ManageServiceRules()
		{
			return View();
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(Get))]
		public async Task<JsonResult> Get([FromQuery] string customerName)
		{
			if (string.IsNullOrEmpty(customerName)) return new JsonResult(new List<ActiveGroupViewModel>());

			var result = await activeGroupProcessor.GetServiceRuleActiveGroupsAsync(customerName);

			var serviceRuleActiveGroups = result.Select(x => new ActiveGroupViewModel
			{
				Id = x.Id,
				Filename = x.Filename,
				Name = x.Name,
				StartDate = x.StartDate,
				UploadedBy = x.AddedBy,
				UploadDate = x.CreateDate
			});

			return new JsonResult(serviceRuleActiveGroups);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(GetServiceRulesByActiveGroupId))]
		public async Task<JsonResult> GetServiceRulesByActiveGroupId([FromBody] ActiveGroupViewModel model)
		{
			var activeGroup = await activeGroupProcessor.GetActiveGroupByIdAsync(model.Id, model.Name);
			var serviceRules = await serviceRuleWebProcessor.GetServiceRulesByActiveGroupIdAsync(model.Id);
			var groupedServiceRules = serviceRules.GroupBy(x=>x.MailCode).OrderBy(x=>x.Key);
			var orderedServiceRules = new List<ServiceRule>();

			foreach (var mailCodeGroup in groupedServiceRules)
			{
				var orderedMailCodeGroup = mailCodeGroup
				.OrderBy(x => x.MinWeight)
				.OrderBy(x => x.MaxWeight)
				.OrderBy(x => x.ZoneMin)
				.OrderBy(x => x.ZoneMax)
				.OrderBy(x => x.ShippingCarrier)
				.OrderBy(x => x.ShippingMethod)
				.OrderBy(x => x.ServiceLevel);

				orderedServiceRules.AddRange(orderedMailCodeGroup);
			}

			var serviceRuleExcels = mapper.Map<List<ServiceRuleExcel>>(orderedServiceRules);
			serviceRuleExcels.ForEach(x => x.SubClientName = activeGroup.Name);
			return new JsonResult(serviceRuleExcels);
		}

		[AjaxOnly]
		[HttpPost(Name = nameof(UploadFile))]
		public async Task<IActionResult> UploadFile(UploadServiceRulesViewModel model)
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

				var subclient = await subClientProcessor.GetSubClientByNameAsync(model.CustomerName);

				var site = subclient.SiteName;

				if (string.IsNullOrEmpty(site))
					return Ok(new
					{
						success = false,
						message = "Invalid site."
					});

				if (string.IsNullOrEmpty(model.CustomerName))
					return Ok(new
					{
						success = false,
						message = "Invalid customer."
					});


				var serviceRules = new List<ServiceRule>();

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
					if (GetStringValue(ws.Cells[rw, 2].Value) == string.Empty)
					{
						continue;
					}
					serviceRules.Add(new ServiceRule
                    {
                        MailCode = GetStringValue(ws.Cells[rw, 2].Value),
                        IsOrmd = GetBoolValue(ws.Cells[rw, 3].Value),
                        IsPoBox = GetBoolValue(ws.Cells[rw, 4].Value),
                        IsOutside48States = GetBoolValue(ws.Cells[rw, 5].Value),
                        IsUpsDas = GetBoolValue(ws.Cells[rw, 6].Value),
                        IsSaturday = GetBoolValue(ws.Cells[rw, 7].Value),
                        IsDduScfBin = GetBoolValue(ws.Cells[rw, 8].Value),                        
                        MinWeight = GetDecimalValue(ws.Cells[rw, 9].Value),
                        MaxWeight = GetDecimalValue(ws.Cells[rw, 10].Value),
                        MinLength = GetDecimalValue(ws.Cells[rw, 11].Value),
                        MaxLength = GetDecimalValue(ws.Cells[rw, 12].Value),
                        MinHeight = GetDecimalValue(ws.Cells[rw, 13].Value),
                        MaxHeight = GetDecimalValue(ws.Cells[rw, 14].Value),
                        MinWidth = GetDecimalValue(ws.Cells[rw, 15].Value),
                        MaxWidth = GetDecimalValue(ws.Cells[rw, 16].Value),
                        MinTotalDimensions = GetDecimalValue(ws.Cells[rw, 17].Value),
                        MaxTotalDimensions = GetDecimalValue(ws.Cells[rw, 18].Value),
                        ZoneMin = GetIntValue(ws.Cells[rw, 19].Value),
                        ZoneMax = GetIntValue(ws.Cells[rw, 20].Value),
                        ShippingCarrier = GetStringValue(ws.Cells[rw, 21].Value),
                        ShippingMethod = GetStringValue(ws.Cells[rw, 22].Value),
                        ServiceLevel = GetStringValue(ws.Cells[rw, 23].Value),
						IsQCRequired = GetBoolValue(ws.Cells[rw, 24].Value)
					});
                }

				var response = await serviceRuleWebProcessor.ImportListOfNewServiceRules(serviceRules, model.UploadStartDate.ToShortDateString(), model.CustomerName, User.GetUsername(), model.UploadFile.FileName);

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
				logger.LogError($"Username: {User.Identity.Name} Exception while importing Service Rules file: {ex}");

				return Ok(new
				{
					success = false,
					message = $"Exception occurred while importing Service Rules file: {ex.Message}."
				});
			}

		}
	}
}
