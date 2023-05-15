using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Models.Account;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace PackageTracker.API.Controllers
{
	[Authorize]
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly ILogger<AccountController> logger;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.logger = logger;
		}

		[AllowAnonymous]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Login(LoginRequest request)
		{
			try
			{
				var signInResult = await signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
				
				if (signInResult.Succeeded)
				{
					var user = await userManager.FindByNameAsync(request.Username);
					var role = await userManager.GetRolesAsync(user);

					var response = new LoginResponse
					{
						Succeeded = true,
						AuthorizationTier = role?.Single() ?? string.Empty,
						Site = user.Site,
						ConsecutiveScansAllowed = user.ConsecutiveScansAllowed
					};
					logger.Log(LogLevel.Information, $"Username {user.UserName} logged in. {JsonSerializer.Serialize(response)}");
					return Ok(JsonSerializer.Serialize(response));
				}
				logger.Log(LogLevel.Error, $"Login failed for username: {request.Username}");
				return Ok(JsonSerializer.Serialize(new LoginResponse()));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Exception on login for username: {request.Username}. Exception: {ex}");
				return BadRequest($"Login failed for username: {request.Username}");
			}
		}


		[HttpPost]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return Ok();
		}

		[Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
		[HttpPost]
		public async Task<IActionResult> CreateUser(CreateUserRequest request)
		{
			logger.Log(LogLevel.Information, $"Attempting to create user: {request.Username}");
			var user = new ApplicationUser { UserName = request.Username, Email = request.Email, Site = request.SiteName };
			var result = await userManager.CreateAsync(user, request.Password);
			if (result.Succeeded)
			{
				var currentUser = await userManager.FindByNameAsync(user.UserName);
				await userManager.AddToRoleAsync(currentUser, request.Role);
				await signInManager.SignInAsync(user, isPersistent: false);
				logger.Log(LogLevel.Information, $"User created: {JsonSerializer.Serialize(result)}");
				return Ok($"User created: {JsonSerializer.Serialize(result)}");
			}
			else if (result.Errors.Any())
			{
				var errorMessage = string.Empty;
				foreach (var error in result.Errors)
				{
					errorMessage += error.Description;
				}

				logger.Log(LogLevel.Error, $"Request failed for username: {request.Username}. Exception: {errorMessage}");
				return BadRequest($"Request failed for username: {request.Username}. Exception: {errorMessage}");
			}
			logger.Log(LogLevel.Error, $"Request failed for username: {request.Username}");
			return BadRequest($"Request failed for username: {request.Username}");
		}

		[Authorize(Roles = IdentityDataConstants.SystemAdministrator + "," + IdentityDataConstants.ScannerService)]
		[ProducesResponseType(typeof(GetUserResponse), 200)]
		[HttpGet]
		public async Task<IActionResult> GetUser(string username)
		{
			logger.Log(LogLevel.Information, $"Attempting to get user: {username}");
			var user = await userManager.FindByNameAsync(username);
			if (user != null)
			{
				var role = await userManager.GetRolesAsync(user);
				var response = new GetUserResponse
				{
					Username = username,
					Role = role?.Single() ?? string.Empty,
					SiteName = user.Site,
					Email = user.Email
				};
				logger.Log(LogLevel.Information, $"Username {user.UserName} logged in. {JsonSerializer.Serialize(response)}");
				return Ok(JsonSerializer.Serialize(response));
			}
			logger.Log(LogLevel.Error, $"Request failed for username: {username}");
			return BadRequest($"Request failed for username: {username}");
		}
	}
}