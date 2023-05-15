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

	[Authorize(Policy = PPGClaim.WebPortal.UserManagement.AddClientUser)]
	public class ClientUserManagementController : UserManagementBaseController
	{
		private readonly ILogger<ClientUserManagementController> logger;

		public ClientUserManagementController(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ISiteRepository siteRepository,
			IClientRepository clientRepository,
			ISubClientRepository subClientRepository,
			IPasswordValidator<ApplicationUser> passwordValidator,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IHttpContextAccessor httpContextAccessor,
			ILogger<ClientUserManagementController> logger, 
			IUserLookupProcessor userLookupProcessor,
			IMemoryCache memoryCache) 
				: base(userManager, roleManager, siteRepository, clientRepository, subClientRepository, 
					  passwordValidator, emailConfiguration, emailService, httpContextAccessor, userLookupProcessor, memoryCache)
		{
			this.logger = logger ?? throw new ArgumentException(nameof(logger));
		}

		public IActionResult Index(string id)
		{
			string userClient = User.GetClient();
			if (string.IsNullOrWhiteSpace(id))
			{
				if (string.IsNullOrWhiteSpace(userClient) || userClient == SiteConstants.AllSites)
				{
					return RedirectToAction("PickClient");
				}
				else
				{
					return RedirectToAction("Client", new { id = User.GetClient() });
				}
			}
			return RedirectToAction("Client", new { id = id });
		}

		public async Task<IActionResult> Client(string id)
		{
			var clients = (await clientRepository.GetClientsAsync()).Select(x => x.Name).ToList();
			if (id == "All")
			{
				var model = new ClientViewModel
				{
					ClientName = null,
					AllClients = true,
					AssignableClients = clients
				};
				return View("Client", model);
			}
			else
			{
				var model = new ClientViewModel
				{
					ClientName = id,
					AllClients = false,
					AssignableClients = clients
				};
				return View("Client", model);
			}
		}

		[Authorize]
		public async Task<IActionResult> PickClient()
		{
			var clients = (await clientRepository.GetClientsAsync()).Select(x => x.Name).ToList();

			return View(clients);
		}

		[HttpGet(Name = nameof(GetUsers))]
		public async Task<JsonResult> GetUsers(string clientName, bool allUsers = false)
		{
			var listOfUsers = new List<ClientUserModel>();

			var users = await userManager.Users.Where(x => allUsers || x.Client == clientName).ToListAsync();

			var siteRoles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientAdmin))
			{
				siteRoles.Add(PPGRole.ClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientUser))
			{
				siteRoles.Add(PPGRole.ClientWebUser);
			}

			foreach (var user in users)
			{
				var userModel = new ClientUserModel
				{
					Id = user.Id,
					Deactivated = user.Deactivated,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					Client = user.Client,
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

			var client = await clientRepository.GetClientByNameAsync(newUser.SubClient);

			if (client == null || !StringHelper.Exists(client.Id))
			{
				return BadRequest("Client does not exist");
			}

			newUser.Site = SiteConstants.AllSites;
			newUser.Client = client.Name;
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

			var client = await clientRepository.GetClientByNameAsync(editUser.Client);

			if (client == null || !StringHelper.Exists(client.Id))
			{
				ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return new JsonResult(new { ErrorMessage = "Client does not exist" });
			}

			editUser.Site = SiteConstants.AllSites;
			editUser.Client = client.Name;
			editUser.SubClient = SiteConstants.AllSites;

			return await base.UpdateUser(key, editUser);
		}

		[HttpGet(Name = nameof(GetRoles))]
		public JsonResult GetRoles()
		{
			var roles = new List<string>();

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientAdmin))
			{
				roles.Add(PPGRole.ClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientUser))
			{
				roles.Add(PPGRole.ClientWebUser);
			}

			return new JsonResult(roles);
		}

		[HttpGet(Name = nameof(GetClients))]
		public async Task<JsonResult> GetClients()
		{
			var clients = Task.Run(() => clientRepository.GetClientsAsync()).GetAwaiter().GetResult().Select(x => x.Name).ToList();

			return new JsonResult(clients);
		}
	}
}