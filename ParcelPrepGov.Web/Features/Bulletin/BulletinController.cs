using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Bulletin.Models;
using ParcelPrepGov.Web.Features.Financials.Models;
using ParcelPrepGov.Web.Infrastructure;
using System;

namespace ParcelPrepGov.Web.Features.Bulletin
{
    [Authorize]
    public class BulletinController : Controller
    {
        private readonly IConfiguration _config;

        public BulletinController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult BulletinBinding()
        {
            //var provider = new BulletinContainer(_config);
            //return View(provider.AzureFileProvider);
            return View();
        } 
    }

     
}
