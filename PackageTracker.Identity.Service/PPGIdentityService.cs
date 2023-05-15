using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PackageTracker.Domain.Models;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service.Configuration;
using PackageTracker.Identity.Service.Interfaces;
using PackageTracker.Identity.Service.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service
{
	public class PPGIdentityService : IIdentityService
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
		private JwtConfiguration _jwtConfiguration;
		private readonly ILogger<AuthenticationService> logger;
		private readonly PackageTrackerIdentityDbContext _dbContext;
		private readonly Microsoft.AspNetCore.Identity.IPasswordValidator<ApplicationUser> _passwordValidator;

		public PPGIdentityService(
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IPasswordHasher<ApplicationUser> passwordHasher,
			RoleManager<IdentityRole> roleManager,
			IOptionsMonitor<JwtConfiguration> jwtConfigurationOptions,
			PackageTrackerIdentityDbContext dbContext,
			Microsoft.AspNetCore.Identity.IPasswordValidator<ApplicationUser> passwordValidator,
			ILogger<AuthenticationService> logger)
		{
			_signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
			_passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
			this.logger = logger ?? throw new ArgumentException(nameof(logger));

			_jwtConfiguration = jwtConfigurationOptions.CurrentValue;

			jwtConfigurationOptions.OnChange(config =>
			{
				_jwtConfiguration = config;
				logger.Log(LogLevel.Information, "The Jwt configuration has been updated.");
			});
		}

		public async Task<GenericResponse<SignInResponse>> SignInAsync(SignInRequest signInRequest)
		{
			var user = await _userManager.FindByNameAsync(signInRequest.Username);
			var roles = await _userManager.GetRolesAsync(user);

			if (user == null)
			{
				logger.Log(LogLevel.Warning, $"Invalid sign in attempt by Username: {signInRequest.Username}");
				return new GenericResponse<SignInResponse>(new SignInResponse(), success: false, message: "Invalid Sign in attempt.");
			}

			// Ensure the user is allowed to sign in
			if (!await _signInManager.CanSignInAsync(user))
			{
				logger.Log(LogLevel.Warning, $"User not allowed to sign in - Username: {signInRequest.Username}");
				return new GenericResponse<SignInResponse>(new SignInResponse(), success: false, message: "Sign in not allowed.");
			}

			// Ensure the user is not already locked out
			if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
			{
				logger.Log(LogLevel.Warning, $"User account is locked - Username: {signInRequest.Username}");
				return new GenericResponse<SignInResponse>(new SignInResponse(), success: false, message: "Account is locked.");
			}

			// Ensure that users that sign in via OpenID Connect are not allowed to sign in directly on the site
			if (string.IsNullOrEmpty(user.PasswordHash) && 
				(await _userManager.GetLoginsAsync(user)).Any(userLoginInfo => userLoginInfo.LoginProvider == OpenIdConnectDefaults.AuthenticationScheme))
            {
				logger.Log(LogLevel.Warning, 
					$"OpenID Connect user (i.e. FedEx user) attempted to sign in directly through the site - Username: {signInRequest.Username}");
				return new GenericResponse<SignInResponse>(new SignInResponse(), success: false, message: "Invalid Sign in method. Try signing in as a FedEx user.");
            }

			// Ensure the password is valid
			if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, signInRequest.Password) != PasswordVerificationResult.Success)
			{
				if (_userManager.SupportsUserLockout)
				{
					await _userManager.AccessFailedAsync(user);
				}
				logger.Log(LogLevel.Warning, $"Invalid sign in attempt by Username: {signInRequest.Username}");
				return new GenericResponse<SignInResponse>(new SignInResponse(), success: false, message: "Invalid sign in attempt.");
			}

			// Reset the lockout count
			if (_userManager.SupportsUserLockout)
			{
				await _userManager.ResetAccessFailedCountAsync(user);
			}

			if (user.Deactivated)
			{
				var signinResponse = new SignInResponse(deactivated: true);
				return new GenericResponse<SignInResponse>(signinResponse, success: true, message: "Account deactivated.");
			}

			if (user.ResetPassword)
			{
				var signinResponse = new SignInResponse(resetPassword: true);
				return new GenericResponse<SignInResponse>(signinResponse, success: true, message: "Password reset required.");
			}

			var IsAutomationStation = roles.Any(x => x.Contains(PPGRole.AutomationStation));

			if (IsAutomationStation == false && user.LastPasswordChangedDate.AddDays(IdentityOptionsHelper.PasswordExpireDays) < DateTime.UtcNow)
			{
				var signinResponse = new SignInResponse(passwordExpired: true);
				return new GenericResponse<SignInResponse>(signinResponse, success: true, message: "Password expired.");
			}			

			#region jwt
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key));
			var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);


			await _signInManager.SignInAsync(user, true);

			var userPrinciple = await _signInManager.CreateUserPrincipalAsync(user);

			var tokenDescriptor = new JwtSecurityToken(
				issuer: _jwtConfiguration.Issuer,
				audience: _jwtConfiguration.Audience,
				claims: userPrinciple.Claims,
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddMinutes(_jwtConfiguration.MinutesToExpiration),
				signingCredentials: signingCredentials);

			var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
			#endregion

			logger.Log(LogLevel.Warning, $"User: {user.UserName} signed in.");


			return new GenericResponse<SignInResponse>(new SignInResponse(token: token, claimsPrincipal: userPrinciple), success: true);

		}

		public async Task<GenericResponse<PasswordResetResponse>> ResetPassword(PasswordResetRequest passwordResetRequest)
		{
			var user = await _userManager.FindByNameAsync(passwordResetRequest.Username);

			if (user == null)
			{
				logger.Log(LogLevel.Warning, $"Invalid password reset attempt by Username: {passwordResetRequest.Username}");
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false, message: "Invalid password reset attemp.");
			}

			//var newPasswordHash = _passwordHasher.HashPassword(user, passwordResetRequest.NewPassword);

			//var result = await _userManager.ChangePasswordAsync(user, passwordResetRequest.CurrentPassword, newPasswordHash);

			var result = await _userManager.ChangePasswordAsync(user, passwordResetRequest.CurrentPassword, passwordResetRequest.NewPassword);

			if (!result.Succeeded)
			{
				logger.Log(LogLevel.Warning, $"Invalid password reset attempt by Username: {passwordResetRequest.Username}");

				var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false, message: new HtmlString(errorMessage).Value);
			}

			user.LastPasswordChangedDate = DateTime.UtcNow;
			user.TemporaryPassword = null;
			user.ResetPassword = false;
			result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(redirectUrl: passwordResetRequest.RedirectUrl), success: true);
			}
			else
			{
				logger.Log(LogLevel.Warning, $"UserManager.ChangePasswordAsync succeed but UserManager.UpdateItemAsync failed for Username: {passwordResetRequest.Username}");

				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false);
			}
		}

		private async Task<ClaimsPrincipal> CreatePrincipal(ApplicationUser user, bool includeRoles = false)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(PPGClaim.Site, user.Site ?? string.Empty),
				new Claim(PPGClaim.Client, user.Client ?? string.Empty),
				new Claim(PPGClaim.SubClient, user.SubClient ?? string.Empty),
				new Claim(ClaimTypes.Email, user.Email?? string.Empty)
			};

			if (user != null)
			{
				var userClaims = await _userManager.GetClaimsAsync(user);

				claims.AddRange(userClaims.ToList());
			}


			if (includeRoles)
			{
				var userRoles = await _userManager.GetRolesAsync(user);
				claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)).ToList());

				foreach (var userRole in userRoles)
				{
					IdentityRole role = await _roleManager.FindByNameAsync(userRole);

					var currentClaims = _roleManager.GetClaimsAsync(role).Result.ToList();

					claims.AddRange(currentClaims);
				}
			}

			var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			return new ClaimsPrincipal(userIdentity);
		}

		public async Task<GenericResponse<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest)
		{

			var user = await _userManager.FindByNameAsync(forgotPasswordRequest.Username);

			if (user == null)
			{
				logger.Log(LogLevel.Warning, $"Invalid forgot password request by Username: {forgotPasswordRequest.Username}");

				return new GenericResponse<ForgotPasswordResponse>(new ForgotPasswordResponse(), success: false, message: "Invalid forgot password request.");
			}

			using (var randomNumberGenerator = new RNGCryptoServiceProvider())
			{
				var securityCodeData = new byte[128];
				randomNumberGenerator.GetBytes(securityCodeData);
				user.SecurityCode = Convert.ToBase64String(securityCodeData);
			}

			user.SecurityCodeExpirationDate = DateTime.UtcNow.AddMinutes(33);

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				logger.Log(LogLevel.Warning, $"Invalid forgot password request by Username: {forgotPasswordRequest.Username}");

				var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

				return new GenericResponse<ForgotPasswordResponse>(new ForgotPasswordResponse(), success: false, message: errorMessage);
			}

			var forgotPasswordResponse = new ForgotPasswordResponse
			{
				Name = user.FirstName,
				Email = user.Email,
				SecurityCode = user.SecurityCode
			};


			return new GenericResponse<ForgotPasswordResponse>(forgotPasswordResponse, success: true, message: "You have successfully requested a new password. Check your e-mail.");
		}

		public async Task<GenericResponse<PasswordResetResponse>> SetPassword(string securityCode, string password)
		{
			if (string.IsNullOrWhiteSpace(securityCode))
			{
				throw new ArgumentNullException(nameof(securityCode));
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException(nameof(password));
			}

			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.SecurityCode == securityCode && u.SecurityCodeExpirationDate >= DateTime.UtcNow);

			if (user == null)
			{
				logger.Log(LogLevel.Warning, $"Invalid forgot password reset attempt by securityCode: {securityCode}");
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false, message: "Invalid password reset request.");
			}

			var identityResult = await _passwordValidator.ValidateAsync(_userManager, null, password);

			if (!identityResult.Succeeded)
			{
				var msg = string.Join(" ", identityResult.Errors.ToList().Select(x => x.Description));
				logger.Log(LogLevel.Warning, $"Invalid forgot password reset attempt by Username: {user.UserName}");
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false, message: msg); ;
			}

			user.SecurityCode = null;
			user.PasswordHash = _passwordHasher.HashPassword(user, password);

			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: true);
			}
			else
			{
				logger.Log(LogLevel.Warning, $"Invalid forgot password reset attempt by Username: {user.UserName}");
				return new GenericResponse<PasswordResetResponse>(new PasswordResetResponse(), success: false, message: "Invalid password reset request.");
			}
		}

		public async Task<GenericResponse<string>> GetUsernameByTemporaryPasswordAsync(string temporaryPassword)
		{
			if (string.IsNullOrWhiteSpace(temporaryPassword))
			{
				throw new ArgumentNullException(nameof(temporaryPassword));
			}

			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.TemporaryPassword == temporaryPassword);
			if (user == null) return new GenericResponse<string>(string.Empty, success: false);

			if (user.TemporaryPasswordExpirationDate > DateTime.UtcNow)
			{
				return new GenericResponse<string>(user.UserName, success: true);
			}

			// temp password expired
			return new GenericResponse<string>(string.Empty, success: false);
		}
	}
}
