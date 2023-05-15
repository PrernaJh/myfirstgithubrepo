using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Returns;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
    [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReturnController : ControllerBase
    {
        private readonly IReturnProcessor returnProcessor;
        private readonly ILogger<ReturnController> logger;

        public ReturnController(IReturnProcessor returnProcessor, ILogger<ReturnController> logger)
        {
            this.returnProcessor = returnProcessor;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetReturnOptionResponse), 200)]
        public async Task<IActionResult> GetReturnOptions(string siteName)
        {
            try
            {
                logger.Log(LogLevel.Information, $"siteName: {siteName}");
                var response = await returnProcessor.GetReturnOptionsAsync(siteName);
                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to get return options. Exception: {ex}");
                return BadRequest($"Server Error");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReturnPackageResponse), 200)]
        public async Task<IActionResult> ReturnPackage(ReturnPackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ReturnPackageRequest: {JsonSerializer.Serialize(request)}");
                var response = await returnProcessor.ReturnPackageAsync(request);
                logger.Log(LogLevel.Information, $"ReturnPackageResponse: {JsonSerializer.Serialize(response)}");
                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to return package. Exception: {ex}");
                return BadRequest($"Server Error");
            }
        }
    }
}