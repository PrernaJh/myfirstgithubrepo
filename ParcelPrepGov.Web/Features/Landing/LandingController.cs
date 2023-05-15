using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelPrepGov.Web.Infrastructure;
using Microsoft.Extensions.Configuration;
using PackageTracker.Identity.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.Landing
{
	[Authorize]
	public class LandingController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _config;

        public LandingController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public IActionResult Index()
		{
			return View();
		}

		public async Task<RedirectResult> HelpDeskRedirect()
		{
			var user = await _userManager.GetUserAsync(User);
			var helpDeskLoginUtil = new PackageTracker.Domain.Utilities.HelpDeskAutoLoginUtility(_userManager, _config);
			var url = await helpDeskLoginUtil.GenerateHelpDeskUrlAsync(user.UserName, user.Email, _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value);

			return Redirect(url);
		}

		public async Task<ActionResult> HelpDeskLogout() 
		{
			var url = await new PackageTracker.Domain.Utilities.HelpDeskAutoLoginUtility(_userManager, _config).GenerateHelpDeskLogoutURLAsync();

			return  Redirect(url);
		}
	}
}
