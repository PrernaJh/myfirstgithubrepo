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

	[Authorize(Policy = PPGClaim.WebPortal.UserManagement.AddSubClientUser)]
	public class SubClientUserManagementController : UserManagementBaseController
	{
		private readonly ILogger<SubClientUserManagementController> logger;

		public SubClientUserManagementController(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ISiteRepository siteRepository,
			IClientRepository clientRepository,
			ISubClientRepository subClientRepository,
			IPasswordValidator<ApplicationUser> passwordValidator,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IHttpContextAccessor httpContextAccessor,
			ILogger<SubClientUserManagementController> logger,
			IUserLookupProcessor userLookupProcessor,
			IMemoryCache memoryCache) 
				: base(userManager, roleManager, siteRepository, clientRepository, subClientRepository, 
					  passwordValidator, emailConfiguration, emailService, httpContextAccessor, userLookupProcessor, memoryCache)
		{
			this.logger = logger ?? throw new ArgumentException(nameof(logger));
		}

		public async Task<IActionResult> Index(string id)
		{
			string userSubClient = User.GetSubClient();
			if (string.IsNullOrWhiteSpace(id))
			{
				if (string.IsNullOrWhiteSpace(userSubClient) || userSubClient == SiteConstants.AllSites)
				{
					return RedirectToAction("PickSubClient");
				}
				else
				{
					return RedirectToAction("SubClient", new { id = User.GetSubClient() });
				}
			}
			return RedirectToAction("SubClient", new { id = id });
		}

		public async Task<IActionResult> SubClient(string id)
		{
			var subclients = await subClientRepository.GetSubClientsAsync();
			if (id == "All")
			{
				var assignableSubclients = subclients.Select(x => x.Name).ToList();
				var model = new SubClientViewModel
				{
					ClientName = User.GetSubClient(),
					SubClientName = null,
					AllSubClients = true,
					AssignableSubClients = assignableSubclients
				};
				return View("SubClient", model);
			}
			else
			{
				var client = (await subClientRepository.GetSubClientByNameAsync(id))?.ClientName;
				string userSubClient = User.GetSubClient();

				if (userSubClient == SiteConstants.AllSites)
				{
					var assignableSubclients = subclients.Where(x => x.ClientName == client).Select(x => x.Name).ToList();
					var model = new SubClientViewModel
					{
						ClientName = client,
						SubClientName = id,
						AllSubClients = false,
						AssignableSubClients = assignableSubclients
					};
					return View("SubClient", model);
				}
				else
				{
					var model = new SubClientViewModel
					{
						ClientName = client,
						SubClientName = id,
						AllSubClients = false,
						AssignableSubClients = new List<string> { userSubClient }
					};
					return View("SubClient", model);
				}
			}
		}

		public async Task<IActionResult> PickSubClient()
		{
			var subclients = (await subClientRepository.GetSubClientsAsync()).Select(x => x.Name).ToList();

			return View(subclients);
		}

		[HttpGet(Name = nameof(GetUsers))]
		public async Task<JsonResult> GetUsers(string clientName, string subClientName, bool allUsers = false)
		{
			var listOfUsers = new List<SubClientUserModel>();

			var users = new List<ApplicationUser>();

			if (allUsers)
			{
				users.AddRange(await userManager.Users.Where(x => x.Client == clientName).ToListAsync());
			}
			else
			{
				users.AddRange(await userManager.Users.Where(x => x.SubClient == subClientName).ToListAsync());
			}

			var siteRoles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientAdmin))
			{
				siteRoles.Add(PPGRole.SubClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientUser))
			{
				siteRoles.Add(PPGRole.SubClientWebUser);
			}

			foreach (var user in users)
			{
				var userModel = new SubClientUserModel
				{
					Id = user.Id,
					Deactivated = user.Deactivated,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					SubClient = user.SubClient,
					ResetPassword = user.ResetPassword,
					LastPasswordChangedDate = user.LastPasswordChangedDate,
					TemporaryPassword = user.TemporaryPassword
				};

				var userRoles = await userManager.GetRolesAsync(user);

				if (userRoles.Any() && !userRoles.Intersect(siteRoles).Any())
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

			var subclient = await subClientRepository.GetSubClientByNameAsync(newUser.SubClient);

			if (subclient == null || !StringHelper.Exists(subclient.Id))
			{
				return BadRequest("SubClient does not exist");
			}

			newUser.Site = SiteConstants.AllSites;
			newUser.Client = subclient.ClientName;
			newUser.SubClient = subclient.Name;

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

			var subclient = await subClientRepository.GetSubClientByNameAsync(editUser.SubClient);

			if (subclient == null || !StringHelper.Exists(subclient.Id))
			{
				ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return new JsonResult(new { ErrorMessage = "SubClient does not exist" });
			}

			editUser.Site = SiteConstants.AllSites;
			editUser.Client = subclient.ClientName;
			editUser.SubClient = subclient.Name;

			return await base.UpdateUser(key, editUser);
		}

		[HttpGet(Name = nameof(GetRoles))]
		public JsonResult GetRoles()
		{
			var roles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientAdmin))
			{
				roles.Add(PPGRole.SubClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientUser))
			{
				roles.Add(PPGRole.SubClientWebUser);
			}

			return new JsonResult(roles);
		}
	}
}