
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.ServiceOverride.Models;
using ParcelPrepGov.Web.Infrastructure;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Globals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParcelPrepGov.Web.Infrastructure.Utilities;
using PackageTracker.Domain.Utilities;

namespace ParcelPrepGov.Web.Features.ServiceOverride
{
    [Authorize(PPGClaim.WebPortal.ServiceManagement.ServiceOverride)]
    public class ServiceOverrideController : Controller
    {
        private readonly IActiveGroupProcessor _activeGroupProcessor;
        private readonly ISubClientProcessor _subClientProcessor;
        private readonly ILogger<ServiceOverrideController> _logger;
        private readonly ISiteProcessor siteProcessor;

        public ServiceOverrideController(IActiveGroupProcessor activeGroupProcessor, ISubClientProcessor subClientProcessor, ILogger<ServiceOverrideController> logger, ISiteProcessor siteProcessor)
        {
            _activeGroupProcessor = activeGroupProcessor ?? throw new ArgumentNullException(nameof(activeGroupProcessor));
            _subClientProcessor = subClientProcessor ?? throw new ArgumentNullException(nameof(subClientProcessor));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            this.siteProcessor = siteProcessor;
        }

        public async Task<IActionResult> Index()
        {
            var site = await siteProcessor.GetSiteBySiteNameAsync(User.GetSite());
            var getShippingCarriers = ShippingDataMapUtility.GetShippingCarrierConstantsDescriptions();
            var serviceOverrideViewModel = new ServiceOverrideViewModel();

            if (StringHelper.Exists(site.Id))
            {
                serviceOverrideViewModel.SiteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
            }
            else
            {
                serviceOverrideViewModel.SiteLocalTime = DateTime.Now;
            }

            getShippingCarriers.ForEach(m => serviceOverrideViewModel.ShippingCarriers.Add(m.CarrierDescription));

            return View(serviceOverrideViewModel);
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(Get))]
        public async Task<JsonResult> Get([FromQuery] string customerName)
        {
            if (string.IsNullOrEmpty(customerName)) return new JsonResult(new List<ServiceOverrideDetail>());

            var subclient = await _subClientProcessor.GetSubClientByNameAsync(customerName);

            var siteName = subclient.SiteName;

            var result = await _activeGroupProcessor.GetShippingMethodOverrideActiveGroupsAsync(customerName);

            var overrides = result.Select(x => new ServiceOverrideDetail
            {
                Id = x.Id,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                OldShippingCarrier = x.ServiceOverride.OldShippingCarrier,
                OldShippingMethod = x.ServiceOverride.OldShippingMethod,
                NewShippingCarrier = x.ServiceOverride.NewShippingCarrier,
                NewShippingMethod = x.ServiceOverride.NewShippingMethod,
                IsEnabled = x.IsEnabled,
                AddedBy = x.AddedBy,
                Name = x.Name,
                CreateDate = x.CreateDate
            });

            return new JsonResult(overrides);

        }


        [HttpPut(Name = nameof(Update))]
        public async Task<IActionResult> Update(string key, string values)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var splitKey = key.Split('|');

                    var serviceOverrideDetail = new ServiceOverrideDetail();
                    JsonConvert.PopulateObject(values, serviceOverrideDetail);

                    var result = await _activeGroupProcessor.GetCurrentActiveGroup(splitKey[0], splitKey[1]);
                    if (result != null)
                    {
                        // update this
                        result.IsEnabled = serviceOverrideDetail.IsEnabled;
                        _activeGroupProcessor.Update(result);
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "There was a problem. No active group found."
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Username: {User.Identity.Name} Exception while overriding service: {ex}");

                return Json(new
                {
                    success = false,
                    message = $"Exception occurred while overriding service: {ex.Message}."
                });
            }

            return null;
        }
        [AjaxOnly]
        [HttpPost(Name = nameof(Post))]
        public async Task<IActionResult> Post(ServiceOverridePost model)
        {
            try
            {
                #region validation
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage))
                    });
                }
                #endregion
                var site = await siteProcessor.GetSiteBySiteNameAsync(User.GetSite());
                var localDateTime = DateTime.Now;
                if (StringHelper.Exists(site.Id))
                {
                    localDateTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
                }

                var shippingCarrierDataMap = ShippingDataMapUtility.GetShippingCarrierConstantsDescriptions();
                var shippingMethodDataMap = ShippingDataMapUtility.GetShippingMethodDisplayConstantsByCarrier();

                var newCarrierConstant = shippingCarrierDataMap
                    .FirstOrDefault(c => c.CarrierDescription == model.NewShippingCarrier).CarrierConstant;

                var newShippingMethodConstant = shippingMethodDataMap
                    .FirstOrDefault(c => c.CarrierConstant == newCarrierConstant).ShippingMethods
                    .FirstOrDefault(m => m.Value == model.NewShippingMethod).Key;

                var oldShippingCarrier = shippingCarrierDataMap
                    .FirstOrDefault(c => c.CarrierDescription == model.OldShippingCarrier).CarrierConstant;

                var oldShippingMethod = shippingMethodDataMap
                    .FirstOrDefault(c => c.CarrierConstant == oldShippingCarrier).ShippingMethods
                    .FirstOrDefault(m => m.Value == model.OldShippingMethod).Key;

                var activeGroup = new ActiveGroup
                {
                    Name = model.CustomerName,
                    AddedBy = User.Identity.Name,
                    ActiveGroupType = ActiveGroupTypeConstants.ServiceOverrides,
                    IsEnabled = model.IsEnabled,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreateDate = localDateTime,
                    ServiceOverride = new PackageTracker.Data.Models.ServiceOverride
                    {
                        NewShippingCarrier = newCarrierConstant,
                        OldShippingCarrier = oldShippingCarrier,
                        NewShippingMethod = newShippingMethodConstant,
                        OldShippingMethod = oldShippingMethod
                    }
                };

                var newActiveGroup = await _activeGroupProcessor.AddActiveGroupAsync(activeGroup);

                if (newActiveGroup != null)
                {
                    TempData["Toast"] = $"Service Override successful, {Toast.Success}";
                    return Json(new
                    {
                        success = true
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Service Override failed"
                });

            }
            catch (Exception ex)
            {
                _logger.LogError($"Username: {User.Identity.Name} Exception while overriding service: {ex}");

                return Json(new
                {
                    success = false,
                    message = "Exception occurred while overriding service: {ex.Message}."
                });
            }
        }

        [AjaxOnly]
        [HttpGet(Name = nameof(GetShippingMethodsByCarrier))]
        public JsonResult GetShippingMethodsByCarrier([FromQuery] string carrierName)
        {
            var shippingMethodsByCarrier = new List<string>();
            var getShippingMethods = ShippingDataMapUtility.GetShippingMethodDisplayConstantsByCarrier();
            var shippingCarrierDataMap = ShippingDataMapUtility.GetShippingCarrierConstantsDescriptions();
            var shippingMethodDataMap = ShippingDataMapUtility.GetShippingMethodDisplayConstantsByCarrier();

            if (StringHelper.Exists(carrierName))
            {
                var carrierConstant = shippingCarrierDataMap.FirstOrDefault(c => c.CarrierDescription == carrierName).CarrierConstant;
                shippingMethodsByCarrier.AddRange(shippingMethodDataMap.FirstOrDefault(s => s.CarrierConstant == carrierConstant).ShippingMethods.Values.ToList());
            }
            return new JsonResult(shippingMethodsByCarrier);
        }
    }
}
