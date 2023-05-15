using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using ParcelPrepGov.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.UserManagement
{
    [Authorize(Roles = PPGRole.Administrator + "," + PPGRole.SystemAdministrator)]
    public class SiteManagementController : Controller
    {
        private readonly IMapper mapper;

        private readonly ISiteProcessor siteProcessor;
        private readonly ILogger<SiteManagementController> logger;

        public SiteManagementController(ISiteProcessor siteProcessor, ILogger<SiteManagementController> logger)
        {
            this.siteProcessor = siteProcessor;
            this.logger = logger;

            mapper = new MapperConfiguration(mapperConfig =>
            {
                mapperConfig.CreateMap<CriticalEmailModel, ModifyCritialEmailListRequest>();
            }).CreateMapper();
            logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        [Route("[controller]/CriticalEmailList/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetCriticalEmailListBySiteId(string id)
        {
            try
            {
                var site = await siteProcessor.GetSiteByIdAsync(id);
                return new JsonResult(GetCriticalEmailModelListForDevExpress(site.SiteName, site.CriticalAlertEmailList));
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on GetCriticalEmailListBySiteId for site  {id}. Exception: {ex}");
            }

            return new JsonResult("Error");
        }

        [Route("[controller]/CriticalEmailList")]
        [HttpGet]
        public async Task<IActionResult> GetCriticalEmailListBySiteName([FromQuery] string siteName)
        {
            try
            {
                var criticalEmailListForSite = await siteProcessor.GetSiteCritialEmailListBySiteName(siteName);

                return new JsonResult(GetCriticalEmailModelListForDevExpress(siteName, criticalEmailListForSite));
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on GetCriticalEmailListBySiteName for site  {siteName}. Exception: {ex}");
            }

            return new JsonResult("Error");
        }


        [Route("[controller]/CriticalEmailList")]
        [HttpPost]
        public async Task<IActionResult> AddCriticalEmailToList([FromForm] string values)
        {
            try
            {
                var request = new ModifyCritialEmailListRequest();
                JsonConvert.PopulateObject(values, request);
                if (BusinessLogicCheck(request))
                { 
                    return new JsonResult(await siteProcessor.AddUserToCriticalEmailList(request));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on AddCriticalEmailToList for query {values}. Exception: {ex}");
            }
            return new JsonResult("Error");
        }

        /// <summary>
        /// original method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("[controller]/CriticalEmailList")]
        [HttpDelete]
        public void RemoveCriticalEmailFromList([FromForm] string key)
        {
            try
            {
                var request = new ModifyCritialEmailListRequest();
                JsonConvert.PopulateObject(key, request);
                if (BusinessLogicCheck(request))
                {
                    siteProcessor.RemoveUserFromCriticalEmailList(request).GetAwaiter().GetResult();
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Error on RemoveCriticalEmailFromList for key {key}. Exception: {ex}");
            }
        }

        private bool BusinessLogicCheck(ModifyCritialEmailListRequest request)
        {
            bool isValid = true;
            if (request.Email != string.Empty && request.SiteName != string.Empty)
            {
                // do nothing is valid
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }

        private IEnumerable<CriticalEmailModel> GetCriticalEmailModelListForDevExpress(string siteName, IEnumerable<string> emails)
        {
            return emails.Select(email =>
            {
                return new CriticalEmailModel()
                {
                    SiteName = siteName,
                    Email = email
                };
            });
        }
    }
}
