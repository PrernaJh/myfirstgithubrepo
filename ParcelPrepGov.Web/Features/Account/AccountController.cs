using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Domain.Models;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service.Interfaces;
using PackageTracker.Identity.Service.Models;
using ParcelPrepGov.Web.Features.Account.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Globals;
using ParcelPrepGov.Web.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ParcelPrepGov.Web.Features.Account
{
    [Authorize]
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IIdentityService _identityService;
		private readonly IEmailConfiguration _emailConfiguration;
		private readonly IEmailService _emailService;
		private readonly ILogger<AccountController> _logger;
		private readonly IAuthorizationService _authorizationService;
		private readonly IConfiguration _config;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,	
            UserManager<ApplicationUser> userManager,
            IIdentityService identityService,
			IEmailConfiguration emailConfiguration,
			IEmailService emailService,
            IAuthorizationService authorizationService,
            ILogger<AccountController> logger, 
			IConfiguration config)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
			_emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _authorizationService = authorizationService;
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _config = config;
        }

        #region SignIn
        [AllowAnonymous]
		[HttpGet]
		public IActionResult SignIn(string returnUrl = "/")
		{
			var is64bit = 8 == IntPtr.Size;
			_logger.LogInformation($"Web Service: Is 64 bit: {is64bit}");

			//if (User.Identity.IsAuthenticated)
			//{
			//    return RedirectToAction("Index", "Dashboard");
			//}

			return View(new SignInRequest { RedirectUrl = returnUrl });
		}

		[AllowAnonymous]
		[HttpPost(Name = nameof(SignIn))]
		public async Task<IActionResult> SignIn(SignInRequest signInRequest)
		{
			if (!ModelState.IsValid)
            {
				return View();
            }

			_logger.LogInformation($"User: {signInRequest.Username} signing in.");

			await _signInManager.SignOutAsync();

			GenericResponse<SignInResponse> signinResponse = await _identityService.SignInAsync(signInRequest);

			var redirectUrl = signInRequest.RedirectUrl ?? Url.Content("~/dashboard");

			if (signinResponse.Success)
			{
				if (signinResponse.Data.Deactivated)
				{
					return RedirectToAction(nameof(AccountController.Deactivated));
				}
				else if (signinResponse.Data.PasswordExpired)
				{
					TempData["Toast"] = $"{signinResponse.Message},{Toast.Info}";
					TempData["RedirectUrl"] = signInRequest.RedirectUrl;
					TempData["PasswordExpiredUsername"] = signInRequest.Username;
					return RedirectToAction(nameof(AccountController.PasswordExpired));
				}
				else if (signinResponse.Data.ResetPassword)
				{
					TempData["ResetPasswordUsername"] = signInRequest.Username;
					TempData["Toast"] = $"{signinResponse.Message},{Toast.Info}";
					return RedirectToAction(nameof(AccountController.ResetPassword));
				}

                return LocalRedirect(redirectUrl);
			}

			TempData["Toast"] = $"{signinResponse.Message},{Toast.Error}";
			return View();
		}

		//[AllowAnonymous]
		//public async Task<IActionResult> SignInPopup()
  //      {
		//	return PartialView("_SignInPopup");
  //      }


		[AllowAnonymous]
		[HttpPost(Name = nameof(AjaxSignIn))]
		public async Task<IActionResult> AjaxSignIn(SignInRequest signInRequest)
		{
			_logger.LogInformation($"User: {signInRequest.Username} signing in.");

			await _signInManager.SignOutAsync();
			await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

			GenericResponse<SignInResponse> signinResponse = await _identityService.SignInAsync(signInRequest);

			if (signinResponse.Success)
			{
				if (signinResponse.Data.Deactivated)
				{
					return Json(new
					{
						success = false,
						message = $"{signinResponse.Message}"
					});
				}
				else if (signinResponse.Data.PasswordExpired)
				{
					return Json(new
					{
						success = false,
						message = $"{signinResponse.Message}"
					}); 
				}
				else if (signinResponse.Data.ResetPassword)
				{
					return Json(new
					{
						success = false,
						message = $"{signinResponse.Message}"
					});
				}

				return Json(new
				{
					success = true,
					message = $"{signinResponse.Message}"
				}); 
			}

			return BadRequest(new { message = $"{signinResponse.Message},{Toast.Error}" });
		}

		//[AllowAnonymous]
		//[HttpPost(Name = nameof(SignInPopup))]
		//public async Task<IActionResult> SignInPopup(SignInRequest signInRequest)
		//{
		//    _logger.LogInformation($"User: {signInRequest.Username} signing in.");

		//    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		//    GenericResponse<SignInResponse> signinResponse = await _identityService.SignInAsync(signInRequest);

		//    var response = signinResponse.Data;

		//    if (signinResponse.Success)
		//    {
		//        await HttpContext.SignInAsync(
		//            CookieAuthenticationDefaults.AuthenticationScheme,
		//            signinResponse.Data.ClaimsPrincipal,
		//            new AuthenticationProperties
		//            {
		//                IsPersistent = true,
		//                ExpiresUtc = DateTime.UtcNow.AddMinutes(5)
		//            });



		//        return new JsonResult(new { Success = true, Message = "Sucessful Login" });
		//    }

		//    return new JsonResult(new { Success = true, Message = $"{signinResponse.Message},{Toast.Error}" });

		//}
		#endregion

		[AllowAnonymous]
		public async Task<IActionResult> SignOut()
		{
            await _signInManager.SignOutAsync();
            var url = await new PackageTracker.Domain.Utilities.HelpDeskAutoLoginUtility(_userManager, _config).GenerateHelpDeskLogoutURLAsync();

			return Redirect(url);
		}

		[AllowAnonymous]
		public IActionResult ResetPassword()
		{
			var model = new PasswordResetRequest
			{
				Username = TempData["ResetPasswordUsername"]?.ToString(),
				RedirectUrl = TempData["RedirectUrl"] != null ? TempData["RedirectUrl"] as string : Url.Content("~/")
			};

			return View(model);
		}

		[AllowAnonymous]
		[HttpGet(Name = nameof(ResetPasswordFromEmail))]
		public async Task<IActionResult> ResetPasswordFromEmail([FromQuery] string temporaryPassword)
		{
			var tempPassword = Crypto.Decrypt(temporaryPassword);

			var response = await _identityService.GetUsernameByTemporaryPasswordAsync(tempPassword);

			if (response.Success)
			{
				var model = new PasswordResetRequest
				{
					Username = response.Data,
					CurrentPassword = tempPassword,
					RedirectUrl = TempData["RedirectUrl"] != null ? TempData["RedirectUrl"] as string : Url.Content("~/")
				};

				return View("ResetPassword", model);
			}

			return View("ResetPassword", new PasswordResetRequest());
		}

		[AllowAnonymous]
		[HttpPost(Name = nameof(ResetPassword))]
		public async Task<IActionResult> ResetPassword(PasswordResetRequest model)
		{
			var response = await _identityService.ResetPassword(model);

			if (!response.Success)
			{

				TempData["Toast"] = $"{response.Message},{Toast.Error}";
				return View(new PasswordResetRequest() { Username = model.Username, RedirectUrl = model.RedirectUrl });
			}

			TempData["Toast"] = $"Password reset complete. Please sign in with your new password.,{Toast.Success}";
			var returnUrl = response.Data.RedirectUrl ?? Url.Content("~/");

			return RedirectToAction(nameof(AccountController.SignIn), new { returnUrl });
		}

		[AllowAnonymous]
		public async Task<IActionResult> Deactivated()
		{
			await _signInManager.SignOutAsync();
			return View();
		}

		[AllowAnonymous]
		public async Task<IActionResult> AccessDenied()
		{
			//await _signInManager.SignOutAsync();
			return View();
		}

		[AllowAnonymous]
		public IActionResult PasswordExpired()
		{
			var model = new PasswordResetRequest
			{
				Username = TempData["PasswordExpiredUsername"]?.ToString(),
				RedirectUrl = TempData["RedirectUrl"] != null ? TempData["RedirectUrl"] as string : Url.Content("~/")
			};

			return View(model);
		}

		[AllowAnonymous]
		[HttpPost(Name = nameof(ForgotPassword))]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
		{

			var response = await _identityService.ForgotPassword(model);

			if (!response.Success)
			{
				TempData["Toast"] = $"{response.Message},{Toast.Error}";
				return RedirectToAction(nameof(AccountController.SignIn));
			}

			// create an activation link
			var link = Url.ActionLink("ForgotPasswordReset", "Account", new { response.Data.SecurityCode });

			var emailModel = new ResetPasswordEmailViewModel
			{
				Name = response.Data.Name,
				Link = link
			};

			string emailHtmlContent = await this.RenderViewToStringAsync("_ResetPasswordEmail", emailModel);

			var fromAddresses = new List<EmailAddress>();
			var fromAddress = new EmailAddress
			{
				Name = _emailConfiguration.SmtpUsername,
				Address = _emailConfiguration.SmtpUsername
			};
			fromAddresses.Add(fromAddress);

			var toAddresses = new List<EmailAddress>();
			toAddresses.Add(new EmailAddress
			{
				Address = response.Data.Email
			});

			EmailMessage msg = new EmailMessage
			{
				Content = emailHtmlContent,
				Subject = "Mail Manifest System Password Change Request",
				FromAddresses = fromAddresses,
				ToAddresses = toAddresses
			};

			await _emailService.SendAsync(msg, true);

			TempData["Toast"] = $"{response.Message} ,{Toast.Success}";

			return RedirectToAction(nameof(AccountController.SignIn));
		}

		[AllowAnonymous]
		[HttpGet(Name = nameof(ForgotPassword))]
		public IActionResult ForgotPasswordReset(string securityCode)
		{
			var vm = new PasswordResetViewModel()
			{
				SecurityCode = securityCode
			};
			return View(vm);
		}

		[AllowAnonymous]
		[HttpPost(Name = nameof(ForgotPasswordReset))]
		public async Task<IActionResult> ForgotPasswordReset(PasswordResetViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(new PasswordResetViewModel { SecurityCode = model.SecurityCode });
			}

			var response = await _identityService.SetPassword(model.SecurityCode, model.NewPassword);

			if (response.Success)
			{
				TempData["Toast"] = $"Your password was successfully changed. ,{Toast.Success}";
				return RedirectToAction(nameof(AccountController.SignIn));
			}
			else
			{
				TempData["Toast"] = $"{response.Message},{Toast.Error}";
				return View(new PasswordResetViewModel { SecurityCode = model.SecurityCode });
			}



		}

		[AllowAnonymous]
		public IActionResult Help()
		{
			return View();
		}

		public async Task<IActionResult> Profile()
		{
			var appUser = await _userManager.FindByNameAsync(User.GetUsername());
			var userRoles = await _userManager.GetRolesAsync(appUser);

			var claims = User.Claims.Select(x => x.Type +":"+x.Value).ToList();

			var context = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

			var model = new ProfileModel
            {
                UserName = appUser.UserName,
                Email = appUser.Email,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Site = appUser.Site,
                Client = appUser.Client,
                SubClient = appUser.SubClient,
                ConsecutiveScansAllowed = appUser.ConsecutiveScansAllowed,
                Roles = userRoles.ToList(),
                Claims = claims
            };

			return View(model);
		}

	}
}
