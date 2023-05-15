using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Infrastructure;

namespace ParcelPrepGov.Web.Features.Supervisor
{
	[Authorize(Roles = PPGRole.SystemAdministrator + "," + PPGRole.Administrator + "," + PPGRole.GeneralManager + "," + PPGRole.Supervisor)]
	public class SupervisorController : Controller
	{
		public IActionResult Index()
		{

			return View();
		}
	}
}
