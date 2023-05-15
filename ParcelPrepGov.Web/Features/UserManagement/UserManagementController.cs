using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using ParcelPrepGov.Web.Infrastructure;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.UserManagement
{

	[Authorize(Roles = PPGRole.SystemAdministrator + "," + PPGRole.Administrator)]
	public class UserManagementController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IPasswordHasher<ApplicationUser> passwordHasher;
		private readonly ILogger<UserManagementController> logger;
		private readonly IEmailConfiguration emailConfiguration;
		private readonly IEmailService emailService;
		private readonly IHttpContextAccessor httpContextAccessor;

		public UserManagementController(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IPasswordHasher<ApplicationUser> passwordHasher,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IHttpContextAccessor httpContextAccessor,
			ILogger<UserManagementController> logger)
		{
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
			this.emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
			this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
			this.logger = logger ?? throw new ArgumentException(nameof(logger));
			this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpGet(Name = nameof(GetUsers))]
		public async Task<JsonResult> GetUsers()
		{
			// only sys-admins can access other administrators
			// administrators can only access themselves
			var users = await userManager.Users.Where(x => x.Site == User.GetSite()).ToListAsync();

			var listOfUsers = new List<UserModel>();

			foreach (var user in users)
			{
				var userModel = new UserModel
				{
					Id = user.Id,
					Deactivated = user.Deactivated,
					SendRecallReleaseAlerts = user.SendRecallReleaseAlerts,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					ResetPassword = user.ResetPassword,
					LastPasswordChangedDate = user.LastPasswordChangedDate,
					TemporaryPassword = user.TemporaryPassword
				};

				var userRoles = await userManager.GetRolesAsync(user);

				if (userRoles.Contains(PPGRole.SystemAdministrator))
					if (!User.IsSystemAdministrator())
						continue;

				if (userRoles.Contains(PPGRole.Administrator))
					if (!User.IsSystemAdministrator() && User.Identity.Name.CompareTo(user.UserName) != 0)
						continue;

				userModel.Roles.AddRange(userRoles);
				listOfUsers.Add(userModel);

			}

			return new JsonResult(listOfUsers);
		}

		[HttpPost(Name = nameof(InsertUser))]
		public async Task<IActionResult> InsertUser([FromForm] string values)
		{
			var newUser = new ApplicationUser();

			JsonConvert.PopulateObject(values, newUser);

			if (newUser == null)
			{
				return BadRequest();
			}

			var checkUsernameUser = await userManager.FindByNameAsync(newUser.UserName);
			if (checkUsernameUser != null)
				return BadRequest("Username already taken.");

			if (!string.IsNullOrEmpty(newUser.Email))
			{
				if (!RegexUtility.IsValidEmail(newUser.Email))
				{
					return BadRequest("Invalid email");
				}
				else
				{
					var checkEmailUser = await userManager.FindByEmailAsync(newUser.Email);
					if (checkEmailUser != null)
						return BadRequest("Email already taken.");
				}
			}

			var passwordValidationSucceeded = false;
			IdentityResult identityResult;
			int i = 0;
			while (!passwordValidationSucceeded)
			{
				newUser.TemporaryPassword = IdentityPasswordHelper.GenerateRandomPassword();
				PPGPasswordValidator<ApplicationUser> test = new PPGPasswordValidator<ApplicationUser>(passwordHasher);
				identityResult = await test.ValidateAsync(userManager, null, newUser.TemporaryPassword);
				passwordValidationSucceeded = identityResult.Succeeded;
				i++;
				if (i > 100)
					break;
			}


			newUser.ResetPassword = true;
			// temp password expires in 1 day
			newUser.LastPasswordChangedDate = DateTime.UtcNow.AddDays(Math.Abs(IdentityOptionsHelper.PasswordExpireDays) * (-1) + 1);
			newUser.TemporaryPasswordExpirationDate = DateTime.UtcNow.AddDays(2);

			newUser.Site = User.GetSite();

			var result = await userManager.CreateAsync(newUser, newUser.TemporaryPassword);

			if (result != IdentityResult.Success)
			{
				var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

				return BadRequest(errorMessage);
			}

			var newUserWithRoles = new UserModel();
			JsonConvert.PopulateObject(values, newUserWithRoles);

			var user = await userManager.FindByIdAsync(newUser.Id);

			foreach (var role in newUserWithRoles.Roles)
			{
				// temporary - adding new role webuser
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new IdentityRole(role));

				await userManager.AddToRoleAsync(user, role);
			}

			var tempPassword = Crypto.Encrypt(user.TemporaryPassword);

			var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{Url.Action("ResetPasswordFromEmail", "Account", new { temporaryPassword = tempPassword })}";

			var model = new EmailAccountCreatedViewModel(user.UserName, user.Email, url, 2);

			await SendAccountCreatedEmailAsync(model);

			return new JsonResult(newUser);
		}

		[HttpPut(Name = nameof(UpdateUser))]
		public async Task<JsonResult> UpdateUser([FromForm] string key, [FromForm] string values)
		{
			var user = await userManager.FindByIdAsync(key);
			JsonConvert.PopulateObject(values, user);

			if (user.ResetPassword)
			{
				user.TemporaryPassword = IdentityPasswordHelper.GenerateRandomPassword();
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				await userManager.ResetPasswordAsync(user, token, user.TemporaryPassword);
			}

			await userManager.UpdateAsync(user);

			var userRoles = await userManager.GetRolesAsync(user);

			UserModel userModel = JsonConvert.DeserializeObject<UserModel>(values);

			if (userModel.Roles.Count == 0)
				return new JsonResult(user);

			// remove all existing user roles
			foreach (var role in userRoles)
				await userManager.RemoveFromRoleAsync(user, role);

			// add user to new role/roles
			foreach (var role in userModel.Roles)
				await userManager.AddToRoleAsync(user, role);

			return new JsonResult(user);
		}

		[HttpGet(Name = nameof(GetRoles))]
		public JsonResult GetRoles()
		{
			var roles = PPGRole.ToList();

			if (!User.IsSystemAdministrator())
			{
				var sysAdminRole = roles.FirstOrDefault(r => r == PPGRole.SystemAdministrator);
				if (sysAdminRole != null)
					roles.Remove(sysAdminRole);

				var adminRole = roles.Single(r => r == PPGRole.Administrator);
				if (adminRole != null)
					roles.Remove(adminRole);
			}

			return new JsonResult(roles);
		}

		private async Task SendAccountCreatedEmailAsync(EmailAccountCreatedViewModel model)
		{
			string emailHtmlContent = await this.RenderViewToStringAsync("_EmailAccountCreated", model);

			var fromAddresses = new List<EmailAddress>();
			var fromAddress = new EmailAddress
			{
				Name = emailConfiguration.SmtpUsername,
				Address = emailConfiguration.SmtpUsername
			};
			fromAddresses.Add(fromAddress);

			var toAddresses = new List<EmailAddress>
			{
				new EmailAddress
				{
					Address = model.Email
				}
			};

			EmailMessage msg = new EmailMessage
			{
				Content = emailHtmlContent,
				Subject = "Mail Manifest System Account",
				FromAddresses = fromAddresses,
				ToAddresses = toAddresses
			};

			await emailService.SendAsync(msg, true);
		}
	}
}