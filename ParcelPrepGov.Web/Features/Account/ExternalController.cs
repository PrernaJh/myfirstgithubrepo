using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Service.Interfaces;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.Account
{
    public class ExternalController : Controller
    {
        private readonly IExternalSignInManager _externalSignInManager;
        private readonly ILogger<ExternalController> _logger;

        public ExternalController(IExternalSignInManager externalSignInManager, ILogger<ExternalController> logger)
        {
            _externalSignInManager = externalSignInManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("[controller]/Callback")]
        public async Task<IActionResult> CallbackAsync(string returnUrl = "/")
        {
            var actualRedirect = returnUrl ?? "/";

            if (await _externalSignInManager.SignInAsync() || await _externalSignInManager.LinkExternalLoginWithUserAsync())
            {
                await _externalSignInManager.UpdateUserRolesIfNeededAsync();

                return LocalRedirect(actualRedirect);
            }
            else if (await _externalSignInManager.CanExternalUserRegisterAsync())
            {
                try
                {
                    await _externalSignInManager.CreateUserFromExternalLoginAsync();

                    if (await _externalSignInManager.SignInAsync())
                    {
                        return LocalRedirect(actualRedirect);
                    }
                }
                catch(ExternalLoginException e)
                {
                    _logger.LogWarning(e, "Invalid external login attempt");
                }
            }

            return RedirectToAction("SignIn", "Account");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var callbackUrl = Url.Action("Callback", "External", values: new { returnUrl });
            var authenticationProperties = _externalSignInManager.GetAuthenticationProperties(OpenIdConnectDefaults.AuthenticationScheme, callbackUrl);
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
