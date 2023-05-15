using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Data.Interfaces;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.UserManagement
{
	[Authorize(PPGClaim.WebPortal.Policy.UserManager)]
	public class AdminUserManagementController : UserManagementBaseController
	{
		private readonly ILogger<ClientUserManagementController> logger;

		public AdminUserManagementController(
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

		public async Task<IActionResult> Index()
		{
			var model = new AdminViewModel
			{
				Clients = await GetAssignableClients(),
				SubClients = await GetAssignableSubClients(),
				Sites = await GetAssignableSites(),
				Roles = GetAssignableRoles()
			};

			return View(model);
		}



		[HttpGet(Name = nameof(GetUsers))]
		public async Task<JsonResult> GetUsers()
		{
			var listOfUsers = new List<BaseUserModel>();

			var users = await userManager.Users.ToListAsync();


			var clients = await GetAssignableClients();
			var subClients = await GetAssignableSubClients();
			var sites = await GetAssignableSites();
			var roles = GetAssignableRoles();

            foreach (var user in users)
            {
                var userModel = new BaseUserModel
                {
                    Id = user.Id,
                    SendRecallReleaseAlerts = user.SendRecallReleaseAlerts,
                    Deactivated = user.Deactivated,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Site = user.Site,
                    Client = user.Client,
                    SubClient = user.SubClient,
                    ResetPassword = user.ResetPassword,
                    ConsecutiveScansAllowed = user.ConsecutiveScansAllowed,
                    LastPasswordChangedDate = user.LastPasswordChangedDate,
                    TemporaryPassword = user.TemporaryPassword
                };

				var userRoles = await userManager.GetRolesAsync(user);

                if (!User.IsInRole(PPGRole.SystemAdministrator))
                {
                    if (userRoles.Any() && !userRoles.Intersect(roles).Any())
                        continue;

                    if (!string.IsNullOrWhiteSpace(userModel.Client) && !clients.Contains(userModel.Client))
                        continue;

                    if (!string.IsNullOrWhiteSpace(userModel.SubClient) && !subClients.Contains(userModel.SubClient))
                        continue;

                    if (!string.IsNullOrWhiteSpace(userModel.Site) && !sites.Contains(userModel.Site))
                        continue;
				}
				

				userModel.Role = userRoles.FirstOrDefault();
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

			return await base.UpdateUser(key, editUser);
		}
	}
}