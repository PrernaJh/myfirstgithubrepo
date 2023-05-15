using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Globals;
using ParcelPrepGov.Web.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.UserManagement
{
    public abstract class UserManagementBaseController : Controller
	{
		protected readonly UserManager<ApplicationUser> userManager;
		protected readonly RoleManager<IdentityRole> roleManager;
		protected readonly ISiteRepository siteRepository;
		protected readonly IClientRepository clientRepository;
		protected readonly ISubClientRepository subClientRepository;
		//protected readonly IPasswordHasher<ApplicationUser> passwordHasher; 
		protected readonly IPasswordValidator<ApplicationUser> passwordValidator;
		protected readonly IEmailConfiguration emailConfiguration;
		protected readonly IEmailService emailService;
		protected readonly IHttpContextAccessor httpContextAccessor;
		protected readonly IMemoryCache memoryCache;
		protected readonly IUserLookupProcessor userLookupProcessor;

		public UserManagementBaseController(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ISiteRepository siteRepository,
			IClientRepository clientRepository,
			ISubClientRepository subClientRepository,
			IPasswordValidator<ApplicationUser> passwordValidator,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
			IHttpContextAccessor httpContextAccessor,
			IUserLookupProcessor userLookupProcessor,
			IMemoryCache memoryCache)
		{
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			this.siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
			this.clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
			this.subClientRepository = subClientRepository ?? throw new ArgumentNullException(nameof(subClientRepository));
			this.passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
			this.emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
			this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
			this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.userLookupProcessor = userLookupProcessor ?? throw new ArgumentNullException(nameof(userLookupProcessor));
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		}

		public List<string> ValidateUser(BaseUserModel user)
		{
			var errors = new List<string>();

			bool isGlobalSite = user.Site == SiteConstants.AllSites;
			bool isGlobalClient = user.Client == SiteConstants.AllSites;
			bool isGlobalSubClient = user.SubClient == SiteConstants.AllSites;

			var assignableSites = Task.Run(async () => await GetAssignableSites()).Result;
			var assignableClients = Task.Run(async () => await GetAssignableClients()).Result;
			var assignableSubClients = Task.Run(async () => await GetAssignableSubClients()).Result;


			if (!assignableSites.Contains(user.Site))
			{
				errors.Add($"You can not assign Site: {user.Site}.");
			}

			if (!assignableClients.Contains(user.Client))
			{
				errors.Add($"You can not assign Client: {user.Client}.");
			}

			if (!assignableSubClients.Contains(user.SubClient))
			{
				errors.Add($"You can not assign SubClient: {user.SubClient}.");
			}

			if (!isGlobalSubClient)
			{
				var subclient = Task.Run(async () => await subClientRepository.GetSubClientByNameAsync(user.SubClient)).Result;
				if (subclient.ClientName != user.Client)
				{
					errors.Add($"Invalid Client of {user.Client} for SubClient {user.SubClient} which has a Client of {subclient.ClientName}.");
				}
			}

			if (!string.IsNullOrEmpty(user.Role))
			{
                switch (user.Role)
				{
					case PPGRole.SystemAdministrator:
						break;
					case PPGRole.Administrator:
						break;
					case PPGRole.GeneralManager:
						if (isGlobalSite)
						{
							errors.Add($"{PPGRole.GeneralManager} can not have Site: {SiteConstants.AllSites}.");
						}
						if (!isGlobalClient)
						{
							errors.Add($"{PPGRole.GeneralManager} must have Client: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.GeneralManager} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.Supervisor:
						if (isGlobalSite)
						{
							errors.Add($"{PPGRole.Supervisor} can not have Site: {SiteConstants.AllSites}.");
						}
						if (!isGlobalClient)
						{
							errors.Add($"{PPGRole.Supervisor} must have Client: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.Supervisor} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.Operator:
						if (isGlobalSite)
						{
							errors.Add($"{PPGRole.Operator} can not have Site: {SiteConstants.AllSites}.");
						}
						if (!isGlobalClient)
						{
							errors.Add($"{PPGRole.Operator} must have Client: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.Operator} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.QualityAssurance:
						if (isGlobalSite)
						{
							errors.Add($"{PPGRole.QualityAssurance} can not have Site: {SiteConstants.AllSites}.");
						}
						if (!isGlobalClient)
						{
							errors.Add($"{PPGRole.QualityAssurance} must have Client: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.QualityAssurance} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.AutomationStation:
						if (isGlobalSite)
						{
							errors.Add($"{PPGRole.AutomationStation} can not have Site: {SiteConstants.AllSites}.");
						}
						if (!isGlobalClient)
						{
							errors.Add($"{PPGRole.AutomationStation} must have Client: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.AutomationStation} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.ClientWebAdministrator:
						if (!isGlobalSite)
						{
							errors.Add($"{PPGRole.ClientWebAdministrator} must have Site: {SiteConstants.AllSites}.");
						}
						if (isGlobalClient)
						{
							errors.Add($"{PPGRole.ClientWebAdministrator} can not have Client: {SiteConstants.AllSites}");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.ClientWebAdministrator} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.ClientWebUser:
						if (!isGlobalSite)
						{
							errors.Add($"{PPGRole.ClientWebUser} must have Site: {SiteConstants.AllSites}.");
						}
						if (isGlobalClient)
						{
							errors.Add($"{PPGRole.ClientWebUser} can not have Client: {SiteConstants.AllSites}");
						}
						if (!isGlobalSubClient)
						{
							errors.Add($"{PPGRole.ClientWebUser} must have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.SubClientWebAdministrator:
						if (!isGlobalSite)
						{
							errors.Add($"{PPGRole.SubClientWebAdministrator} must have Site: {SiteConstants.AllSites}.");
						}
						if (isGlobalClient)
						{
							errors.Add($"{PPGRole.SubClientWebAdministrator} can not have Client: {SiteConstants.AllSites}.");
						}
						if (isGlobalSubClient)
						{
							errors.Add($"{PPGRole.SubClientWebAdministrator} can not have SubClient: {SiteConstants.AllSites}.");
						}
						break;
					case PPGRole.SubClientWebUser:
						if (isGlobalClient)
						{
							errors.Add($"{PPGRole.SubClientWebUser} can not have Client: {SiteConstants.AllSites}.");
						}
						if (isGlobalSubClient)
						{
							errors.Add($"{PPGRole.SubClientWebUser} can not have SubClient: {SiteConstants.AllSites}.");
						}
						if (!isGlobalSite)
						{
							errors.Add($"{PPGRole.SubClientWebUser} must have Site: {SiteConstants.AllSites}.");
						}
						break;
				}
			}
            else
            {
                errors.Add($"Role is Required");
			}

			return errors;
		}

		public async Task<List<string>> GetAssignableSites()
		{
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSubClient = User.GetSubClient() == SiteConstants.AllSites;

			if (isGlobalSite)
			{
				if (isGlobalClient && isGlobalSubClient)
				{
					var sites = (await siteRepository.GetAllSitesAsync()).Select(x => x.SiteName).ToList();
					sites.Insert(0, SiteConstants.AllSites);
					return sites;
				}
				else
				{
					var sites = new List<string> { SiteConstants.AllSites };
					return sites;
				}
			}
			else
			{
				var sites = new List<string> { User.GetSite() };
				return sites;
			}
		}

		public async Task<List<string>> GetAssignableClients()
		{
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;

			if (!isGlobalSite)
			{
				var clients = new List<string> { SiteConstants.AllSites };
				return clients;
			}
			else if (isGlobalClient)
			{
				var clients = (await clientRepository.GetClientsAsync()).Select(x => x.Name).ToList();
				clients.Insert(0, SiteConstants.AllSites);
				return clients;
			}
			else
			{
				var clients = new List<string> { User.GetClient() };
				return clients;
			}
		}

		public async Task<List<string>> GetAssignableSubClients()
		{
			bool isGlobalSite = User.GetSite() == SiteConstants.AllSites;
			bool isGlobalClient = User.GetClient() == SiteConstants.AllSites;
			bool isGlobalSubClient = User.GetSubClient() == SiteConstants.AllSites;

			if (!isGlobalSite)
			{
				var subclients = new List<string> { SiteConstants.AllSites };
				return subclients;
			}
			if (isGlobalClient)
			{
				var subclients = (await subClientRepository.GetSubClientsAsync()).Select(x => x.Name).ToList();
				subclients.Insert(0, SiteConstants.AllSites);
				return subclients;
			}
			else if (isGlobalSubClient)
			{
				var subclients = (await subClientRepository.GetSubClientsAsync())
					.Where(x => x.ClientName == User.GetClient())
					.Select(x => x.Name).ToList();
				subclients.Insert(0, SiteConstants.AllSites);
				return subclients;
			}
			else
			{
				var subclients = new List<string> { User.GetSubClient() };
				return subclients;
			}
		}

		public List<string> GetAssignableRoles()
		{
			var roles = new List<string>();

            // Admin Users
            if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSystemAdmin))
            {
                roles.Add(PPGRole.SystemAdministrator);
            }

            if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddAdministrator))
			{
				roles.Add(PPGRole.Administrator);
			}

			// Client Users
			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientAdmin))
			{
				roles.Add(PPGRole.ClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientUser))
			{
				roles.Add(PPGRole.ClientWebUser);
			}
			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientPackageSearchUser))
			{
				roles.Add(PPGRole.ClientWebPackageSearchUser);
			}
			// SubClient Users
			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientAdmin))
			{
				roles.Add(PPGRole.SubClientWebAdministrator);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddSubClientUser))
			{
				roles.Add(PPGRole.SubClientWebUser);
			}

			// Site Users
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

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddFSCWebFinancialUser))
			{
				roles.Add(PPGRole.FSCWebFinancialUser);
			}
			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddTransportationUser))
			{
				roles.Add(PPGRole.TransportationUser);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddClientWebFinancialUser))
			{
				roles.Add(PPGRole.ClientWebFinancialUser);
			}

			if (User.HasClaimType(PPGClaim.WebPortal.UserManagement.AddCustomerServiceUser))
			{
				roles.Add(PPGRole.CustomerService);
			}

			return roles;
		}

		public async Task<IActionResult> InsertUser(BaseUserModel newUser)
		{
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

			int i = 0;
			while (!passwordValidationSucceeded)
			{
				newUser.TemporaryPassword = IdentityPasswordHelper.GenerateRandomPassword();
				IPasswordValidator<ApplicationUser> test = passwordValidator;
				IdentityResult identityResult = await test.ValidateAsync(userManager, null, newUser.TemporaryPassword);
				passwordValidationSucceeded = identityResult.Succeeded;
				i++;
				if (i > 100)
					break;
			}

			newUser.ResetPassword = true;
			// temp password expires in 1 day
			newUser.LastPasswordChangedDate = DateTime.UtcNow.AddDays(Math.Abs(IdentityOptionsHelper.PasswordExpireDays) * (-1) + 1);

            ApplicationUser newAppUser = new ApplicationUser
            {
                UserName = newUser.UserName,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                ConsecutiveScansAllowed = newUser.ConsecutiveScansAllowed ?? 0,
                SendRecallReleaseAlerts = newUser.SendRecallReleaseAlerts.GetValueOrDefault(),
                Deactivated = newUser.Deactivated.GetValueOrDefault(),
                ResetPassword = newUser.ResetPassword.GetValueOrDefault(),
                LastPasswordChangedDate = newUser.LastPasswordChangedDate.GetValueOrDefault(),
                TemporaryPassword = newUser.TemporaryPassword,
                Site = newUser.Site,
                Client = newUser.Client,
                SubClient = newUser.SubClient,
                TemporaryPasswordExpirationDate = DateTime.UtcNow.AddDays(2)
            };

			var roleErrors = ValidateUser(newUser);

			if (roleErrors.Any())
			{
				var errorMessage = string.Join(" ", roleErrors);

				return BadRequest(errorMessage);
			}

			var result = await userManager.CreateAsync(newAppUser, newUser.TemporaryPassword);
			await userLookupProcessor.UpsertUser(newAppUser);

			if (result != IdentityResult.Success)
			{
				var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

				return BadRequest(errorMessage);
			}

			var user = await userManager.FindByIdAsync(newAppUser.Id);

			if (await roleManager.RoleExistsAsync(newUser.Role))
			{
				await userManager.AddToRoleAsync(user, newUser.Role);
			}
            else
            {
				// Shouldn't ever happen unless it got deleted from page open to save
                var errorMessage = $"{newUser.Role} does not exist";

                return BadRequest(errorMessage);
			}
			

			if (newUser.SendEmail.GetValueOrDefault() && !string.IsNullOrEmpty(user.Email))
			{
				var tempPassword = Crypto.Encrypt(user.TemporaryPassword);

				var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{Url.Action("ResetPasswordFromEmail", "Account", new { temporaryPassword = tempPassword })}";

				var model = new EmailAccountCreatedViewModel(user.UserName, user.Email, url, 2);

				await SendAccountCreatedEmailAsync(model);
			}


			return new JsonResult(newUser);
		}

		public async Task<JsonResult> UpdateUser(string key, BaseUserModel editUser)
		{			
			var user = await userManager.FindByIdAsync(key);
			var userRoles = await userManager.GetRolesAsync(user);

			ClearDropDownCachingKeys(user.UserName);

            // Dev Express DataGrid Form only returns modified values
            var updatedUser = new BaseUserModel
            {
                Id = editUser.Id ?? user.Id,
                SendRecallReleaseAlerts = editUser.SendRecallReleaseAlerts ?? user.SendRecallReleaseAlerts,
                Deactivated = editUser.Deactivated ?? user.Deactivated,
                Email = editUser.Email ?? user.Email,
                FirstName = editUser.FirstName ?? user.FirstName,
                LastName = editUser.LastName ?? user.LastName,
                UserName = editUser.UserName ?? user.UserName,
                Site = editUser.Site ?? user.Site,
                Client = editUser.Client ?? user.Client,
                SubClient = editUser.SubClient ?? user.SubClient,
                ConsecutiveScansAllowed = editUser.ConsecutiveScansAllowed ?? user.ConsecutiveScansAllowed,
                ResetPassword = editUser.ResetPassword ?? user.ResetPassword,
                LastPasswordChangedDate = editUser.LastPasswordChangedDate ?? user.LastPasswordChangedDate,
                TemporaryPassword =  editUser.TemporaryPassword ??= user.TemporaryPassword,
                Role =  editUser.Role ?? userRoles.FirstOrDefault()
            };        
			
			var roleErrors = ValidateUser(updatedUser);

			if (roleErrors.Any())
			{
				var errorMessage = string.Join("\n", roleErrors);

				ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return new JsonResult(new { ErrorMessage = errorMessage });
			}

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Client = updatedUser.Client;
            user.SubClient = updatedUser.SubClient;
            user.Site = updatedUser.Site;
            user.ConsecutiveScansAllowed = updatedUser.ConsecutiveScansAllowed.GetValueOrDefault();
            user.SendRecallReleaseAlerts = updatedUser.SendRecallReleaseAlerts.GetValueOrDefault();
            user.Deactivated = updatedUser.Deactivated.GetValueOrDefault();
            user.ResetPassword = updatedUser.ResetPassword.GetValueOrDefault();

			//Use edit user because we want to know if it was newly changed
			if (AllowUserToResetPassword(editUser, userRoles) == true)
            {
				user.TemporaryPassword = IdentityPasswordHelper.GenerateRandomPassword();
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				user.TemporaryPasswordExpirationDate = DateTime.UtcNow.AddDays(2);
				var resetResult = await userManager.ResetPasswordAsync(user, token, user.TemporaryPassword);

				if (!resetResult.Succeeded)
				{
					var errorMessage = "Generating Random Password Failed. Try again. \n" + string.Join("\n", resetResult.Errors.Select(x => x.Description));

					ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
					return new JsonResult(new { ErrorMessage = errorMessage });
				}				            
			}
            else
            {
				if (user.ResetPassword)
				{
					var errorMessage = "Password reset not allowed";
					ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
					return new JsonResult(new { ErrorMessage = errorMessage });
				}
			}

			await userManager.UpdateAsync(user);
			await userLookupProcessor.UpsertUser(user);

			if (string.IsNullOrEmpty(updatedUser.Role))
				return new JsonResult(user);

			// remove all existing user roles
			foreach (var role in userRoles)
				await userManager.RemoveFromRoleAsync(user, role);

			// add user to new role/roles
			await userManager.AddToRoleAsync(user, updatedUser.Role);

            if (editUser.SendEmail.GetValueOrDefault() && !string.IsNullOrEmpty(user.Email) && user.TemporaryPassword != null)
            {
                var tempPassword = Crypto.Encrypt(user.TemporaryPassword);

                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{Url.Action("ResetPasswordFromEmail", "Account", new { temporaryPassword = tempPassword })}";

                var model = new EmailAccountCreatedViewModel(user.UserName, user.Email, url, 2);

                await SendAccountCreatedEmailAsync(model);
            }

			return new JsonResult(user);
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
		private void ClearDropDownCachingKeys(string username)
        {
			var subClientsCacheKey = $"{username}_{CacheKeys.SubClients}_SelectBoxData";
			var clientsCacheKey = $"{username}_{CacheKeys.Clients}_SelectBoxData";
			var siteCacheKey = $"{username}_{CacheKeys.Sites}_SelectBoxData";
			var keys = new string[]{ subClientsCacheKey, clientsCacheKey, siteCacheKey };
            foreach (var item in keys)
            {
				var cacheItem = memoryCache.Get(item);
				if(cacheItem != null)
                {
					memoryCache.Remove(item);
                }
            }
		}
		private bool AllowUserToResetPassword(BaseUserModel editUser, IList<string> EditUserRoles)
        {
            bool DoAllowReset;

            if (User.IsInRole(PPGRole.SystemAdministrator) == false && EditUserRoles.Contains(PPGRole.AutomationStation) && editUser.ResetPassword == true)
            {
				DoAllowReset = false;				
				// TODO: return message to the user?
				// CANNOT RESET PASSWORD ON AUTOMATIONSTATION ACCOUNT FOR NON SYSTEM ADMINS
            }
			else
            {
				DoAllowReset = editUser.ResetPassword.GetValueOrDefault();
			}			

			return DoAllowReset;
        }
	}
}
