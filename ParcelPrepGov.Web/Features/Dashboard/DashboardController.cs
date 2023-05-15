using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Infrastructure;
using System;
using System.Threading.Tasks;
using PackageTracker.Identity.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ParcelPrepGov.Web.Features.Dashboard
{
	[Authorize(Roles = PPGRole.SystemAdministrator + "," + PPGRole.Administrator + "," + PPGRole.GeneralManager + "," + PPGRole.Supervisor)]
	public class DashboardController : Controller
	{
		private readonly IPpgDashboardStorage _dashboardStorage;

        public DashboardController(IPpgDashboardStorage dashboardStorage)
        {
            _dashboardStorage = dashboardStorage ?? throw new ArgumentNullException(nameof(dashboardStorage));
        }

        public async Task<IActionResult> Index()
		{
			return View();
		}



		//Jeff J: We don't have a use case for deleting dashboards at this time.
		//[Route("api/dashboards/data/Reporting/DeleteDashboard")]
		//public ActionResult DeleteDashboard(string dashboardId)
		//{
		//	_dashboardStorage.DeleteDashboard(dashboardId);
		//	return new EmptyResult();
		//}

	}
}