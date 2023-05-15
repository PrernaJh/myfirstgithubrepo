using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelPrepGov.Web.Features.Financials.Models;
using ParcelPrepGov.Web.Infrastructure;

namespace ParcelPrepGov.Web.Features.Financials
{
    [Authorize]
    public class FinancialsController : Controller
    {
        public IActionResult AzureServerBinding()
        { 
            return View();
        }         
    }
}
