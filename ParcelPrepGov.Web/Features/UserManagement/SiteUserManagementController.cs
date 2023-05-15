using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.UserManagement
{

	[Authorize(Policy = PPGClaim.WebPortal.UserManagement.AddSiteUser)]
	public class SiteUserManagementController : UserManagementBaseController
	{
		private readonly ILogger<SiteUserManagementController> logger;

		public SiteUserManagementController(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ISiteRepository siteRepository,
			IClientRepository clientRepository,
			ISubClientRepository subClientRepository,
			IPasswordValidator<ApplicationUser> passwordHasher,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IHttpContextAccessor httpContextAccessor,
			ILogger<SiteUserManagementController> logger,
			IUserLookupProcessor userLookupProcessor,
			IMemoryCache memoryCache)
				: base(userManager, roleManager, siteRepository, clientRepository, subClientRepository, 
					  passwordHasher, emailConfiguration, emailService, httpContextAccessor, userLookupProcessor, memoryCache)
		{
			this.logger = logger ?? throw new ArgumentException(nameof(logger));
		}


		public async Task<IActionResult> ManageSiteAlerts()
        {
			return  View("ManageSiteAlerts");
        }
		public async Task<IActionResult> Index(string id)
		{
			string userSite = User.GetSite();
			if (string.IsNullOrWhiteSpace(id))
			{
				if (userSite == SiteConstants.AllSites)
				{
					return RedirectToAction("PickSite");
				}
				else
				{
					return RedirectToAction("Site", new { id = User.GetSite() });
				}
			}
			return RedirectToAction("Site", new { id = id });
		}

		public async Task<IActionResult> Site(string id)
		{
			var sites = (await siteRepository.GetAllSitesAsync()).Select(x => x.SiteName).ToList();
			if (id == "All")
			{
				var model = new SiteUserManagementViewModel
				{
					SiteName = null,
					AllSites = true,
					AssignableSiteLocations = sites
				};
				return View("Site", model);
			}
			else
			{
				var model = new SiteUserManagementViewModel
				{
					SiteName = id,
					AssignableSiteLocations = sites
				};

				return View("Site", model);
			}
		}

		[Authorize]
		public async Task<IActionResult> PickSite()
		{
			var sites = (await siteRepository.GetAllSitesAsync()).Select(x => x.SiteName).ToList();

			return View(sites);
		}


		[HttpGet(Name = nameof(GetUsers))]
		public async Task<JsonResult> GetUsers(string location, bool allUsers = false)
		{
			var users = await userManager.Users.Where(x => allUsers || x.Site == location).ToListAsync();

			var listOfUsers = new List<SiteUserModel>();

			var siteRoles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddGeneralManager))
			{
				siteRoles.Add(PPGRole.GeneralManager);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteSupervisor))
			{
				siteRoles.Add(PPGRole.Supervisor);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteOperator))
			{
				siteRoles.Add(PPGRole.Operator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteQA))
			{
				siteRoles.Add(PPGRole.QualityAssurance);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddAutomationStation))
			{
				siteRoles.Add(PPGRole.AutomationStation);
			}

			foreach (var user in users)
			{
				var userModel = new SiteUserModel
				{
					Id = user.Id,
					Deactivated = user.Deactivated,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					Site = user.Site,
					ResetPassword = user.ResetPassword,
					LastPasswordChangedDate = user.LastPasswordChangedDate,
					TemporaryPassword = user.TemporaryPassword
				};

				var userRoles = await userManager.GetRolesAsync(user);

				if (!userRoles.Intersect(siteRoles).Any())
					continue;

				userModel.Roles.AddRange(userRoles);
				listOfUsers.Add(userModel);
			}

			return new JsonResult(listOfUsers);
		}


		[HttpPost(Name = nameof(InsertUser))]
		public async Task<IActionResult> InsertUser([FromForm] string values)
		{
			if (values == null)
			{
				return BadRequest();
			}

			var newUser = JsonConvert.DeserializeObject<BaseUserModel>(values);

			var site = await siteRepository.GetSiteBySiteNameAsync(newUser.Site);

			if (site == null || !StringHelper.Exists(site.Id))
			{
				return BadRequest("SubClient does not exist");
			}

			newUser.Site = newUser.Site;
			newUser.Client = SiteConstants.AllSites;
			newUser.SubClient = SiteConstants.AllSites;

			return await base.InsertUser(newUser);
		}

		[HttpPut(Name = nameof(UpdateUser))]
		public async Task<JsonResult> UpdateUser([FromForm] string key, [FromForm] string values)
		{
			if (values == null)
			{
				ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return new JsonResult(new { ErrorMessage = "Bad Request" });
			}

			var editUser = JsonConvert.DeserializeObject<BaseUserModel>(values);

			editUser.Site = editUser.Site;
			editUser.Client = SiteConstants.AllSites;
			editUser.SubClient = SiteConstants.AllSites;

			return await base.UpdateUser(key, editUser);
		}

		[HttpGet(Name = nameof(GetRoles))]
		public JsonResult GetRoles()
		{
			var roles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddGeneralManager))
			{
				roles.Add(PPGRole.GeneralManager);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteSupervisor))
			{
				roles.Add(PPGRole.Supervisor);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteOperator))
			{
				roles.Add(PPGRole.Operator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSiteQA))
			{
				roles.Add(PPGRole.QualityAssurance);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddAutomationStation))
			{
				roles.Add(PPGRole.AutomationStation);
			}

			return new JsonResult(roles);
		}

		[HttpGet(Name = nameof(GetLocations))]
		public async Task<JsonResult> GetLocations()
		{
			var sites = (await siteRepository.GetAllSitesAsync()).Select(x => x.SiteName).ToList();

			return new JsonResult(sites);
		}
	}
}