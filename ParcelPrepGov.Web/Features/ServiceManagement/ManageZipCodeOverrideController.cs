using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Web.Features.ServiceManagement.Models;
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
        public IActionResult ZipCodeServiceOverride()
        {
            return View();
        }

        [AjaxOnly]
        [HttpPost(Name = nameof(GetZipCodeOverrideByActiveGroupId))]
        public async Task<JsonResult> GetZipCodeOverrideByActiveGroupId([FromBody] ActiveGroupViewModel model)
        {
            var response = await zipOverrideWebProcessor.GetZipOverridesByActiveGroupIdAsync(model.Id);
            var data = response.Select(x => new
            {
                x.ZipCode,
                x.FromShippingCarrier,
                x.FromShippingMethod,
                x.ToShippingCarrier,
                x.ToShippingMethod
            }).ToList();

            return new JsonResult(data);
        }

        [AjaxOnly]
        public async Task<JsonResult> GetZipCodeOverrideActiveGroups(string subClient)
        {
            var result = await activeGroupProcessor.GetZipOverrideActiveGroupsAsync(subClient);
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
        [HttpPost(Name = nameof(ImportZipCodeOverride))]
        public async Task<IActionResult> ImportZipCodeOverride(ManageZipCodeOverrideViewModel model)
        {
            try
            {
                var returnMessages = new List<string>();

                if (model.UploadFile != null)
                {
                    var zipOverrideSat = new List<ZipOverride>();

                    using (var ms = new MemoryStream())
                    {
                        await model.UploadFile.CopyToAsync(ms);

                        var ws = new ExcelWorkSheet(ms);
                        if (ws.RowCount == 0)
                            throw new ArgumentException($"Import aborted because file: {model.UploadFile.FileName} is empty");

                        var now = DateTime.Now;
                        for (int row = ws.HeaderRow + 1; row <= ws.RowCount; row++)
                        {
                            var zipCode = ws.GetFormattedIntValue(row, "Zip Code", 5);
                            if (StringHelper.Exists(zipCode))
                            {
                                zipOverrideSat.Add(new ZipOverride
                                {
                                    ActiveGroupType = ActiveGroupTypeConstants.ZipCarrierOverride,
                                    CreateDate = now,
                                    ZipCode = zipCode,
                                    FromShippingCarrier = ws.GetStringValue(row, "From Shipping Carrier"),
                                    FromShippingMethod = ws.GetStringValue(row, "From Shipping Method"),
                                    ToShippingCarrier = ws.GetStringValue(row, "To Shipping Carrier"),
                                    ToShippingMethod = ws.GetStringValue(row, "To Shipping Method")
                                });
                            }
                        }
                    }

                    var response = await zipOverrideWebProcessor.ImportZipOverrides(zipOverrideSat, User.GetUsername(), model.SelectedSubClient, model.UploadStartDate, model.UploadFile.FileName);
                    if (!response.IsSuccessful)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"Ups service overrides upload failed: {response.Message}"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Ups service overrides file has been successfully uploaded."
                        });
                    }
                }

                return Ok(new
                {
                    success = true
                });

            }
            catch (ArgumentException ex)
            {
                logger.LogError($"Username: {User.Identity.Name} Exception while importing Zip Overrides file: {ex}");

                return Ok(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError($"Username: {User.Identity.Name} Exception while importing Zip Overrides file: {ex}");

                return Ok(new
                {
                    success = false,
                    message = $"Exception occurred while importing Zip Overrides file: {ex.Message}.."
                });
            }
        }


    }
}
