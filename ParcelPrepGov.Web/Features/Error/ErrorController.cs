using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelPrepGov.Web.Features.Error.Models;
using ParcelPrepGov.Web.Infrastructure;
using System.Diagnostics;

namespace ParcelPrepGov.Web.Features.Error
{
	[AllowAnonymous]
	public class ErrorController : Controller
	{
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Index()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
